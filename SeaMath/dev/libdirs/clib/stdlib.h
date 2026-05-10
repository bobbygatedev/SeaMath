#ifndef _STDLIB_H
#define _STDLIB_H

#include "sea.h"
#include <stddef.h>

int SEATR(altname(atoi_exp), sea_no_vect) atoi(const char* s);
long SEATR(altname(atol_exp), sea_no_vect) atol(const char* s);
double SEATR(altname(atof_exp), sea_no_vect) atof(const char* s);
long SEATR(altname(strtol_exp), sea_no_vect) strtol(const char* nptr, char** endptr, int base);
unsigned long SEATR(altname(strtoul_exp), sea_no_vect) strtoul(const char* nptr, char** endptr, int base);
double SEATR(altname(strtod_exp), sea_no_vect) strtod(const char* nptr, char** endptr);

void* SEATR(noimpl) malloc(size_t size);
void* SEATR(noimpl) calloc(size_t n, size_t size);
void* SEATR(noimpl) realloc(void* ptr, size_t size);
void SEATR(noimpl) free(void *ptr);

int SEATR(altname(rand_exp), sea_no_vect) rand(void);
void SEATR(altname(srand_exp), sea_no_vect) srand(unsigned int seed);

void SEATR(altname(qsort_exp)) qsort(
   void* base,        // puntatore all’array
   size_t nitems,     // numero di elementi
   size_t size,       // dimensione di ogni elemento
   int (*compar)(const void* p1, const void* p2) // funzione di confronto
);

void* SEATR(altname(bsearch_exp)) bsearch(
   const void* key,          // elemento da cercare
   const void* base,         // array ordinato
   size_t nitems,            // numero di elementi
   size_t size,              // dimensione di ogni elemento
   int (*compar)(const void* p1, const void* p2) // funzione di confronto
);

int SEATR(altname(system_exp), sea_no_vect) system(const char* sys);

void SEATR(noimpl) exit(int exit);
void SEATR(noimpl) abort(void);

int SEATR(noimpl) atexit(void (*func)(void));

#endif
