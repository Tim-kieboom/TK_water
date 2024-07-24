#include "BigTimer.h"
#define MILLI_TO_MINUTES 60000

BigTimer::BigTimer(BigTimerType bigTimerType)
    : timer(Timer(millieSeconds)), bigTimerType(bigTimerType)
{
}

BigTimer::~BigTimer()
{
}

void BigTimer::startTimer()
{
    timeBegin = getCurrentTime();
}

void BigTimer::resetBeginTime()
{
    currentTime = 0;
    timeBegin = getCurrentTime();
}

bool BigTimer::waitTime(uint64_t time)
{
    countTime();

    if(getCurrentTime() - timeBegin > time)
    {
        resetBeginTime();
        return true;
    }

    return false;
}

void BigTimer::countTime()
{
    if(!timer.waitTime(MILLI_TO_MINUTES))
        return;

    if(bigTimerType == minutes)
    {
        currentTime++;
        return;
    }

    minuteCounter++;
    if(minuteCounter >= 60)
    {
        currentTime++;
        minuteCounter = 0;
    }
}

uint64_t BigTimer::getCurrentTime()
{
    if(bigTimerType == days)
        return (currentTime < 24) ? 0 : currentTime / 24;

    return currentTime;
}
