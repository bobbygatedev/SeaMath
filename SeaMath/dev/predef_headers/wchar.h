#ifndef WCHAR_H
#define WCHAR_H

#include "sea.h"
#include "stdint.h"

typedef __WCHAR_TYPE__ wchar_t;
typedef int32_t        wint_t;

#define WEOF ((wint_t)-1)

/* --- R/W wide single char --- */
wint_t fgetwc(FILE *f);
wint_t fputwc(wchar_t c, FILE *f);
int fwprintf(FILE *stream, const wchar_t *format, ...);

/* --- Lettura / scrittura stringhe wide --- */
wchar_t* fgetws(wchar_t *s, int n, FILE *f);
int      fputws(const wchar_t *s, FILE *f);

#endif // WCHAR_H
