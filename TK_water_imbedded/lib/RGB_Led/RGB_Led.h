#ifndef RGB_LED_H
#define RGB_LED_H

#pragma once
#include <Arduino.h>

enum RGB_LedColor
{
    red,
    green,
    blue,
    yellow,
    cyan,
    magenta,
    white,
    off
};

class RGB_Led
{
private:
    uint8_t pinR;
    uint8_t pinG;
    uint8_t pinB;

public:
    RGB_Led(uint8_t pinR, uint8_t pinG, uint8_t pinB);
    void turn(RGB_LedColor color);

private:

};

#endif