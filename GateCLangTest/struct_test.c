//f1 + f2
struct BaseStruct
{
   typedef int inda;

   int f1;

   union
   {
      char bf1 : 2;
      char bf2 : 2;
   };
};

 typedef struct TestStruct
 {
    union
    {
       struct BaseStruct bs;

       int i32;

       struct
       {
          int b2f1 : 31;
          int b2f2 : 4;
          char  b2f3 : 5;
       };
    };
 };

int g1,g2;

 void main(int p1 , int p2)
 {
    int a, b;

    {
       float p1;
    }

 }