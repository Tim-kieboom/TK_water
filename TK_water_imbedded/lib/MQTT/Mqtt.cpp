#include "Mqtt.h"

static void (*globalCallBack)(PubSubClient *client, char *topic, char *message, unsigned int length);
static PubSubClient *globalClient;

void callback(char *topic, uint8_t *payload, unsigned int length)
{
    char message[length + 1]; // +1 for null terminator

    for (int i = 0; i < length; i++)
    {
        message[i] = (char)payload[i];
    }
    message[length] = '\0';

    globalCallBack(globalClient, topic, message, length);
}

//----------------------------------------------------------------

Mqtt::Mqtt(const char* ID, const char* MqttUser, const char* MqttPass)
    : BOT_ID(ID), MqttUser(MqttUser), MqttPass(MqttPass)
{
    globalClient = client = new PubSubClient(espClient);
}

Mqtt::Mqtt(const char* ssid, const char* password, const char* ID, const char* MqttUser, const char* MqttPass)
    : ssid(ssid), password(password), BOT_ID(ID), MqttUser(MqttUser), MqttPass(MqttPass)
{
    setupWifi();

    globalClient = client = new PubSubClient(espClient);
}

Mqtt::~Mqtt()
{
    if (client != nullptr)
        delete client;
}

void Mqtt::setCallback(const char* mqttHost, int mqttPort, void (*mqttStartup)(PubSubClient *client), void (*myCallBack)(PubSubClient *client, char *topic, char *message, unsigned int length))
{
    this->pubSub = mqttStartup;
    globalCallBack = myCallBack;

    client->setServer(mqttHost, mqttPort);
    client->setCallback(callback);
}

void Mqtt::setLastWill(const char* topic, const char* message)
{
    lastWillTopic = topic;
    lastWillMessage = message;
}

void Mqtt::setLastWill(String topic, const char* message)
{
    char* willTopic = new char[topic.length() + 1];
    strcpy(willTopic, topic.c_str());
    lastWillTopic = willTopic;
    
    lastWillMessage = message;
}

void Mqtt::loop()
{
    if(WiFi.status() != WL_CONNECTED)
    {
        Serial.println("[mqtt.loop()] Wifi not connected");
        return;
    }

    if (!client->connected())
        reconnect();
    
    client->loop();
}

void Mqtt::setupWifi()
{
    if(WiFi.status() == WL_CONNECTED)
        return;

    delay(10);

    // We start by connecting to a WiFi network
    Serial.println();
    Serial.print("Connecting to ");
    Serial.println(ssid);

    WiFi.mode(WIFI_STA);
    WiFi.begin(ssid, password);

    while (WiFi.status() != WL_CONNECTED)
    {
        delay(500);
        Serial.print(".");
    }

    Serial.println("");
    Serial.println("WiFi connected");
    Serial.println("IP address: ");
    Serial.println(WiFi.localIP());
}

void Mqtt::reconnect()
{
    WiFi.mode(WIFI_STA);

    uint8_t timeoutCounter = 0;
    while (!client->connected())
    {
        if(++timeoutCounter >= 10)
            break;

        Serial.println("Attempting MQTT connection...");

        bool isConnected = false;
        if(lastWillTopic == nullptr || lastWillMessage == nullptr)
            isConnected = client->connect(BOT_ID, MqttUser, MqttPass);
        else
            isConnected = client->connect(BOT_ID, MqttUser, MqttPass, lastWillTopic, 0, true, lastWillMessage);

        if (isConnected)
        {
            pubSub(client);
            return;
        }
        else
        {
            delay(1000);
        }
    }

    Serial.println("!!MQTT connected failed!!");
}

bool Mqtt::isConnected()
{
    return client->connected();
}

bool Mqtt::publish(const char *topic, const char *payload)
{
    if(client == nullptr || !client->connected())
    {
        Serial.println("Not connected to MQTT broker");
        return false;
    }

    return client->publish(topic, payload);
}

bool Mqtt::publish(String topic, String payload)
{
    return publish(topic.c_str(), payload.c_str());
}

bool Mqtt::publish(const char * topic, String payload)
{
    return publish(topic, payload.c_str());
}

bool Mqtt::publish(String topic, const char * payload)
{
    return publish(topic.c_str(), payload);
}

PubSubClient *Mqtt::getClient()
{
    return client;
}

const char* Mqtt::getUTCTimeNow()
{
    tm timeInfo;
    if(!getLocalTime(&timeInfo))
    {
        Serial.println("Failed to obtain time");
        return "";
    }

    String year    = String(timeInfo.tm_year + 1900);
    String month   = String(timeInfo.tm_mon+1);
    String day     = String(timeInfo.tm_mday);
    String hour    = String(timeInfo.tm_hour);
    String minute  = String(timeInfo.tm_min);
    String second  = String(timeInfo.tm_sec);

    String time = year + '-' + month + '-' + day + 'T';
    time += (timeInfo.tm_hour < 10) ? "0" : "";
    time += hour + ':';
    time += (timeInfo.tm_min < 10) ? "0" : "";
    time += minute + ':';
    time += (timeInfo.tm_sec < 10) ? "0" : "";
    time += second + 'Z';

    char* buffer = new char[time.length() + 1];
    strcpy(buffer, time.c_str());
    return buffer;
}