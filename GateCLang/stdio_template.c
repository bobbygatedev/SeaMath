#define _CRT_SECURE_NO_WARNINGS
#define __USE_MINGW_ANSI_STDIO

#include <stdio.h>  
#include <stdint.h>  
#include <stdarg.h>  
#include <string.h>  

#define EXPORT __declspec(dllexport)  

typedef enum {
   ARG_POINTER = 0,
#[Types:]#
   ARG_#[TypeName]#=#[TypeCode]#,
#[;]#
}ArgType;

typedef struct 
{
   ArgType type;
   void* value_p;
}Arg_t;

EXPORT int sscanf_wrapper(const char* buffer, const char* format, ...);
EXPORT int sprintf_wrapper(char* outBuffer, size_t outBufferSize, const char* format, const Arg_t* args, int argCount);


EXPORT int sscanf_wrapper(const char* buffer, const char* format, ...)
{
   va_list ars;
   va_start(ars, format);

   int res = vsscanf(buffer, format, ars);

   va_end(ars);

   return res;
}

EXPORT int sprintf_wrapper(char* outBuffer, size_t outBufferSize, const char* format, const Arg_t* args, int argCount)
{
   if (!format || (!args && argCount > 0) || !outBuffer) {
      return -1; // Error  
   }

   // Temporary buffer to hold the arguments unpacked  
   int cur_arg = 0;

   // We'll build the formatted string step by step  
   const char* frm_p = format;
   char* ob_p = outBuffer;
   int n_wri = 0;

   while (*frm_p)
   {
      if (*frm_p == '%' && (*(frm_p + 1) != '%'))
      {
         // Start parsing  
         char spc[32] = { 0 };
         int spc_len = 0;

         // Copy the format specifier (naive parsing for simplicity)  
         const char* fmt_sta = frm_p;

         do {
            spc[spc_len++] = *frm_p++;
         } while (*frm_p && !strchr("diufFeEgGxXoscpaAn", *frm_p));

         spc[spc_len++] = *frm_p++; // include the last char  
         spc[spc_len] = 0;

         //if not enough arguments, continue (ie format string is replaced by empty string)
         if (cur_arg >= argCount) { continue; }

         Arg_t arg = args[cur_arg++];

         // Switch based on arg type  
         switch (arg.type)
         {
         case ARG_POINTER:
            n_wri += snprintf(ob_p, outBufferSize - n_wri, spc, (uint64_t)arg.value_p);
            break;

#[Types:]#
         case  ARG_#[TypeName]#:
            n_wri += snprintf(ob_p, outBufferSize - n_wri, spc, *((#[TypeCpp]#*)arg.value_p));
            break;

#[;]#
         default:
            snprintf(outBuffer, outBufferSize, "Unknown argument type.");
            return -2;
         }

         ob_p = outBuffer + n_wri;
      }
      else
      {
         *ob_p++ = *frm_p++;
         n_wri++;
      }
   }

   *ob_p = '\0';

   return n_wri;
}

EXPORT int wsscanf_wrapper(const wchar_t* buffer, const wchar_t* format, ...);
EXPORT int wsprintf_wrapper(wchar_t* outBuffer, size_t outBufferSize, const wchar_t* format, const Arg_t* args, int argCount);


EXPORT int wsscanf_wrapper(const wchar_t* buffer, const wchar_t* format, ...)
{
   va_list ars;
   va_start(ars, format);

   int res = vswscanf(buffer, format, ars);

   va_end(ars);

   return res;
}

EXPORT int wsprintf_wrapper(wchar_t* outBuffer, size_t outBufferSize, const wchar_t* format, const Arg_t* args, int argCount)
{
   if (!format || (!args && argCount > 0) || !outBuffer) {
      return -1; // Error  
   }

   // Temporary buffer to hold the arguments unpacked  
   int cur_arg = 0;

   // We'll build the formatted string step by step  
   const wchar_t* frm_p = format;
   wchar_t* ob_p = outBuffer;
   int n_wri = 0;

   while (*frm_p)
   {
      if (*frm_p == '%' && (*(frm_p + 1) != '%'))
      {
         // Start parsing  
         wchar_t spc[32] = { 0 };
         int spc_len = 0;

         // Copy the format specifier (naive parsing for simplicity)  
         const wchar_t* fmt_sta = frm_p;

         do {
            spc[spc_len++] = *frm_p++;
         } while (*frm_p && !wcschr(L"diufFeEgGxXoscpaAn", *frm_p));

         spc[spc_len++] = *frm_p++; // include the last char  
         spc[spc_len] = 0;

         //if not enough arguments, continue (ie format string is replaced by empty string)
         if (cur_arg >= argCount) { continue; }

         Arg_t arg = args[cur_arg++];

         // Switch based on arg type  
         switch (arg.type)
         {
         case ARG_POINTER:
            n_wri += swprintf(ob_p, outBufferSize - n_wri, spc, (uint64_t)arg.value_p);
            break;

#[Types:]#
         case  ARG_#[TypeName]#:
            n_wri += swprintf(ob_p, outBufferSize - n_wri, spc, *((#[TypeCpp]#*)arg.value_p));
            break;

#[;]#
         default:
            swprintf(outBuffer, outBufferSize, L"Unknown argument type.");
            return -2;
         }

         ob_p = outBuffer + n_wri;
      }
      else
      {
         *ob_p++ = *frm_p++;
         n_wri++;
      }
   }

   *ob_p = '\0';

   return n_wri;
}
