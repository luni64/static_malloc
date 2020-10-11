#include "Arduino.h"
#include "oscilloscope.h"
#include "static_malloc.h"

constexpr size_t bufSize = 1024 * 200; // 200kB memory pool ...
EXTMEM uint8_t myHeap[bufSize];        // ... on external RAM

//----------------------

Oscilloscope scope;

constexpr uint8_t stopPin = 2;

void setup()
{
    sm_set_default_pool(myHeap, bufSize, false, nullptr); // initialize memory pool

    pinMode(stopPin, INPUT_PULLUP);                       // pin to stop measurement and print results
    scope.begin(0, 1'000);                                // scope on pin 0 with buffer for 1k transitions (class uses buffer from mem pool)

    while (!Serial){}
    Serial.printf("Measuring... press button on pin %u to stop and print results\n", stopPin);
}

void loop()
{
    if (digitalReadFast(stopPin) == LOW)
    {
        scope.stop();
        scope.print(&Serial);
        scope.reset();

        delay(500); // prevent bouncing of stop pin
        Serial.printf("Measuring... press button on pin %u to stop and print results\n");
    }
}
