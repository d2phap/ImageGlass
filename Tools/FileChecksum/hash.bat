
@echo off

echo:
echo **********************************************
echo *          ImageGlass File checksum          *
echo **********************************************
echo:
echo:

set VERSION=8.8.3.28
set PATHS[0]="../../Setup/AdvancedInstaller/Bin/ImageGlass_Kobe_%VERSION%_x64.msi"
set PATHS[1]="../../Setup/AdvancedInstaller/Bin/ImageGlass_Kobe_%VERSION%_x86.msi"
set PATHS[2]="../../Setup/AdvancedInstaller/Bin/ImageGlass_Kobe_%VERSION%_x64.zip"
set PATHS[3]="../../Setup/AdvancedInstaller/Bin/ImageGlass_Kobe_%VERSION%_x86.zip"


set x=0
:SymLoop
if defined PATHS[%x%] (
    call echo __________________________________________________________________________________________________________
    call echo [%x%]. Hashing %%PATHS[%x%]%%
    call echo:
    call fciv.exe -sha1 -add %%PATHS[%x%]%%
    call echo:
    call echo:
    call echo:

    set /a "x+=1"
    GOTO :SymLoop
)

echo:
echo:
pause
