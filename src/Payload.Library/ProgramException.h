#pragma once
#include "stdafx.h"

class ProgramException : public runtime_error 
{
public:
    ProgramException( char* message ) : runtime_error(message), m_Message(message), m_ErrorCode(GetLastError()) { }
    ProgramException( char* message, DWORD errorCode ) : runtime_error(message), m_Message(message), m_ErrorCode(errorCode) { }

    virtual const char * what() const;

private:
    mutable string m_Info;
    char* m_Message;
    DWORD m_ErrorCode;
};