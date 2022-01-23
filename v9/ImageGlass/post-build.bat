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
set DEFAULT_CODEC=%OUTPUT_DIR:\ImageGlass\bin\=\IgCodecs\ImageMagickIgCodec\bin\%ImageMagick.IgCodec.dll
set THEME_DIR=%SOLUTION_DIR%..\Setup\Assets\Themes\


echo 1. Creating variables:
echo -----------------------------------------------------------------------
echo SOLUTION_DIR: %SOLUTION_DIR%
echo OUTPUT_DIR: %OUTPUT_DIR%
echo DEFAULT_CODEC: %DEFAULT_CODEC%
echo THEME_DIR: %THEME_DIR%
echo:
echo:


echo 2. Copy default codec: %DEFAULT_CODEC%
echo -----------------------------------------------------------------------
xcopy /Q /K /D /H /Y %DEFAULT_CODEC% %OUTPUT_DIR%\Codecs\
echo:
echo:


echo 3. Copy default theme: %THEME_DIR% (if not exist)
echo -----------------------------------------------------------------------
IF NOT EXIST Themes\ (
	mkdir Themes
	xcopy /Q /K /D /H /E /Y %THEME_DIR%*.* %OUTPUT_DIR%\Themes\
)
echo:
echo:
