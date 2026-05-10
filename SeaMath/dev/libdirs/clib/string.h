#ifndef _STRING_H_
#define _STRING_H_

#include "sea.h"
#include <stddef.h>

void* SEATR(altname(memcpy_exp)) memcpy(void* dest, const void* src, size_t n);

void* SEATR(altname(memmove_exp)) memmove(void* dest, const void* src, size_t n);

void* SEATR(altname(memset_exp)) memset(void* s, int c, size_t n);

int   SEATR(altname(memcmp_exp)) memcmp(const void* s1, const void* s2, size_t n);

void* SEATR(altname(memchr_exp)) memchr(const void* s, int c, size_t n);

size_t SEATR(altname(strlen_exp)) strlen(const char* s);

char* SEATR(altname(strcpy_exp)) strcpy(char* dest, const char* src);

char* SEATR(altname(strncpy_exp)) strncpy(char* dest, const char* src, size_t n);

char* SEATR(altname(strcat_exp)) strcat(char* dest, const char* src);

char* SEATR(altname(strncat_exp)) strncat(char* dest, const char* src, size_t n);

int SEATR(altname(strcmp_exp)) strcmp(const char* s1, const char* s2);

int SEATR(altname(strncmp_exp)) strncmp(const char* s1, const char* s2, size_t n);

int SEATR(altname(strcoll_exp)) SEATR(sea_no_vect) strcoll(const char* s1, const char* s2);

char* SEATR(altname(strchr_exp)) strchr(const char* s, int c);

char* SEATR(altname(strrchr_exp)) strrchr(const char* s, int c);

char* SEATR(altname(strstr_exp)) strstr(const char* haystack, const char* needle);

size_t SEATR(altname(strspn_exp)) strspn(const char* s, const char* accept);

size_t SEATR(altname(strcspn_exp)) strcspn(const char* s, const char* reject);

char* SEATR(altname(strpbrk_exp)) strpbrk(const char* s, const char* accept);

char* SEATR(altname(strtok_exp)) strtok(char* str, const char* delim);

char* SEATR(altname(strerror_exp)) strerror(int errnum);

#endif
