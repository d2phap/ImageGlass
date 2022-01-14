
@echo off

echo:
echo *********************************************************************
echo * ImageGlass Code signing
echo * https://www.ssl.com/how-to/using-your-code-signing-certificate
echo *********************************************************************
echo:
echo:


set PLATFORM=x64
set PATHS[0]="..\..\Source\ImageGlass\bin\%PLATFORM%\Release\ImageGlass.exe"
set PATHS[1]="..\..\Source\ImageGlass\bin\%PLATFORM%\Release\igcmd.exe"
set PATHS[2]="..\..\Source\ImageGlass\bin\%PLATFORM%\Release\igcmdWin10.exe"
set PATHS[3]="..\..\Source\ImageGlass\bin\%PLATFORM%\Release\igtasks.exe"

set PATHS[4]="..\..\Source\ImageGlass\bin\%PLATFORM%\Release\ImageGlass.Base.dll"
set PATHS[5]="..\..\Source\ImageGlass\bin\%PLATFORM%\Release\ImageGlass.Heart.dll"
set PATHS[6]="..\..\Source\ImageGlass\bin\%PLATFORM%\Release\ImageGlass.ImageBox.dll"
set PATHS[7]="..\..\Source\ImageGlass\bin\%PLATFORM%\Release\ImageGlass.ImageListView.dll"
set PATHS[8]="..\..\Source\ImageGlass\bin\%PLATFORM%\Release\ImageGlass.Library.dll"
set PATHS[9]="..\..\Source\ImageGlass\bin\%PLATFORM%\Release\ImageGlass.Services.dll"
set PATHS[10]="..\..\Source\ImageGlass\bin\%PLATFORM%\Release\ImageGlass.Settings.dll"
set PATHS[11]="..\..\Source\ImageGlass\bin\%PLATFORM%\Release\ImageGlass.UI.dll"


:: C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe
set TOOL="signtool.exe"
set x=0


:SymLoop
if defined PATHS[%x%] (
    call echo _________________________________________________________________________________________
    call echo [%x%].
    call echo:
    call %TOOL% sign /fd sha256 /tr http://ts.ssl.com /td sha256 /n "Duong Dieu Phap" /a %%PATHS[%x%]%%
    call %TOOL% verify /pa %%PATHS[%x%]%%
    call echo:
    call echo:
    call echo:

    set /a "x+=1"
    GOTO :SymLoop
)

echo:
echo:
pause
