#pragma once

#include <Windows.h>
#include <stdio.h>
#include <stdint.h>
#include <vcclr.h>

using namespace System;
using namespace System::Text;
using namespace System::Runtime::InteropServices;
using namespace Gate::Tools::Programming;

#define DLL_IMPORT _declspec(dllimport)

#define DLL_NAME "LongDouble.dll"

#define OPERATOR_PLUS  "operator_plus"
#define OPERATOR_MINUS  "operator_minus"
#define OPERATOR_MULTIPLY  "operator_multiply"
#define OPERATOR_DIVIDE  "operator_divide"
#define OPERATOR_MINUS_UNARY  "operator_minus_unary"
#define OPERATOR_SQRT  "operator_sqrt"

#define OPERATOR_COS  "operator_cos"
#define OPERATOR_SIN  "operator_sin"
#define OPERATOR_TAN  "operator_tan"
#define OPERATOR_ATAN  "operator_atan"
#define OPERATOR_ATAN2  "operator_atan2"
#define OPERATOR_EXP  "operator_exp"

#define LONG_DOUBLE_SPRINTF  "long_double_sprintf"
#define LONG_DOUBLE_WSSCANF    "long_double_scanf"

#define OPERATOR_FROM_DOUBLE  "operator_from_double"
#define OPERATOR_TO_DOUBLE  "operator_to_double"

#define GET_MAX_VALUE  "get_max_value"
#define GET_MIN_VALUE  "get_min_value"

#define LONG_DOUBLE_DLL_NAME  "LongDouble"

namespace Gate
{
   namespace LangBase
   {
      namespace ExtraTypes
      {
         typedef void(__cdecl* HandlerBinaryPp)(void* o1, void* o2, void* res);
         typedef void(__cdecl* HandlerUnaryPp)(void* o1, void* res);
         typedef void(__cdecl* HandlerSimplePp)(void* o1);
         typedef void(__cdecl* HandlerFromDoublePp)(double in, void* ldp);
         typedef double(__cdecl* HandlerToDoublePp)(void* in);
         typedef void(__cdecl* HandlerLongDoubleSprintfPp)(void* in, char* buf, int bufSize);
         typedef void(__cdecl* HandlerLongDoubleWscanfPp)(void* out, const wchar_t* buf);

#ifdef B32
         [StructLayout(LayoutKind::Sequential, Size = 12)]
#else
         [StructLayout(LayoutKind::Sequential, Size = 16)]
#endif
         public value class LongDouble
         {
            static IntPtr hLdLib;

            static ExtraDllCppCode^ myCppCode;

            static HandlerFromDoublePp myFromDoubleHandler;
            static HandlerToDoublePp myOperatorToDouble;
            static HandlerLongDoubleSprintfPp myHandlerHandlerLongDoubleSprintf;
            static HandlerLongDoubleWscanfPp myHandlerLongDoubleWscanfPp;

            static HandlerSimplePp myHandlerGetMaxValue;

            static HandlerSimplePp myHandlerGetMinValue;

            ref class InnerBinaryOperator
            {
               const char* mySign;
               const char* myFunctionName;
               const char* myOperation = NULL;
               HandlerBinaryPp myHandler = NULL;

            public:

               InnerBinaryOperator(const char* sign, const char* functionName)
               {
                  mySign = sign;
                  myFunctionName = functionName;
               }

               InnerBinaryOperator(const char* sign, const char* functionName, const char* operation)
               {
                  mySign = sign;
                  myFunctionName = functionName;
                  myOperation = operation;
               }

               LongDouble Go(LongDouble o1, LongDouble o2)
               {
                  LongDouble res;

                  if (myHandler == NULL)
                  {
                     myHandler = (HandlerBinaryPp)GetProcAddress((HMODULE)hLdLib.ToPointer(), myFunctionName);

                     if (myHandler == NULL)
                     {
                        throw gcnew Gate::Tools::ToolsException("Function " + gcnew String(myFunctionName) + " not found in dll " DLL_NAME );
                     }
                  }

                  (*myHandler)(&o1, &o2, &res);

                  return res;
               }

               property String^ Body
               {
                  String^ get()
                  {
                     if (myOperation != NULL)
                     {
                        return
                           "EXPORT void " + gcnew String(myFunctionName) +
                           "(long double* o1, long double* o2, long double* res) { *res = " + gcnew String(myOperation) + "; }";
                     }
                     else
                     {
                        return
                           "EXPORT void " + gcnew String(myFunctionName) +
                           "(long double* o1, long double* o2, long double* res) { *res = *o1 " + gcnew String(mySign) + " *o2; }";
                     }
                  }
               }
            };

            ref class InnerUnaryOperator
            {
               HandlerUnaryPp myHandler = NULL;
               const char* myFunctionName;
               const char* myFunctionBody;

            public:

               InnerUnaryOperator(const char* functionName, const char* functiobnBody)
               {
                  myFunctionName = functionName;
                  myFunctionBody = functiobnBody;
               }

               LongDouble Go(LongDouble o1)
               {
                  LongDouble res;

                  if (myHandler == NULL)
                  {
                     myHandler = (HandlerUnaryPp)GetProcAddress((HMODULE)hLdLib.ToPointer(), myFunctionName);

                     if (myHandler == NULL)
                     {
                        throw gcnew System::NullReferenceException();
                     }
                  }

                  (*myHandler)(&o1, &res);

                  return res;
               }

               property String^ Body
               {
                  String^ get()
                  {
                     return "EXPORT void " + gcnew String(myFunctionName) + "(long double* o1, long double* res ) { " + gcnew String(myFunctionBody) + " }";
                  }
               }
            };

            static InnerBinaryOperator^ myOperatorPlusInstance = gcnew InnerBinaryOperator("+", OPERATOR_PLUS);
            static InnerBinaryOperator^ myOperatorMinusInstance = gcnew InnerBinaryOperator("-", OPERATOR_MINUS);
            static InnerBinaryOperator^ myOperatorMultiplyInstance = gcnew InnerBinaryOperator("*", OPERATOR_MULTIPLY);
            static InnerBinaryOperator^ myOperatorDivideInstance = gcnew InnerBinaryOperator("/", OPERATOR_DIVIDE);

            static InnerUnaryOperator^ myOperatorUnaryMinusInstance = gcnew InnerUnaryOperator(OPERATOR_MINUS_UNARY, "*res = -*o1;");
            static InnerUnaryOperator^ myOperatorUnarySqrt = gcnew InnerUnaryOperator(OPERATOR_SQRT, "*res = sqrtl(*o1);");

            static InnerUnaryOperator^ myOperatorUnaryCos = gcnew InnerUnaryOperator(OPERATOR_COS, "*res = cosl(*o1);");
            static InnerUnaryOperator^ myOperatorUnarySin = gcnew InnerUnaryOperator(OPERATOR_SIN, "*res = sinl(*o1);"); 
            static InnerUnaryOperator^ myOperatorUnaryTan = gcnew InnerUnaryOperator(OPERATOR_TAN, "*res = tanl(*o1);"); 
            static InnerUnaryOperator^ myOperatorUnaryAtan = gcnew InnerUnaryOperator(OPERATOR_ATAN, "*res = atanl(*o1);");
            static InnerUnaryOperator^ myOperatorUnaryExp = gcnew InnerUnaryOperator(OPERATOR_EXP, "*res = expl(*o1);");
            static InnerBinaryOperator^ myOperatorBinaryAtan = gcnew InnerBinaryOperator("/", OPERATOR_ATAN2 , "atan2(*o1,*o2)");

#ifdef WIN32
            //96 bit
            System::UInt64 myLow;
            System::UInt32 myHi;
#else
            //128 bit
            System::UInt64 myLow;
            System::UInt64 myHi;
#endif

         public:
            literal String^ SPECIFIER = "long double";

            static operator LongDouble(double value)
            {
               LongDouble res;

               (*myFromDoubleHandler)(value, &res);

               return res;
            }

            static operator double(LongDouble lngDblValue)
            {
               return (*myOperatorToDouble)(&lngDblValue);
            }

            static LongDouble Parse(String^ value)
            {
               pin_ptr<const wchar_t> ptr = PtrToStringChars(value);
               LongDouble res;

               (*myHandlerLongDoubleWscanfPp)(&res, (const wchar_t*)ptr);

               return res;
            }

            static void ForceInit(ICompileEnv^ compileRnv)
            {
               if (myCppCode == nullptr)
               {
                  auto sb = gcnew System::Text::StringBuilder(100);

                  //create a gcc source code for handling long double(not available in MSVC)
                  sb->AppendLine("#define __USE_MINGW_ANSI_STDIO");
                  sb->AppendLine("#include <stdint.h>");
                  sb->AppendLine("#include <stdio.h>");
                  sb->AppendLine("#include <float.h>");
                  sb->AppendLine("#include <math.h>");
                  sb->AppendLine("#include <wchar.h>");
                  sb->AppendLine();
                  sb->AppendLine("#define EXPORT __declspec(dllexport)");
                  sb->AppendLine();
                  sb->AppendLine("EXPORT void " GET_MAX_VALUE "(long double* ldp) { *ldp = LDBL_MAX; }");
                  sb->AppendLine("EXPORT void " GET_MIN_VALUE "(long double* ldp) { *ldp = LDBL_MIN; }");
                  sb->AppendLine("EXPORT void " OPERATOR_FROM_DOUBLE "(double in, long double* ldp) { *ldp = (long double)in; }");
                  sb->AppendLine("EXPORT double " OPERATOR_TO_DOUBLE "(long double* in) { return (double)*in; }");

                  sb->AppendLine("EXPORT void " LONG_DOUBLE_SPRINTF "(long double* in, char* buf, int bufSize) { snprintf(buf,bufSize,\"%Lf\",*in); }");
                  sb->AppendLine("EXPORT void " LONG_DOUBLE_WSSCANF "(long double* out, const wchar_t* buf) { swscanf(buf,L\"%Lf\",out); }");

                  sb->AppendLine(myOperatorPlusInstance->Body);
                  sb->AppendLine(myOperatorMinusInstance->Body);
                  sb->AppendLine(myOperatorMultiplyInstance->Body);
                  sb->AppendLine(myOperatorDivideInstance->Body);

                  sb->AppendLine(myOperatorUnaryMinusInstance->Body);
                  sb->AppendLine(myOperatorUnarySqrt->Body);
                  sb->AppendLine(myOperatorUnaryCos->Body);
                  sb->AppendLine(myOperatorUnarySin->Body);
                  sb->AppendLine(myOperatorUnaryTan->Body);
                  sb->AppendLine(myOperatorUnaryAtan->Body);
                  sb->AppendLine(myOperatorUnaryExp->Body);
                  sb->AppendLine(myOperatorBinaryAtan->Body);

                  if (compileRnv == nullptr)
                  {
                     compileRnv = gcnew CompileEnvGcc();
                  }

                  myCppCode = gcnew ExtraDllCppCode(LONG_DOUBLE_DLL_NAME, sb->ToString(), compileRnv, true);

                  hLdLib = (IntPtr)LoadLibraryA(DLL_NAME);

                  //create pointer to the dll
                  myFromDoubleHandler = (HandlerFromDoublePp)GetProcAddress((HMODULE)hLdLib.ToPointer(), OPERATOR_FROM_DOUBLE);
                  myOperatorToDouble = (HandlerToDoublePp)GetProcAddress((HMODULE)hLdLib.ToPointer(), OPERATOR_TO_DOUBLE);

                  myHandlerHandlerLongDoubleSprintf = (HandlerLongDoubleSprintfPp)GetProcAddress((HMODULE)hLdLib.ToPointer(), LONG_DOUBLE_SPRINTF);
                  myHandlerLongDoubleWscanfPp = (HandlerLongDoubleWscanfPp)GetProcAddress((HMODULE)hLdLib.ToPointer(), LONG_DOUBLE_WSSCANF);
                  myHandlerGetMaxValue = (HandlerSimplePp)GetProcAddress((HMODULE)hLdLib.ToPointer(), GET_MAX_VALUE);
                  myHandlerGetMinValue = (HandlerSimplePp)GetProcAddress((HMODULE)hLdLib.ToPointer(), GET_MIN_VALUE);
               }
            }

            static LongDouble operator +(LongDouble o1, LongDouble o2) { return myOperatorPlusInstance->Go(o1, o2); }
            static LongDouble operator -(LongDouble o1, LongDouble o2) { return myOperatorMinusInstance->Go(o1, o2); }
            static LongDouble operator *(LongDouble o1, LongDouble o2) { return myOperatorMultiplyInstance->Go(o1, o2); }
            static LongDouble operator /(LongDouble o1, LongDouble o2) { return myOperatorDivideInstance->Go(o1, o2); }

            static LongDouble operator -(LongDouble o1) { return myOperatorUnaryMinusInstance->Go(o1); }
            
            LongDouble Sqrt() { return myOperatorUnarySqrt->Go(*this); }
            LongDouble Cos() { return myOperatorUnaryCos->Go(*this); }
            LongDouble Sin() { return myOperatorUnarySin->Go(*this); }
            LongDouble Tan() { return myOperatorUnaryTan->Go(*this); }
            LongDouble Atan() { return myOperatorUnaryAtan->Go(*this); }
            LongDouble Exp() { return myOperatorUnaryExp->Go(*this); }


            static LongDouble Atan2(LongDouble o1, LongDouble o2) { return myOperatorBinaryAtan->Go(o1,o2); }

            static property LongDouble MAX_VALUE
            {
               LongDouble get()
               {
                  LongDouble res;

                  myHandlerGetMaxValue(&res);

                  return res;
               }
            }

            static property LongDouble MIN_VALUE
            {
               LongDouble get()
               {
                  LongDouble res;

                  myHandlerGetMinValue(&res);

                  return res;
               }
            }

            virtual String^ ToString() override
            {
               uint64_t tmp[2] = { myLow, myHi };
               char tmp_buf[128];

               LongDouble res = *this;

               (*myHandlerHandlerLongDoubleSprintf)(&res, tmp_buf, sizeof(tmp_buf));

               return gcnew String(tmp_buf);
            }
         };
      }
   }
}