#ifndef UNITCLIENT_H
#define UNITCLIENT_H
#pragma once

#include <WifiClientSecure.h>

String httpPostRequest(const char* path, const char* payload);
bool connectToServer(const char* _serverURL, uint16_t _port);

#endif