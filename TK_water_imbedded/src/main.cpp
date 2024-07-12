#include "network/networkEEPROM_data/eepromData.h"
#include "moistureSensor/moistureSensor.h"
#include "network/initServer/initServer.h"
#include "waterPump/WaterPump.h"
#include "network/UnitClient.h"
#include "network/wifi/wifi.h"
#include <StringTools.h>
#include <Arduino.h>
#include <EEPROM.h>
#include <WiFi.h>
#include <functional>

#define MOISTURE_SENSOR_1 A0
#define WATER_PUMP_1 2
#define BUTTON_OPEN_INIT_SERVER 15

static const uint8_t wateringThreshold_address = 1;
static const char* serverURL = "https://localhost";
static const uint16_t serverPort = 7299;

static uint8_t wateringThreshold = 70;
static uint8_t moistureLevel = 0;

static WifiState wifiState = notFound;
static const String wifiUnitID = String(hash(WiFi.macAddress().c_str()));

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
    moistureLevel = waterSensor.getAverageReading();
    if(moistureLevel < wateringThreshold)
      pumpWater = true;

    httpPostRequest
    (
      "postUnitMeasurement", 
      ("{\"unitID\": \""+wifiUnitID+"\", \"moistureLevel\": "+String(moistureLevel)+"}").c_str()
    );
  }

  if(pumpWater)
  {
    if(waterPump.turnOnFor(200, millieSeconds))
      pumpWater = false;
  }
}
