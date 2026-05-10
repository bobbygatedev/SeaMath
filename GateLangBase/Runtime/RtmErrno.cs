namespace Gate.LangBase.Runtime
{
   /// <summary>
   ///  POSIX + seamath error codes
   /// </summary>
   public enum RtmErrno
   {
      /// <summary>
      /// 
      /// </summary>
      EPERM = 1,
      ENOENT = 2,
      ESRCH = 3,
      EINTR = 4,
      EIO = 5,
      ENXIO = 6,
      E2BIG = 7,
      ENOEXEC = 8,
      EBADF = 9,
      ECHILD = 10,
      EAGAIN = 11,
      ENOMEM = 12,
      EACCES = 13,
      EFAULT = 14,
      EBUSY = 16,
      EEXIST = 17,
      EXDEV = 18,
      ENODEV = 19,
      ENOTDIR = 20,
      EISDIR = 21,
      ENFILE = 23,
      EMFILE = 24,
      ENOTTY = 25,
      EFBIG = 27,
      ENOSPC = 28,
      ESPIPE = 29,
      EROFS = 30,
      EMLINK = 31,
      EPIPE = 32,
      EDOM = 33,
      EDEADLK = 36,
      ENAMETOOLONG = 38,
      ENOLCK = 39,
      ENOSYS = 40,
      ENOTEMPTY = 41,
      EINVAL = 22,
      ERANGE = 34,
      EILSEQ = 42,
      STRUNCATE = 80,
      ENOTSUP = 129,
      EAFNOSUPPORT = 102,
      EADDRINUSE = 100,
      EADDRNOTAVAIL = 101,
      EISCONN = 113,
      ENOBUFS = 119,
      ECONNABORTED = 106,
      EALREADY = 103,
      ECONNREFUSED = 107,
      ECONNRESET = 108,
      EDESTADDRREQ = 109,
      EHOSTUNREACH = 110,
      EMSGSIZE = 115,
      ENETDOWN = 116,
      ENETRESET = 117,
      ENETUNREACH = 118,
      ENOPROTOOPT = 123,
      ENOTSOCK = 128,
      ENOTCONN = 126,
      ECANCELED = 105,
      EINPROGRESS = 112,
      EOPNOTSUPP = 130,
      EWOULDBLOCK = 140,
      EOWNERDEAD = 133,
      EPROTO = 134,
      EPROTONOSUPPORT = 135,
      EBADMSG = 104,
      EIDRM = 111,
      ENODATA = 120,
      ENOLINK = 121,
      ENOMSG = 122,
      ENOSR = 124,
      ENOSTR = 125,
      ENOTRECOVERABLE = 127,
      ETIME = 137,
      ETXTBSY = 139,
      ETIMEDOUT = 138,
      ELOOP = 114,
      EPROTOTYPE = 136,

      [RtmErrno(Message = "Overflow")]
      EOVERFLOW = 132,

      gatertm_error_base = 0x100000,

      [RtmErrno(Message = "Undefined error")]

      gatertm_error_undef = gatertm_error_base + 1,

      [RtmErrno(Message = "Assertion failed")]
      gatertm_assert_fail = 2,
   }
}
