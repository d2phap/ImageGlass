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
echo "CD: %CD%"
echo:
echo:


set SOLUTION_DIR=%1
set OUTPUT_DIR=%2
set THEME_DIR=%SOLUTION_DIR%..\Setup\v9\Assets\Themes\


echo 1. Creating variables:
echo -----------------------------------------------------------------------
echo SOLUTION_DIR: %SOLUTION_DIR%
echo OUTPUT_DIR: %OUTPUT_DIR%
echo THEME_DIR: %THEME_DIR%
echo:
echo:


echo 3. Copy default theme: %THEME_DIR% (if not exist)
echo -----------------------------------------------------------------------
IF NOT EXIST %OUTPUT_DIR%Themes (
	mkdir %OUTPUT_DIR%Themes
	xcopy /E /H /C /I %THEME_DIR% %OUTPUT_DIR%Themes
)
echo:
echo:
