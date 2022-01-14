@echo off

echo:
echo:
echo *********************************************************************
echo * ImageGlass.Base Post-build script
echo *********************************************************************
echo:
echo:


echo Arguments: -----------------------------------------------------------
echo "1: %1"
echo "CD: %CD%"
echo:
echo:


echo Arguments: -----------------------------------------------------------
echo "1: %1"
echo "2: %2"
echo "CD: %CD%"
echo:
echo:


echo 1. Creating variables:
echo -----------------------------------------------------------------------
set SOLUTION_DIR=%1
set BIN_FILE=%2
echo SOLUTION_DIR: %SOLUTION_DIR%
echo BIN_FILE: %BIN_FILE%
echo:
echo:


echo 2. Signing binary files
echo -----------------------------------------------------------------------
set TOOL=%SOLUTION_DIR%..\Tools\CodeSigning\signtool.exe
echo TOOL: %TOOL%
echo:

%TOOL% sign /fd sha256 /tr http://ts.ssl.com /td sha256 /n "Duong Dieu Phap" /a %BIN_FILE%
%TOOL% verify /pa %BIN_FILE%
echo:
echo:
