#ifndef _STDDEF_H_
#define _STDDEF_H_

#include "stdint.h"

#define NULL ((void *)0)
#define EXIT_SUCCESS 0
#define EXIT_FAILURE 1

typedef uint32_t size_t;

#define offsetof(type, member) ((size_t)&(((type *)0)->member))

#endif /* _STDDEF_H_ */