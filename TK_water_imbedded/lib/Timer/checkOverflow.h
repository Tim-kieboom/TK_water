#ifndef _CHECKOVERFLOW_H_
#define _CHECKOVERFLOW_H_

//checks if (a > maxValue invertsOfOperator b) || (a < minValue invertsOfOperator b)
#define IS_ADD_OVERFLOW(a, b, type) IS_OVERFLOW(a, b, type, -, a > 0)

//checks if (a > maxValue invertsOfOperator b) || (a < minValue invertsOfOperator b)
#define IS_SUB_OVERFLOW(a, b, type) IS_INV_OVERFLOW(a, b, type, +, a < 0)

//checks if (a > maxValue invertsOfOperator b) || (a < minValue invertsOfOperator b)
#define IS_MULL_OVERFLOW(a, b, type) IS_OVERFLOW(a, b, type, /, a > 0)

//checks if (a > maxValue inverseOfOperator b) || (a < minValue inverseOfOperator b)
#define IS_DIV_OVERFLOW(a, b, type) (b == 0 || (a == std::numeric_limits<T>::min() && b == -1))

#define IS_OVERFLOW(a, b, type, inverseOperator, condition)                  \
({                                                                           \
    bool overflow = false;                                                   \
    if(condition)                                                            \
        overflow = (a > std::numeric_limits<type>::max() inverseOperator b); \
    else                                                                     \
        overflow = (a < std::numeric_limits<type>::min() inverseOperator b); \
    overflow;                                                                \
})
#endif