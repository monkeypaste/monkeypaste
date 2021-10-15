#pragma once

#include "resource.h"

LRESULT APIENTRY MainWndProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
BOOL WINAPI IsDisplayableFormat(UINT uFormat);
LPCSTR GetPredefinedClipboardFormatName(UINT uFormat);
void WINAPI InitMenu(HWND hwnd, HMENU hmenu);
void WINAPI SetAutoView(HWND hwnd);
