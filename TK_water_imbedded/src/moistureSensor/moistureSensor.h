#ifndef MOISTURESENSOR_H
#define MOISTURESENSOR_H
#pragma once
#include <Arduino.h>

class MoistureSensor
{
private:
    const uint8_t pin;
    const uint16_t maxValue;

public:
    MoistureSensor(uint8_t pin, uint8_t ADC_bitSize = 12);
    double getAveragePercentage(uint8_t measureAmount = 20);
    bool getAveragePercentage_forked(double &percentage, uint8_t measureAmount/*default = 20*/);

};



#endif