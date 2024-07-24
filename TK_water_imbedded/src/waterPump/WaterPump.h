#ifndef WATERPUMP_H
#define WATERPUMP_H
#pragma once

#include <Arduino.h>
#include <TK_Timer.h>

class WaterPump
{
private:
    uint8_t pin;

public:
    WaterPump(uint8_t pin);
    bool turnOnFor(uint16_t time, TimerType timeType);
    void turnOn();
    void turnOff();
};

#endif