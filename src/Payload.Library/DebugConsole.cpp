#include "StdAfx.h"
#include "DebugConsole.h"

DebugConsole::DebugConsole() :
    _cinbuf(0), _coutbuf(0), _cerrbuf(0)
{
    Open();
}

DebugConsole::~DebugConsole() 
{
    Close();
}

void DebugConsole::Open()
{
    AllocConsole();
    AttachConsole(GetCurrentProcessId());

    _cinbuf = std::cin.rdbuf();
    _console_cin.open("CONIN$");
    std::cin.rdbuf(_console_cin.rdbuf());

    _coutbuf = std::cout.rdbuf();
    _console_cout.open("CONOUT$");
    std::cout.rdbuf(_console_cout.rdbuf());

    _cerrbuf = std::cerr.rdbuf();
    _console_cerr.open("CONOUT$");
    std::cerr.rdbuf(_console_cerr.rdbuf());
}

void DebugConsole::Close()
{
    _console_cout.close();
    std::cout.rdbuf(_coutbuf);

    _console_cin.close();
    std::cin.rdbuf(_cinbuf);

    _console_cerr.close();
    std::cerr.rdbuf(_cerrbuf);

    FreeConsole();
}
