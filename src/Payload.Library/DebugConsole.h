#pragma once
#include "stdafx.h"

/// 
/// Debug console to capture cout, etc from GUI applications
/// Original from http://blog.signalsondisplay.com/?p=85
/// 
class DebugConsole
{
public:
    DebugConsole();
    ~DebugConsole();

private:
    streambuf* _cinbuf;
    streambuf* _coutbuf;
    streambuf* _cerrbuf;
    ifstream _console_cin;
    ofstream _console_cout;
    ofstream _console_cerr;

    void Open();
    void Close();
};
