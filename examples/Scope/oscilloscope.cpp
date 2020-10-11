#include "oscilloscope.h"
#include "Arduino.h"
#include "attachInterruptEx.h"
#include "static_malloc.h"

void Oscilloscope::begin(uint8_t pin, unsigned bufSize)
{
    this->pin = pin;
    this->bufSize = bufSize;
    pinMode(pin, INPUT_PULLUP);

    buffer = (Entry*)sm_zalloc(bufSize * sizeof(Entry)); // allocate zeroed memory
    reset();
}

void Oscilloscope::ISR()
{
    int state = digitalReadFast(0);
    double time = ARM_DWT_CYCCNT / 600.0;
    if (current < bufSize)
    {
        buffer[current].state = state;
        buffer[current].timestamp = time;
        current++;
    } else
        stop();

}

void Oscilloscope::reset()
{
    current = 0;
    attachInterruptEx(pin, [this] { ISR(); }, CHANGE); // attach the ISR to the pin change interrupt
}

void Oscilloscope::print(Stream* output)
{
    stop();
    for (unsigned i = 0; i < current; i++)
    {
        output->printf("%.2f %u\n", buffer[i].timestamp, buffer[i].state);
    }
}

void Oscilloscope::stop()
{
    detachInterrupt(pin);
}
