#pragma once
#include "stdafx.h"

class WinApiException : public runtime_error 
{
public:
    WinApiException( char* message ) : runtime_error(message), m_Message(message), m_ErrorCode(GetLastError()) { }
    WinApiException( char* message, DWORD errorCode ) : runtime_error(message), m_Message(message), m_ErrorCode(errorCode) { }

    virtual const char * what() const;

private:
    mutable string m_Info;
    char* m_Message;
    DWORD m_ErrorCode;
};