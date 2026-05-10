#ifndef MATH_CONSTANT_H
#define MATH_CONSTANT_H

/*
 * Mathematical constants
 *
 * - Defined as uppercase macros
 * - No dependency on non-standard extensions (M_PI, etc.)
 * - Suitable for numerical and scientific code
 * - Double and long double variants provided
 */

/* =========================================================
 * DOUBLE PRECISION CONSTANTS
 * ========================================================= */

/* Pi (π) */
#define MC_PI          3.14159265358979323846

/* Euler's number (e) */
#define MC_E           2.71828182845904523536

/* Natural logarithm of 2 */
#define MC_LN2         0.69314718055994530942

/* Natural logarithm of 10 */
#define MC_LN10        2.30258509299404568402

/* Square root of 2 */
#define MC_SQRT2       1.41421356237309504880

/* Inverse square root of 2 (1 / sqrt(2)) */
#define MC_INV_SQRT2   0.70710678118654752440

/* =========================================================
 * LONG DOUBLE PRECISION CONSTANTS
 * ========================================================= */

/* Pi (π) */
#define MC_PIL         3.14159265358979323846264338327950288L

/* Euler's number (e) */
#define MC_EL          2.71828182845904523536028747135266250L

/* Natural logarithm of 2 */
#define MC_LN2L        0.69314718055994530941723212145817656L

/* Natural logarithm of 10 */
#define MC_LN10L       2.30258509299404568401799145468436421L

/* Square root of 2 */
#define MC_SQRT2L      1.41421356237309504880168872420969808L

/* Inverse square root of 2 (1 / sqrt(2)) */
#define MC_INV_SQRT2L  0.70710678118654752440084436210484904L

#endif /* MATH_CONSTANT_H */
