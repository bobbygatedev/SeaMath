typedef long long int int64_t;

#pragma pack(4)

typedef struct S1
{
   char f1;
   char f2 : 5;
   char f3 : 7;
}S1;

typedef union U1
{
   S1   s1;
   int64_t  u1;
}U1;

U1 u1;

__declspec(dllexport) void set_u1(int64_t v) { u1.u1 = v; }
__declspec(dllexport) int64_t get_u1() { return u1.u1; }

__declspec(dllexport) int64_t get_u1_f1() { return u1.s1.f1; }
__declspec(dllexport) int64_t get_u1_f2() { return u1.s1.f2; }
__declspec(dllexport) int64_t get_u1_f3() { return u1.s1.f3; }

__declspec(dllexport) void set_u1_f1(int64_t v) { u1.s1.f1 = v; }
__declspec(dllexport) void set_u1_f2(int64_t v) { u1.s1.f2 = v; }
__declspec(dllexport) void set_u1_f3(int64_t v) { u1.s1.f3 = v; }

__declspec(dllexport) int64_t get_s1_sof() { return sizeof(S1); }

typedef struct S2
{
   char f1;
   S1 f2;
}S2;

typedef union U2
{
   S2   s2;
   int64_t  u2;
}U2;

U2 u2;

__declspec(dllexport) void set_u2(int64_t v) { u2.u2 = v; }
__declspec(dllexport) int64_t get_u2() { return u2.u2; }

__declspec(dllexport) int64_t get_u2_f1() { return u2.s2.f1; }
__declspec(dllexport) int64_t get_u2_f2_f1() { return u2.s2.f2.f1; }
__declspec(dllexport) int64_t get_u2_f2_f2() { return u2.s2.f2.f2; }
__declspec(dllexport) int64_t get_u2_f2_f3() { return u2.s2.f2.f3; }

__declspec(dllexport) void set_u2_f1(int64_t v) { u2.s2.f1 = v; }
__declspec(dllexport) void set_u2_f2_f1(int64_t v) { u2.s2.f2.f1 = v; }
__declspec(dllexport) void set_u2_f2_f2(int64_t v) { u2.s2.f2.f2 = v; }
__declspec(dllexport) void set_u2_f2_f3(int64_t v) { u2.s2.f2.f3 = v; }

__declspec(dllexport) int64_t get_s2_sof() { return sizeof(S2); }

#pragma pack(1)
typedef struct S3
{
   char f1;
   int f2;
   char f3;
}S3;

typedef union U3
{
   S3   s3;
   int64_t  u3;
}U3;

U3 u3;

__declspec(dllexport) void set_u3(int64_t v) { u3.u3 = v; }
__declspec(dllexport) int64_t get_u3() { return u3.u3; }

__declspec(dllexport) int64_t get_u3_f1() { return u3.s3.f1; }
__declspec(dllexport) int64_t get_u3_f2() { return u3.s3.f2; }
__declspec(dllexport) int64_t get_u3_f3() { return u3.s3.f3; }

__declspec(dllexport) void set_u3_f1(int64_t v) { u3.s3.f1 = v; }
__declspec(dllexport) void set_u3_f2(int64_t v) { u3.s3.f2 = v; }
__declspec(dllexport) void set_u3_f3(int64_t v) { u3.s3.f3 = v; }

__declspec(dllexport) int64_t get_s3_sof() { return sizeof(S3); }

#pragma pack(1)
typedef struct S4
{
   char f1;
   int f2;
   char f3;
}S4;

typedef union U4
{
   S4   s4;
   int64_t  u4;
}U4;

U4 u4;

__declspec(dllexport) void set_u4(int64_t v) { u4.u4 = v; }
__declspec(dllexport) int64_t get_u4() { return u4.u4; }

__declspec(dllexport) int64_t get_u4_f1() { return u4.s4.f1; }
__declspec(dllexport) int64_t get_u4_f2() { return u4.s4.f2; }
__declspec(dllexport) int64_t get_u4_f3() { return u4.s4.f3; }

__declspec(dllexport) void set_u4_f1(int64_t v) { u4.s4.f1 = v; }
__declspec(dllexport) void set_u4_f2(int64_t v) { u4.s4.f2 = v; }
__declspec(dllexport) void set_u4_f3(int64_t v) { u4.s4.f3 = v; }

__declspec(dllexport) int64_t get_s4_sof() { return sizeof(S4); }

