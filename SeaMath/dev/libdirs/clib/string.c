#include <stddef.h>
#include <string.h>
#include "sea_lib_only.h"

/// <summary>
/// 
/// </summary>
/// <param name="dest"></param>
/// <param name="src"></param>
/// <param name="n"></param>
/// <returns></returns>
DLL_EXPORT void* memcpy_exp(void* dest, const void* src, size_t n)
{
   unsigned char* d = (unsigned char*)dest;
   const unsigned char* s = (const unsigned char*)src;

   for (size_t i = 0; i < n; i++)
      d[i] = s[i];

   return dest;
}

/// <summary>
/// 
/// </summary>
/// <param name="dest"></param>
/// <param name="src"></param>
/// <param name="n"></param>
/// <returns></returns>
DLL_EXPORT void* memmove_exp(void* dest, const void* src, size_t n)
{
   unsigned char* d = (unsigned char*)dest;
   const unsigned char* s = (const unsigned char*)src;

   if (d < s) {
      for (size_t i = 0; i < n; i++)
         d[i] = s[i];
   }
   else if (d > s) {
      for (size_t i = n; i > 0; i--)
         d[i - 1] = s[i - 1];
   }

   return dest;
}

/// <summary>
/// 
/// </summary>
/// <param name="s"></param>
/// <param name="c"></param>
/// <param name="n"></param>
/// <returns></returns>
DLL_EXPORT void* memset_exp(void* s, int c, size_t n)
{
   unsigned char* p = (unsigned char*)s;

   for (size_t i = 0; i < n; i++)
      p[i] = (unsigned char)c;

   return s;
}

/// <summary>
/// 
/// </summary>
/// <param name="s1"></param>
/// <param name="s2"></param>
/// <param name="n"></param>
/// <returns></returns>
DLL_EXPORT int memcmp_exp(const void* s1, const void* s2, size_t n)
{
   const unsigned char* a = s1;
   const unsigned char* b = s2;

   for (size_t i = 0; i < n; i++) {
      if (a[i] != b[i])
         return a[i] - b[i];
   }

   return 0;
}

/// <summary>
/// 
/// </summary>
/// <param name="s"></param>
/// <param name="c"></param>
/// <param name="n"></param>
/// <returns></returns>
DLL_EXPORT void* memchr_exp(const void* s, int c, size_t n)
{
   const unsigned char* p = s;

   for (size_t i = 0; i < n; i++) {
      if (p[i] == (unsigned char)c)
         return (void*)(p + i);
   }

   return NULL;
}


/// <summary>
/// 
/// </summary>
/// <param name="s"></param>
/// <returns></returns>
DLL_EXPORT size_t strlen_exp(const char* s)
{
   size_t len = 0;
   while (s[len])
      len++;
   return len;
}

/// <summary>
/// 
/// </summary>
/// <param name="dest"></param>
/// <param name="src"></param>
/// <returns></returns>
DLL_EXPORT char* strcpy_exp(char* dest, const char* src)
{
   char* d = dest;
   while ((*d++ = *src++));
   return dest;
}

/// <summary>
/// 
/// </summary>
/// <param name="dest"></param>
/// <param name="src"></param>
/// <param name="n"></param>
/// <returns></returns>
DLL_EXPORT char* strncpy_exp(char* dest, const char* src, size_t n)
{
   size_t i = 0;

   for (; i < n && src[i]; i++)
      dest[i] = src[i];

   for (; i < n; i++)
      dest[i] = '\0';

   return dest;
}

DLL_EXPORT char* strcat_exp(char* dest, const char* src)
{
   char* d = dest + strlen_exp(dest);
   while ((*d++ = *src++));
   return dest;
}

DLL_EXPORT char* strncat_exp(char* dest, const char* src, size_t n)
{
   char* d = dest + strlen_exp(dest);
   size_t i = 0;

   while (i < n && src[i]) {
      d[i] = src[i];
      i++;
   }
   d[i] = '\0';

   return dest;
}

DLL_EXPORT int strcmp_exp(const char* s1, const char* s2)
{
   while (*s1 && (*s1 == *s2)) 
   {
      s1++;
      s2++;
   }
   
   return (unsigned char)*s1 - (unsigned char)*s2;
}

DLL_EXPORT int strncmp_exp(const char* s1, const char* s2, size_t n)
{
   for (size_t i = 0; i < n; i++) 
   {
      if (s1[i] != s2[i])
         return (unsigned char)s1[i] - (unsigned char)s2[i];
      if (s1[i] == '\0')
         return 0;
   }
   return 0;
}

DLL_EXPORT char* strchr_exp(const char* s, int c)
{
   while (*s) 
   {
      if (*s == (char)c)
         return (char*)s;
      s++;
   }

   return (c == '\0') ? (char*)s : NULL;
}

DLL_EXPORT char* strrchr_exp(const char* s, int c)
{
   const char* last = NULL;

   while (*s) {
      if (*s == (char)c)
         last = s;
      s++;
   }

   if (c == '\0')
      return (char*)s;

   return (char*)last;
}

DLL_EXPORT char* strstr_exp(const char* haystack, const char* needle)
{
   if (!*needle)
      return (char*)haystack;

   for (; *haystack; haystack++) {
      const char* h = haystack;
      const char* n = needle;

      while (*h && *n && (*h == *n)) {
         h++;
         n++;
      }

      if (!*n)
         return (char*)haystack;
   }

   return NULL;
}

DLL_EXPORT size_t strspn_exp(const char* s, const char* accept)
{
   const char* p = s;

   while (*p) {
      const char* a = accept;
      int found = 0;

      while (*a) {
         if (*p == *a++) {
            found = 1;
            break;
         }
      }

      if (!found)
         break;

      p++;
   }

   return (size_t)(p - s);
}

DLL_EXPORT size_t strcspn_exp(const char* s, const char* reject)
{
   const char* p = s;

   while (*p) {
      const char* r = reject;
      while (*r) {
         if (*p == *r)
            return (size_t)(p - s);
         r++;
      }
      p++;
   }

   return (size_t)(p - s);
}

DLL_EXPORT char* strpbrk_exp(const char* s, const char* accept)
{
   while (*s) {
      const char* a = accept;
      while (*a) {
         if (*s == *a)
            return (char*)s;
         a++;
      }
      s++;
   }
   return NULL;
}

DLL_EXPORT char* strtok_exp(char* str, const char* delim)
{
   static char* next;
   char* start;

   if (str)
      next = str;
   if (!next)
      return NULL;

   while (*next) {
      const char* d = delim;
      int is_delim = 0;

      while (*d) {
         if (*next == *d++) {
            is_delim = 1;
            break;
         }
      }

      if (!is_delim)
         break;
      next++;
   }

   if (!*next)
      return NULL;

   start = next;

   while (*next) {
      const char* d = delim;
      while (*d) {
         if (*next == *d++) {
            *next++ = '\0';
            return start;
         }
      }
      next++;
   }

   return start;
}

/* ========================= */
/*  Error handling           */
/* ========================= */

DLL_EXPORT char* strerror_exp(int errnum)
{
   (void)errnum;
   return "Unknown error";
}

static const unsigned char collation_table[256] = {
   /* 0–31 */ 0,
   /* spazio */ 1,
   ['A'] = 2,['a'] = 2,
   ['B'] = 3,['b'] = 3,
   ['C'] = 4,['c'] = 4,
   /* ... */
   ['Z'] = 27,['z'] = 27,
};

DLL_EXPORT int strcoll_exp(const char* s1, const char* s2)
{
   return strcoll(s1, s2);
   //while (*s1 && *s2) {
   //   unsigned char c1 = collation_table[(unsigned char)*s1];
   //   unsigned char c2 = collation_table[(unsigned char)*s2];

   //   if (c1 != c2)
   //      return c1 - c2;

   //   s1++;
   //   s2++;
   //}

//   return collation_table[(unsigned char)*s1] - collation_table[(unsigned char)*s2];
}