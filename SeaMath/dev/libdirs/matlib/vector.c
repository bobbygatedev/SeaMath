#include "sea_lib_only.h"

DLL_EXPORT double vecscalard(const double* v1 , int v1Size, const double* v2, int v2Size)
{
   int sz     = v1Size < v2Size ? v1Size : v2Size;
   double res = 0;

   for (int i = 0; i < sz; i++)
   {
      res += v1[i] * v2[i];
   }
   
   return res; 
}

DLL_EXPORT float vecscalarf(const float* v1, int v1Size, const float* v2, int v2Size)
{
   int sz = v1Size < v2Size ? v1Size : v2Size;
   float res = 0;

   for (int i = 0; i < sz; i++)
   {
      res += v1[i] * v2[i];
   }

   return res;
}

DLL_EXPORT seacmpd vecscalarcd(const seacmpd* v1, int v1Size, const seacmpd* v2, int v2Size)
{
   int sz = v1Size < v2Size ? v1Size : v2Size;
   seacmpd res = 0;
   seacmpsd* v2_p = (seacmpsd*)v2;

   for (int i = 0; i < sz; i++)
   {
      res += v1[i] * (v2_p[i].re -1I * v2_p[i].im);
   }

   return res;
}

DLL_EXPORT seacmpf vecscalarcf(const seacmpf* v1, int v1Size, const seacmpf* v2, int v2Size)
{
   int sz = v1Size < v2Size ? v1Size : v2Size;
   seacmpf res = 0;

   seacmpsf* v2_p = (seacmpsf*)v2;

   for (int i = 0; i < sz; i++)
   {
      res += v1[i] * (v2_p[i].re -1.0fI * v2_p[i].im);
   }

   return res;
}
