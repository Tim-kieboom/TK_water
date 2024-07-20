#ifndef _CHECKOVERFLOW_H_
#define _CHECKOVERFLOW_H_
#pragma once

//checks if (a > maxValue invertsOfOperator b) || (a < minValue invertsOfOperator b)
#define IS_ADD_OVERFLOW(a, b, type)                                 \
    ((b > 0) && (a > std::numeric_limits<type>::max() - b)) ||      \
    ((b < 0) && (a < std::numeric_limits<type>::min() - b))

//checks if (a < maxValue invertsOfOperator b) || (a > minValue invertsOfOperator b)
#define IS_SUB_OVERFLOW(a, b, type)                                 \
    ((b > 0) && (a < std::numeric_limits<type>::max() + b)) ||      \
    ((b < 0) && (a > std::numeric_limits<type>::min() + b))

//checks if (a > maxValue invertsOfOperator b) || (a < minValue invertsOfOperator b)
#define IS_MULL_OVERFLOW(a, b, type)                                \
    ((b != 0) &&                                                    \
        ((a > std::numeric_limits<type>::max() / b) ||              \
         (a < std::numeric_limits<type>::min() + b)))

//checks if (a > maxValue inverseOfOperator b) || (a < minValue inverseOfOperator b)
#define IS_DIV_OVERFLOW(a, b, type)                                 \
    ((b == 0) || (a == std::numeric_limits<T>::min() && b == -1))

#endif