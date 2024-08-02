#include "moistureSensor.h"

MoistureSensor::MoistureSensor(uint8_t pin, uint8_t ADC_BitSize/*default = 12*/)
  : pin(pin), maxValue(((1 << ADC_BitSize) - 1))
{
  pinMode(pin, INPUT);
}

uint8_t MoistureSensor::getAverageReading(uint8_t measureAmount/*default = 20*/)
{
  uint16_t soilMoistureValue = analogRead(pin);

  for(uint8_t i = 0; i < measureAmount-1; i++)
    soilMoistureValue = (soilMoistureValue + analogRead(pin)) / 2;
  
  return map(soilMoistureValue, 0, maxValue, 100, 0);

}