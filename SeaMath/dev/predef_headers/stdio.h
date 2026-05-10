#ifndef STDIO_FUNCTIONS_H
#define STDIO_FUNCTIONS_H

#include "sea.h"
#include "stdint.h"

#ifndef SEEK_SET
#define SEEK_SET 0
#define SEEK_CUR 1
#define SEEK_END 2
#endif

typedef int64_t fpos_t;

/* ---------------------------------------------------------
   Funzioni di OUTPUT
   --------------------------------------------------------- */
int printf(const char *format, ...);
int fprintf(FILE *stream, const char *format, ...);
int sprintf(char *str, const char *format, ...);
int snprintf(char *str, unsigned int size, const char *format, ...);

int puts(const char *s);
int fputs(const char *s, FILE* stream);

int putchar(int c);
int fputc(int c, FILE* stream);


/* ---------------------------------------------------------
   Funzioni di INPUT
   --------------------------------------------------------- */
int scanf(const char *format, ...);
int fscanf(FILE* stream, const char *format, ...);
int sscanf(const char *str, const char *format, ...);

char *gets(char *s);         /* DEPRECATA in C11 */
char *fgets(char *s, int size, FILE* stream);

int getchar(void);
int fgetc(FILE* stream);


/* ---------------------------------------------------------
   Gestione void
   --------------------------------------------------------- */
FILE* fopen(const char *voidname, const char *mode);
FILE* freopen(const char *voidname, const char *mode, FILE* stream);
int fclose(FILE* stream);

void rewind(FILE* stream);
int fflush(FILE* stream);


/* ---------------------------------------------------------
   Lettura/Scrittura binaria
   --------------------------------------------------------- */
unsigned int fread(void *ptr, unsigned int size, unsigned int nmemb, FILE* stream);
unsigned int fwrite(const void *ptr, unsigned int size, unsigned int nmemb, FILE* stream);


/* ---------------------------------------------------------
   Posizionamento nel void
   --------------------------------------------------------- */
int fseek(FILE* stream, long offset, int whence);
long ftell(FILE* stream);
int fgetpos(FILE* stream, void *pos);
int fsetpos(FILE* stream, const void *pos);


/* ---------------------------------------------------------
   Controllo errori
   --------------------------------------------------------- */
void clearerr(FILE* stream);
int feof(FILE* stream);
int ferror(FILE* stream);

/* ---------------------------------------------------------
   Standard streams
   --------------------------------------------------------- */

#define stdin  seagetstdin()
#define stdout seagetstdout()
#define stderr seagetstderr()

#endif /* STDIO_FUNCTIONS_H */
