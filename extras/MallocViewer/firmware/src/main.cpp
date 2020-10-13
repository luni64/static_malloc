#include "Arduino.h"
#include "static_malloc.h"
#include "smalloc/smalloc_i.h"

unsigned onError(smalloc_pool*, unsigned);
void printInfo(unsigned idx, char type);

constexpr unsigned heapSize = 800 * 70; // must match  GUI
constexpr unsigned nrOfChunks = 500;
constexpr unsigned maxChunkLen = 250;

void* chunks[nrOfChunks];
uint8_t myHeap[heapSize];


void setup()
{
    pinMode(LED_BUILTIN, OUTPUT);
    memset(chunks, 0, sizeof(chunks));
    sm_set_default_pool(myHeap, sizeof(myHeap), false, onError);
}

void loop()
{
    if (Serial.available())
    {
        switch (Serial.read())
        {
            case '*': // allocate/deallocate
            {
                size_t chunkLen = random(1, maxChunkLen);
                unsigned idx = random(0, nrOfChunks);

                if (chunks[idx] == nullptr) // if slot empty allocate memory and store pointer
                {
                    chunks[idx] = sm_malloc(chunkLen);
                    printInfo(idx, '+');

                } else
                {
                    printInfo(idx, '-');
                    sm_free(chunks[idx]);
                    chunks[idx] = nullptr;
                }
                break;
            }

            case 'c': // clear all memory
                for (unsigned i = 0; i < nrOfChunks; i++)
                {
                    if (chunks[i] != nullptr)
                    {
                        sm_free(chunks[i]);
                        chunks[i] = nullptr;
                    }
                }
                break;

            default:
                break;
        }
    }
    static elapsedMillis stopwatch = 0;
    if (stopwatch > 500)
    {
        stopwatch = 0;
        digitalToggle(LED_BUILTIN);
    }
}

// Helpers ------------------------------------------

void printInfo(unsigned idx, char type)
{
    size_t total, totalUser, totalFree;
    int nrBlocks;

    sm_malloc_stats(&total, &totalUser, &totalFree, &nrBlocks);

    smalloc_hdr* hdr = USER_TO_HEADER(chunks[idx]);

    unsigned blockStart = (char*)hdr - (char*)myHeap; // address relative to buffer start
    unsigned usrStart = blockStart + 12;              // block starts with 12byte header
    unsigned TagStart = usrStart + hdr->rsz;          // tag starts rsz bytes after user start
    unsigned blockEnd = TagStart + 12;                // tag is always 12 byte long

    Serial.printf("%c %u %u %u %u %u %u %u\n", type, idx, blockStart, blockEnd, total, totalUser, totalFree, nrBlocks);
}

unsigned onError(smalloc_pool*, unsigned)
{
    Serial.println("e 0 0 0\n");
    pinMode(LED_BUILTIN, OUTPUT);
    while (true)
    {
        digitalToggle(LED_BUILTIN);
        delay(50);
    }
}
