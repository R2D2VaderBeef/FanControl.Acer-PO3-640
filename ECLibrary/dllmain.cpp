// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "ec.hpp"

EmbeddedController ec;
BOOL ready = false;

extern "C" BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

extern "C" {
    __declspec(dllexport) void Setup() {
        ec = EmbeddedController();
        ec.endianness = BIG_ENDIAN;
        if (ec.driverFileExist && ec.driverLoaded) {
            ready = TRUE;
        }
        else {
            ready = FALSE;
        }
    }

    __declspec(dllexport) void Shutdown() {
        if (ready == TRUE) {
            ec.close();
        }
    }

    __declspec(dllexport) WORD ReadWord(BYTE _register) {
        if (ready == TRUE) {
            return ec.readWord(_register);
        }
        else return 0x0000;
    }

    __declspec(dllexport) BYTE WriteWord(BYTE _register, WORD _value) {
        if (ready == TRUE) {
            bool result = ec.writeWord(_register, _value);
            if (result == true) {
                return 0x01;
            }
            else return 0x00;
        }
        else return 0x00;
    }
}