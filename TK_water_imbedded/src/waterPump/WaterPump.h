#ifndef WATERPUMP_H
#define WATERPUMP_H
#pragma once

#include <Arduino.h>
#include <TK_Timer.h>

class WaterPump
{
private:
    uint8_t pin;
    ITimer* oscillateTimer = nullptr;

public:
    WaterPump(uint8_t pin);
    ~WaterPump();

    //Set turnOffBeforeTime to true if you need to interrupt and jump to something else (if you dont it will remember the timer and mess up the time)
    bool turnOnFor(uint16_t time, TimerType timeType, bool turnOffBeforeTime = false);
    void turnOn();

    /*
    Set turnOffBeforeTime to true if you need to interrupt and jump to something else (if you dont it will remember the timer and mess up the time).
    It will oscillate every 'dripFrequency' ms. 0 is no oscillation (only works in if it keep getting called in a loop).
    */
    bool oscillateFor(uint16_t time, TimerType timeType, uint16_t onTime, uint16_t offTime, bool turnOffBeforeTime = false);
    //It will oscillate every 'dripFrequency' ms. 0 is no oscillation (only works in if it keep getting called in a loop)
    void oscillate(uint16_t onTime, uint16_t offTime);

    void turnOff();
};

#endif