#ifndef ITIMER_H
#define ITIMER_H
#pragma once

#include <Arduino.h>
#define SET_TIMER_IN_MS true
#define SET_TIMER_IN_US false

class ITimer
{
public:
    virtual void startTimer() = 0;          //set timeBegin AND timeNow to the current time
    virtual void resetBeginTime() = 0;      //set timeBegin to the current time
    
    virtual bool waitTime(uint64_t time) = 0; //wait for given time returns false until time is reached

private:
    virtual uint64_t getCurrentTime() = 0;

};

#endif