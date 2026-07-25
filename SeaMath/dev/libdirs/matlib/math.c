#include "sea_lib_only.h"
#include <math.h>

DLL_EXPORT double cosd(double val) { return cos(val); }

DLL_EXPORT double sind(double val) { return sin(val); }

DLL_EXPORT double tand(double val) { return tan(val); }

DLL_EXPORT double acosd(double val) { return acos(val);}

DLL_EXPORT double asind(double val) { return asin(val);}

DLL_EXPORT double atand(double val) { return atan(val);}

DLL_EXPORT float cosf_exp(float val) { return cosf(val);}

DLL_EXPORT float sinf_exp(float val) { return sinf(val);}

DLL_EXPORT float tanf_exp(float val) { return tanf(val);}

DLL_EXPORT float acosf_exp(float val) { return acosf(val);}

DLL_EXPORT float asinf_exp(float val) { return asinf(val);}

DLL_EXPORT float atanf_exp(float val) { return atanf(val);}

//sin(x+iy)=sinxcoshy+icosxsinhy
DLL_EXPORT seacmpd sincd(seacmpd z) 
{ 
   return sin(((seacmpsd*)&z)->re)* cos(((seacmpsd*)&z)->im) + 1.0I*(cos(((seacmpsd*)&z)->re) * sin(((seacmpsd*)&z)->im)); 
}

//cos(z)=cos(x+iy)=cos(x)*cosh(y)−i*sin(x)*sinh(y)
DLL_EXPORT seacmpd coscd(seacmpd z) 
{ 
   return cos(((seacmpsd*)&z)->re) * cos(((seacmpsd*)&z)->im) + 1.0I * (sin(((seacmpsd*)&z)->re) * sin(((seacmpsd*)&z)->im)); 
}

//sin(x+iy)=sinxcoshy+icosxsinhy
DLL_EXPORT seacmpf sincf(seacmpf z) 
{ 
   return sinf(((seacmpsf*)&z)->re) * cosf(((seacmpsf*)&z)->im) + 1.0fI * (cos(((seacmpsf*)&z)->re) * sin(((seacmpsd*)&z)->im)); 
}

//cos(z)=cos(x+iy)=cos(x)*cosh(y)−i*sin(x)*sinh(y)
DLL_EXPORT seacmpf coscf(seacmpf z) 
{ 
   return cosf(((seacmpsf*)&z)->re) * cosf(((seacmpsf*)&z)->im) + 1.0fI * (sin(((seacmpsf*)&z)->re) * sin(((seacmpsf*)&z)->im)); 
}

DLL_EXPORT float expf_exp(float val) { return expf(val); }

DLL_EXPORT double expd(double val) { return exp(val); }

DLL_EXPORT seacmpd expcd(seacmpd z)
{
   return exp(((seacmpsd*)&z)->re) * (cos(((seacmpsd*)&z)->re) + 1.0I* sin(((seacmpsd*)&z)->im));
}

DLL_EXPORT seacmpf expcf(seacmpf z)
{
   return expf(((seacmpsf*)&z)->re) * (cosf(((seacmpsf*)&z)->re) + 1.0fI * sinf(((seacmpsf*)&z)->im));
}

DLL_EXPORT float atan2f_exp(float y, float x) { return atan2f(y, x); }

DLL_EXPORT double atan2d(double y, double x) { return atan2(y, x); }

DLL_EXPORT sealdbl atan2l_exp(sealdbl y, sealdbl x) { return atan2l(y, x); }

DLL_EXPORT seacmpd catancd(seacmpd z) { return atan2(((seacmpsd*)&z)->im, ((seacmpsd*)&z)->re); }

DLL_EXPORT seacmpf catancf(seacmpf z) { return atan2f(((seacmpsf*)&z)->im, ((seacmpsf*)&z)->re); }

DLL_EXPORT seacmpld catancl(seacmpld z) { return atan2l(((seacmpsld*)&z)->im, ((seacmpsld*)&z)->re); }

DLL_EXPORT double logd(double val) { return log(val); }

DLL_EXPORT float logf_exp(float val) { return logf(val); }

DLL_EXPORT sealdbl logl_exp(sealdbl val) { return logl(val); }

DLL_EXPORT double log10d(double val) { return log10(val); }

DLL_EXPORT float log10f_exp(float val) { return log10f(val); }

DLL_EXPORT sealdbl log10l_exp(sealdbl val) { return log10l(val); }

DLL_EXPORT double powd(double x, double y) { return pow(x,y); }

DLL_EXPORT float powf_exp(float x, float y) { return powf(x,y); }

DLL_EXPORT sealdbl powl_exp(sealdbl x, sealdbl y) { return powl(x,y); }

DLL_EXPORT double ceild(double val) { return ceil(val); }

DLL_EXPORT float ceilf_exp(float val) { return ceilf(val); }

DLL_EXPORT sealdbl ceill_exp(sealdbl val) { return ceill(val); }

DLL_EXPORT double floord(double val) { return floor(val); }

DLL_EXPORT float floorf_exp(float val) { return floorf(val); }

DLL_EXPORT sealdbl floorl_exp(sealdbl val) { return floorl(val); }

DLL_EXPORT double sqrtd(double val) { return sqrt(val); }

DLL_EXPORT float sqrtf_exp(float val) { return sqrtf(val); }

DLL_EXPORT sealdbl sqrtl_exp(sealdbl val) { return sqrtl(val); }

DLL_EXPORT double fabsd(double val) { return fabs(val); }

DLL_EXPORT float fabsf_exp(float val) { return fabsf(val); }

DLL_EXPORT sealdbl fabsl_exp(sealdbl val) { return fabsl(val); }

DLL_EXPORT double fmodd(double x, double y) { return fmod(x,y); }

DLL_EXPORT float fmodf_exp(float x, float y) { return fmodf(x,y); }

DLL_EXPORT sealdbl fmodl_exp(sealdbl x, sealdbl y) { return fmodl(x,y); }

DLL_EXPORT double frexpd(double x, int* exp) { return frexp(x,exp); }

DLL_EXPORT float  frexpf_exp(float x, int* exp) { return frexpf(x, exp); }

DLL_EXPORT sealdbl frexpl_exp(sealdbl x, int* exp) { return frexpl(x, exp); }

DLL_EXPORT double ldexpd(double x, int exp) { return ldexp(x, exp); }

DLL_EXPORT float  ldexpf_exp(float x, int exp) { return ldexpf(x, exp); }

DLL_EXPORT sealdbl ldexpl_exp(sealdbl x, int exp) { return ldexpl(x, exp); }

DLL_EXPORT double coshd(double val) { return cosh(val); }

DLL_EXPORT float  coshf_exp(float val) { return coshf(val); }

DLL_EXPORT sealdbl coshl_exp(sealdbl val) { return coshl(val); }
