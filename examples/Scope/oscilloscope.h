#pragma once

#include <cstdint>
#include <cstddef>

class Stream;

class Oscilloscope
{
 public:
    void begin(uint8_t pin, unsigned bufSize);

    void stop();
    void reset();
    void print(Stream* output);

 protected:
    void ISR();

    uint8_t pin;
    size_t current;
    size_t bufSize;

    struct Entry
    {
        bool state;
        double timestamp;
    };
    Entry* buffer;
};
