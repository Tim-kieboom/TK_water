#include <Arduino.h>
#include "moistureSensor/moistureSensor.h"
#include "network/initServer/initServer.h"
#include "waterPump/WaterPump.h"
#include <EEPROM.h>
#include <WiFi.h>

#define MOISTURE_SENSOR_1 A0
#define WATER_PUMP_1 2
#define BUTTON_OPEN_INIT_SERVER 15

static const uint8_t wateringThreshold_address = 1;
static const char* serverURL = "https://localhost";
static const uint16_t serverPort = 7299;

enum WifiState
{
  notFound,
  notConnected,
  connected
};

bool getWifiData_fromEEPROM(/*out*/String &ssid, /*out*/String &password)
{
  Serial.println(EEPROM.read(boolHasWifi_address));
  if(EEPROM.read(boolHasWifi_address) != 1)
    return false;

  ssid = EEPROM.readString(ssid_address);
  password = EEPROM.readString(password_address);

  return true;
}

bool connectToWifi(const char* ssid, const char* password, /*out*/WifiState &wifiState)
{
  static Timer* timer = nullptr;
  if(timer == nullptr)
  {
    timer = new Timer(millieSeconds);
    WiFi.begin(ssid, password);
  }

  static uint8_t timeout = 0;
  if(timer->waitTime(500))
  {
    Serial.printf("connect attempt %d\n", timeout);

    if(++timeout >= 10)
    {
      timeout = 0;
      wifiState = notConnected;
      delete timer;
      return true;
    }
  }

  if(WiFi.status() != WL_CONNECTED)
    return false;

  timeout = 0;
  delete timer;
  wifiState = connected;
  return true;
}

WifiState wifiState = notFound;

void wifiInit()
{
  String ssid = "";
  String password = "";
  if(!getWifiData_fromEEPROM(/*out*/ssid, /*out*/password))
    wifiState = notFound;
  else
    while(!connectToWifi(ssid.c_str(), password.c_str(), /*out*/wifiState));

  switch(wifiState)
  {
    case notFound:
      Serial.println("Wifi not found");
      break;

    case notConnected:
      Serial.println("Wifi not connected");
      break;

    case connected:
      Serial.println("Wifi connected");
      break;

    default:
      break;
  }
}

bool setupInitServer()
{
  static bool startOfFunction = true;

  if(startOfFunction)
  {
    if(WiFi.status() == WL_CONNECTED)
      WiFi.disconnect();

    startServer();
  }

  startOfFunction = false;

  String ssid = "";
  String password = "";
  if(!getWifiData_fromEEPROM(/*out*/ssid, /*out*/password))
  {
    wifiState = notFound;
    return false;
  }

  if(connectToWifi(ssid.c_str(), password.c_str(), /*out*/wifiState))
  {
    startOfFunction = true;
    return true;
  }

  return false;
}

uint8_t wateringThreshold = 70;
void setup() 
{
  Serial.begin(115200);
  EEPROM.begin(512);

  uint8_t threshold = EEPROM.read(wateringThreshold_address);
  if(threshold != 255)
    wateringThreshold = threshold;

  wifiInit();
}

void loop() 
{
  static Timer dataTimer = Timer(millieSeconds);

  static bool pumpWater = false;
  static bool initServer = false;
  static bool connectedToWifi = true;
  static MoistureSensor waterSensor = MoistureSensor(MOISTURE_SENSOR_1);
  static WaterPump waterPump = WaterPump(WATER_PUMP_1);

  if(digitalRead(BUTTON_OPEN_INIT_SERVER) == HIGH)
    initServer = true;
    
  if(initServer)
  {
    if(setupInitServer())
      initServer = false;
  }

  if(dataTimer.waitTime(700))
  {
    if(waterSensor.getAverageReading() < wateringThreshold)
      pumpWater = true;

    //TODO: send data to server
  }

  if(pumpWater)
  {
    if(waterPump.turnOnFor(200, millieSeconds))
      pumpWater = false;
  }



}
