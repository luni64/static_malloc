//------------------------------------------------
// from https://github.com/luni64/TeensyHelpers
//------------------------------------------------

#pragma once

#include <functional>

extern void attachInterruptEx(unsigned pin, std::function<void(void)> callback, int mode);