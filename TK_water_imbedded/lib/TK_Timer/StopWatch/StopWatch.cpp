#include "StopWatch.h"

StopWatch::StopWatch(TimerType timerType)
    : timerType(timerType)
{
    startTimer();
}

void StopWatch::startTimer()
{
    beginTime = getCurrentTime();
}

uint64_t StopWatch::getTime()
{
    return getCurrentTime() - beginTime;
}

uint64_t StopWatch::getCurrentTime()
{
    switch (timerType)
    {
    case seconds:
        return millis() / 1000; // time in seconds is handled in waitTime

    case millieSeconds:
        return millis();

    case microSeconds:
        return micros();
    
    default:
        break;
    }

    return 0;
}