
@echo off

set FILE=%1

echo:
echo *********************************************************************
echo * ImageGlass Code Signing tool v8.4
echo * https://imageglass.org
echo *
echo * https://www.ssl.com/how-to/using-your-code-signing-certificate
echo *********************************************************************
echo:
echo:


:: C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe
set TOOL="signtool.exe"

call %TOOL% sign /fd sha256 /tr http://timestamp.sectigo.com /td sha256 /n "Duong Dieu Phap" /a %FILE%
call %TOOL% verify /pa %FILE%


echo:
echo:
pause
