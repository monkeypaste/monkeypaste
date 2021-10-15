// ClipboardListener.cpp : Defines the entry point for the application.
//

#include "framework.h"
#include "ClipboardListener.h"

#define MAX_LOADSTRING 100
#define IDM_AUTO    6

HINSTANCE hinst;
UINT uFormat = (UINT)(-1);
BOOL fAuto = TRUE;

//LRESULT APIENTRY MainWndProc(hwnd, uMsg, wParam, lParam)
//HWND hwnd;
//UINT uMsg;
//WPARAM wParam;
//LPARAM lParam;
LRESULT APIENTRY MainWndProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
    static HWND hwndNextViewer;

    HDC hdc;
    HDC hdcMem;
    PAINTSTRUCT ps;
    LPPAINTSTRUCT lpps;
    RECT rc;
    LPRECT lprc;
    HGLOBAL hglb;
    LPSTR lpstr;
    HBITMAP hbm;
    HENHMETAFILE hemf;
    HWND hwndOwner;

    switch (uMsg)
    {
    case WM_PAINT:
        hdc = BeginPaint(hwnd, &ps);

        // Branch depending on the clipboard format. 

        switch (uFormat)
        {
        case CF_OWNERDISPLAY:
            hwndOwner = GetClipboardOwner();
            hglb = GlobalAlloc(GMEM_MOVEABLE,
                sizeof(PAINTSTRUCT));
            lpps = (LPPAINTSTRUCT)GlobalLock(hglb);
            memcpy(lpps, &ps, sizeof(PAINTSTRUCT));
            GlobalUnlock(hglb);

            SendMessage(hwndOwner, WM_PAINTCLIPBOARD,
                (WPARAM)hwnd, (LPARAM)hglb);

            GlobalFree(hglb);
            break;

        case CF_BITMAP:
            hdcMem = CreateCompatibleDC(hdc);
            if (hdcMem != NULL)
            {
                if (OpenClipboard(hwnd))
                {
                    hbm = (HBITMAP)
                        GetClipboardData(uFormat);
                    SelectObject(hdcMem, hbm);
                    GetClientRect(hwnd, &rc);

                    BitBlt(hdc, 0, 0, rc.right, rc.bottom,
                        hdcMem, 0, 0, SRCCOPY);
                    CloseClipboard();
                }
                DeleteDC(hdcMem);
            }
            break;

        case CF_TEXT:
            if (OpenClipboard(hwnd))
            {
                hglb = GetClipboardData(uFormat);
                lpstr = (LPSTR)GlobalLock(hglb);

                GetClientRect(hwnd, &rc);
                DrawText(hdc, (LPCWSTR)lpstr, -1, &rc, DT_LEFT);

                GlobalUnlock(hglb);
                CloseClipboard();
            }
            break;

        case CF_ENHMETAFILE:
            if (OpenClipboard(hwnd))
            {
                hemf = (HENHMETAFILE)GetClipboardData(uFormat);
                GetClientRect(hwnd, &rc);
                PlayEnhMetaFile(hdc, hemf, &rc);
                CloseClipboard();
            }
            break;

        case 0:
            GetClientRect(hwnd, &rc);
            DrawText(hdc, L"The clipboard is empty.", -1,
                &rc, DT_CENTER | DT_SINGLELINE |
                DT_VCENTER);
            break;

        default:
            GetClientRect(hwnd, &rc);
            DrawText(hdc, L"Unable to display format.", -1,
                &rc, DT_CENTER | DT_SINGLELINE |
                DT_VCENTER);
        }
        EndPaint(hwnd, &ps);
        break;

    case WM_SIZE:
        if (uFormat == CF_OWNERDISPLAY)
        {
            hwndOwner = GetClipboardOwner();
            hglb = GlobalAlloc(GMEM_MOVEABLE, sizeof(RECT));
            lprc = (LPRECT)GlobalLock(hglb);
            GetClientRect(hwnd, lprc);
            GlobalUnlock(hglb);

            SendMessage(hwndOwner, WM_SIZECLIPBOARD,
                (WPARAM)hwnd, (LPARAM)hglb);

            GlobalFree(hglb);
        }
        break;

    case WM_CREATE:

        // Add the window to the clipboard viewer chain. 

        hwndNextViewer = SetClipboardViewer(hwnd);
        break;

    case WM_CHANGECBCHAIN:

        // If the next window is closing, repair the chain. 

        if ((HWND)wParam == hwndNextViewer)
            hwndNextViewer = (HWND)lParam;

        // Otherwise, pass the message to the next link. 

        else if (hwndNextViewer != NULL)
            SendMessage(hwndNextViewer, uMsg, wParam, lParam);

        break;

    case WM_DESTROY:
        ChangeClipboardChain(hwnd, hwndNextViewer);
        PostQuitMessage(0);
        break;

    case WM_DRAWCLIPBOARD:  // clipboard contents changed. 

        // Update the window by using Auto clipboard format. 

        SetAutoView(hwnd);

        // Pass the message to the next window in clipboard 
        // viewer chain. 

        SendMessage(hwndNextViewer, uMsg, wParam, lParam);
        break;

    case WM_INITMENUPOPUP:
        if (!HIWORD(lParam))
            InitMenu(hwnd, (HMENU)wParam);
        break;

    case WM_COMMAND:
        switch (LOWORD(wParam))
        {
        case IDM_EXIT:
            DestroyWindow(hwnd);
            break;

        /*case IDM_AUTO:
            SetAutoView(hwnd);
            break;*/

        default:
            fAuto = FALSE;
            uFormat = LOWORD(wParam);
            InvalidateRect(hwnd, NULL, TRUE);
        }
        break;

    default:
        return DefWindowProc(hwnd, uMsg, wParam, lParam);
    }
    return (LRESULT)NULL;
}

void WINAPI SetAutoView(HWND hwnd)
{
    static UINT auPriorityList[] = {
        CF_OWNERDISPLAY,
        CF_TEXT,
        CF_ENHMETAFILE,
        CF_BITMAP
    };

    uFormat = GetPriorityClipboardFormat(auPriorityList, 4);
    fAuto = TRUE;

    InvalidateRect(hwnd, NULL, TRUE);
    UpdateWindow(hwnd);
}

void WINAPI InitMenu(HWND hwnd, HMENU hmenu)
{
    UINT uFormat;
    char szFormatName[80];
    LPCSTR lpFormatName;
    UINT fuFlags;
    UINT idMenuItem;

    // If a menu is not the display menu, no initialization is necessary. 

    if (GetMenuItemID(hmenu, 0) != IDM_AUTO)
        return;

    // Delete all menu items except the first. 

    while (GetMenuItemCount(hmenu) > 1)
        DeleteMenu(hmenu, 1, MF_BYPOSITION);

    // Check or uncheck the Auto menu item. 

    fuFlags = fAuto ? MF_BYCOMMAND | MF_CHECKED :
        MF_BYCOMMAND | MF_UNCHECKED;
    CheckMenuItem(hmenu, IDM_AUTO, fuFlags);

    // If there are no clipboard formats, return. 

    if (CountClipboardFormats() == 0)
        return;

    // Open the clipboard. 

    if (!OpenClipboard(hwnd))
        return;

    // Add a separator and then a menu item for each format. 

    AppendMenu(hmenu, MF_SEPARATOR, 0, NULL);
    uFormat = EnumClipboardFormats(0);

    while (uFormat)
    {
        // Call an application-defined function to get the name 
        // of the clipboard format. 

        lpFormatName = GetPredefinedClipboardFormatName(uFormat);

        // For registered formats, get the registered name. 

        if (lpFormatName == NULL)
        {

            // Note that, if the format name is larger than the
            // buffer, it is truncated. 
            if (GetClipboardFormatName(uFormat, (LPWSTR)szFormatName,
                sizeof(szFormatName)))
                lpFormatName = szFormatName;
            else
                lpFormatName = "(unknown)";
        }

        // Add a menu item for the format. For displayable 
        // formats, use the format ID for the menu ID. 

        if (IsDisplayableFormat(uFormat))
        {
            fuFlags = MF_STRING;
            idMenuItem = uFormat;
        }
        else
        {
            fuFlags = MF_STRING | MF_GRAYED;
            idMenuItem = 0;
        }
        AppendMenu(hmenu, fuFlags, idMenuItem, (LPCWSTR)lpFormatName);

        uFormat = EnumClipboardFormats(uFormat);
    }
    CloseClipboard();

}

LPCSTR GetPredefinedClipboardFormatName(UINT uFormat)
{
    return NULL;
}

BOOL WINAPI IsDisplayableFormat(UINT uFormat)
{
    switch (uFormat)
    {
    case CF_OWNERDISPLAY:
    case CF_TEXT:
    case CF_ENHMETAFILE:
    case CF_BITMAP:
        return TRUE;
    }
    return FALSE;
}
