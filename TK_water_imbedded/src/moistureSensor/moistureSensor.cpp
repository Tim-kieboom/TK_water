#include "moistureSensor.h"

#define INVERT(value, max) max - value
#define TO_PERCENTAGE(value, max) (((double)value / max) * 100.0f)

MoistureSensor::MoistureSensor(uint8_t pin, uint8_t ADC_BitSize/*default = 12*/)
  : pin(pin), maxValue(((1 << ADC_BitSize) - 1))
{
  pinMode(pin, INPUT);
}

double MoistureSensor::getAveragePercentage(uint8_t measureAmount/*default = 20*/)
{
  long allValues = 0;

  for(uint8_t i = 0; i < measureAmount; i++)
    allValues += analogRead(pin);
  
  //get the average
  double soilMoistureValue = (double)allValues / measureAmount;

  //make from max(dry)-min(wet) to max(wet)-min(dry)
  soilMoistureValue = INVERT(soilMoistureValue, maxValue);

  return TO_PERCENTAGE(soilMoistureValue, maxValue);
}

bool MoistureSensor::getAveragePercentage_forked(double &percentage, uint8_t measureAmount/*default = 20*/)
{
  static long allValues = 0;
  static uint8_t counter = 0;

  if(counter >= measureAmount)
  {
    counter++;
    allValues += analogRead(pin);
    return false;
  }

  double soilMoistureValue = (double)allValues / measureAmount;
  soilMoistureValue = INVERT(soilMoistureValue, maxValue);
  percentage = TO_PERCENTAGE(soilMoistureValue, maxValue);

  counter = 0;
  soilMoistureValue = 0;
  return true;
}