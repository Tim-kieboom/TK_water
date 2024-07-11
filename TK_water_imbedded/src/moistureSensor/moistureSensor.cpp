#include "moistureSensor.h"

#define ANALOG_TO_PERCENT(x) (double)((double)x / ((double)100.0/1023))

MoistureSensor::MoistureSensor(uint8_t pin) 
{
  this->pin = pin;
  pinMode(pin, INPUT);
}

float MoistureSensor::getAverageReading()
{
  const static uint8_t measureAmount = 20;

  uint64_t soilMoistureValue = 0;
  for(uint8_t i = 0; i < measureAmount; i++)
    soilMoistureValue =+ analogRead(pin);

  return ANALOG_TO_PERCENT(soilMoistureValue)/(double)measureAmount;
}