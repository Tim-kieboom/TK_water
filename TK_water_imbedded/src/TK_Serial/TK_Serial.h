#ifndef TK_SERIAL_H
#define TK_SERIAL_H

#pragma once
#include <Arduino.h>
#include <Mqtt.h>

class TK_Serial
{
private:
    static Mqtt* mqtt;
    static const char* logTopic;

public:
    static void setMqtt(Mqtt* mqtt, size_t id);

    template <typename T>
    static size_t print(T val);

    static size_t print(const String& val);
    static size_t println(const String& val);

    static size_t print(StringSumHelper val);
    static size_t println(StringSumHelper val);

    template <typename T>
    static size_t println(T val);

    static size_t printf(const char *format, ...);
};
#endif
