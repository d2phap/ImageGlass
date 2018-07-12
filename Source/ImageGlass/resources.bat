IF NOT EXIST DefaultTheme\ (

	mkdir DefaultTheme
	xcopy /Q ..\..\..\..\Setup\DefaultTheme\*.* DefaultTheme
)