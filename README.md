# static_malloc

This library is just an Arduino wrapper around [Andrey Rys](https://github.com/electrorys) static memory allocator library **smalloc** (https://github.com/electrorys/smalloc). The full credit for the very useful code goes to Andrey of course. Here the [man-page of smalloc](manpage.txt)

For the convenience of Arduino users I sorted the original files in the folders required by the [Arduino library specification](https://arduino.github.io/arduino-cli/library-specification/) and added a few usage examples. I also added a header handling `extern "C"` wrapper required for the usual Arduino ino/cpp projects.

The main purpose of the library is to allow dynamic memory allocation on a predefined static buffer. Since this buffer can be placed wherever you want, it can also be placed on external memory chips likke the 8MB PSRam chip on the Teensy 4.1.

Here a quick example showing the usage:

```c++
#include "static_malloc.h"

constexpr size_t myHeapSize = 1024 * 100;
EXTMEM uint8_t myHeap[myHeapSize]; // 100kB memory pool on the external ram chip

//----------------------

float* floatArray;
uint32_t* u1;
char* text;

void setup()
{
    sm_set_default_pool(myHeap, myHeapSize, false, nullptr); // use a memory pool on the external ram

    u1 = (uint32_t*)sm_malloc(sizeof(uint32_t));        // 1 uint32_t
    floatArray = (float*)sm_malloc(10 * sizeof(float)); // Array of 10 floats
    text = (char*)sm_malloc(100);                       // c-string 100 bytes

    *u1 = 100;
    for (int i = 0; i < 10; i++) { floatArray[i] = i * M_PI; }
    text = strcpy(text, "Hello World");

    while (!Serial) {}
    Serial.println(text);
    Serial.println(*u1);
    Serial.println(floatArray[4]);
}

void loop()
{
}
```

Off course smalloc also provides `sm_free` and `sm_calloc`. It can also be used to handle more than one memory pool which might come in handy from time to time.

smalloc also provides a "out of memory" callback.

See the examples folder in the sources for more examples.

