#ifndef NETWORK_H
#define NETWORK_H

#include <TK_httpsClient.h>
#include <PubSubClient.h>

#define MOISTURE_SENSOR_1 A3
#define WATER_PUMP_1 32
#define BUTTON_OPEN_INIT_SERVER 15

#define LED_1_R 33
#define LED_1_G 27
#define LED_1_B 12

#define wateringThreshold_address 97

#define BASE_TOPIC(hashID) "unit/" + String(hashID)
#define STR_EQUALS(x, y) (strcmp(x, y) == 0)

void setLedToState();
void network_startup(/*out*/uint8_t &wateringThreshold, size_t unitID, void (*mqttStartup)(PubSubClient *client), void (*myCallBack)(PubSubClient * client, char* topic, char* message, unsigned int length));

//!!! you need to put this function in the loop else mqtt will not work !!! 
void keepMqttAliveLoop();

WifiState getWifiState();
bool signIn();

void setupInitServer();
bool setThreshold(const char* response, /*out*/uint8_t &wateringThreshold);
void sendData(uint8_t moistureLevel, bool isBackendConnected, /*out*/uint8_t &wateringThreshold);

#endif