#ifndef FILTERING_H
#define FILTERING_H

#include "sea.h"

// filtering.h - seamath filtering operations
// this is seamath dll-library include files
// just used function shall be declared here
// types can be from predef directory
// library dir valid in c file only


int convf(const float* v1, int v1Size, const float* v2, int v2Size, float* output, int outSize);

int convd(const double* v1, int v1Size, const double* v2, int v2Size, double* output, int outSize);

int convcd(const seacmpd* v1, int v1Size, const seacmpd* v2, int v2Size, seacmpd* output, int outSize);

int convcf(const seacmpf* v1, int v1Size, const seacmpf* v2, int v2Size, seacmpf* output, int outSize);
#endif

