#include "eepromData.h"
#include <EEPROM.h>

void setWifiData_toEEPROM(const char* username, const char* password)
{
    EEPROM.write(boolHasWifi_address, 1);
    EEPROM.writeString(ssid_address, username);
    EEPROM.writeString(password_address, password);
    EEPROM.commit();
}

bool getWifiData_fromEEPROM(/*out*/String &ssid, /*out*/String &password)
{
  Serial.println(EEPROM.read(boolHasWifi_address));
  if(EEPROM.read(boolHasWifi_address) != 1)
    return false;

  ssid = EEPROM.readString(ssid_address);
  password = EEPROM.readString(password_address);

  return true;
}