#pragma once
#ifndef TIMER_H
#define TIMER_H

#include "ITimer.h"
#include <Arduino.h>
#define SET_TIMER_IN_MS true
#define SET_TIMER_IN_US false

enum TimerType
{
    noTimerType,
    seconds,
    millieSeconds,
    microSeconds
};

class Timer : public ITimer
{
private:
    uint64_t timeBegin;
    TimerType timerType;

public:
    Timer(TimerType timerType); //select what time unit is used, also calls startTimer

    void startTimer();          //set timeBegin AND timeNow to the current time
    void resetBeginTime();      //set timeBegin to the current time
    
    bool waitTime(uint64_t time); //wait for given time returns false until time is reached

private:
    uint64_t getCurrentTime();
    void millieSeconds_To_Seconds(uint64_t& now, uint64_t& begin); // convert millieSeconds to seconds(checks for overflow)

};

#endif