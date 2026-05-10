#ifndef _COMPLEX_H_
#define _COMPLEX_H_ 

// complex.h - complex number operations
// this is seamath dll-library include files
// just used function shall be declared here
// types can be from predef directory
// library dir valid in c file only

#include "sea.h"

double crealcd(seacmpd z);

double cimagcd(seacmpd z);

float crealcf(seacmpf z);

float cimagcf(seacmpf z);

int8_t crealci8(seaci8 z);

int8_t cimagci8(seaci8 z);

int16_t crealci16(seaci16 z);

int16_t cimagci16(seaci16 z);

int32_t crealci32(seaci32 z);

int32_t cimagci32(seaci32 z);

int64_t crealci64(seaci64 z);

int64_t cimagci64(seaci64 z);

uint8_t crealcu8(seacu8 z);

uint8_t cimagcu8(seacu8 z);

uint16_t crealcu16(seacu16 z);

uint16_t cimagcu16(seacu16 z);

uint32_t crealcu32(seacu32 z);

uint32_t cimagcu32(seacu32 z);

uint64_t crealcu64(seacu64 z);

uint64_t cimagcu64(seacu64 z);

sealdbl crealcl(seacmpld z);

sealdbl cimagcl(seacmpld z);

seacmpd conjcd(seacmpd z);

seacmpf conjcf(seacmpf z);

seacmpd conjcd(seacmpd z);

seacmpf conjcf(seacmpf z);

double cargcd(seacmpd z);

float cargcf(seacmpf z);

seacmpd cprojcd(seacmpd z);

seacmpf cprojcf(seacmpf z);

#endif