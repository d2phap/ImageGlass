
Imports System.IO

Public Class ImageGlass_IniFile

    Sub New()
        ' TODO: Complete member initialization 
    End Sub

    Private Declare Ansi Function GetPrivateProfileString Lib "kernel32.dll" Alias "GetPrivateProfileStringA" _
    (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpDefault As String, _
    ByVal lpReturnedString As System.Text.StringBuilder, ByVal nSize As Integer, ByVal lpFileName As String) As Integer

    Private Declare Ansi Function WritePrivateProfileString Lib "kernel32.dll" Alias "WritePrivateProfileStringA" _
    (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpString As String, ByVal lpFileName As String) As Integer

    Private Declare Ansi Function GetPrivateProfileInt Lib "kernel32.dll" Alias "GetPrivateProfileIntA" _
    (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal nDefault As Integer, ByVal lpFileName As String) As Integer

    Private Declare Ansi Function FlushPrivateProfileString Lib "kernel32.dll" Alias "WritePrivateProfileStringA" _
    (ByVal lpApplicationName As Integer, ByVal lpKeyName As Integer, ByVal lpString As Integer, ByVal lpFileName As String) As Integer

    Dim strFilename As String
    ' Constructor, accepting a filename
    Public Sub New(ByVal Filename As String)
        strFilename = Filename
    End Sub

    ' Read-only filename property
    ReadOnly Property FileName() As String
        Get
            Return strFilename
        End Get
    End Property

    Public Function GetString(ByVal Section As String, ByVal Key As String, ByVal [Default] As String) As String
        ' Returns a string from your INI file
        On Error Resume Next
        Dim intCharCount As Integer
        Dim objResult As New System.Text.StringBuilder(256)
        intCharCount = GetPrivateProfileString(Section, Key, [Default], objResult, objResult.Capacity, strFilename)
        If intCharCount > 0 Then GetString = Left(objResult.ToString, intCharCount)
    End Function

    Public Function GetInteger(ByVal Section As String, ByVal Key As String, ByVal [Default] As Integer) As Integer
        ' Returns an integer from your INI file
        On Error Resume Next
        Return GetPrivateProfileInt(Section, Key, [Default], strFilename)
    End Function

    Public Function GetBoolean(ByVal Section As String, ByVal Key As String, ByVal [Default] As Boolean) As Boolean
        ' Returns a boolean from your INI file
        On Error Resume Next
        Return (GetPrivateProfileInt(Section, Key, CInt([Default]), strFilename) = 1)
    End Function

    Public Sub WriteString(ByVal Section As String, ByVal Key As String, ByVal Value As String)
        ' Writes a string to your INI file
        On Error Resume Next
        WritePrivateProfileString(Section, Key, Value, strFilename)
        Flush()
    End Sub

    Public Sub WriteInteger(ByVal Section As String, ByVal Key As String, ByVal Value As Integer)
        ' Writes an integer to your INI file
        On Error Resume Next
        WriteString(Section, Key, CStr(Value))
        Flush()
    End Sub

    Public Sub WriteBoolean(ByVal Section As String, ByVal Key As String, ByVal Value As Boolean)
        ' Writes a boolean to your INI file
        On Error Resume Next
        WriteString(Section, Key, CStr(CInt(Value)))
        Flush()
    End Sub

    Private Sub Flush()
        On Error Resume Next
        ' Stores all the cached changes to your INI file
        FlushPrivateProfileString(0, 0, 0, strFilename)
    End Sub

    Public Sub WriteINI_MultiKey(ByVal MainKey As String, ByVal SubKey As String, _
    ByVal Value As String, Optional ByVal dequy As Boolean = False)

        Dim flg As Boolean
        Dim mMainKey As String
        Dim mCount As Integer, i As Integer
        Dim T As Integer, K2 As String

        T = CInt(GetString(SubKey, "Total", "0"))

        '====================
        '= Code: xuanquy_th =
        '====================

        mCount = CInt(GetString(MainKey, "Count", 0))
        flg = ((mCount = 0) And dequy)

        For i = 1 To mCount

            mMainKey = GetString(MainKey, CStr(i), "")
            If UCase(mMainKey) <> UCase(MainKey) Then
                Call WriteINI_MultiKey(mMainKey, SubKey, Value, True) 'flg=true

                Call WriteString(SubKey, "Total", "1")

            End If

            flg = (flg Or (UCase(mMainKey) = UCase(SubKey)))

        Next i

        If Not flg Then 'flg=false

            mCount = mCount + 1
            Call WriteString(MainKey, "Count", CStr(mCount))
            Call WriteString(MainKey, CStr(mCount), SubKey)

        End If

        '====================
        '= Code: PhapSuXeko =
        '====================

        T = T + 1   'Total + 1

        If T = 1 Then Call WriteString(SubKey, "2", "")
        K2 = GetString(SubKey, "2", "")

        Call WriteString(SubKey, "Total", CStr(T))
        Call WriteString(SubKey, CStr(T), Value)


        If K2 <> "" Then Call WriteString(SubKey, "2", K2)

    End Sub

End Class
