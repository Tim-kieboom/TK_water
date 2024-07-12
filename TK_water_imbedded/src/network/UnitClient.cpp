#include "UnitClient.h"
#include <HTTPClient.h>
#include <StringTools.h>
#include "../Secret.h"

static WiFiClientSecure client;
static const char* serverURL = "";
static uint16_t port = 443;

bool connectToServer(const char* _serverURL, uint16_t _port)
{
  client.setCACert(certificate);
  int responseCode = client.connect(_serverURL, _port);

  if(!client.connected())
  {
    Serial.printf("client not connected: %d\n", responseCode);
    return false;
  }
  client.stop();

  serverURL = _serverURL;
  port = _port;
  return true;
}

String httpPostRequest(const char* path, const char* payload) 
{
  HTTPClient https;
  https.setTimeout(2000);
  https.begin(client, String(serverURL)+'/'+path);

  https.addHeader("Content-Type", "application/json");
  int httpResponseCode = https.POST(payload);

  if(httpResponseCode < 0)
  {
    Serial.print("Error on sending POST: ");
    Serial.println(httpResponseCode);
    return "";
  }

  String response = https.getString();
  Serial.println("response: " + String(response));
  https.end();
  return response;
}