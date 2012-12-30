

Imports System
Imports System.Drawing


Public Class ImageGlass_Animation

    Public Shared Sub ExtractAllFrames(ByVal AnimationFile As String, ByVal DestinationFolder As String)
        Dim ani As New ClsExtractAnimation()
        ani.ExtractAllFrames(AnimationFile, DestinationFolder)

    End Sub


End Class


Class ClsExtractAnimation

    Private img As Bitmap
    Private IsAnimating As Boolean = False
    Private i As Integer
    Private Filename As String
    Private Des_Folder As String



    Public Sub ExtractAllFrames(ByVal AnimationFile As String, ByVal DestinationFolder As String)
        '//initiate class

        IsAnimating = False
        Filename = AnimationFile
        Des_Folder = DestinationFolder

        img = New Bitmap(AnimationFile)
        i = 1

        '//begin extract
        AnimateImage()

    End Sub

    'This method begins the animation.
    Private Sub AnimateImage()
        If Not IsAnimating Then

            'Begin the animation only once.
            ImageAnimator.Animate(img, New EventHandler(AddressOf Me.SaveFrames))
            IsAnimating = True
        End If

    End Sub

    Private Sub SaveFrames()
        '//kiểm tra đã hết frame chưa
        If i > Image.FromFile(Filename).GetFrameCount(Imaging.FrameDimension.Time) Then
            IsAnimating = False
            Exit Sub
        End If

        '//Begin the animation.
        AnimateImage()

        '//Get the next frame ready for rendering.
        ImageAnimator.UpdateFrames()

        '//Draw the next frame in the animation.
        img.Save((Des_Folder & "\").Replace("\\", "\") & _
                 IO.Path.GetFileNameWithoutExtension(Filename) & _
                 " - " & CStr(i) & ".png", Imaging.ImageFormat.Png)

        '//go to next frmae
        i = i + 1

    End Sub

End Class