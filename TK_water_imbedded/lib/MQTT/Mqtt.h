#pragma once
#ifndef _MQTT_H_
#define _MQTT_H_

#include "Arduino.h"
#include <WiFi.h>
#include <PubSubClient.h>
#include <time.h>

class Mqtt
{
private:
    const char* BOT_ID;
    const char* ssid;
    const char* password;

    const char* MqttUser;
    const char* MqttPass;

    const char* lastWillTopic = nullptr;
    const char* lastWillMessage = nullptr;

    WiFiClient espClient;
    PubSubClient *client;

    void (*pubSub)(PubSubClient *client);

public:
    Mqtt(const char* ID, const char* MqttUser, const char* MqttPass);
    Mqtt(const char* ssid, const char* password, const char* ID, const char* MqttUser, const char* MqttPass);
    ~Mqtt();

    void setCallback(const char* mqttServer, int mqttPort, void (*pubSub)(PubSubClient *client), void (*myCallBack)(PubSubClient *client, char *topic, char *message, unsigned int length));
    void setLastWill(const char* topic, const char* message);
    void setLastWill(String topic, const char* message);

    void loop();
    void reconnect();

    bool isConnected();
    bool publish(const char *topic, const char *payload);
    bool publish(String topic, String payload);
    bool publish(const char *topic, String payload);
    bool publish(String topic, const char *payload);

    PubSubClient *getClient();
    const char* getUTCTimeNow();

private:
    void setupWifi();
};

#endif