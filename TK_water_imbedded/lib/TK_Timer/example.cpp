#include "TK_Timer.h" 

/*
Classes:
    - Stopwatch:    A way to measure time of a stretch of code.
    - Timer:        A timer that can wait in micro, millie or seconds.
    - BigTimer:     A timer that can wait in minutes, hours, or days (takes up more memory then Timer).
*/

void setup()
{
    Serial.begin(115200);

//--------StopWatch example--------

    StopWatch stopWatch = StopWatch(microSeconds);

    stopWatch.startTimer();
    delay(69);
    uint64_t time = stopWatch.getTime();

    Serial.println("stopwatch time: " + String(time));

//----------------
}

void loop()
{
//--------Timer Example--------

    //the start of the timer is at construction (you can also manually set start with startTimer())
    static ITimer* timerSwitch = new Timer(microSeconds);
    static Timer timerSeconds = Timer(seconds);
    static BigTimer timerHours = BigTimer(hours);

    //if time is up, returns true and automatically resets the timer
    if(timerSwitch->waitTime(1))
    {  
        //if you use ITimer* you can switch Timer<->BigTimer like so
        delete timerSwitch;
        timerSwitch = new BigTimer(minutes);

        delete timerSwitch;
        timerSwitch = new Timer(millieSeconds);
    }

    if(timerSeconds.waitTime(1))
    {   
        Serial.println("you just waited 1 second");
    }

    if(timerHours.waitTime(1))
    {
        Serial.println("you just waited 1 hour");
    }

//----------------
}