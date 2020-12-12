/*
	A simple system proxy control program. Reference document:
	https://docs.microsoft.com/en-us/windows/desktop/wininet/wininet-vs-winhttp

	xinlake@outlook.com
*/

#include <windows.h>
#include <Wininet.h>

#pragma comment(lib, "Wininet.lib")

#define PROXYCTRL_EXPORT extern "C" __declspec(dllexport)

static int refreshSetting();


/*
	Reference code:
	https://docs.microsoft.com/en-us/windows/desktop/WinInet/setting-and-retrieving-internet-options
*/
PROXYCTRL_EXPORT BOOL EnableProxy(WCHAR* pServer, WCHAR* pBypass)
{
	INTERNET_PER_CONN_OPTION_LIST list;
	DWORD   dwBufSize = sizeof(list);
	BOOL    bReturn;

	list.dwSize = sizeof(list); // Fill the list structure.	
	list.pszConnection = NULL; // NULL == LAN, otherwise connectoid name.

	// Set three options.
	list.dwOptionCount = 3;
	list.pOptions = new INTERNET_PER_CONN_OPTION[3];
	if (NULL == list.pOptions)
	{
		// Return FALSE if the memory wasn't allocated.
		return FALSE;
	}

	// Set flags.
	list.pOptions[0].dwOption = INTERNET_PER_CONN_FLAGS;
	list.pOptions[0].Value.dwValue = PROXY_TYPE_DIRECT | PROXY_TYPE_PROXY;

	// Set proxy name.
	list.pOptions[1].dwOption = INTERNET_PER_CONN_PROXY_SERVER;
	list.pOptions[1].Value.pszValue = pServer;

	// Set proxy override.
	list.pOptions[2].dwOption = INTERNET_PER_CONN_PROXY_BYPASS;
	list.pOptions[2].Value.pszValue = pBypass;

	// Set the options on the connection.
	bReturn = InternetSetOption(NULL, INTERNET_OPTION_PER_CONNECTION_OPTION, &list, dwBufSize);
	refreshSetting();

	// Free the allocated memory.
	delete[] list.pOptions;
	return bReturn;
}

PROXYCTRL_EXPORT BOOL DisableProxy()
{
	INTERNET_PER_CONN_OPTION_LIST list;
	DWORD   dwBufSize = sizeof(list);
	BOOL    bReturn;

	list.dwSize = sizeof(list);// Fill the list structure.
	list.pszConnection = NULL; // NULL == LAN, otherwise connectoid name.

	// Set options.
	list.dwOptionCount = 1;
	list.pOptions = new INTERNET_PER_CONN_OPTION;
	if (NULL == list.pOptions)
	{
		// Return FALSE if the memory wasn't allocated.
		return FALSE;
	}

	list.pOptions[0].dwOption = INTERNET_PER_CONN_FLAGS;
	list.pOptions[0].Value.dwValue = PROXY_TYPE_AUTO_DETECT | PROXY_TYPE_DIRECT;

	bReturn = InternetSetOption(NULL, INTERNET_OPTION_PER_CONNECTION_OPTION, &list, dwBufSize);
	refreshSetting();

	delete list.pOptions;
	return bReturn;
}


/*
	Flags:
	https://docs.microsoft.com/en-us/windows/desktop/WinInet/option-flags
*/
static BOOL refreshSetting()
{
	BOOL result;

	// Alerts the current WinInet instance that proxy settings have changed and that they must update with the new settings
	result = InternetSetOption(NULL, INTERNET_OPTION_PROXY_SETTINGS_CHANGED, NULL, 0);
	if (!result) return FALSE;

	// Causes the proxy data to be reread from the registry
	result = InternetSetOption(NULL, INTERNET_OPTION_REFRESH, NULL, 0);
	if (!result) return FALSE;

	return TRUE;
}
