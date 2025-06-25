// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "ec.hpp"

EmbeddedController ec;

extern "C" BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        ec = EmbeddedController();
        if (ec.driverFileExist && ec.driverLoaded) {
            return TRUE;
        }
        else {
            return FALSE;
        }
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
        ec.close();
        break;
    }
    return TRUE;
}

