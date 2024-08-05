#include "WaterPump.h"

WaterPump::WaterPump(uint8_t pin)
    : pin(pin)
{
    pinMode(pin, OUTPUT);
}

WaterPump::~WaterPump()
{
    if(oscillateTimer != nullptr)
        delete oscillateTimer;
}

bool WaterPump::turnOnFor(uint16_t time, TimerType timeType, bool turnOffBeforeTime/*default = false*/)
{
    static ITimer* timer;
    if(timer == nullptr)
        timer = new Timer(timeType);

    turnOn();

    if(timer->waitTime(time) || turnOffBeforeTime)
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

bool WaterPump::oscillateFor(uint16_t time, TimerType timeType, uint16_t onTime, uint16_t offTime, bool turnOffBeforeTime/*default = false*/)
{
    static ITimer* timer;
    if(timer == nullptr)
        timer = new Timer(timeType);

    oscillate(onTime, offTime);

    if(timer->waitTime(time) || turnOffBeforeTime)
    {
        turnOff();
        
        timer = nullptr;
        delete timer;
        return true;
    }

    return false;
}

void WaterPump::oscillate(uint16_t onTime, uint16_t offTime)
{
    static bool isOn = false;

    if(oscillateTimer == nullptr)
        oscillateTimer = new Timer(millieSeconds);

    uint16_t waitTime = (isOn) ? onTime : offTime;

    if(oscillateTimer->waitTime(waitTime))
        isOn = !isOn;


    if(isOn)
        digitalWrite(pin, HIGH);
    else
        digitalWrite(pin, LOW);
}

void WaterPump::turnOff()
{
    digitalWrite(pin, LOW);

    if(oscillateTimer != nullptr)
    {
        delete oscillateTimer;
        oscillateTimer = nullptr;
    }
}