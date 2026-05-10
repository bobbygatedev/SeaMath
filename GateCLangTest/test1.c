

#include <test1.h>

/**/
#pragma


#define A B+C	
#define B A
#define C 66
#define S1(s) #s s
#define S(s) S1(s+B)
#define V(a,...) a __VA_ARGS__
#define S2(...) #__VA_ARGS__
#define S3(...) X ## __VA_ARGS__

//EXP:?XA+66 ?
? S3(A) ?
//EXP:?"A" ?
? S2(A) ?
//EXP:?"A+66+B" A+66+B+66  ?
? S(A) ?
//EXP:?A+66 B+66,66 ?
? V(A, B, C) ?

#define M1(a) micky a
#define M(a,b) a x ## y b

//EXP:A+66
A
//EXP:B+66
B

#ifdef A
//EXP:Yes
Yes
//EXP:!A+66 xy B+66 !
!M(A, B)!
//EXP:66
C
//EXP:3.14+A+66 xy  
3.14 + M(A)
#else
//NOAPP
No
#endif