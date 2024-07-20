#include <ESPAsyncWebServer.h>
#include <TK_httpsClient.h>
#include <StringTools.h>
#include "initServer.h"
#include <Arduino.h>
#include <SPIFFS.h>
#include <sstream>
#include <WiFi.h>

static AsyncWebServer server(80);
static IPAddress IP;

void handleConnect(AsyncWebServerRequest *request, uint8_t *data, size_t len, size_t index, size_t total)
{
    std::stringstream sUser;
    std::stringstream sPassword;
    bool newline = false;
    for (int i = 0; i < len; i++)
    {
        if (data[i] == '\n')
        {
            newline = true;
            continue;
        }

        if (newline)
            sPassword << data[i];
        else
            sUser << data[i];
    }

    request->send(SPIFFS, /*routingPath=*/"/index.html", /*contextType=*/String(), /*download=*/false);

    std::string username = sUser.str();
    std::string password = sPassword.str();

    if (username.length() == 0 || password.length() == 0)
        return;

    SPIFFS.end();
    server.end();
    WiFi.disconnect();
    WiFi.mode(WIFI_STA);

    setWifiData_toEEPROM(username.c_str(), password.c_str());
}

void serverController()
{
    // starts html
    server.on("/", HTTP_GET,
        [](AsyncWebServerRequest *request)
        {
        request->send(SPIFFS, /*routingPath=*/"/index.html", /*contextType=*/String(), /*download=*/false);
        });

    // starts css
    server.on("/style.css", HTTP_GET,
        [](AsyncWebServerRequest *request)
        {
        request->send(SPIFFS, /*routingPath=*/"/style.css", /*contextType=*/"text/css");
        });

    // connect to wifi
    server.on("/connect", HTTP_POST, [](AsyncWebServerRequest *request) {}, NULL, handleConnect);
}

void initServer()
{
    if (!SPIFFS.begin(true))
    {
        Serial.println("SPIFFS couldn't start");
        exit(1);
    }

    String ssid = "water Unit ";
    ssid += (long)(hash(WiFi.macAddress().c_str()) * 0.6f);

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