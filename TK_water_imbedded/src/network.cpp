#include <Arduino.h>

#include "initServer/initServer.h"
#include "TK_Serial/TK_Serial.h"
#include <ArduinoQueue.h>
#include <StringTools.h>
#include <ArduinoJson.h>
#include <RGB_Led.h>
#include "network.h"
#include <EEPROM.h>
#include "Secret.h"
#include <sstream>
#include <WiFi.h>
#include "time.h"
#include <Mqtt.h>

#define TRY_DELETE_C_STR(str) if(!(str == nullptr || strcmp(str, "") == 0)) delete[] dateTime

struct BackupStorage
{
  uint8_t moistureLevel;
  const char* dateTime;

  ~BackupStorage() 
  {
    TRY_DELETE_C_STR(dateTime);
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
    configTime(0, 0, "pool.ntp.org");
    delay(2000);
    mqttInit(mqttStartup, myCallBack);
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

void backupData(ArduinoQueue<BackupStorage*> &queue, uint8_t moistureLevel)
{
  if(queue.isFull())
  {
    BackupStorage *dummy = queue.dequeue();
    
    if(dummy != nullptr)
      delete dummy;
  }

  BackupStorage *backupRecord = new BackupStorage();
  backupRecord->moistureLevel = moistureLevel;
  backupRecord->dateTime = mqtt->getUTCTimeNow();

  TK_Serial::println("[backup_queue.push] moistureLevel: " + String(backupRecord->moistureLevel) + " - dateTime: " + String(backupRecord->dateTime));
  queue.enqueue(backupRecord);
}

void sendBackupData(ArduinoQueue<BackupStorage*> &queue)
{
  while(!queue.isEmpty())
  {
    BackupStorage *backupRecord = queue.dequeue();

    TK_Serial::println("[backup_queue.pop] moistureLevel: " + String(backupRecord->moistureLevel) + String(" - dateTime: ") + backupRecord->dateTime);

    String payload = "{\"unitID\": \""+ String(wifiUnitID) +"\", \"record\": {\"moistureLevel\":" + String(backupRecord->moistureLevel) + String(", \"dateTime\": \"") + backupRecord->dateTime + "\"}";
    mqtt->publish("/postUnitMeasurement", payload.c_str());

    if(backupRecord != nullptr)
      delete backupRecord;
  }
}

void sendData(uint8_t moistureLevel, bool isBackendConnected, /*out*/uint8_t &wateringThreshold)
{
  static ArduinoQueue<BackupStorage*> queue(720);
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
  
  TK_Serial::println("[mqtt.publish] \"unit/~/postUnitMeasurement\" successful \"unitID\": \""+ String(wifiUnitID) +"\", \"moistureLevel\": "+ moistureLevel_str );
  
  if(!queue.isEmpty())
    sendBackupData(queue);
}