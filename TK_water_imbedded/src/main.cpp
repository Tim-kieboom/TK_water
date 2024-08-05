#include <Arduino.h>
#include <EEPROM.h>

#include "moistureSensor/moistureSensor.h"
#include "initServer/initServer.h"
#include "waterPump/WaterPump.h"
#include "network.h"

static uint8_t wateringThreshold = 60;

void setup() 
{
  Serial.begin(115200);
  EEPROM.begin(512);

  network_startup(/*out*/wateringThreshold);
}

void loop() 
{
  static BigTimer dataTimer = BigTimer(minutes);

  static MoistureSensor waterSensor = MoistureSensor(MOISTURE_SENSOR_1);
  static WaterPump waterPump = WaterPump(WATER_PUMP_1);

  static bool pumpWater = false;
  static bool initServer = false;

  if(digitalRead(BUTTON_OPEN_INIT_SERVER) == LOW)
    initServer = true;

  if(initServer)
    setupInitServer();

  if(dataTimer.waitTime(10))
  {
    uint8_t moistureLevel = waterSensor.getAveragePercentage();
    if(moistureLevel > wateringThreshold)
      pumpWater = true;

    Serial.println("moistureLevel: " + String(moistureLevel));
    Serial.println("wateringThreshold: " + String(wateringThreshold));

    WifiState wifiState = getWifiState();
    if(hasWifi(wifiState))
      sendData(moistureLevel, /*out*/wateringThreshold);
  }

  if(pumpWater)
  {
    if(waterPump.turnOnFor(200, millieSeconds))
      pumpWater = false;
  }

  setLedToState();
}