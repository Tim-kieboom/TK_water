#ifndef STRINGTOOLS_H
#define STRINGTOOLS_H
#pragma once
#include <Arduino.h>

template <typename... Params>
const char* add_c_str(Params... params);
size_t hash(const char* str);

#endif