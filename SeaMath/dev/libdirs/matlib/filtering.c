#include "sea_lib_only.h"
#include <stddef.h>

int DLL_EXPORT convf(const float* v1, int v1Size, const float* v2, int v2Size, float* output, int outSize)
{
   if(v1 == NULL){ return v1Size + v2Size - 1; }

   //N  + M − 1
   int n_out = SEAMIN(outSize,v1Size+v2Size-1);

   for (int i = 0; i < n_out; i++)
   {
      output[i] = 0;

      for (int j = 0; j < n_out; j++)
      {
         if (j < v1Size && i - j >= 0 && i - j < v2Size)
         {
            output[i] += v1[j] * v2[i - j];
         }
      }
   }

   return n_out;
}

int DLL_EXPORT convd(const double* v1, int v1Size, const double* v2, int v2Size, double* output, int outSize)
{
   if(v1 == NULL){ return v1Size + v2Size - 1; }

   //N  + M − 1
   int n_out = SEAMIN(outSize, v1Size + v2Size - 1);

   for (int i = 0; i < n_out; i++)
   {
      output[i] = 0;

      for (int j = 0; j < n_out; j++)
      {
         if (j < v1Size && i - j >= 0 && i - j < v2Size)
         {
            output[i] += v1[j] * v2[i - j];
         }
      }
   }

   return n_out;
}

int DLL_EXPORT convcd(const seacmpd* v1, int v1Size, const seacmpd* v2, int v2Size, seacmpd* output, int outSize)
{
   if(v1 == NULL){ return v1Size + v2Size - 1; }

   //N  + M − 1
   int n_out = SEAMIN(outSize, v1Size + v2Size - 1);

   for (int i = 0; i < n_out; i++)
   {
      output[i] = 0;

      for (int j = 0; j < n_out; j++)
      {
         if (j < v1Size && i - j >= 0 && i - j < v2Size)
         {
            output[i] += v1[j] * v2[i - j];
         }
      }
   }

   return n_out;
}

int DLL_EXPORT convcf(const seacmpf* v1, int v1Size, const seacmpf* v2, int v2Size, seacmpf* output, int outSize)
{
   if(v1 == NULL){ return v1Size + v2Size - 1; }

   //N  + M − 1
   int n_out = SEAMIN(outSize, v1Size + v2Size - 1);

   for (int i = 0; i < n_out; i++)
   {
      output[i] = 0;

      for (int j = 0; j < n_out; j++)
      {
         if (j < v1Size && i - j >= 0 && i - j < v2Size)
         {
            output[i] += v1[j] * v2[i - j];
         }
      }
   }

   return n_out;
}


