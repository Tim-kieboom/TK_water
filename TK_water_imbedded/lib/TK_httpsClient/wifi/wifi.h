#ifndef WIFI_H
#define WIFI_H
#pragma once

#include <Arduino.h>
#include "../networkEEPROM_data/eepromData.h"

enum WifiState
{
  notFound,
  serverNotFound,
  serverRequestFailed,
  notConnected,
  connecting,
  connected,
  hotspot
};

WifiState wifiInit_EEPROM(uint8_t timeout = 10, uint16_t boolHasWifi_address = default_boolHasWifi_address, uint16_t ssid_address = default_ssid_address, uint16_t password_address = default_password_address);
bool hasWifi(WifiState &wifiState);
bool connectToWifi(const char* ssid, const char* password, /*out*/WifiState &wifiState, uint8_t timeout = 10);
String toString(WifiState wifiState);

#endif