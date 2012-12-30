
Imports System.Windows.Forms
Imports System.Drawing


Public Class ImageGlass_Image


    Private Declare Auto Function SystemParametersInfo Lib "user32.dll" (ByVal uAction As Integer, ByVal uParam As Integer, ByVal lpvParam As String, ByVal fuWinIni As Integer) As Integer

    ''' <summary>
    ''' Đặt làm hình nền desktop
    ''' </summary>
    ''' <param name="filename"></param>
    ''' <remarks></remarks>
    Public Shared Sub SetWallpaper(ByVal filename As String)
        On Error Resume Next

        Dim pic As Image = Image.FromFile(filename)
        Dim ext As String = IO.Path.GetExtension(filename).ToLower()

        Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Control Panel\Desktop\", "TileWallpaper", "0")
        Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Control Panel\Desktop\", "WallpaperStyle", "0")

        If ext <> ".bmp" Or ext <> ".jpg" Or ext <> ".jpeg" Then

            Dim s As String = (My.Computer.FileSystem.SpecialDirectories.Temp & "\" _
                               ).Replace("\\", "\") & "wallpaper.phapsoftware.wordpress.com.bmp"
            pic.Save(s, Imaging.ImageFormat.Bmp)
            SystemParametersInfo(&H14, -1, s, &H1)

            Exit Sub
        End If

        SystemParametersInfo(&H14, -1, filename, &H1)
    End Sub


    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="pic"></param>
    ''' <param name="filename"></param>
    ''' <remarks></remarks>
    Public Shared Sub ConvertImage(ByVal pic As Image, ByVal filename As String)

        Dim D As New SaveFileDialog
        D.Title = "Chọn thư mục lưu file chuyển đổi"
        D.Filter = "BMP|*.BMP|EMF|*.EMF|EXIF|*.EXIF|GIF|*.GIF|ICO|*.ICO|JPG|*.JPG|PNG|*.PNG|TIFF|*.TIFF|WMF|*.WMF"

        D.FileName = IO.Path.GetFileNameWithoutExtension(filename)
        Dim ext As String = IO.Path.GetExtension(filename).Substring(1)


        Select Case ext.ToLower
            Case "bmp" : D.FilterIndex = 1
            Case "emf" : D.FilterIndex = 2
            Case "exif" : D.FilterIndex = 3
            Case "gif" : D.FilterIndex = 4
            Case "ico" : D.FilterIndex = 5
            Case "jpg" : D.FilterIndex = 6
            Case "png" : D.FilterIndex = 7
            Case "tiff" : D.FilterIndex = 8
            Case "wmf" : D.FilterIndex = 9
        End Select

        If D.ShowDialog() = Windows.Forms.DialogResult.OK Then
            Dim s As String = D.FileName

            Select Case D.FilterIndex

                Case 1 : pic.Save(s, Imaging.ImageFormat.Bmp)
                Case 2 : pic.Save(s, Imaging.ImageFormat.Emf)
                Case 3 : pic.Save(s, Imaging.ImageFormat.Exif)
                Case 4 : pic.Save(s, Imaging.ImageFormat.Gif)
                Case 5 : pic.Save(s, Imaging.ImageFormat.Icon)
                Case 6 : pic.Save(s, Imaging.ImageFormat.Jpeg)
                Case 7 : pic.Save(s, Imaging.ImageFormat.Png)
                Case 8 : pic.Save(s, Imaging.ImageFormat.Tiff)
                Case 9 : pic.Save(s, Imaging.ImageFormat.Wmf)

            End Select
        End If

    End Sub

    ''' <summary>
    ''' Delete a file
    ''' </summary>
    ''' <param name="filename"></param>
    ''' <param name="recycle"></param>
    ''' <remarks></remarks>
    Public Shared Sub DeleteFile(ByVal filename As String, Optional ByVal recycle As Boolean = True)
        If recycle Then
            My.Computer.FileSystem.DeleteFile(filename, FileIO.UIOption.OnlyErrorDialogs, _
                                              FileIO.RecycleOption.SendToRecycleBin)
        Else
            My.Computer.FileSystem.DeleteFile(filename, FileIO.UIOption.OnlyErrorDialogs, _
                                              FileIO.RecycleOption.DeletePermanently)
        End If

    End Sub

    ''' <summary>
    ''' Rename a file
    ''' </summary>
    ''' <param name="oldfilename"></param>
    ''' <param name="newname"></param>
    ''' <remarks></remarks>
    Public Shared Sub RenameFile(oldfilename As String, newname As String)
        My.Computer.FileSystem.RenameFile(oldfilename, newname)
    End Sub



End Class
