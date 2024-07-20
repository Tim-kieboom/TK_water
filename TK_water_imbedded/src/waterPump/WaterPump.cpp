#include "WaterPump.h"

WaterPump::WaterPump(uint8_t pin)
{
    this->pin = pin;
    pinMode(pin, OUTPUT);
}


bool WaterPump::turnOnFor(uint16_t time, TimerType timeType)
{
    static Timer* timer;
    if(timer == nullptr)
        timer = new Timer(timeType);

    turnOn();

    if(timer->waitTime(time))
    {
        turnOff();
        
        timer = nullptr;
        delete timer;
        return true;
    }

    return false;
}

void WaterPump::turnOn()
{
    digitalWrite(pin, HIGH);
}

void WaterPump::turnOff()
{
    digitalWrite(pin, LOW);
}