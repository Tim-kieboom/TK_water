#ifndef BIGTIMER_H
#define BIGTIMER_H

#include "Timer/Timer.h"

#pragma once

enum BigTimerType
{
    noBigTimerType,
    minutes,
    hours,
    days
};

class BigTimer : public ITimer
{
private:
    Timer timer;

    //only for hours and days only
    uint8_t minuteCounter = 0;
    uint64_t currentTime = 0;

    BigTimerType bigTimerType;
    uint64_t timeBegin;

public:
    BigTimer(BigTimerType bigTimerType);
    ~BigTimer();

    void startTimer();              //set timeBegin AND timeNow to the current time
    void resetBeginTime();          //set timeBegin to the current time
    
    bool waitTime(uint64_t time);   //wait for given time returns false until time is reached

private:
    void countTime();
    uint64_t getCurrentTime();

};

#endif