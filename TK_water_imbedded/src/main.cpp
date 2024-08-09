#include <Arduino.h>
#include <EEPROM.h>
#include <StringTools.h>

#include "moistureSensor/moistureSensor.h"
#include "initServer/initServer.h"
#include "waterPump/WaterPump.h"
#include "TK_Serial/TK_Serial.h"
#include "network.h"
#include <Mqtt.h>

#define ONLINE 1
#define OFFLINE 0

static const size_t wifiUnitID = hash(WiFi.macAddress().c_str());

static uint8_t wateringThreshold = 55;

static BigTimer dataTimer = BigTimer(minutes);
static MoistureSensor waterSensor = MoistureSensor(MOISTURE_SENSOR_1);
static WaterPump waterPump = WaterPump(WATER_PUMP_1);

static bool pumpWater = false;
static bool initServer = false;
static bool signedIn = false;

static bool isBackendConnected = false;

void checkMoisture();

void myCallBack(PubSubClient *client, char *topic, char *message, unsigned int length)
{
  Serial.printf("received [mqtt(%s)]: %s\n", topic, message);

  if(STR_EQUALS(topic, (BASE_TOPIC(wifiUnitID) + "/checkManual").c_str()))
  {
    Serial.println("checkManual");
    checkMoisture();
  }

  if(STR_EQUALS(topic, "backend/online"))
  {
    uint8_t status = atoi(message);

    if(status == ONLINE)
      isBackendConnected = true;

    else if(status == OFFLINE)
      isBackendConnected = false;
  }
}

void mqttStartup(PubSubClient *client)
{
  bool isSuccess = false;

  const char* topic = (BASE_TOPIC(wifiUnitID) + "/#").c_str();
  isSuccess = client->subscribe(topic);
  isSuccess = client->subscribe("backend/online");

  isSuccess = client->publish((BASE_TOPIC(wifiUnitID) + "/online").c_str(), "1");

  if(isSuccess)
    TK_Serial::println("MQTT successfully connected to broker");
  else
    TK_Serial::println("MQTT failed to connect to broker");
}

void checkMoisture()
{
  uint8_t moistureLevel = waterSensor.getAveragePercentage(200);
  
  if(hasWifi(getWifiState()))
    sendData(moistureLevel, isBackendConnected, /*out*/wateringThreshold);

  if(moistureLevel < wateringThreshold)
    pumpWater = true;
}

void setup() 
{
  Serial.begin(115200);
  EEPROM.begin(512);

  network_startup(/*out*/wateringThreshold, wifiUnitID, mqttStartup, myCallBack);
}

void loop() 
{
//!! need this to make mqtt work please dont touch :) !!
  keepMqttAliveLoop();
//

  if(!signedIn)
    signedIn = signIn();

  if(digitalRead(BUTTON_OPEN_INIT_SERVER) == LOW)
    initServer = true;

  if(initServer)
    setupInitServer();

  if(dataTimer.waitTime(10))
    checkMoisture();

  if(pumpWater)
  {
    if(waterPump.turnOnFor(10, seconds))
      pumpWater = false;
  }

  setLedToState();
}