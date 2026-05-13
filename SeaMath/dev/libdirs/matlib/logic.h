#ifndef _LOGIC_H_
#define _LOGIC_H_

// logic.h - seamath logic.h module   
// this is seamath dll-library include files
// just used function shall be declared here
// types can be from predef directory
// library dir valid in c file only

#include "sea.h"

uint64_t swapbyten (uint64_t val, int nbit);

uint8_t swapbyte8(uint8_t val);
uint16_t swapbyte16(uint16_t val);
uint32_t swapbyte32(uint32_t val);
uint64_t swapbyte64(uint64_t val);

uint64_t swapbitn (uint64_t val, int nbit);

uint8_t swapbit8(uint8_t val);
uint16_t swapbit16(uint16_t val);
uint32_t swapbit32(uint32_t val);
uint64_t swapbit64(uint64_t val);

#endif
