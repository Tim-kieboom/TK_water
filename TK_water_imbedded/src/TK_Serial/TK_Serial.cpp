#include "TK_Serial.h"
#include <Arduino.h>

Mqtt* TK_Serial::mqtt = nullptr;
const char* TK_Serial::logTopic = nullptr;

void TK_Serial::setMqtt(Mqtt *mqtt, size_t id)
{
    TK_Serial::mqtt = mqtt;

    String string = "unit/" + String(id) + "/serialLog";
    char* topic = new char[string.length() + 1];
    strcpy(topic, string.c_str());

    TK_Serial::logTopic = topic;
}

size_t TK_Serial::print(const String &val)
{
    return TK_Serial::print(val.c_str());
}

size_t TK_Serial::println(const String &val)
{
    return TK_Serial::println(val.c_str());
}

size_t TK_Serial::print(StringSumHelper val)
{
    return TK_Serial::print(val.c_str());
}

size_t TK_Serial::println(StringSumHelper val)
{
    return TK_Serial::println(val.c_str());
}

template <typename T>
size_t TK_Serial::print(T val)
{
    size_t size = Serial.print(String(val));

    if(mqtt != nullptr && logTopic != nullptr && mqtt->isConnected())
        mqtt->publish(logTopic, (String(val) + "\n").c_str());

    return size;
}

template <typename T>
size_t TK_Serial::println(T val)
{
    size_t size = Serial.println(String(val));

    if(mqtt != nullptr && logTopic != nullptr && mqtt->isConnected())
        mqtt->publish(logTopic, (String(val) + "\n").c_str());

    return size;
}

size_t TK_Serial::printf(const char *format, ...)
{
    va_list args;
    va_start(args, format);
    char buffer[100];	
    vsnprintf(buffer, sizeof(buffer), format, args);
    va_end(args);

    return TK_Serial::print(buffer);
}

template size_t TK_Serial::print(char val);
template size_t TK_Serial::print(unsigned char val);
template size_t TK_Serial::print(int val);
template size_t TK_Serial::print(unsigned int val);
template size_t TK_Serial::print(long val);
template size_t TK_Serial::print(unsigned long val);
template size_t TK_Serial::print(double val);
template size_t TK_Serial::print(const char* val);

template size_t TK_Serial::println(char val);
template size_t TK_Serial::println(unsigned char val);
template size_t TK_Serial::println(int val);
template size_t TK_Serial::println(unsigned int val);
template size_t TK_Serial::println(long val);
template size_t TK_Serial::println(unsigned long val);
template size_t TK_Serial::println(double val);
template size_t TK_Serial::println(const char* val);