xcopy /Q /K /D /H /Y ..\..\..\Ultilities\igcmdWin10\bin\Release\*.* .\

IF NOT EXIST DefaultTheme\ (

	mkdir DefaultTheme
	xcopy /Q /K /D /H /Y ..\..\..\..\Assets\Setup\DefaultTheme\*.* DefaultTheme
)

