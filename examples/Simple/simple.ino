#include "static_malloc.h"

// reserve some space for sm_malloc. sm_malloc will never overwrite this buffer
// You can generate the buffer wherever you want. I.e. in normal DTCM Memory, on the external RAM chip (EXTMEM) etc.

constexpr size_t myHeapSize = 1024 * 100;
uint8_t myHeap[myHeapSize];

void setup()
{
    sm_set_default_pool(myHeap, myHeapSize, 0, nullptr);

    while (!Serial)
        ;

    float* f1 = (float*)sm_malloc(sizeof(float)); // allocate a float
    char* t1 = (char*)sm_malloc(100);             // allocate a 100 byte cstring
    float* f2 = (float*)sm_malloc(sizeof(float)); // another float

    *f1 = M_PI;
    *f2 = M_SQRT2;
    snprintf(t1, 100, "This is a text");

    Serial.printf("f1 address: %p, value: %f\n", f1, *f1);
    Serial.printf("f2 address: %p, value: %f\n", f2, *f2);
    Serial.printf("t2 address: %p, value: %s\n", t1, t1);
    Serial.println("----------------------------\n");

    sm_free(f1);

    float* f3 = (float*)sm_malloc(sizeof(float)); // allocate a float
    *f3 = 42;

    Serial.printf("f3 address: %p, value: %f\n", f3, *f3); // should get the same address as f1 had
    Serial.printf("f2 address: %p, value: %f\n", f2, *f2);
    Serial.printf("t2 address: %p, value: %s\n", t1, t1);
}

void loop()
{
}
