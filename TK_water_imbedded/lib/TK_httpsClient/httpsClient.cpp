#include "httpsClient.h"
#include <HTTPClient.h>
#include <Arduino.h>
#include <string.h>
#include <sstream>

#define C_STR_EQUALS(str1, str2) (strcmp(str1, str2) == 0)

Certificates::Certificates(const char* ca_cert) 
{
  this->ca_cert = ca_cert;
  this->client_key = "";
  this->client_cert = "";
}

Certificates::~Certificates() 
{
  if(this->ca_cert != nullptr)
    delete[] this->ca_cert;

  if(this->client_key != nullptr)
    delete[] this->client_key;

  if(this->client_cert != nullptr)
    delete[] this->client_cert;
}

Certificates::Certificates(const char* ca_cert, const char* client_key, const char* client_cert)
  : ca_cert(ca_cert), client_key(client_key), client_cert(client_cert)
{
}

httpsRequest_config::httpsRequest_config(const char* serverURL, Certificates* certificate/*default = nullptr*/)
  : serverURL(serverURL), certificate(certificate)
{
}

httpsRequest_config::~httpsRequest_config()
{
  if(certificate != nullptr)
    delete certificate;
}


static WiFiClientSecure client;

const char* httpsPost(const char* path, String jsonPayload, httpsRequest_config* config, /*out*/WifiState &wifiState) 
{
  return httpsRequest(config->serverURL, path, /*out*/wifiState, config->certificate, jsonPayload.c_str());
}

const char* httpsPost(const char* path, const char* jsonPayload, httpsRequest_config* config, /*out*/WifiState &wifiState) 
{
  return httpsRequest(config->serverURL, path, /*out*/wifiState, config->certificate, jsonPayload);
}

const char* httpsGet(const char* path, httpsRequest_config* config, /*out*/WifiState &wifiState) 
{
  return httpsRequest(config->serverURL, path, /*out*/wifiState, config->certificate);
}

const char* httpsRequest(const char* serverURL, const char* path, /*out*/WifiState &wifiState, Certificates* certificate, const char* jsonPayload/*default = nullptr*/)
{
  if(WiFi.status() != WL_CONNECTED)
  {
    Serial.println("!!Wifi not connected at httpPostRequest()!!");
    wifiState = notConnected;
    return "";
  }

  if(certificate != nullptr)
  {
    if(!C_STR_EQUALS(certificate->ca_cert, ""))
      client.setCACert(certificate->ca_cert);

    if(!C_STR_EQUALS(certificate->client_cert, ""))
      client.setCertificate(certificate->client_cert);

    if(!C_STR_EQUALS(certificate->client_key, ""))
      client.setPrivateKey(certificate->client_key);
  }
  else
  {
    client.setInsecure();
  }

  if(strlen(serverURL) == 0)
  {
    Serial.println("!!no serverURL in httpPostRequest() (use setServerURL() to set the url)!!");
    wifiState = serverNotFound;
    return "";
  }

  HTTPClient https;
  String url = String(serverURL)+'/'+path;
  if(!https.begin(client, url))
  {
    Serial.println("!!Failed to connect to server with post request!!");
    wifiState = serverNotFound;
    https.end();
    return "";
  }

  https.addHeader("Content-Type", "application/json");
  
  int httpResponseCode = -404;

  if(jsonPayload == nullptr)
    httpResponseCode = https.GET();
  else
    httpResponseCode = https.POST(jsonPayload);
  
  if(httpResponseCode < 0)
  {
    Serial.print("Error on sending POST: ");
    Serial.println(httpResponseCode);
    wifiState = serverRequestFailed;
    https.end();
    return "";
  }

  //this code is because arduino String often has memory issues
  String response = https.getString();
  char* responseArray = new char [response.length()+1];
  strcpy(responseArray, response.c_str());

  wifiState = connected;
  https.end();
  return responseArray;
}