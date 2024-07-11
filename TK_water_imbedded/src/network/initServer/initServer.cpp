#include "initServer.h"
#include <ESPAsyncWebServer.h>
#include <Arduino.h>
#include <SPIFFS.h>
#include <WiFi.h>
#include <EEPROM.h>

static AsyncWebServer server(80);
static IPAddress IP;

size_t hash(const char* str, size_t len, size_t index = 0, size_t hashNumber = 5381)
{
    return (index < len) ? hash(str, len, index + 1, ((hashNumber << 5) + hashNumber) + str[index]) : hashNumber;
}

size_t hash(String str)
{
    return hash(str.c_str(), str.length());
}

void handleConnect(AsyncWebServerRequest *request)
{
    const char* username = "";
    const char* password = "";
    int paramsSize = request->params();
    for(int i = 0; i < paramsSize; i++)
    {
        AsyncWebParameter* param = request->getParam(i);
        if(param->name() == "username")
        {
            username = param->value().c_str();
        }
        else if(param->name() == "password")
        {
            password = param->value().c_str();
        }
    }

    request->send(SPIFFS, /*routingPath=*/"/index.html", /*contextType=*/String(), /*download=*/false);

    if(strlen(username) == 0 || strlen(password) == 0)
        return;

    SPIFFS.end();
    server.end();
    WiFi.disconnect();
    WiFi.mode(WIFI_STA);

    EEPROM.write(boolHasWifi_address, 1);
    EEPROM.writeString(ssid_address, username);
    EEPROM.writeString(password_address, password);
    EEPROM.commit();

}

void serverController()
{
    //starts html
    server.on("/", HTTP_GET, 
        [](AsyncWebServerRequest *request)
        {
            request->send(SPIFFS, /*routingPath=*/"/index.html", /*contextType=*/String(), /*download=*/false);
        });

    //starts css
    server.on("/style.css", HTTP_GET, 
        [](AsyncWebServerRequest *request)
        {
            request->send(SPIFFS, /*routingPath=*/"/style.css", /*contextType=*/"text/css");
        });

    //connect to wifi
    server.on("/connect", HTTP_POST, handleConnect);
}

void initServer()
{
    if(!SPIFFS.begin(true))
    {
        Serial.println("SPIFFS couldn't start");
        exit(1);
    }

    String ssid = "water Unit ";
    ssid += hash(WiFi.macAddress().c_str());

    WiFi.softAP(ssid, "testPassword");

    IP = WiFi.softAPIP();

    Serial.print("ip address: ");
    Serial.println(IP);
    Serial.println(ssid);

    serverController();

    server.begin();
}

void startServer()
{
    initServer();
    serverController();
}