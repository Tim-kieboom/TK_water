#include "eepromData.h"
#include <EEPROM.h>

void clearWifiData_fromEEPROM(uint16_t boolHasWifi_address/*default = default_boolHasWifi_address*/)
{
  EEPROM.write(boolHasWifi_address, 0);
  EEPROM.commit();
}

bool EEPROM_hasWifiData(uint16_t boolHasWifi_address/*default = default_boolHasWifi_address*/)
{
  return EEPROM.read(boolHasWifi_address) == 1;
}

void setWifiData_toEEPROM(const char* username, const char* password, uint16_t boolHasWifi_address/*default = default_boolHasWifi_address*/, uint16_t ssid_address/*default = default_ssid_address*/, uint16_t password_address/*default = default_password_address*/)
{
  EEPROM.write(boolHasWifi_address, 1);
  EEPROM.writeString(ssid_address, username);
  EEPROM.writeString(password_address, password);
  EEPROM.commit();
}

bool getWifiData_fromEEPROM(/*out*/String &ssid, /*out*/String &password, uint16_t boolHasWifi_address/*default = default_boolHasWifi_address*/, uint16_t ssid_address/*default = default_ssid_address*/, uint16_t password_address/*default = default_password_address*/)
{
  if(!EEPROM_hasWifiData(boolHasWifi_address))
    return false;

  ssid = EEPROM.readString(ssid_address);
  password = EEPROM.readString(password_address);
  
  return true;
}