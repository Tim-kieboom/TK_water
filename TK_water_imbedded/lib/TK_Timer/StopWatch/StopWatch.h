#ifndef STOPWATCH_H
#define STOPWATCH_H
#pragma once

#include "../Timer/Timer.h"

class StopWatch
{
private:
    TimerType timerType;
    uint64_t beginTime = 0;

public:
    StopWatch(TimerType timerType);
    void startTimer();
    uint64_t getTime();

private:
    uint64_t getCurrentTime();

};

#endif