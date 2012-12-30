
Imports System.Runtime.InteropServices
Imports System.Drawing

Public Class ImageGlass_ImageInfo

    <StructLayout(LayoutKind.Sequential)> Private Structure SHELLEXECUTEINFO
        Public cbSize As Integer
        Public fMask As Integer
        Public hwnd As IntPtr
        Public lpVerb As String
        Public lpFile As String
        Public lpParameters As String
        Public lpDirectory As String
        Public nShow As Integer
        Public hInstApp As IntPtr
        Public lpIDList As IntPtr
        Public lpClass As String
        Public hkeyClass As IntPtr
        Public dwHotKey As Integer
        Public hIcon As IntPtr
        Public hProcess As IntPtr
    End Structure

    <DllImport("shell32.dll", EntryPoint:="ShellExecuteEx")> _
    Private Shared Function ShellExecute(ByRef s As SHELLEXECUTEINFO) As Integer
    End Function

    ''' <summary>
    ''' Hiển thị hộp thoại Properties của tập tin, thư mục
    ''' </summary>
    ''' <param name="FileName"></param>
    ''' <param name="hwnd"></param>
    ''' <remarks></remarks>
    Public Shared Sub DisplayFileProperties(ByVal FileName As String, ByVal hwnd As IntPtr)
        Const SEE_MASK_INVOKEIDLIST As Integer = &HC
        Const SW_SHOW As Integer = 5
        Dim shInfo As New SHELLEXECUTEINFO()
        With shInfo
            .cbSize = Marshal.SizeOf(shInfo)
            .lpFile = FileName
            .nShow = SW_SHOW
            .fMask = SEE_MASK_INVOKEIDLIST
            .lpVerb = "properties"
            .hwnd = hwnd
        End With
        ShellExecute(shInfo)
    End Sub

    ''' <summary>
    ''' Lấy tên loại định dạng của hình ảnh
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetImageFileType(ByVal filename As String) As String

        Dim ext As String = IO.Path.GetExtension(filename).Replace(".", "").ToLower()
        Select Case ext
            Case "bmp" : Return "Bitmap Image File"
            Case "dib" : Return "Device Independent Bitmap File"
            Case "jpg" : Return "JPEG Image File"
            Case "jpeg" : Return "Joint Photographic Experts Group"
            Case "jfif" : Return "JPEG File Interchange Format"
            Case "jpe" : Return "JPEG Image File"
            Case "png" : Return "Portable Network Graphics"
            Case "gif" : Return "Graphics Interchange Format File"
            Case "ico" : Return "Icon File"
            Case "emf" : Return "Enhanced Windows Metafile"
            Case "exif" : Return "Exchangeable Image Information File"
            Case "wmf" : Return "Windows Metafile"
            Case "tif" : Return "Tagged Image File"
            Case "tiff" : Return "Tagged Image File Format"
            Case Else : Return ext.ToUpper & " File"
        End Select
    End Function

    ''' <summary>
    ''' Lấy dung lượng của tập tin kèm theo đơn vị
    ''' </summary>
    ''' <param name="Filename"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetFileSize(ByVal Filename As String) As String
        Try
            Dim Fi As New System.IO.FileInfo(Filename)
            Dim Size As Single = Math.Round(Fi.Length / 1024, 2)

            If Size < 1024 Then 'get the size in KB
                Return CStr(Size) & " KB"
            Else 'get the size in MB
                Return CStr(Math.Round(Size / 1024, 2)) & " MB"
            End If
        Catch ex As Exception
            Return " "
        End Try
    End Function

    ''' <summary>
    ''' Lấy kích thước của tập tin, bao gồm W x H
    ''' </summary>
    ''' <param name="Filename"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetWxHSize(ByVal Filename As String) As String
        Try
            If IO.Path.GetExtension(Filename).ToLower <> ".ico" Then
                Dim I As Image = Image.FromFile(Filename)
                'get Width x Height
                Return CStr(I.Width) & " x " & CStr(I.Height)
            Else
                Dim I As New Icon(Filename)
                'get Width x Height
                Return CStr(I.Width) & " x " & CStr(I.Height)
            End If
        Catch ex As Exception
            Return String.Empty
        End Try

    End Function

    ''' <summary>
    ''' Lấy độ phân giải ngang của hình ảnh
    ''' </summary>
    ''' <param name="Filename"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetHorizontalResolution(ByVal Filename As String) As Single
        Try
            If IO.Path.GetExtension(Filename).ToLower <> ".ico" Then
                Dim I As Image = Image.FromFile(Filename)
                'get HorizontalResolution 
                Return Math.Round(I.HorizontalResolution, 2)
            Else
                Dim I As New Icon(Filename)
                'get HorizontalResolution 
                Return Math.Round(I.ToBitmap.HorizontalResolution, 2)
            End If
        Catch ex As Exception
            Return Nothing
        End Try

    End Function

    ''' <summary>
    ''' Lấy độ phân giải dọc của hình ảnh
    ''' </summary>
    ''' <param name="Filename"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetVerticalResolution(ByVal Filename As String) As Single
        Try
            If IO.Path.GetExtension(Filename).ToLower <> ".ico" Then
                Dim I As Image = Image.FromFile(Filename)
                'get VerticalResolution 
                Return I.VerticalResolution
            Else
                Dim I As New Icon(Filename)
                'get VerticalResolution 
                Return I.ToBitmap.VerticalResolution
            End If
        Catch ex As Exception
            Return Nothing
        End Try

    End Function

    ''' <summary>
    ''' Lấy thời gian tạo ra tập tin
    ''' </summary>
    ''' <param name="Filename"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetCreateTime(ByVal Filename As String) As Date
        Dim Fi As New System.IO.FileInfo(Filename)

        'get Create Time
        Return Fi.CreationTime
    End Function

    ''' <summary>
    ''' Lấy thời gian truy cập mới nhất của tập tin
    ''' </summary>
    ''' <param name="Filename"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetLastAccess(ByVal Filename As String) As Date
        Dim Fi As New System.IO.FileInfo(Filename)
        'get Create Time
        Return Fi.LastAccessTime
    End Function

    ''' <summary>
    ''' Lấy thời gian ghi tập tin
    ''' </summary>
    ''' <param name="Filename"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetWriteTime(ByVal Filename As String) As Date
        Dim Fi As New System.IO.FileInfo(Filename)

        Return Fi.LastWriteTime
    End Function


End Class
