
IF NOT EXIST ..\..\..\Ultilities\igcmdWin10\bin\Release\ (
	xcopy /Q /K /D /H /Y ..\..\..\..\Ultilities\igcmdWin10\bin\Release\*.* .\
) ELSE (
	xcopy /Q /K /D /H /Y ..\..\..\Ultilities\igcmdWin10\bin\Release\*.* .\
)


IF NOT EXIST DefaultTheme\ (

	mkdir DefaultTheme

	IF NOT EXIST ..\..\..\..\Assets\Setup\DefaultTheme\ (
		xcopy /Q /K /D /H /Y ..\..\..\..\..\Assets\Setup\DefaultTheme\*.* DefaultTheme
	)
	ELSE (
		xcopy /Q /K /D /H /Y ..\..\..\..\Assets\Setup\DefaultTheme\*.* DefaultTheme
	)
)

