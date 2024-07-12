#ifndef WIFI_H
#define WIFI_H
#pragma once

#include <Arduino.h>

enum WifiState
{
  notFound,
  notConnected,
  connected
};

void wifiInit();
bool connectToWifi(const char* ssid, const char* password, /*out*/WifiState &wifiState);

#endif