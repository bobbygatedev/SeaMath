#ifndef _ERRNO
#define _ERRNO

typedef int errno_t;

/// <summary>
/// 
/// </summary>
/// <returns></returns>
int* seaerrnolocation(void);

/// <summary>
/// 
/// </summary>
/// <param name="value"></param>
void seaerrnoset(int value);

/// <summary>
/// 
/// </summary>
/// <param name=""></param>
/// <returns></returns>
int seaerrnoget(void);

#define errno (*seaerrnolocation())

#define EPERM 1
#define ENOENT 2
#define ESRCH 3
#define EINTR 4
#define EIO 5
#define ENXIO 6
#define E2BIG 7
#define ENOEXEC 8
#define EBADF 9
#define ECHILD 10
#define EAGAIN 11
#define ENOMEM 12
#define EACCES 13
#define EFAULT 14
#define EBUSY 16
#define EEXIST 17
#define EXDEV 18
#define ENODEV 19
#define ENOTDIR 20
#define EISDIR 21
#define ENFILE 23
#define EMFILE 24
#define ENOTTY 25
#define EFBIG 27
#define ENOSPC 28
#define ESPIPE 29
#define EROFS 30
#define EMLINK 31
#define EPIPE 32
#define EDOM 33
#define EDEADLK 36
#define ENAMETOOLONG 38
#define ENOLCK 39
#define ENOSYS 40
#define ENOTEMPTY 41
#define EINVAL 22
#define ERANGE 34
#define EILSEQ 42
#define STRUNCATE 80
#define ENOTSUP         129
#define EAFNOSUPPORT 102
#define EADDRINUSE 100
#define EADDRNOTAVAIL 101
#define EISCONN 113
#define ENOBUFS 119
#define ECONNABORTED 106
#define EALREADY 103
#define ECONNREFUSED 107
#define ECONNRESET 108
#define EDESTADDRREQ 109
#define EHOSTUNREACH 110
#define EMSGSIZE 115
#define ENETDOWN 116
#define ENETRESET 117
#define ENETUNREACH 118
#define ENOPROTOOPT 123
#define ENOTSOCK 128
#define ENOTCONN 126
#define ECANCELED 105
#define EINPROGRESS 112
#define EOPNOTSUPP 130
#define EWOULDBLOCK 140
#define EOWNERDEAD 133
#define EPROTO 134
#define EPROTONOSUPPORT 135
#define EBADMSG 104
#define EIDRM 111
#define ENODATA 120
#define ENOLINK 121
#define ENOMSG 122
#define ENOSR 124
#define ENOSTR 125
#define ENOTRECOVERABLE 127
#define ETIME 137
#define ETXTBSY 139
#define ETIMEDOUT 138
#define ELOOP 114
#define EPROTOTYPE 136
#define EOVERFLOW 132

#endif