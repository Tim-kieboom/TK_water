#ifndef HTTPSCLIENT_H
#define HTTPSCLIENT_H
#pragma once

#include <WifiClientSecure.h>
#include "wifi/wifi.h"
#include <string.h>

#define IS_EMPTY_C_STR(c_str) (c_str == nullptr || strcmp(c_str, "") == 0)
#define TRY_DELETE_RESPONSE(response) if(!IS_EMPTY_C_STR(response)) delete[] response

struct Certificates
{
    const char* ca_cert;
    const char* client_key;
    const char* client_cert;

    Certificates(const char* ca_cert);
    Certificates(const char* ca_cert, const char* client_key, const char* client_cert);
    ~Certificates();
};

struct httpsRequest_config
{
    //set the url to the server you want to connect to (ex: https://111.222.3.4:1234)
    const char* serverURL;
    Certificates* certificate;

    httpsRequest_config(const char* serverURL, Certificates* certificate = nullptr);
    ~httpsRequest_config();
};

//if you leave the certificate empty in the config, the connection will be setInsecure(RECOMMENDED FOR TESTING ONLY!!)
const char* httpsPost(const char* path, const char* jsonPayload, httpsRequest_config* config, /*out*/WifiState &wifiState);

//if you leave the certificate empty in the config, the connection will be setInsecure(RECOMMENDED FOR TESTING ONLY!!)
const char* httpsPost(const char* path, String jsonPayload, httpsRequest_config* config, /*out*/WifiState &wifiState);

//if you leave the certificate empty in the config, the connection will be setInsecure(RECOMMENDED FOR TESTING ONLY!!)
const char* httpsGet(const char* path, httpsRequest_config* config, /*out*/WifiState &wifiState);

//if you leave the certificate empty in the config, the connection will be setInsecure(RECOMMENDED FOR TESTING ONLY!!)=
const char* httpsRequest(const char* serverURL, const char* path, /*out*/WifiState &wifiState, Certificates* certificate, const char* jsonPayload = nullptr);

#endif