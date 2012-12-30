'2006 kleinma MSMVP
'www.vbforums.com

Imports System.Net
Imports System.IO
Imports System.Net.Mail

Public Class ImageGlass_DownloadFile
    Public Event AmountDownloadedChanged(ByVal iNewProgress As Long)
    Public Event FileDownloadSizeObtained(ByVal iFileSize As Long)
    Public Event FileDownloadComplete()
    Public Event FileDownloadFailed(ByVal ex As Exception)

    Private mCurrentFile As String = String.Empty

    Public ReadOnly Property CurrentFile() As String
        Get
            Return mCurrentFile
        End Get
    End Property
    Public Function DownloadFile(ByVal URL As String, ByVal Location As String) As Boolean
        Try
            mCurrentFile = GetFileName(URL)
            Dim WC As New WebClient
            WC.DownloadFile(URL, Location)
            RaiseEvent FileDownloadComplete()
            Return True
        Catch ex As Exception
            RaiseEvent FileDownloadFailed(ex)
            Return False
        End Try
    End Function

    Private Function GetFileName(ByVal URL As String) As String
        Try
            Return URL.Substring(URL.LastIndexOf("/") + 1)
        Catch ex As Exception
            Return URL
        End Try
    End Function
    Public Function DownloadFileWithProgress(ByVal URL As String, ByVal Location As String) As Boolean
        Dim FS As FileStream
        Try


            mCurrentFile = GetFileName(URL)
            Dim wRemote As WebRequest
            Dim bBuffer As Byte()
            ReDim bBuffer(256)
            Dim iBytesRead As Integer
            Dim iTotalBytesRead As Integer

            FS = New FileStream(Location, FileMode.Create, FileAccess.Write)
            wRemote = WebRequest.Create(URL)
            Dim myWebResponse As WebResponse = wRemote.GetResponse
            RaiseEvent FileDownloadSizeObtained(myWebResponse.ContentLength)
            Dim sChunks As Stream = myWebResponse.GetResponseStream
            Do
                iBytesRead = sChunks.Read(bBuffer, 0, 256)
                FS.Write(bBuffer, 0, iBytesRead)
                iTotalBytesRead += iBytesRead
                If myWebResponse.ContentLength < iTotalBytesRead Then
                    RaiseEvent AmountDownloadedChanged(myWebResponse.ContentLength)
                Else
                    RaiseEvent AmountDownloadedChanged(iTotalBytesRead)
                End If
            Loop While Not iBytesRead = 0
            sChunks.Close()
            FS.Close()
            RaiseEvent FileDownloadComplete()
            Return True
        Catch ex As Exception
            If Not (FS Is Nothing) Then
                FS.Close()
                FS = Nothing
            End If
            RaiseEvent FileDownloadFailed(ex)
            Return False
        End Try
    End Function

    Public Shared Function FormatFileSize(ByVal Size As Long, ByRef DonVi As String) As String
        Try
            Dim KB As Integer = 1024
            Dim MB As Integer = KB * KB
            ' Return size of file in kilobytes.
            If Size < KB Then
                DonVi = " bytes"
                Return Size.ToString("D")
            Else
                Select Case Size / KB
                    Case Is < 1000
                        DonVi = " KB"
                        Return (Size / KB).ToString("N")
                    Case Is < 1000000
                        DonVi = " MB"
                        Return (Size / MB).ToString("N")
                    Case Is < 10000000
                        DonVi = " GB"
                        Return (Size / MB / KB).ToString("N")
                End Select
            End If
        Catch ex As Exception
            Return Size.ToString
        End Try
        Return ""
    End Function


    ''' <summary>
    ''' Send an email to Gmail
    ''' </summary>
    ''' <param name="SendTo"></param>
    ''' <param name="SendFrom"></param>
    ''' <param name="Account"></param>
    ''' <param name="Password"></param>
    ''' <param name="SMTP"></param>
    ''' <param name="Subject"></param>
    ''' <param name="Body"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function Send_Email(ByVal SendTo As String, ByVal SendFrom As String, ByVal Account As String, _
                               ByVal Password As String, ByVal SMTP As String, ByVal Subject As String, _
                               ByVal Body As String, ByVal FileAttach As String) As Integer

        'Start by creating a mail message object
        Dim MyMailMessage As New MailMessage()

        'From requires an instance of the MailAddress type
        MyMailMessage.From = New MailAddress(SendFrom)

        'To is a collection of MailAddress types
        MyMailMessage.To.Add(SendTo)

        MyMailMessage.Subject = Subject
        MyMailMessage.Body = Body

        MyMailMessage.Attachments.Add(New System.Net.Mail.Attachment(FileAttach))

        'Create the SMTPClient object and specify the SMTP GMail server
        Dim SMTPServer As New SmtpClient(SMTP)
        SMTPServer.Port = 587
        SMTPServer.Credentials = New System.Net.NetworkCredential(Account, Password)
        SMTPServer.EnableSsl = True

        Send_Email = 1

        Try
            SMTPServer.Send(MyMailMessage)
        Catch ex As SmtpException
            Send_Email = 0
        End Try

    End Function

    ''' <summary>
    ''' Send an email to Gmail
    ''' </summary>
    ''' <param name="SendTo"></param>
    ''' <param name="SendFrom"></param>
    ''' <param name="Account"></param>
    ''' <param name="Password"></param>
    ''' <param name="SMTP"></param>
    ''' <param name="Subject"></param>
    ''' <param name="Body"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function Send_Email(ByVal SendTo As String, ByVal SendFrom As String, ByVal Account As String, _
                               ByVal Password As String, ByVal SMTP As String, ByVal Subject As String, _
                               ByVal Body As String) As Integer

        'Start by creating a mail message object
        Dim MyMailMessage As New MailMessage()

        'From requires an instance of the MailAddress type
        MyMailMessage.From = New MailAddress(SendFrom)

        'To is a collection of MailAddress types
        MyMailMessage.To.Add(SendTo)

        MyMailMessage.Subject = Subject
        MyMailMessage.Body = Body

        'Create the SMTPClient object and specify the SMTP GMail server
        Dim SMTPServer As New SmtpClient(SMTP)
        SMTPServer.Port = 587
        SMTPServer.Credentials = New System.Net.NetworkCredential(Account, Password)
        SMTPServer.EnableSsl = True

        Send_Email = 1

        Try
            SMTPServer.Send(MyMailMessage)
        Catch ex As SmtpException
            Send_Email = 0
        End Try

    End Function

End Class

