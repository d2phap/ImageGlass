
@echo off

:: Platform: x64 or x86
set PLATFORM=%1

:: Config modes: Release or Debug
set CONFIG_MODE=%2


echo:
echo *********************************************************************
echo * ImageGlass Code Signing tool v8.4
echo * https://imageglass.org
echo *
echo * PLATFORM: %PLATFORM%
echo * CONFIG_MODE: %CONFIG_MODE%
echo *
echo * https://www.ssl.com/how-to/using-your-code-signing-certificate
echo *********************************************************************
echo:
echo:


set PATH=..\..\Source\ImageGlass\bin\%PLATFORM%\%CONFIG_MODE%

:: Executable files
set FILES[0]=ImageGlass.exe
set FILES[1]=igcmd.exe
set FILES[2]=igcmdWin10.exe
set FILES[3]=igtasks.exe

:: Library files
set FILES[4]=ImageGlass.Base.dll
set FILES[5]=ImageGlass.Heart.dll
set FILES[6]=ImageGlass.ImageBox.dll
set FILES[7]=ImageGlass.ImageListView.dll
set FILES[8]=ImageGlass.Library.dll
set FILES[9]=ImageGlass.Services.dll
set FILES[10]=ImageGlass.Settings.dll
set FILES[11]=ImageGlass.UI.dll
set FILES[12]=ImageGlass.WebP.dll

set FILES[13]=bs-sdk.dll
set FILES[14]=FileWatcherEx.dll
set FILES[15]=ExplorerSortOrder.dll


:: C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe
set TOOL="signtool.exe"
set x=0


:SymLoop
if defined FILES[%x%] (
    call echo _________________________________________________________________________________________
    call echo [%x%]. %%FILES[%x%]%%
    call echo:
    call %TOOL% sign /fd sha256 /tr http://ts.ssl.com /td sha256 /n "Duong Dieu Phap" /a %PATH%\%%FILES[%x%]%%
    call %TOOL% verify /pa %PATH%\%%FILES[%x%]%%
    call echo:
    call echo:
    call echo:

    set /a "x+=1"
    GOTO :SymLoop
)

echo:
echo:
pause
