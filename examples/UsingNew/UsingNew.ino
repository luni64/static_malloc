#include <new>                                     // need to include <new> for placement new
#include "static_malloc.h"

constexpr size_t bufSize = 1024 * 100;
EXTMEM uint8_t myHeap[bufSize];                 // 100kB memory pool on the external ram chip

//----------------------

void isr(){
    digitalToggle(LED_BUILTIN);
}

IntervalTimer* timer;

void setup()
{
    sm_set_default_pool(myHeap, bufSize, false, nullptr);

    pinMode(LED_BUILTIN, OUTPUT);

    void* mem = sm_malloc(sizeof(IntervalTimer));  // allocate memory for an IntervalTimer on the external RAM chip
    timer = new(mem) IntervalTimer();              // use "placement new" to generate the object at the given address https://stackoverflow.com/questions/222557/what-uses-are-there-for-placement-new

    timer->begin(isr, 200'000);
}

void loop(){
}


