#include "static_malloc.h"

constexpr size_t myHeapSize = 100; // only 100 byte heap to test the memory overflow handler
uint8_t myHeap[myHeapSize];

// define a callback which is invoked if sm_alloc doesn't find enough free memory
size_t onMemoryOverflow(smalloc_pool* poolInfo, size_t requested)
{
    Serial.printf("Memory overrun, %u requested bytes can not be allocated", requested);

    pinMode(LED_BUILTIN, OUTPUT);
    while (true)
    {
        digitalToggle(LED_BUILTIN);
        delay(50);
    }
}

void setup()
{
    sm_set_default_pool(myHeap, myHeapSize, false, onMemoryOverflow); // attach memory pool to library

    while (!Serial) {}
    Serial.println("Start");

    sm_malloc(10);
    Serial.println("10 bytes allocated");

    sm_malloc(200); // <- will generate a memory overflow
    Serial.println("200 bytes allocated");
}

void loop()
{
}