#include "wifi.h"
#include <Timer.h>
#include <WiFi.h>
#include "../networkEEPROM_data/eepromData.h"

String getWifiUnitID()
{
  return WiFi.macAddress();
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
