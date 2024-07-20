#ifndef EEPROMDATA_H
#define EEPROMDATA_H
#pragma once

#include <Arduino.h>
#define default_boolHasWifi_address 0  //Length 1     | uses EEPROM 0-0
#define default_ssid_address 1         //maxLength 32 | uses EEPROM 1-32
#define default_password_address 33    //maxLength 63 | uses EEPROM 33-96

void clearWifiData_fromEEPROM(uint16_t boolHasWifi_address = default_boolHasWifi_address);
bool EEPROM_hasWifiData(uint16_t boolHasWifi_address = default_boolHasWifi_address);
void setWifiData_toEEPROM(const char* username, const char* password, uint16_t boolHasWifi_address = default_boolHasWifi_address, uint16_t ssid_address = default_ssid_address, uint16_t password_address = default_password_address);
bool getWifiData_fromEEPROM(/*out*/String &ssid, /*out*/String &password, uint16_t boolHasWifi_address = default_boolHasWifi_address, uint16_t ssid_address = default_ssid_address, uint16_t password_address = default_password_address);

#endif