#include <TK_Timer.h>
#include <WiFi.h>
#include "wifi.h"

bool hasWifi(WifiState wifiState)
{
  return (wifiState != notFound && wifiState != hotspot && wifiState != notConnected);
}

bool connectToWifi(const char* ssid, const char* password, /*out*/WifiState &wifiState, uint8_t timeout_sec/*default = 10*/)
{
  static Timer* timer = nullptr;
  if(timer == nullptr)
  {
    timer = new Timer(millieSeconds);
    WiFi.begin(ssid, password);
  }

  static uint8_t timeoutCounter = 0;
  if(timer->waitTime(1000))
  {
    Serial.printf("connect attempt %d\n", timeoutCounter);

    if(++timeoutCounter >= timeout_sec)
    {
      timeoutCounter = 0;
      wifiState = notConnected;

      delete timer;
      timer = nullptr;
      return true;
    }
  }

  if(WiFi.status() != WL_CONNECTED)
    return false;

  timeoutCounter = 0;
  delete timer;
  timer = nullptr;
  wifiState = connected;
  return true;
}

static WifiState wifiState = notFound;

WifiState wifiInit_EEPROM(uint8_t timeout/*default = 10*/, uint16_t boolHasWifi_address/*default = default_boolHasWifi_address*/, uint16_t ssid_address/*default = default_ssid_address*/, uint16_t password_address/*default = default_password_address*/)
{
  String ssid = "";
  String password = "";
  if(!getWifiData_fromEEPROM(/*out*/ssid, /*out*/password, boolHasWifi_address, ssid_address, password_address))
    wifiState = notFound;
  else
    while(!connectToWifi(ssid.c_str(), password.c_str(), /*out*/wifiState, timeout));

  switch(wifiState)
  {
    case notFound:
      Serial.println("Wifi not found");
      return notFound;

    case notConnected:
      Serial.println("Wifi not connected");
      return notConnected;

    case connected:
      Serial.println("Wifi connected");
      return connected;

    default:
      break;
  }

  return notFound;
}

String toString(WifiState wifiState)
{
  switch(wifiState)
  {
    case notFound:
      return "notFound";

    case serverNotFound:
      return "serverNotFound";

    case serverRequestFailed:
      return "serverRequestFailed";

    case notConnected:
      return "notConnected";

    case connecting:
      return "connecting";

    case connected:
      return "connected";

    case hotspot:
      return "hotspot";

    default:
      return "unknown";
  }
}
