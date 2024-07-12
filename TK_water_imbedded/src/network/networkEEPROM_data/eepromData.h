#ifndef EEPROMDATA_H
#define EEPROMDATA_H
#pragma once

#include <Arduino.h>
const uint8_t boolHasWifi_address = 0;
const uint8_t ssid_address = 2;
const uint8_t password_address = 42;

bool getWifiData_fromEEPROM(/*out*/String &ssid, /*out*/String &password);
void setWifiData_toEEPROM(const char* username, const char* password);
#endif