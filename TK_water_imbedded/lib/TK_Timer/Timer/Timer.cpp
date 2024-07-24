#include "Timer.h"

Timer::Timer(TimerType timerType)
: timerType(timerType)
{
    startTimer();
}

void Timer::startTimer()
{
    timeBegin = getCurrentTime();
}

bool Timer::waitTime(uint64_t time)
{
    uint64_t now = getCurrentTime();
    uint64_t begin = timeBegin;
    
    if(timerType == seconds)
        millieSeconds_To_Seconds(now, begin);

    if(now - begin > time)
    {
        resetBeginTime();
        return true;
    }

    return false;
}


void Timer::resetBeginTime()
{
    timeBegin = getCurrentTime();
}

uint64_t Timer::getCurrentTime()
{
    switch (timerType)
    {
    case seconds:
        return millis(); // time in seconds is handled in waitTime

    case millieSeconds:
        return millis();

    case microSeconds:
        return micros();
    
    default:
        break;
    }

    return 0;
}

void Timer::millieSeconds_To_Seconds(uint64_t& now, uint64_t& begin)
{
    now /= 1000;
    begin /= 1000;
}