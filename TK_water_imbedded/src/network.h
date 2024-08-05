#ifndef NETWORK_H
#define NETWORK_H

#include <TK_httpsClient.h>

#define MOISTURE_SENSOR_1 A3
#define WATER_PUMP_1 32
#define BUTTON_OPEN_INIT_SERVER 15

#define LED_1_R 33
#define LED_1_G 27
#define LED_1_B 12

#define wateringThreshold_address 97

void setLedToState();
void network_startup(/*out*/uint8_t &wateringThreshold);

WifiState getWifiState();

void setupInitServer();
void setThreshold(const char* response, /*out*/uint8_t &wateringThreshold);
void sendData(uint8_t moistureLevel, /*out*/uint8_t &wateringThreshold);

#endif