echo "1: %1"
echo "CD: %CD%"

set SOLUTION_DIR=%1
set OUTPUT_DIR=%CD%
set IGCMD_DIR=%OUTPUT_DIR:\ImageGlass\bin\=\Ultilities\igcmdWin10\bin\%
set THEME_DIR=%SOLUTION_DIR%..\Setup\Assets\Themes

echo SOLUTION_DIR: %SOLUTION_DIR%
echo OUTPUT_DIR: %OUTPUT_DIR%
echo IGCMD_DIR: %IGCMD_DIR%
echo THEME_DIR: %THEME_DIR%

echo Copy from %IGCMD_DIR%
xcopy /Q /K /D /H /Y %IGCMD_DIR%\*.* .\

IF NOT EXIST Themes\ (

	mkdir Themes
	echo Copy from %THEME_DIR%
	xcopy /Q /K /D /H /E /Y %THEME_DIR%\*.* Themes
)

