IF NOT EXIST DefaultTheme\ (

	mkdir DefaultTheme
	xcopy /Q ..\..\..\..\Assets\Setup\DefaultTheme\*.* DefaultTheme
)