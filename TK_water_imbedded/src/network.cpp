#include <Arduino.h>

#include "initServer/initServer.h"
#include <StringTools.h>
#include <ArduinoJson.h>
#include <RGB_Led.h>
#include <EEPROM.h>
#include "network.h"
#include "Secret.h"
#include <WiFi.h>


static WifiState wifiState = notFound;
static const size_t wifiUnitID = hash(WiFi.macAddress().c_str());

static RGB_Led led = RGB_Led(LED_1_R, LED_1_G, LED_1_B);

static httpsRequest_config* httpConfig = new httpsRequest_config("https://192.168.3.79:1038");	

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

void network_startup(/*out*/uint8_t &wateringThreshold)
{
  pinMode(BUTTON_OPEN_INIT_SERVER, INPUT_PULLUP);

  uint8_t threshold = EEPROM.read(wateringThreshold_address);
  if(threshold != 255)
    wateringThreshold = threshold;

  setLedToState();

  wifiState = wifiInit_EEPROM();

  if(signIn())
    Serial.println("Connected to server");
}

WifiState getWifiState()
{
  return wifiState;
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

void setThreshold(const char* response, /*out*/uint8_t &wateringThreshold)
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

void sendData(uint8_t moistureLevel, /*out*/uint8_t &wateringThreshold)
{
  String moistureLevel_str = String(moistureLevel);

  const char* response = httpsPost
  (
    "controlCentrum/postUnitMeasurement", 
    "{\"unitID\": \""+ String(wifiUnitID) +"\", \"moistureLevel\": "+ moistureLevel_str +"}", 
    httpConfig, 
    /*out*/wifiState
  );
  setThreshold(response, wateringThreshold);

  Serial.println("[post(postUnitMeasurement)] response: " + String(response) + "\n");

  TRY_DELETE_RESPONSE(response);
}