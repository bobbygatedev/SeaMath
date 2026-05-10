#ifndef ASSERT_H
#define ASSERT_H

#include "sea.h"
#include "wchar.h"
#include <stdint.h>

void seaassert_w (const wchar_t *msg, const wchar_t *file, uint32_t line);
void seaassert   (const char    *msg, const char *file,    uint32_t line);

#if defined(UNICODE)
#  define assert(expr) ( (!!(expr)) || (seaassert_w(SEAWIDE(#expr),__SEAWIDE(__FILE__),__LINE__)) )
#else /* not unicode */
#  define assert(expr) ( (!!(expr)) || (seaassert(#expr,__FILE__,__LINE__)) )
#endif /* _UNICODE||UNICODE */

#endif //#ifndef ASSERT_H

