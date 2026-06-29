#include "sea_lib_only.h"
#include <math.h>


DLL_EXPORT uint64_t swapbyten (uint64_t val, int nbit)
{
   uint64_t res = 0x0;
   int      nb  = nbit >> 3; 

   for ( int i = 0 ; i < nb ; i++ ) 
   {
      ((uint8_t*)&res)[nb-1-i] = ((uint8_t*)&val)[i];
   }

   return res;
}

DLL_EXPORT uint8_t swapbyte8(uint8_t val)
{
   return swapbyten(val,8);
}

DLL_EXPORT uint16_t swapbyte16(uint16_t val)
{
   return swapbyten(val,16);
}

DLL_EXPORT uint32_t swapbyte32(uint32_t val)
{
   return swapbyten(val,32);
}

DLL_EXPORT uint64_t swapbyte64(uint64_t val)
{
   return swapbyten(val,64);
}

DLL_EXPORT uint64_t swapbitn (uint64_t val, int nbit)
{
   uint64_t res = 0x0;
   
   for ( int i = 0 ; i < nbit ; i++ )
   {
      res |= (1 << (nbit-1-i)) *( (val & (1 << i)) != 0 ); 
   } 

   return res;
}


DLL_EXPORT uint8_t swapbit8(uint8_t val)
{
   return swapbitn(val,8);
}

DLL_EXPORT uint16_t swapbit16(uint16_t val)
{
   return swapbitn(val,16);
}

DLL_EXPORT uint32_t swapbit32(uint32_t val)
{
   return swapbitn(val,32);
}

DLL_EXPORT uint64_t swapbit64(uint64_t val)
{
   return swapbitn(val,64);
}



