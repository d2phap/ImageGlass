IF NOT EXIST DefaultTheme\ (

	mkdir DefaultTheme
	xcopy /Q ..\..\..\..\Setup\DefaultTheme\*.* DefaultTheme
)

IF NOT EXIST 7z-32.dll (
	xcopy /Q ..\..\..\..\Setup\7z*.dll
)