@echo off

echo:
echo:
echo *********************************************************************
echo * ImageGlass Post-build script
echo *********************************************************************
echo:
echo:


echo Arguments: -----------------------------------------------------------
echo "1: %1"
echo "2: %2"
echo "3: %3"
echo "4: %4"
echo "CD: %CD%"
echo:
echo:


set SOLUTION_DIR=%1
set OUTPUT_DIR=%2
set BUILD_TYPE=%3
set PLATFORM=%4
set IGCMD10_DIR=%SOLUTION_DIR%Utilities\igcmd10\bin\%PLATFORM%\%BUILD_TYPE%\net6.0-windows10.0.19041.0\
set THEME_DIR=%SOLUTION_DIR%..\Setup\v9\Assets\Themes\


echo ######## 1. Creating variables:
echo -----------------------------------------------------------------------
echo SOLUTION_DIR: %SOLUTION_DIR%
echo OUTPUT_DIR: %OUTPUT_DIR%
echo BUILD_TYPE: %BUILD_TYPE%
echo PLATFORM: %PLATFORM%
echo IGCMD10_DIR: %IGCMD10_DIR%
echo THEME_DIR: %THEME_DIR%
echo:
echo:


echo ######## 2. IGCMD10_DIR: Copy files from %IGCMD10_DIR%
echo -----------------------------------------------------------------------
xcopy /Q /K /D /H /Y %IGCMD10_DIR%*.* %OUTPUT_DIR%
echo:
echo:


echo ######## 3. THEME_DIR: Copy default theme: %THEME_DIR% (if not exist)
echo -----------------------------------------------------------------------
IF NOT EXIST %OUTPUT_DIR%Themes (
	mkdir %OUTPUT_DIR%Themes
	xcopy /E /H /C /I %THEME_DIR% %OUTPUT_DIR%Themes
)
echo:
echo:
