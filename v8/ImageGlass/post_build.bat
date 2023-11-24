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
echo "CD: %CD%"
echo:
echo:


set SOLUTION_DIR=%1
:: Debug, Release, Release_MSIX
set BUILD_CONFIG=%2
:: x64, x86, ...
set PLATFORM=%3
set OUTPUT_DIR=%CD%\
set IGCMD_WIN10_DIR=%OUTPUT_DIR:\ImageGlass\bin\=\Ultilities\igcmdWin10\bin\%
set THEME_DIR=%SOLUTION_DIR%..\Setup\v8\Assets\Themes\


echo 1. Creating variables:
echo -----------------------------------------------------------------------
echo SOLUTION_DIR: %SOLUTION_DIR%
echo BUILD_CONFIG: %BUILD_CONFIG%
echo PLATFORM: %PLATFORM%
echo OUTPUT_DIR: %OUTPUT_DIR%
echo IGCMD_WIN10_DIR: %IGCMD_WIN10_DIR%
echo THEME_DIR: %THEME_DIR%
echo:
echo:


echo 2. Copy files from %IGCMD_WIN10_DIR%
echo -----------------------------------------------------------------------
IF NOT %BUILD_CONFIG% == Release_MSIX (
  echo Copying files...
  xcopy /Q /K /D /H /Y %IGCMD_WIN10_DIR%*.* .\
) ELSE (
  echo Skip!
)
echo:
echo:


echo 3. Copy files from %THEME_DIR% if not exist
echo -----------------------------------------------------------------------
IF NOT EXIST Themes\ (
	mkdir Themes
	xcopy /Q /K /D /H /E /Y %THEME_DIR%*.* Themes
) ELSE (
  echo Skip!
)
echo:
echo:
