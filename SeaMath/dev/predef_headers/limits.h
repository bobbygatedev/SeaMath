#ifndef _LIMITS_H
#define _LIMITS_H

/* Numero di bit in un char */
#define CHAR_BIT 8

/* Valori min/max per signed char */
#define SCHAR_MIN  (-128)
#define SCHAR_MAX  127

/* Valori min/max per unsigned char */
#define UCHAR_MAX  255

#define CHAR_MIN  SCHAR_MIN
#define CHAR_MAX  SCHAR_MAX

/* short */
#define SHRT_MIN   (-32768)
#define SHRT_MAX   32767
#define USHRT_MAX  65535

/* int */
#define INT_MIN    (-2147483647 - 1)
#define INT_MAX    2147483647
#define UINT_MAX   4294967295U

/* LP64: long = 64 bit */
#define LONG_MIN   (-9223372036854775807L - 1)
#define LONG_MAX    9223372036854775807L
#define ULONG_MAX   18446744073709551615UL

/* long long (sempre almeno 64 bit) */
#define LLONG_MIN  (-9223372036854775807LL - 1)
#define LLONG_MAX   9223372036854775807LL
#define ULLONG_MAX  18446744073709551615ULL

#endif /* _LIMITS_H */
