#include "sea_lib_only.h"
#include <stdlib.h>

DLL_EXPORT int atoi_exp(const char* s) { return atoi(s); }
DLL_EXPORT long atol_exp(const char* s) { return atol(s); }
DLL_EXPORT double atof_exp(const char* s) { return atof(s); }

DLL_EXPORT long strtol_exp(const char* nptr, char** endptr, int base) { return strtol(nptr, endptr, base); }
DLL_EXPORT unsigned long strtoul_exp(const char* nptr, char** endptr, int base) { return strtoul(nptr, endptr, base); }
DLL_EXPORT double strtod_exp(const char* nptr, char** endptr) { return strtod(nptr, endptr); }
DLL_EXPORT int rand_exp(void) { return rand(); }
DLL_EXPORT void srand_exp(unsigned int seed) { srand(seed); }

DLL_EXPORT void qsort_exp(
   void* base,        // puntatore all’array
   size_t nitems,     // numero di elementi
   size_t size,       // dimensione di ogni elemento
   int (*compar)(const void*, const void*) // funzione di confronto
)
{
   return qsort(base, nitems, size, compar);
}

DLL_EXPORT void* bsearch_exp(
   const void* key,          // elemento da cercare
   const void* base,         // array ordinato
   size_t nitems,            // numero di elementi
   size_t size,              // dimensione di ogni elemento
   int (*compar)(const void*, const void*) // funzione di confronto
)
{
   return bsearch(key, base, nitems, size, compar);
}

DLL_EXPORT int system_exp(const char* sys) { return system(sys); }
