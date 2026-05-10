
#ifndef SEA_H
#define SEA_H

#include "stdint.h"

#define SEAMAX(a,b) ((a)>=(b)?(a):(b))
#define SEAMIN(a,b) ((a)<=(b)?(a):(b))
#define SEATR(...) __attribute__(( __VA_ARGS__ ))

#define PI 3.14159265358979323846

typedef void FILE;

typedef _Complex             seacmp;
typedef long double          sealdbl;
typedef double _Complex      seacmpd;
typedef float _Complex       seacmpf;
typedef long double _Complex seacmpld;

typedef char  _Complex         seaci8;
typedef short _Complex         seaci16;
typedef int _Complex           seaci32;
typedef long long int _Complex seaci64;

typedef unsigned char  _Complex seacu8;
typedef unsigned short _Complex seacu16;
typedef unsigned int   _Complex seacu32;
typedef unsigned long long int _Complex seacu64;

#ifndef SEAWIDE
#define __SEAWIDE(_String) L ## _String
#define SEAWIDE(_String) __SEAWIDE(_String)
#endif 

/// <summary>
/// Complex double struct
/// </summary>
typedef struct
{
   double re;
   double im;
}seacmpsd;

typedef struct
{
   long double re;
   long double im;
}seacmpsld;

typedef struct
{
   float re;
   float im;
}seacmpsf;

typedef struct
{
   int64_t re;
   int64_t im;
}seacsi64;

typedef struct
{
   int32_t re;
   int32_t im;
}seacsi32;

typedef struct
{
   int16_t re;
   int16_t im;
}seacsi16;

typedef struct
{
   int8_t re;
   int8_t im;
}seacsi8;

typedef struct
{
   uint64_t re;
   uint64_t im;
}seacsu64;

typedef struct
{
   uint32_t re;
   uint32_t im;
}seacsu32;

typedef struct
{
   uint16_t re;
   uint16_t im;
}seacsu16;

typedef struct
{
   uint8_t re;
   uint8_t im;
}seacsu8;

#endif

