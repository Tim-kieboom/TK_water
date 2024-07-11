#include "Timer.h"
#include "checkOverflow.h"

Timer::Timer(TimerType timerType)
: timerType(timerType)
{
    startTimer();
}

void Timer::startTimer()
{
    timeBegin = getCurrentTime();
    timeNow   = getCurrentTime();
}

bool Timer::waitTime(uint64_t time)
{
    updateTimer();
    uint64_t now = timeNow;
    uint64_t begin = timeBegin;
    
    if(timerType == seconds)
        millieSeconds_To_Seconds(time, now, begin);

    if(timeNow - timeBegin > time)
    {
        resetBeginTime();
        return true;
    }

    return false;
}

void Timer::updateTimer()
{
    timeNow = getCurrentTime();
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

void Timer::millieSeconds_To_Seconds(uint64_t& time, uint64_t& now, uint64_t& begin)
{
    if (IS_MULL_OVERFLOW(time, 1000, uint64_t))
    {
        now /= 1000;
        begin /= 1000;
    }
    else
    {
        time *= 1000;
    }
}