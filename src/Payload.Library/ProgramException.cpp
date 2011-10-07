#include "stdafx.h"
#include "ProgramException.h"

const char* ProgramException::what() const
{
    // Convert error code to string
    stringstream errorCode;
    errorCode << m_ErrorCode;

    // Format message
    m_Info = m_Message;
    m_Info += " ";
    m_Info += errorCode.str();

    return m_Info.c_str();
}
