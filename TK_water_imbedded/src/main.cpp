#include "moistureSensor/moistureSensor.h"
#include "initServer/initServer.h"
#include "waterPump/WaterPump.h"
#include <TK_httpsClient.h>
#include <StringTools.h>
#include <ArduinoJson.h>
#include <Arduino.h>
#include <RGB_Led.h>
#include <EEPROM.h>
#include "Secret.h"
#include <WiFi.h>

#define MOISTURE_SENSOR_1 A4
#define WATER_PUMP_1 32
#define BUTTON_OPEN_INIT_SERVER 15

#define LED_1_R 33
#define LED_1_G 27
#define LED_1_B 12

#define wateringThreshold_address 97

static uint8_t wateringThreshold = 70;

static WifiState wifiState = notFound;
static const size_t wifiUnitID = hash(WiFi.macAddress().c_str());

static RGB_Led led = RGB_Led(LED_1_R, LED_1_G, LED_1_B);

static httpsRequest_config* httpConfig = new httpsRequest_config("https://192.168.3.79:1038");	

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

bool signIn()
{
  const char* response = httpsPost
  (
    "controlCentrum/signIn", 
    "{\"unitID\": \""+ String(wifiUnitID) +"\"}", 
    httpConfig, 
    /*out*/wifiState
  );

  Serial.println("[post(signIn)] response: " + String(response));
  TRY_DELETE_RESPONSE(response);

  return (wifiState == connected);
}

void setThreshold(const char* response)
{
  JsonDocument json;

  if(deserializeJson(json, response) != DeserializationError::Ok)
    return;

  uint8_t threshold = json["moistureThreshold"];

  if(threshold > 100)
    return;

  wateringThreshold = threshold;
  EEPROM.write(wateringThreshold_address, wateringThreshold);
  EEPROM.commit();
}

void sendData(uint8_t moistureLevel)
{
  String moistureLevel_str = String(moistureLevel);

  const char* response = httpsPost
  (
    "controlCentrum/postUnitMeasurement", 
    "{\"unitID\": \""+ String(wifiUnitID) +"\", \"moistureLevel\": "+ moistureLevel_str +"}", 
    httpConfig, 
    /*out*/wifiState
  );

  Serial.println("[post(postUnitMeasurement)] response: " + String(response) + "\n");

  TRY_DELETE_RESPONSE(response);
}

void setup() 
{
  Serial.begin(115200);
  EEPROM.begin(512);

  pinMode(BUTTON_OPEN_INIT_SERVER, INPUT_PULLUP);

  uint8_t threshold = EEPROM.read(wateringThreshold_address);
  if(threshold != 255)
    wateringThreshold = threshold;

  setLedToState();

  wifiState = wifiInit_EEPROM();

  if(signIn())
    Serial.println("Connected to server");
}

void loop() 
{
  static BigTimer dataTimer = BigTimer(minutes);

  static bool pumpWater = false;
  static bool initServer = false;

  static MoistureSensor waterSensor = MoistureSensor(MOISTURE_SENSOR_1);
  static WaterPump waterPump = WaterPump(WATER_PUMP_1);

  if(digitalRead(BUTTON_OPEN_INIT_SERVER) == LOW)
    initServer = true;

  if(initServer)
    setupInitServer();

  if(dataTimer.waitTime(10))
  {
    uint8_t moistureLevel = random(0,100);//waterSensor.getAverageReading();
    if(moistureLevel > wateringThreshold)
      pumpWater = true;

    Serial.println("wateringThreshold: " + String(wateringThreshold));

    if(hasWifi(wifiState))
      sendData(moistureLevel);
  }

  if(pumpWater)
  {
    if(waterPump.turnOnFor(200, millieSeconds))
      pumpWater = false;
  }

  setLedToState();
}
