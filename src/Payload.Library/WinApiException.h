#pragma once
#include "stdafx.h"

class WinApiException : public runtime_error 
{
public:
    WinApiException( char* message ) : runtime_error(message), m_Message(message), m_ErrorCode(GetLastError()) { }
    WinApiException( char* message, DWORD errorCode ) : runtime_error(message), m_Message(message), m_ErrorCode(errorCode) { }

    virtual const char * what() const
    {
        // Convert error code to string
        stringstream errorCode;
        errorCode << m_ErrorCode;

        // Format message
        m_Info = m_Message;
        m_Info += "; error ";
        m_Info += errorCode.str();

        return m_Info.c_str();
    }


private:
    mutable string m_Info;
    char* m_Message;
    DWORD m_ErrorCode;
};