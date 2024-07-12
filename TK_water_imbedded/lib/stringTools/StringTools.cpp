#include "StringTools.h"
#include <cstdarg>
#include <iostream>
#include <sstream>
#include <string.h>

using namespace std;

template <typename... Params>
const char* add_c_str(Params... params) 
{
    size_t paramsSize = sizeof...(params);
    const char* strs[paramsSize] = {params...};

    stringstream ss;
    for(size_t i = 0; i < paramsSize; i++)
    {
        ss << strs[i];
    }

    return ss.str().c_str();
}

size_t _hash(const char* str, size_t len, size_t index = 0, size_t hashNumber = 5381)
{
    return (index < len) ? _hash(str, len, index + 1, ((hashNumber << 5) + hashNumber) + str[index]) : hashNumber;
}

size_t hash(const char* str)
{
    return _hash(str, strlen(str));
}