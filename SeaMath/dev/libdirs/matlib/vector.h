// vector.h - vector operations
// this is seamath dll-library include files
// just used function shall be declared here
// types can be from predef directory
// library dir valid in c file only

#ifndef _VECTOR_H_
#define _VECTOR_H_

#include "sea.h"

double vecscalard(const double* v1, int v1Size, const double* v2, int v2Size);

float vecscalarf(const float* v1, int v1Size, const float* v2, int v2Size);

seacmpd vecscalarcd(const seacmpd* v1, int v1Size, const seacmpd* v2, int v2Size);

seacmpf vecscalarcf(const seacmpf* v1, int v1Size, const seacmpf* v2, int v2Size);

#endif