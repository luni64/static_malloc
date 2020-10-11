#include "static_malloc.h"

smalloc_pool DTCM_Pool;
smalloc_pool EXTM_Pool;

void setup()
{
    constexpr size_t bufSize = 1024 * 200;                          // 100kB
    static EXTMEM uint8_t EXTM_Buf[bufSize];                        // external RAM buffer
    static uint8_t DTCM_Buf[bufSize];                               // internal fast RAM (DTCM) buffer

    sm_set_pool(&EXTM_Pool, EXTM_Buf, bufSize, true, nullptr);      // register EXTMEM pool
    sm_set_pool(&DTCM_Pool, DTCM_Buf, bufSize, true, nullptr);      // register DTCM pool
                                                                    //
    //-------------------------------------------------------------------------------------------
    while(!Serial){}

    float* f  = (float*)sm_malloc_pool(&DTCM_Pool, sizeof(float));  // dynamically allocate float in DTCM pool
    char* str  = (char*)sm_malloc_pool(&EXTM_Pool, 2 * 1024);       // dynamically allocate text buffer in EXTMEM pool

    *f = M_PI;                                                      // use heap variables..
    strcpy(str, "Hello Buffer");

    Serial.printf("DTCM:     f:  %f\n", *f);
    Serial.printf("EXTMEM: str:  %s\n", str);

    sm_free_pool(&DTCM_Pool, f);                                    // release used memory
    sm_free_pool(&EXTM_Pool, str);
}

void loop(){
}
