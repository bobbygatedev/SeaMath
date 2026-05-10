#ifndef UNISTD_H
#define UNISTD_H

/// <summary>
/// 
/// </summary>
/// <param name="oldFd"></param>
/// <returns></returns>
int dup(int oldFd);

/// <summary>
/// 
/// </summary>
/// <param name="oldFd"></param>
/// <param name="newFd"></param>
/// <returns></returns>
int dup2(int oldFd, int newFd);

#endif // UNISTD_H
