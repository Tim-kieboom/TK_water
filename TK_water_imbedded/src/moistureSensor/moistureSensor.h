#ifndef MOISTURESENSOR_H
#define MOISTURESENSOR_H
#pragma once
#include <Arduino.h>

class MoistureSensor
{
private:
    uint8_t pin;

public:
    MoistureSensor(uint8_t pin);
    float getAverageReading();
};



#endif