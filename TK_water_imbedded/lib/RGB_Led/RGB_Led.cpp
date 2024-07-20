#include "RGB_Led.h"

    
RGB_Led::RGB_Led(uint8_t pinR, uint8_t pinG, uint8_t pinB)
    : pinR(pinR), pinG(pinG), pinB(pinB)
{
    pinMode(pinR, OUTPUT);
    pinMode(pinG, OUTPUT);
    pinMode(pinB, OUTPUT);
}

void RGB_Led::turn(RGB_LedColor color)
{
    switch (color)
    {
    case red:
        digitalWrite(pinR, HIGH);
        digitalWrite(pinG, LOW);
        digitalWrite(pinB, LOW);
        break;

    case green:
        digitalWrite(pinR, LOW);
        digitalWrite(pinG, HIGH);
        digitalWrite(pinB, LOW);
        break;

    case blue:
        digitalWrite(pinR, LOW);
        digitalWrite(pinG, LOW);
        digitalWrite(pinB, HIGH);
        break;

    case yellow:
        digitalWrite(pinR, HIGH);
        digitalWrite(pinG, HIGH);
        digitalWrite(pinB, LOW);
        break;

    case cyan:
        digitalWrite(pinR, LOW);
        digitalWrite(pinG, HIGH);
        digitalWrite(pinB, HIGH);
        break;

    case magenta:
        digitalWrite(pinR, HIGH);
        digitalWrite(pinG, LOW);
        digitalWrite(pinB, HIGH);
        break;

    case white:
        digitalWrite(pinR, HIGH);
        digitalWrite(pinG, HIGH);
        digitalWrite(pinB, HIGH);
        break;

    case off:
    default:
        digitalWrite(pinR, LOW);
        digitalWrite(pinG, LOW);
        digitalWrite(pinB, LOW);
        break;
    }
}
