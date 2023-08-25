'Platformer with Level Editor
'
'Platformer with level editor is a game where the player controls a character that
'jumps and runs through a level while avoiding obstacles and enemies.
'The level editor allows players to create their own custom levels.

'MIT License
'Copyright(c) 2023 Joseph W. Lumbley

'Permission Is hereby granted, free Of charge, to any person obtaining a copy
'of this software And associated documentation files (the "Software"), to deal
'in the Software without restriction, including without limitation the rights
'to use, copy, modify, merge, publish, distribute, sublicense, And/Or sell
'copies of the Software, And to permit persons to whom the Software Is
'furnished to do so, subject to the following conditions:

'The above copyright notice And this permission notice shall be included In all
'copies Or substantial portions of the Software.

'THE SOFTWARE Is PROVIDED "AS IS", WITHOUT WARRANTY Of ANY KIND, EXPRESS Or
'IMPLIED, INCLUDING BUT Not LIMITED To THE WARRANTIES Of MERCHANTABILITY,
'FITNESS FOR A PARTICULAR PURPOSE And NONINFRINGEMENT. IN NO EVENT SHALL THE
'AUTHORS Or COPYRIGHT HOLDERS BE LIABLE For ANY CLAIM, DAMAGES Or OTHER
'LIABILITY, WHETHER In AN ACTION Of CONTRACT, TORT Or OTHERWISE, ARISING FROM,
'OUT OF Or IN CONNECTION WITH THE SOFTWARE Or THE USE Or OTHER DEALINGS IN THE
'SOFTWARE.

'Monica is our an AI assistant.
'https://monica.im/

'I'm making a video to explain the code on my YouTube channel.
'https://www.youtube.com/@codewithjoe6074
'

Imports System.ComponentModel
Imports System.Numerics
Imports System.Threading

Public Class Form1

    Private Context As New BufferedGraphicsContext

    Private Buffer As BufferedGraphics

    Private FrameCount As Integer = 0

    Private StartTime As DateTime = Now 'Get current time.

    Private TimeElapsed As TimeSpan

    Private SecondsElapsed As Double = 0

    Private FPS As Integer = 0

    Private ReadOnly FPSFont As New Font(FontFamily.GenericSansSerif, 25)

    Private FPS_Postion As New Point(0, 0)

    Private CurrentFrame As DateTime

    Private LastFrame As DateTime

    Private DeltaTime As TimeSpan

    Private Gravity As Single = 3000

    Private AirResistance As Single = 100.0F


    '2000 slippery 4000 grippy
    Private Friction As Single = 5000

    Private Enum AppState As Integer
        Start
        Playing
        Editing
    End Enum

    Private GameState As AppState = AppState.Start

    Private Structure GameObject

        Public Position As Vector2

        Public Acceleration As Vector2

        Public Velocity As Vector2

        Public MaxVelocity As Vector2

        Public Rect As Rectangle

        Public Text As String

    End Structure

    Private OurHero As GameObject

    Private Platforms() As GameObject

    Private Blocks() As GameObject

    Private Clouds() As GameObject

    Private Bushes() As GameObject

    Private Cash() As GameObject

    Private EditPlayButton As GameObject

    Private ToolBarBackground As GameObject

    Private PointerToolButton As GameObject

    Private BlockToolButton As GameObject

    Private BlockToolIcon As GameObject

    Private Title As GameObject

    Private TitlePlayButton As GameObject

    Private TitleEditButton As GameObject


    Private SelectedCloud As Integer = -1

    Private SelectedBlock As Integer = -1

    Private SelectedPlatform As Integer = -1

    Private SelectedBill As Integer = -1

    Private SelectedBush As Integer = -1



    Private ReadOnly AlineCenter As New StringFormat With {.Alignment = StringAlignment.Center}

    Private ReadOnly AlineCenterMiddle As New StringFormat With {.Alignment = StringAlignment.Center,
                                                                 .LineAlignment = StringAlignment.Center}

    Private GameLoopCancellationToken As New CancellationTokenSource()

    Private ReadOnly CWJFont As New Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold)

    Private RightArrowDown As Boolean = False

    Private LeftArrowDown As Boolean = False

    Private BDown As Boolean = False

    Private BUp As Boolean = False

    Private BPress As Boolean = False

    Private Jumped As Boolean = False

    Private OutinePen As New Pen(Color.Black, 4)

    Private ReadOnly PointerToolFont As New Font(New FontFamily("Wingdings"), 25, FontStyle.Bold)

    Private ReadOnly TitleFont As New Font(New FontFamily("Bahnschrift"), 38, FontStyle.Bold)

    Private LightSkyBluePen As New Pen(Color.LightSkyBlue, 4)

    Private LawnGreenPen As New Pen(Color.LawnGreen, 4)

    Private SeaGreenPen As New Pen(Color.SeaGreen, 4)

    Private GridSize As Integer = 64

    Private GridLineBitmap As New Bitmap(Screen.PrimaryScreen.WorkingArea.Size.Width, Screen.PrimaryScreen.WorkingArea.Size.Height)

    Private GridLineBuffer As Graphics = Graphics.FromImage(GridLineBitmap)

    Private GameLoopTask As Task =
        Task.Factory.StartNew(Sub()
                                  Try

                                      Thread.CurrentThread.Priority = ThreadPriority.Normal

                                      Do While Not GameLoopCancellationToken.IsCancellationRequested

                                          UpdateFrame()

                                          'Refresh the form to trigger a redraw.
                                          If Not Me.IsDisposed AndAlso Me.IsHandleCreated Then

                                              Me.Invoke(Sub() Me.Refresh())

                                          End If

                                          ' Wait for next frame
                                          Thread.Sleep(TimeSpan.Zero)

                                          'For uncapped frame rate use TimeSpan.Zero
                                          'Thread.Sleep(TimeSpan.Zero), the thread relinquishes the
                                          'remainder of its time slice to any thread of equal priority
                                          'that is ready to run. If there are no other threads of equal
                                          'priority that are ready to run, execution of the current
                                          'thread is not suspended.

                                          'For a capped frame rate set interval.
                                          'For 60 FPS set sleep interval to 15 ms.
                                          '1 second = 1000 milliseconds.
                                          '16.66666666666667 ms = 1000 ms / 60 FPS
                                          'Thread.Sleep(15)

                                      Loop

                                  Catch ex As Exception

                                      Debug.WriteLine(ex.ToString())

                                  End Try

                              End Sub)

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        InitializeApp()

    End Sub

    Private Sub InitializeApp()

        OurHero.Rect = New Rectangle(100, 500, 64, 64)

        OurHero.Position = New PointF(OurHero.Rect.X, OurHero.Rect.Y)

        OurHero.Velocity = New PointF(0, 0)

        OurHero.MaxVelocity = New PointF(400, 1100)

        OurHero.Acceleration = New PointF(200, 300)


        ReDim Blocks(0)
        Blocks(Blocks.Length - 1).Rect = New Rectangle(0, 832, 2000, 64)

        Blocks(0).Position = New PointF(Blocks(0).Rect.X, Blocks(0).Rect.Y)

        Array.Resize(Blocks, Blocks.Length + 1)
        Blocks(Blocks.Length - 1).Rect = New Rectangle(1088, 576, 64, 64)

        Blocks(Blocks.Length - 1).Position = New PointF(Blocks(Blocks.Length - 1).Rect.X, Blocks(Blocks.Length - 1).Rect.Y)

        Array.Resize(Blocks, Blocks.Length + 1)
        Blocks(Blocks.Length - 1).Rect = New Rectangle(1344, 576, 320, 64)

        Blocks(Blocks.Length - 1).Position = New PointF(Blocks(Blocks.Length - 1).Rect.X, Blocks(Blocks.Length - 1).Rect.Y)

        Array.Resize(Blocks, Blocks.Length + 1)
        Blocks(Blocks.Length - 1).Rect = New Rectangle(1472, 320, 64, 64)

        Blocks(Blocks.Length - 1).Position = New PointF(Blocks(Blocks.Length - 1).Rect.X, Blocks(Blocks.Length - 1).Rect.Y)


        ReDim Clouds(0)
        Clouds(Clouds.Length - 1).Rect = New Rectangle(600, 200, 64, 64)

        Array.Resize(Clouds, Clouds.Length + 1)
        Clouds(Clouds.Length - 1).Rect = New Rectangle(1400, 100, 64, 64)


        ReDim Bushes(0)
        Bushes(Bushes.Length - 1).Rect = New Rectangle(750, 768, 300, 64)

        Array.Resize(Bushes, Bushes.Length + 1)
        Bushes(Bushes.Length - 1).Rect = New Rectangle(1600, 768, 100, 64)


        ReDim Cash(0)
        Cash(Cash.Length - 1).Rect = New Rectangle(1071, 506, 64, 64)

        Array.Resize(Cash, Cash.Length + 1)
        Cash(Cash.Length - 1).Rect = New Rectangle(1400, 506, 64, 64)


        OutinePen.LineJoin = Drawing2D.LineJoin.Round

        EditPlayButton.Rect = New Rectangle(ClientRectangle.Left + 210, ClientRectangle.Bottom - 90, 120, 100)

        Title.Text = "Platformer" & vbCrLf & "with Level Editor"

        InitializeForm()

        InitializeBuffer()

    End Sub

    Private Sub InitializeForm()

        Me.WindowState = FormWindowState.Maximized

        Text = "Platformer with Level Editor - Code with Joe"

        SetStyle(ControlStyles.UserPaint, True)

        SetStyle(ControlStyles.OptimizedDoubleBuffer, True)

        SetStyle(ControlStyles.AllPaintingInWmPaint, True)

    End Sub

    Private Sub UpdateFrame()

        If GameState = AppState.Playing Then

            UpdateDeltaTime()

            UpdateOurHero()

        End If

    End Sub

    Private Sub UpdateDeltaTime()
        'Delta time (Δt) is the elapsed time since the last frame.

        CurrentFrame = Now

        DeltaTime = CurrentFrame - LastFrame 'Calculate delta time

        LastFrame = CurrentFrame 'Update last frame time

    End Sub

    Private Sub UpdateOurHero()

        If IsOnBlock() > -1 Then

            UpdateBlocks()

        ElseIf IsOnPlatform() > -1 Then

            'UpdatePlatform

        Else

            If OurHero.Velocity.Y >= 0 Then
                'Apply gravity to our hero. FALLING.

                OurHero.Velocity.Y += Gravity * DeltaTime.TotalSeconds

                'Max falling speed.
                If OurHero.Velocity.Y > OurHero.MaxVelocity.Y Then OurHero.Velocity.Y = OurHero.MaxVelocity.Y

                'Steering
                If RightArrowDown = True Then

                    OurHero.Velocity.X += 0.5F

                ElseIf LeftArrowDown = True Then

                    OurHero.Velocity.X += -0.5F

                End If

                'Move the rectangle down.
                'OurHero.Position.Y += OurHero.Velocity.Y * DeltaTime.TotalSeconds 'Δs = V * Δt

            Else
                'Apply gravity to our hero. JUMPING.

                OurHero.Velocity.Y += Gravity * DeltaTime.TotalSeconds

                'Max falling speed.
                If OurHero.Velocity.Y > OurHero.MaxVelocity.Y Then OurHero.Velocity.Y = OurHero.MaxVelocity.Y


                'air resistance
                If OurHero.Velocity.X >= 0 Then

                    OurHero.Velocity.X += -AirResistance * DeltaTime.TotalSeconds

                    If OurHero.Velocity.X < 0 Then OurHero.Velocity.X = 0

                Else

                    OurHero.Velocity.X += AirResistance * DeltaTime.TotalSeconds

                    If OurHero.Velocity.X > 0 Then OurHero.Velocity.X = 0

                End If



            End If

        End If

        Wraparound()

        UpdateHeroMovement()

    End Sub

    Private Sub Wraparound()

        'When the rectangle exits the bottom side of the client area.
        If OurHero.Position.Y > ClientRectangle.Bottom Then

            OurHero.Velocity.Y = 0F
            OurHero.Velocity.X = 0F

            OurHero.Position.X = 1500.0F

            'The rectangle reappears on the top side the client area.
            OurHero.Position.Y = ClientRectangle.Top - OurHero.Rect.Height

        End If

    End Sub

    Private Sub UpdateHeroMovement()

        'Move our hero horizontally.
        OurHero.Position.X += OurHero.Velocity.X * DeltaTime.TotalSeconds 'Δs = V * Δt
        'Displacement = Velocity x Delta Time

        OurHero.Rect.X = Math.Round(OurHero.Position.X)

        'Move our hero vertically.
        OurHero.Position.Y += OurHero.Velocity.Y * DeltaTime.TotalSeconds 'Δs = V * Δt
        'Displacement = Velocity x Delta Time

        OurHero.Rect.Y = Math.Round(OurHero.Position.Y)

    End Sub

    Private Function IsOnPlatform() As Integer

        If Platforms IsNot Nothing Then

            For Each Plateform In Platforms

                If OurHero.Rect.IntersectsWith(Plateform.Rect) = True Then

                    'return index of Plateform
                    Return Array.IndexOf(Platforms, Plateform)

                End If

            Next

        End If

        Return -1

    End Function

    Private Function IsOnBlock() As Integer

        If Blocks IsNot Nothing Then

            For Each Block In Blocks

                If OurHero.Rect.IntersectsWith(Block.Rect) = True Then

                    'return index of Plateform
                    Return Array.IndexOf(Blocks, Block)

                End If

            Next

        End If

        Return -1

    End Function

    Private Sub UpdateBlocks()

        If Blocks IsNot Nothing Then

            For Each Block In Blocks

                'Is our hero colliding with the block?
                If OurHero.Rect.IntersectsWith(Block.Rect) = True Then
                    'Yes, our hero is colliding with the block.

                    If OurHero.Velocity.Y > 0 Then
                        'Falling

                        'Stop hero vertical movement.
                        OurHero.Velocity.Y = 0

                        'Above
                        If OurHero.Position.Y <= Block.Rect.Top - Block.Rect.Height \ 2 Then

                            'Place our hero on top of our block.
                            If OurHero.Position.Y <> Block.Rect.Top - OurHero.Rect.Height + 1 Then

                                OurHero.Position.Y = Block.Rect.Top - OurHero.Rect.Height + 1

                            End If

                        Else

                        End If

                    ElseIf OurHero.Velocity.Y < 0 Then
                        'Jumping

                        'Stop hero movement.
                        OurHero.Velocity.Y = 0
                        OurHero.Velocity.X = 0

                        If OurHero.Position.Y >= Block.Rect.Bottom - Block.Rect.Height \ 4 Then
                            'Under

                            OurHero.Position.Y = Block.Rect.Bottom

                        Else
                            'Not under

                            If OurHero.Position.X > Block.Rect.Left Then
                                'Right

                                OurHero.Position.X = Block.Rect.Right

                            Else
                                'Left

                                OurHero.Position.X = Block.Rect.Left - OurHero.Rect.Width

                            End If

                        End If

                    Else

                        'Is our hero on top of the block.
                        If OurHero.Position.Y = Block.Rect.Top - OurHero.Rect.Height + 1 Then
                            'On top

                            If RightArrowDown = True Then

                                If OurHero.Velocity.X < 0 Then

                                    OurHero.Velocity.X = 0

                                Else

                                    OurHero.Velocity.X += OurHero.Acceleration.X * DeltaTime.TotalSeconds

                                    If OurHero.Velocity.X > OurHero.MaxVelocity.X Then OurHero.Velocity.X = OurHero.MaxVelocity.X

                                End If

                            ElseIf LeftArrowDown = True Then
                                'OurHero.Velocity.X = -400.0F

                                If OurHero.Velocity.X > 0 Then

                                    OurHero.Velocity.X = 0

                                Else

                                    OurHero.Velocity.X += -OurHero.Acceleration.X * DeltaTime.TotalSeconds

                                    If OurHero.Velocity.X < -OurHero.MaxVelocity.X Then OurHero.Velocity.X = -OurHero.MaxVelocity.X

                                End If

                            Else

                                If OurHero.Velocity.X > 0F Then

                                    OurHero.Velocity.X -= Friction * DeltaTime.TotalSeconds

                                    If OurHero.Velocity.X < 0F Then
                                        OurHero.Velocity.X = 0F
                                    End If

                                ElseIf OurHero.Velocity.X < 0F Then

                                    OurHero.Velocity.X += Friction * DeltaTime.TotalSeconds

                                    If OurHero.Velocity.X > 0F Then
                                        OurHero.Velocity.X = 0F
                                    End If

                                End If

                            End If

                            If BDown = True Then

                                If Jumped = False Then

                                    OurHero.Velocity.Y += -1300.0F

                                    Jumped = True

                                End If

                            End If

                        Else

                            OurHero.Velocity.X = 0

                            If OurHero.Position.X > Block.Rect.Left Then
                                'Right

                                OurHero.Position.X = Block.Rect.Right

                            Else
                                'Left

                                OurHero.Position.X = Block.Rect.Left - OurHero.Rect.Width

                            End If

                        End If

                    End If

                End If

            Next

        End If

    End Sub

    Private Sub InitializeBuffer()

        'Set context to the context of this app.
        Context = BufferedGraphicsManager.Current

        'Set buffer size to the primary working area.
        Context.MaximumBuffer = Screen.PrimaryScreen.WorkingArea.Size

        'Create buffer.
        Buffer = Context.Allocate(CreateGraphics(), ClientRectangle)

    End Sub

    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)

        DrawFrame()

        'Show buffer on form.
        Buffer.Render(e.Graphics)

        'Release memory used by buffer.
        Buffer.Dispose()
        Buffer = Nothing

        'Create new buffer.
        Buffer = Context.Allocate(CreateGraphics(), ClientRectangle)

        'Use these settings when drawing to the backbuffer.
        With Buffer.Graphics

            'Bug Fix
            .CompositingMode = Drawing2D.CompositingMode.SourceOver 'Don't Change.
            'To fix draw string error with anti aliasing: "Parameters not valid."
            'Set the compositing mode to source over.

            .TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit
            .SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            .CompositingQuality = Drawing2D.CompositingQuality.HighQuality
            .InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            .PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality

        End With

        UpdateFrameCounter()

    End Sub

    Private Sub DrawFrame()

        Select Case GameState

            Case AppState.Start

                DrawStartScreen()

            Case AppState.Playing

                DrawPlaying()

            Case AppState.Editing

                DrawEditing()

        End Select

    End Sub

    Private Sub DrawStartScreen()

        DrawBackground()

        DrawTitle()

        DrawTitleEditButton()

        DrawTitlePlayButton()

    End Sub

    Private Sub DrawPlaying()

        DrawBackground()

        DrawClouds()

        DrawBushes()

        DrawBlocks()

        DrawCoins()

        DrawOurHero()

        DrawFPS()

        DrawEditButton()

    End Sub

    Private Sub DrawEditing()

        DrawBackground()

        DrawGridLines()

        DrawClouds()

        DrawBushes()

        DrawBlocks()

        DrawCoins()

        DrawOurHero()

        DrawFPS()

        DrawPlayButton()

        DrawToolBar()

    End Sub

    Private Sub DrawToolBar()

        DrawToolbarBackground()

        DrawPointerToolButton()

        DrawBlockToolButton()

    End Sub

    Private Sub DrawToolbarBackground()

        With Buffer.Graphics

            .FillRectangle(Brushes.DarkGray, ToolBarBackground.Rect)

        End With

    End Sub

    Private Sub DrawPointerToolButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black, PointerToolButton.Rect)

            .DrawString("ë", PointerToolFont, Brushes.White, PointerToolButton.Rect, AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawBlockToolButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black, BlockToolButton.Rect)

            .FillRectangle(Brushes.Chocolate, BlockToolIcon.Rect)

        End With

    End Sub

    Private Sub DrawPlayButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black, EditPlayButton.Rect)

            .DrawString("Play", FPSFont, Brushes.White, EditPlayButton.Rect, AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawEditButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black, EditPlayButton.Rect)

            .DrawString("Edit", FPSFont, Brushes.White, EditPlayButton.Rect, AlineCenterMiddle)

        End With

    End Sub


    Private Sub DrawFPS()

        With Buffer.Graphics

            .DrawString(FPS.ToString & " FPS", FPSFont, Brushes.White, FPS_Postion)

        End With

    End Sub


    Private Sub DrawOurHero()

        With Buffer.Graphics

            .FillRectangle(Brushes.Red, OurHero.Rect)

            .DrawString("Hero", CWJFont, Brushes.White, OurHero.Rect, AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawCoins()

        With Buffer.Graphics

            If Cash IsNot Nothing Then

                For Each Bill In Cash

                    .FillRectangle(Brushes.Goldenrod, Bill.Rect)

                    .DrawString("$", FPSFont, Brushes.OrangeRed, Bill.Rect, AlineCenterMiddle)

                    If SelectedBill = Array.IndexOf(Cash, Bill) Then

                        .DrawRectangle(New Pen(Color.Red, 6), Bill.Rect)

                    End If

                Next

            End If

        End With

    End Sub

    Private Sub DrawBlocks()

        With Buffer.Graphics

            If Blocks IsNot Nothing Then

                For Each Block In Blocks

                    .FillRectangle(Brushes.Chocolate, Block.Rect)

                    If SelectedBlock = Array.IndexOf(Blocks, Block) Then

                        .DrawRectangle(New Pen(Color.Red, 6), Block.Rect)

                    End If

                Next

            End If

        End With

    End Sub

    Private Sub DrawBushes()

        With Buffer.Graphics

            If Bushes IsNot Nothing Then

                For Each Bush In Bushes

                    .FillRectangle(Brushes.GreenYellow, Bush.Rect)

                    .DrawLine(SeaGreenPen, Bush.Rect.Right - 10, Bush.Rect.Top + 10, Bush.Rect.Right - 10, Bush.Rect.Bottom - 10)

                    .DrawLine(SeaGreenPen, Bush.Rect.Left + 10, Bush.Rect.Bottom - 10, Bush.Rect.Right - 10, Bush.Rect.Bottom - 10)

                    .DrawRectangle(OutinePen, Bush.Rect)

                    If SelectedBush = Array.IndexOf(Bushes, Bush) Then

                        .DrawRectangle(New Pen(Color.Red, 6), Bush.Rect)

                    Else

                        .DrawRectangle(OutinePen, Bush.Rect)

                    End If

                Next

            End If

        End With

    End Sub

    Private Sub DrawClouds()

        With Buffer.Graphics

            If Clouds IsNot Nothing Then

                For Each Cloud In Clouds

                    .FillRectangle(Brushes.White, Cloud.Rect)

                    .DrawLine(LightSkyBluePen, Cloud.Rect.Right - 10, Cloud.Rect.Top + 10, Cloud.Rect.Right - 10, Cloud.Rect.Bottom - 10)

                    .DrawLine(LightSkyBluePen, Cloud.Rect.Left + 10, Cloud.Rect.Bottom - 10, Cloud.Rect.Right - 10, Cloud.Rect.Bottom - 10)

                    If SelectedCloud = Array.IndexOf(Clouds, Cloud) Then

                        .DrawRectangle(New Pen(Color.Red, 6), Cloud.Rect)

                    Else

                        .DrawRectangle(OutinePen, Cloud.Rect)

                    End If

                Next

            End If

        End With

    End Sub

    Private Sub DrawTitlePlayButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black,
                           TitlePlayButton.Rect)

            .DrawString("Play",
                        FPSFont,
                        Brushes.White,
                        TitlePlayButton.Rect,
                        AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawTitleEditButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black,
                           TitleEditButton.Rect)

            .DrawString("Edit",
                        FPSFont,
                        Brushes.White,
                        TitleEditButton.Rect,
                        AlineCenterMiddle)

        End With

    End Sub


    Private Sub DrawTitle()

        With Buffer.Graphics

            'Draw drop shadow.
            .DrawString(Title.Text,
                    TitleFont,
                    New SolidBrush(Color.FromArgb(128, Color.Black)),
                    New Rectangle(Title.Rect.X + 5, Title.Rect.Y + 5, Title.Rect.Width, Title.Rect.Height),
                    AlineCenterMiddle)

            'Draw title.
            .DrawString(Title.Text,
                    TitleFont,
                    Brushes.Black,
                    Title.Rect,
                    AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawBackground()

        With Buffer.Graphics

            .Clear(Color.LightSkyBlue)

        End With

    End Sub

    Private Sub DrawGridLines()

        With Buffer.Graphics

            .DrawImageUnscaled(GridLineBitmap, 0, 0)

        End With





        'GridLineBuffer.Clear(Color.Transparent)

        '' Draw vertical lines  |
        'For x As Integer = 0 To ClientSize.Width Step GridSize
        '    'Buffer.Graphics.DrawLine(Pens.Black, x, 0, x, ClientSize.Height)

        '    GridLineBuffer.DrawLine(Pens.Black, x, 0, x, ClientSize.Height)

        'Next

        '' Draw horizontal lines ---
        'For y As Integer = 0 To ClientSize.Width Step GridSize
        '    'Buffer.Graphics.DrawLine(Pens.Black, 0, y, ClientSize.Width, y)

        '    GridLineBuffer.DrawLine(Pens.Black, 0, y, ClientSize.Width, y)


        'Next

    End Sub

    Private Sub BufferGridLines()

        GridLineBuffer.Clear(Color.Transparent)

        ' Draw vertical lines  |
        For x As Integer = 0 To ClientSize.Width Step GridSize
            'Buffer.Graphics.DrawLine(Pens.Black, x, 0, x, ClientSize.Height)

            GridLineBuffer.DrawLine(Pens.Black, x, 0, x, ClientSize.Height)

        Next

        ' Draw horizontal lines ---
        For y As Integer = 0 To ClientSize.Width Step GridSize
            'Buffer.Graphics.DrawLine(Pens.Black, 0, y, ClientSize.Width, y)

            GridLineBuffer.DrawLine(Pens.Black, 0, y, ClientSize.Width, y)


        Next

    End Sub

    Private Sub UpdateFrameCounter()

        TimeElapsed = Now.Subtract(StartTime)

        SecondsElapsed = TimeElapsed.TotalSeconds

        If SecondsElapsed < 1 Then

            FrameCount += 1

        Else

            FPS = FrameCount

            FrameCount = 0

            StartTime = Now

        End If

    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize

        'Place the FPS display at the bottom of the client area.
        FPS_Postion.Y = ClientRectangle.Bottom - 75

        EditPlayButton.Rect = New Rectangle(ClientRectangle.Left + 210, ClientRectangle.Bottom - 90, 120, 90)

        ToolBarBackground.Rect = New Rectangle(ClientRectangle.Left + 331, ClientRectangle.Bottom - 90, ClientRectangle.Width, 100)

        PointerToolButton.Rect = New Rectangle(ClientRectangle.Left + 331, ClientRectangle.Bottom - 90, 90, 90)

        BlockToolButton.Rect = New Rectangle(ClientRectangle.Left + 422, ClientRectangle.Bottom - 90, 90, 90)

        BlockToolIcon.Rect = New Rectangle(ClientRectangle.Left + 447, ClientRectangle.Bottom - 65, 40, 40)

        Title.Rect = New Rectangle(ClientRectangle.Left, ClientRectangle.Top, ClientRectangle.Width, ClientRectangle.Height)

        TitleEditButton.Rect = New Rectangle(ClientRectangle.Width \ 2 - 200, ClientRectangle.Height \ 2 + 100, 150, 90)

        TitlePlayButton.Rect = New Rectangle(ClientRectangle.Width \ 2 + 100, ClientRectangle.Height \ 2 + 100, 150, 90)

        'DrawGridLines()
        BufferGridLines()

    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles MyBase.Closing

        GameLoopCancellationToken.Cancel(True)

    End Sub

    Protected Overrides Sub OnPaintBackground(ByVal e As PaintEventArgs)

        'Intentionally left blank. Do not remove.

    End Sub

    Private Sub Form1_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown

        Select Case GameState

            Case AppState.Start

                If TitlePlayButton.Rect.Contains(e.Location) Then

                    LastFrame = Now

                    GameState = AppState.Playing

                End If

            Case AppState.Playing

                If EditPlayButton.Rect.Contains(e.Location) Then

                    GameState = AppState.Editing

                End If

            Case AppState.Editing

                SelectedBlock = CheckBlockSelection(e)

                SelectedCloud = CheckCloudSelection(e)

                SelectedBill = CheckBillSelection(e)

                SelectedBush = CheckBushSelection(e)



                If EditPlayButton.Rect.Contains(e.Location) Then

                    LastFrame = Now

                    GameState = AppState.Playing

                End If

        End Select

    End Sub

    Private Function CheckCloudSelection(e As MouseEventArgs) As Integer

        If Clouds IsNot Nothing Then

            For Each Cloud In Clouds

                'Has the player selected a cloud?
                If Cloud.Rect.Contains(e.Location) Then
                    'Yes, the player has selected a cloud.

                    Return Array.IndexOf(Clouds, Cloud)

                    Exit Function

                End If

            Next

        End If

        Return -1

    End Function

    Private Function CheckBlockSelection(e As MouseEventArgs) As Integer

        If Blocks IsNot Nothing Then

            For Each Block In Blocks

                'Has the player selected a cloud?
                If Block.Rect.Contains(e.Location) Then
                    'Yes, the player has selected a cloud.

                    Return Array.IndexOf(Blocks, Block)

                    Exit Function

                End If

            Next

        End If

        Return -1

    End Function

    Private Function CheckBillSelection(e As MouseEventArgs) As Integer

        If Cash IsNot Nothing Then

            For Each Bill In Cash

                'Has the player selected a cloud?
                If Bill.Rect.Contains(e.Location) Then
                    'Yes, the player has selected a cloud.

                    Return Array.IndexOf(Cash, Bill)

                    Exit Function

                End If

            Next

        End If

        Return -1

    End Function

    Private Function CheckBushSelection(e As MouseEventArgs) As Integer

        If Bushes IsNot Nothing Then

            For Each Bush In Bushes

                'Has the player selected a cloud?
                If Bush.Rect.Contains(e.Location) Then
                    'Yes, the player has selected a cloud.

                    Return Array.IndexOf(Bushes, Bush)

                    Exit Function

                End If

            Next

        End If

        Return -1

    End Function

    Private Sub Form1_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove

        If GameState = AppState.Editing Then

            If SelectedCloud > -1 Then

                If e.Button = MouseButtons.Left Then

                    'Snap to grid
                    Clouds(SelectedCloud).Rect.X = CInt(Math.Round(e.X / GridSize)) * GridSize

                    Clouds(SelectedCloud).Rect.Y = CInt(Math.Round(e.Y / GridSize)) * GridSize

                End If

            End If

            If SelectedBlock > -1 Then

                If e.Button = MouseButtons.Left Then

                    'Snap to grid
                    Blocks(SelectedBlock).Rect.X = CInt(Math.Round(e.X / GridSize)) * GridSize

                    Blocks(SelectedBlock).Rect.Y = CInt(Math.Round(e.Y / GridSize)) * GridSize

                End If

            End If

            If SelectedBill > -1 Then

                If e.Button = MouseButtons.Left Then

                    'Snap to grid
                    Cash(SelectedBill).Rect.X = CInt(Math.Round(e.X / GridSize)) * GridSize

                    Cash(SelectedBill).Rect.Y = CInt(Math.Round(e.Y / GridSize)) * GridSize

                End If

            End If

            If SelectedBush > -1 Then

                If e.Button = MouseButtons.Left Then

                    'Snap to grid
                    Bushes(SelectedBush).Rect.X = CInt(Math.Round(e.X / GridSize)) * GridSize

                    Bushes(SelectedBush).Rect.Y = CInt(Math.Round(e.Y / GridSize)) * GridSize

                End If

            End If

        End If

    End Sub


    Private Sub Form1_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp

    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown

        'Arrow right.
        Select Case e.KeyCode

            Case Keys.Right

                RightArrowDown = True

                LeftArrowDown = False

            Case Keys.Left

                LeftArrowDown = True

                RightArrowDown = False

            Case Keys.B

                BDown = True

        End Select

    End Sub

    Private Sub Form1_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp

        'Arrow right.
        Select Case e.KeyCode

            Case Keys.Right

                RightArrowDown = False

            Case Keys.Left

                LeftArrowDown = False

            Case Keys.B

                If Jumped = True Then Jumped = False

                BDown = False

        End Select

    End Sub

    Private Sub Form1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles Me.KeyPress

        Select Case e.KeyChar
            Case "b"

        End Select

    End Sub

    Private Sub Form1_Move(sender As Object, e As EventArgs) Handles Me.Move

    End Sub

End Class


