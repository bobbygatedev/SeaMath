#ifndef _MATH_H_
#define _MATH_H_

// math.h - seamath math.h module   
// this is seamath dll-library include files
// just used function shall be declared here
// types can be from predef directory
// library dir valid in c file only

#include "sea.h"

double cosd(double val);
double sind(double val);
double tand(double val);
double acosd(double val);
double asind(double val);
double atand(double val);

float SEATR(altname(cosf_exp))  cosf(float val);
float SEATR(altname(sinf_exp))  sinf(float val);
float SEATR(altname(tanf_exp))  tanf(float val);
float SEATR(altname(acosf_exp)) acosf(float val);
float SEATR(altname(asinf_exp)) asinf(float val);
float SEATR(altname(atanf_exp)) atanf(float val);
float SEATR(altname(expf_exp))  expf(float val);

double expd(double val);
seacmpd expcd(seacmpd z);
seacmpf expcf(seacmpf z);

seacmpd sincd(seacmpd z);
seacmpd coscd(seacmpd z);
seacmpf sincf(seacmpf z);
seacmpf coscf(seacmpf z);

double atan2d(double y, double x);

float   SEATR(altname(atan2f_exp)) atan2f(float y, float x);
sealdbl SEATR(altname(atan2l_exp)) atan2l(sealdbl y, sealdbl x);

seacmpd  catancd(seacmpd z);
seacmpf  catancf(seacmpf z);
seacmpld catancl(seacmpld z);

double logd(double val);

float   SEATR(altname(logf_exp))  logf(float val);
sealdbl SEATR(altname(logl_exp))  logl(sealdbl val);

double log10d(double val);

float   SEATR(altname(log10f_exp)) log10f(float val);
sealdbl SEATR(altname(log10l_exp)) log10l(sealdbl val);

double powd(double x, double y);

float   SEATR(altname(powf_exp)) powf(float x, float y);
sealdbl SEATR(altname(powl_exp)) powl(sealdbl x, sealdbl y);

double ceild(double val);

float   SEATR(altname(ceilf_exp)) ceilf(float val);
sealdbl SEATR(altname(ceill_exp)) ceill(sealdbl val);

double floord(double val);

float   SEATR(altname(floorf_exp)) floorf(float val);
sealdbl SEATR(altname(floorl_exp)) floorl(sealdbl val);

double sqrtd(double val);

float   SEATR(altname(sqrtf_exp)) sqrtf(float val);
sealdbl SEATR(altname(sqrtl_exp)) sqrtl(sealdbl val);

double fabsd(double val);

float   SEATR(altname(fabsf_exp)) fabsf(float val);
sealdbl SEATR(altname(fabsl_exp)) fabsl(sealdbl val);

double fmodd(double x, double y);

float   SEATR(altname(fmodf_exp)) fmodf(float x, float y);
sealdbl SEATR(altname(fmodl_exp)) fmodl(sealdbl x, sealdbl y);

double frexpd(double x, int* exp);

float   SEATR(altname(frexpf_exp)) frexpf(float x, int* exp);
sealdbl SEATR(altname(frexpl_exp)) frexpl(sealdbl x, int* exp);

double ldexpd(double x, int exp);

float   SEATR(altname(ldexpf_exp)) ldexpf(float x, int exp);
sealdbl SEATR(altname(ldexpl_exp)) ldexpl(sealdbl x, int exp);

double coshd(double val);

float   SEATR(altname(coshf_exp)) coshf(float val);
sealdbl SEATR(altname(coshl_exp)) coshl(sealdbl val);

#endif
