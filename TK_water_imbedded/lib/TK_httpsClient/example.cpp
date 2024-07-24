#include "httpsClient.h"
#include "wifi/wifi.h"
#include <Arduino.h>
#include <EEPROM.h>

#define CONNECT_WITH_EEPROM false

static WifiState wifiState = notFound;

void connect_withEEPROM()
{
    EEPROM.begin(512);

    if(!EEPROM_hasWifiData())
        setWifiData_toEEPROM("wifi_name", "wifi_password");

    //try's to getWifiData_toEEPROM() and then try's to connectToWifi()
    wifiInit_EEPROM(/*out*/wifiState);

    if(wifiState == notConnected)
        Serial.println("wifiConnect failed");

    if(wifiState == connected)
        Serial.println("wifiConnect success");
}

void connectManually()
{
    //if timeout_sec(default is 10) is reached then connectToWifi returns true but will set wifiState to notConnected
    while(!connectToWifi("wifi_name", "wifi_password", /*out*/wifiState));

    if(wifiState == notConnected)
        Serial.println("wifiConnect failed");

    if(wifiState == connected)
        Serial.println("wifiConnect success");
}

void setup()
{
    Serial.begin(115200);

    if(CONNECT_WITH_EEPROM)
        connect_withEEPROM();
    else
        connectManually();

    //there i leave certificates empty in the ctor that means that the connection made is not secure. add a certificate if you want a secure connection. 
    httpsRequest_config httpsConfig = httpsRequest_config("https://www.google.com"); 

    const char* response = httpsGet("/path", &httpsConfig, /*out*/wifiState);
    Serial.println("response: " +  String(response)); //should return html page
    
    //safe way to delete c_str when your done with it.
    TRY_DELETE_RESPONSE(response);

    Serial.println("wifiState: " + toString(wifiState));
}

void loop()
{
}