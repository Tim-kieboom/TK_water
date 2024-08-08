#include <Arduino.h>

#include "initServer/initServer.h"
#include "TK_Serial/TK_Serial.h"
#include <StringTools.h>
#include <ArduinoJson.h>
#include <cppQueue.h>
#include <RGB_Led.h>
#include "network.h"
#include <EEPROM.h>
#include "Secret.h"
#include <sstream>
#include <WiFi.h>
#include "time.h"
#include <Mqtt.h>

struct BackupStorage
{
  uint8_t moistureLevel;
  const char* dateTime;

  ~BackupStorage() 
  {
    TRY_DELETE_RESPONSE(dateTime);
  }	
};

static WifiState wifiState = notFound;
static size_t wifiUnitID = 0;

static RGB_Led led = RGB_Led(LED_1_R, LED_1_G, LED_1_B);

static httpsRequest_config* httpConfig = new httpsRequest_config("https://192.168.3.79:1038");	
Mqtt* mqtt = nullptr;

void mqttInit(void (*mqttStartup)(PubSubClient *client), void (*myCallBack)(PubSubClient * client, char* topic, char* message, unsigned int length))
{
  String id = String(wifiUnitID);
  char* unitID = new char[id.length() + 1];
  strcpy(unitID, id.c_str());

  mqtt = new Mqtt((const char*)unitID, "TK_water_unit", "password");
  mqtt->setCallback("192.168.3.79", 9800, mqttStartup, myCallBack);
  mqtt->setLastWill(BASE_TOPIC(wifiUnitID) + "/online", "0");

  TK_Serial::setMqtt(mqtt, wifiUnitID);
}

void keepMqttAliveLoop()
{
  if(mqtt == nullptr)
  {
    TK_Serial::println("keepMqttAliveLoop() mqtt is null");
    return;
  }

  mqtt->loop();
}

void network_startup(/*out*/uint8_t &wateringThreshold, size_t unitID, void (*mqttStartup)(PubSubClient *client), void (*myCallBack) (PubSubClient * client, char* topic, char* message, unsigned int length))
{
  pinMode(BUTTON_OPEN_INIT_SERVER, INPUT_PULLUP);

  wifiUnitID = unitID;

  uint8_t threshold = EEPROM.read(wateringThreshold_address);
  if(threshold != 255)
    wateringThreshold = threshold;

  setLedToState();

  wifiState = wifiInit_EEPROM();

  if(hasWifi(wifiState))
  {
    mqttInit(mqttStartup, myCallBack);
    configTime(0, 0, "pool.ntp.org");
  }
  else
  {
    TK_Serial::println("no wifi");
  }

  Serial.println("about to signin");
}

WifiState getWifiState()
{
  return wifiState;
}

bool signIn()
{
  return mqtt->publish(BASE_TOPIC(wifiUnitID) + "/signIn", String(wifiUnitID));
}

void setLedToState()
{
  switch(wifiState)
  {
    case notFound:
      led.turn(RGB_LedColor::red);
      return;

    case serverNotFound:
      led.turn(RGB_LedColor::cyan);
      break;

    case notConnected:  
      led.turn(RGB_LedColor::yellow);
      return;

    case connecting:
      led.turn(RGB_LedColor::white);
      return;

    case connected:
      led.turn(RGB_LedColor::green);
      return;

    case hotspot:
      led.turn(RGB_LedColor::magenta);
      return;

    default:
      led.turn(RGB_LedColor::off);
      return;
  }
}

const char* getTimeNow()
{
  tm timeInfo;
  if(!getLocalTime(&timeInfo))
  {
    TK_Serial::println("Failed to obtain time");
    return "";
  }

  char* buffer = new char[50];
  strftime(buffer, sizeof(buffer), "%A, %B %d %Y %H:%M:%S", &timeInfo);
  return buffer;
}

void setupInitServer()
{
  static bool startOfFunction = true;

  if(startOfFunction)
  {
    if(WiFi.status() == WL_CONNECTED)
      WiFi.disconnect();

    startServer();
    clearWifiData_fromEEPROM();
    wifiState = hotspot;
  }

  startOfFunction = false;

  String ssid = "";
  String password = "";
  if(getWifiData_fromEEPROM(/*out*/ssid, /*out*/password))
    ESP.restart();
  
  return;
}

bool setThreshold(const char* response, /*out*/uint8_t &wateringThreshold)
{
  JsonDocument json;

  if(deserializeJson(json, response) != DeserializationError::Ok)
    return false;

  uint8_t threshold = json["moistureThreshold"];

  if(threshold > 100)
    return false;

  wateringThreshold = threshold;
  EEPROM.write(wateringThreshold_address, wateringThreshold);
  EEPROM.commit();

  return true;
}

void backupData(cppQueue &queue, uint8_t moistureLevel)
{
  if(queue.isFull())
  {
    BackupStorage *dummy;
    queue.pop(dummy);
    delete dummy;
  }

  BackupStorage *backupRecord = new BackupStorage();
  backupRecord->moistureLevel = moistureLevel;
  backupRecord->dateTime = getTimeNow();

  TK_Serial::println("[backup_queue.push] moistureLevel: " + String(backupRecord->moistureLevel) + " - dateTime: " + String(backupRecord->dateTime));
  queue.push(backupRecord);
}

void sendBackupData(cppQueue &queue)
{
  std::stringstream ss;
  ss << ("{\"unitID\": \""+ String(wifiUnitID) +"\", \"records\": [").c_str();

  BackupStorage *backupRecord;
  queue.pop(backupRecord);
  TK_Serial::println("[backup_queue.pop] moistureLevel: " + String(backupRecord->moistureLevel) + String(" - dateTime: ") + backupRecord->dateTime);

  ss << ("{\"moistureLevel\":" + String(backupRecord->moistureLevel) + String(", \"dateTime\": \"") + backupRecord->dateTime + "\"}").c_str();

  if(backupRecord != nullptr)
    delete backupRecord;

  while(!queue.isEmpty())
  {
    BackupStorage *backupRecord;
    queue.pop(backupRecord);
    TK_Serial::println("[backup_queue.pop] moistureLevel: " + String(backupRecord->moistureLevel) + String(" - dateTime: ") + backupRecord->dateTime);

    ss << ',';
    ss << ("{\"moistureLevel\":" + String(backupRecord->moistureLevel) + String(", \"dateTime\": \"") + backupRecord->dateTime + "\"}").c_str();

    if(backupRecord != nullptr)
      delete backupRecord;
  }
  ss << "]}";

  const char* backUpJson = ss.str().c_str();
  TK_Serial::println(backUpJson);

  mqtt->publish("/postUnitMeasurement", ss.str().c_str());
}

void sendData(uint8_t moistureLevel, bool isBackendConnected, /*out*/uint8_t &wateringThreshold)
{
  static cppQueue queue = cppQueue(sizeof(BackupStorage), 720);
  uint8_t oldThreshold = wateringThreshold;
  String moistureLevel_str = String(moistureLevel);
  
  if(!isBackendConnected)
  {
    TK_Serial::println("[mqtt.publish] Backend offline");
    backupData(queue, moistureLevel);
    return;
  }

  bool mqttSuccess = mqtt->publish(BASE_TOPIC(wifiUnitID) + "/postUnitMeasurement", "{\"unitID\": \""+ String(wifiUnitID) +"\", \"moistureLevel\": "+ moistureLevel_str +"}");

  if(!mqttSuccess)
  {
    TK_Serial::println("[mqtt.publish] \"unit/~/postUnitMeasurement\" failed to publish");
    backupData(queue, moistureLevel);
    return;
  }
  
  if(!queue.isEmpty())
    sendBackupData(queue);
}