
:: if Platform is X64
IF NOT EXIST ..\..\..\Ultilities\igcmdWin10\bin\Release\ (
	xcopy /Q /K /D /H /Y ..\..\..\..\Ultilities\igcmdWin10\bin\Release\*.* .\
)
:: Platform is AnyCPU
ELSE (
	xcopy /Q /K /D /H /Y ..\..\..\Ultilities\igcmdWin10\bin\Release\*.* .\
)


IF NOT EXIST DefaultTheme\ (

	mkdir DefaultTheme

	:: if Platform is X64
	IF NOT EXIST ..\..\..\..\Assets\Setup\DefaultTheme\ (
		xcopy /Q /K /D /H /Y ..\..\..\..\..\Assets\Setup\DefaultTheme\*.* DefaultTheme
	)
	:: Platform is AnyCPU
	ELSE (
		xcopy /Q /K /D /H /Y ..\..\..\..\Assets\Setup\DefaultTheme\*.* DefaultTheme
	)
)

