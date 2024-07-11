#include "UnitClient.h"
#include <HTTPClient.h>
#include "../Secret.h"

static WiFiClientSecure client;

void connectToServer(const char* serverURL, uint16_t port)
{
  client.setCACert(certificate);
  client.connect(serverURL, port);

  if(!client.connected())
    Serial.println("client not connected");
}

String httpGETRequest(const char* serverName) 
{


  // WiFiClient client;
  // HTTPClient http;
    
  // // Your Domain name with URL path or IP address with path
  // http.begin(client, serverName);
  
  // // Send HTTP POST request
  // int httpResponseCode = http.GET();
  
  // String payload = "--"; 
  
  // if (httpResponseCode>0) {
  //   Serial.print("HTTP Response code: ");
  //   Serial.println(httpResponseCode);
  //   payload = http.getString();
  // }
  // else {
  //   Serial.print("Error code: ");
  //   Serial.println(httpResponseCode);
  // }
  // // Free resources
  // http.end();

  // return payload;
}