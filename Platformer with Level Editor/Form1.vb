﻿'Platformer with Level Editor

'Help our hero navigate through a level by jumping on platforms,
'collecting cash and eliminating enemies.
'Create your own level with the editor.

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

'Level music by Joseph Lumbley Jr.

'Monica is our an AI assistant.
'https://monica.im/

'I'm making a video to explain the code on my YouTube channel.
'https://www.youtube.com/@codewithjoe6074

Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Numerics
Imports System.Runtime.InteropServices
Imports System.Threading

Public Class Form1

    Private Enum AppState As Integer
        Start
        Playing
        Editing
        Clear
    End Enum

    Private Enum ObjectID As Integer
        Level
        Block
        Bill
        Bush
        Cloud
        Goal
        Enemy
    End Enum

    Private Enum Tools As Integer
        Pointer
        Block
        Bill
        Bush
        Cloud
        Goal
        Enemy
    End Enum

    Private Enum Direction As Integer
        Right
        Left
    End Enum

    Private Structure GameObject

        Public ID As ObjectID

        Public Position As Vector2

        Public Acceleration As Vector2

        Public Velocity As Vector2

        Public MaxVelocity As Vector2

        Public Rect As Rectangle

        Public Text As String

        Public Collected As Boolean

        Public PatrolA As Vector2

        Public PatrolB As Vector2

        Public PatrolDirection As Direction

        Public Eliminated As Boolean

    End Structure

    Private GameState As AppState = AppState.Start

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

    Private EditorCurrentFrame As DateTime

    Private EditorLastFrame As DateTime

    Private EditorDeltaTime As TimeSpan

    Private Gravity As Single = 2000

    Private AirResistance As Single = 100.0F

    '500 slippery 1000 grippy
    Private Friction As Single = 1500

    Private OurHero As GameObject

    Private Goal As GameObject

    Private Platforms() As GameObject

    Private Blocks() As GameObject

    Private Clouds() As GameObject

    Private Bushes() As GameObject

    Private Cash() As GameObject

    Private Enemies() As GameObject

    Private FileObjects() As GameObject

    Private EditPlayButton As GameObject

    Private SaveButton As GameObject

    Private ToolBarBackground As GameObject

    Private MenuBackground As GameObject

    Private MenuButton As GameObject

    Private PointerToolButton As GameObject

    Private BlockToolButton As GameObject

    Private BlockToolIcon As GameObject

    Private BillToolButton As GameObject

    Private BillToolIcon As GameObject

    Private CloudToolButton As GameObject

    Private CloundToolIcon As GameObject

    Private BushToolButton As GameObject

    Private BushToolIcon As GameObject

    Private GoalToolButton As GameObject

    Private GoalToolIcon As GameObject

    Private EnemyToolButton As GameObject

    Private EnemyToolIcon As GameObject

    Private SelectedTool As Tools = Tools.Pointer

    Private ShowToolPreview As Boolean = False

    Private ShowMenu As Boolean = False

    Private Title As GameObject

    Private StartScreenOpenButton As GameObject

    Private StartScreenNewButton As GameObject

    Private OpenButton As GameObject

    Private NewButton As GameObject

    Private ExitButton As GameObject

    Private ScoreIndicators As GameObject

    Private Level As GameObject

    Private Camera As GameObject

    Private CameraPlayPostion As Point

    Private ToolPreview As Rectangle

    Private SelectedCloud As Integer = -1

    Private SelectedBlock As Integer = -1

    Private SelectedPlatform As Integer = -1

    Private SelectedBill As Integer = -1

    Private SelectedBush As Integer = -1

    Private SelectedEnemy As Integer = -1

    Private RightArrowDown As Boolean = False

    Private LeftArrowDown As Boolean = False

    Private BDown As Boolean = False

    Private DeleteDown As Boolean = False

    Private BUp As Boolean = False

    Private BPress As Boolean = False

    Private Jumped As Boolean = False

    Private GridSize As Integer = 64

    Private GridLineBitmap As New Bitmap(Screen.PrimaryScreen.WorkingArea.Size.Width, Screen.PrimaryScreen.WorkingArea.Size.Height)

    Private GridLineBuffer As Graphics = Graphics.FromImage(GridLineBitmap)

    Private SizingHandle As New Rectangle(0, 0, 25, 25)

    Private SizingHandleSelected As Boolean = False

    Private SelectionOffset As Point

    Private CashCollected As Integer = 0

    Private CashCollectedPostion As New Point(0, 0)

    Private ReadOnly PointerToolFont As New Font(New FontFamily("Wingdings"), 25, FontStyle.Bold)

    Private ReadOnly GoalToolFont As New Font(New FontFamily("Wingdings"), 25, FontStyle.Bold)

    Private ReadOnly GoalFont As New Font(New FontFamily("Wingdings"), 35, FontStyle.Regular)

    Private ReadOnly BillIconFont As New Font(FontFamily.GenericSansSerif, 16, FontStyle.Regular)

    Private ReadOnly EnemyIconFont As New Font(FontFamily.GenericSansSerif, 16, FontStyle.Regular)

    Private ReadOnly TitleFont As New Font(New FontFamily("Bahnschrift"), 52, FontStyle.Bold)

    Private ReadOnly NewButtonIconFont As New Font(New FontFamily("Yu Gothic"), 25, FontStyle.Regular)

    Private OutinePen As New Pen(Color.Black, 4)

    Private MenuOutinePen As New Pen(Color.White, 16)

    Private MenuShadowPen As New Pen(Color.FromArgb(128, Color.Black), 16)

    Private BButtonIconOutinePen As New Pen(Color.Red, 3)

    Private YButtonIconOutinePen As New Pen(Color.Yellow, 3)

    Private XButtonIconOutinePen As New Pen(Color.Blue, 3)

    Private ReadOnly ButtonIconFont As New Font(FontFamily.GenericSansSerif, 20)

    Private ReadOnly RightTriggerIconFont As New Font(FontFamily.GenericSansSerif, 20)


    Private CloundToolIconOutinePen As New Pen(Color.Black, 3)

    Private BushToolIconOutinePen As New Pen(Color.Black, 3)

    Private LightSkyBluePen As New Pen(Color.LightSkyBlue, 4)

    Private CloundToolIconPen As New Pen(Color.LightSkyBlue, 3)

    Private BushToolIconPen As New Pen(Color.SeaGreen, 3)

    Private LawnGreenPen As New Pen(Color.LawnGreen, 4)

    Private SeaGreenPen As New Pen(Color.SeaGreen, 4)

    Private CharcoalGrey As Color = Color.FromArgb(255, 60, 65, 66)

    Private DarkCharcoalGrey As Color = Color.FromArgb(255, 48, 52, 53)

    Private CharcoalGreyBrush As New SolidBrush(CharcoalGrey)

    Private DarkCharcoalGreyBrush As New SolidBrush(DarkCharcoalGrey)

    Private ReadOnly AlineCenter As New StringFormat With {.Alignment = StringAlignment.Center}

    Private ReadOnly AlineCenterMiddle As New StringFormat With {.Alignment = StringAlignment.Center,
                                                                 .LineAlignment = StringAlignment.Center}

    Private ReadOnly CWJFont As New Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold)

    Private ReadOnly EnemyFont As New Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold)

    Private GameLoopCancellationToken As New CancellationTokenSource()

    Private IsFileLoaded As Boolean = False

    <DllImport("XInput1_4.dll")>
    Private Shared Function XInputGetState(dwUserIndex As Integer, ByRef pState As XINPUT_STATE) As Integer
    End Function

    'XInput1_4.dll seems to be the current version
    'XInput9_1_0.dll is maintained primarily for backward compatibility. 

    <StructLayout(LayoutKind.Explicit)>
    Public Structure XINPUT_STATE
        <FieldOffset(0)>
        Public dwPacketNumber As UInteger 'Unsigned 32-bit (4-byte) integer range 0 through 4,294,967,295.
        <FieldOffset(4)>
        Public Gamepad As XINPUT_GAMEPAD
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure XINPUT_GAMEPAD
        Public wButtons As UShort 'Unsigned 16-bit (2-byte) integer range 0 through 65,535.
        Public bLeftTrigger As Byte 'Unsigned 8-bit (1-byte) integer range 0 through 255.
        Public bRightTrigger As Byte
        Public sThumbLX As Short 'Signed 16-bit (2-byte) integer range -32,768 through 32,767.
        Public sThumbLY As Short
        Public sThumbRX As Short
        Public sThumbRY As Short
    End Structure

    <DllImport("XInput1_4.dll")>
    Private Shared Function XInputSetState(playerIndex As Integer, ByRef vibration As XINPUT_VIBRATION) As Integer
    End Function

    Public Structure XINPUT_VIBRATION
        Public wLeftMotorSpeed As UShort
        Public wRightMotorSpeed As UShort
    End Structure

    'The start of the thumbstick neutral zone.
    Private Const NeutralStart As Short = -16256 'Signed 16-bit (2-byte) integer range -32,768 through 32,767.

    'The end of the thumbstick neutral zone.
    Private Const NeutralEnd As Short = 16256

    'Set the trigger threshold to 64 or 1/4 pull.
    Private Const TriggerThreshold As Byte = 64 '63.75 = 255 / 4
    'The trigger position must be greater than the trigger threshold to register as pressed.

    Private ReadOnly Connected(0 To 3) As Boolean 'True or False

    Private ControllerNumber As Integer = 0

    Private ControllerPosition As XINPUT_STATE

    Private Vibration As XINPUT_VIBRATION

    Private ControllerA As Boolean = False

    Private ControllerB As Boolean = False

    Private ControllerRight As Boolean = False

    Private ControllerLeft As Boolean = False

    Private ControllerJumped As Boolean = False

    Private IsMouseDown As Boolean = False

    Private IsStartDown As Boolean = False

    Private IsBackgroundLoopPlaying As Boolean = False

    Private ClearScreenTimer As TimeSpan

    Private ClearScreenTimerStart As DateTime

    Private StopClearScreenTimer As Boolean = True

    Private GoalSelected As Boolean = False

    Private LevelSelected As Boolean = False

    Private LevelName As String = "Untitled"

    Private IsBDown As Boolean = False

    Private ScreenOffset As Point

    Private IsMuted As Boolean = False

    <StructLayout(LayoutKind.Sequential)>
    Private Structure INPUTStruc
        Public type As UInteger
        Public union As InputUnion
    End Structure

    <StructLayout(LayoutKind.Explicit)>
    Private Structure InputUnion
        <FieldOffset(0)>
        Public mi As MOUSEINPUT
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure MOUSEINPUT
        Public dx As Integer
        Public dy As Integer
        Public mouseData As UInteger
        Public dwFlags As UInteger
        Public time As UInteger
        Public dwExtraInfo As IntPtr
    End Structure

    Private Const INPUT_MOUSE As UInteger = 0
    Private Const MOUSEEVENTF_LEFTDOWN As UInteger = &H2
    Private Const MOUSEEVENTF_LEFTUP As UInteger = &H4

    <DllImport("user32.dll")>
    Private Shared Function SendInput(nInputs As UInteger, pInputs As INPUTStruc(), cbSize As Integer) As UInteger
    End Function

    Private Shared Sub ClickMouseLeft()
        ' Simulate a left mouse button down event
        Dim inputDown As New INPUTStruc()
        inputDown.type = INPUT_MOUSE
        inputDown.union.mi.dwFlags = MOUSEEVENTF_LEFTDOWN

        ' Simulate a left mouse button up event
        Dim inputUp As New INPUTStruc()
        inputUp.type = INPUT_MOUSE
        inputUp.union.mi.dwFlags = MOUSEEVENTF_LEFTUP

        ' Send the input events using SendInput
        Dim inputs As INPUTStruc() = {inputDown, inputUp}
        SendInput(CUInt(inputs.Length), inputs, Marshal.SizeOf(GetType(INPUTStruc)))
    End Sub
    Private Shared Sub DoMouseLeftDown()

        ' Simulate a left mouse button down event
        Dim inputDown As New INPUTStruc()
        inputDown.type = INPUT_MOUSE
        inputDown.union.mi.dwFlags = MOUSEEVENTF_LEFTDOWN

        ' Send the input events using SendInput
        Dim inputs As INPUTStruc() = {inputDown}
        SendInput(CUInt(inputs.Length), inputs, Marshal.SizeOf(GetType(INPUTStruc)))

    End Sub

    Private Shared Sub DoMouseLeftUp()

        ' Simulate a left mouse button up event
        Dim inputUp As New INPUTStruc
        inputUp.type = INPUT_MOUSE
        inputUp.union.mi.dwFlags = MOUSEEVENTF_LEFTUP

        ' Send the input events using SendInput
        Dim inputs As INPUTStruc() = {inputUp}
        SendInput(CUInt(inputs.Length), inputs, Marshal.SizeOf(GetType(INPUTStruc)))

    End Sub

    Private GameLoopTask As Task =
        Task.Factory.StartNew(Sub()
                                  Try

                                      Thread.CurrentThread.Priority = ThreadPriority.Normal
                                      'Thread.CurrentThread.SetApartmentState(ApartmentState.STA)

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

    Private Sub UpdateFrame()

        Select Case GameState

            Case AppState.Start

                UpdateControllerData()

            Case AppState.Playing

                UpdateControllerData()

                UpdateDeltaTime()

                UpdateOurHero()

                UpdateCamera()

                UpdateEnemies()

            Case AppState.Editing

                UpdateControllerData()

                UpdateEditorDeltaTime()

            Case AppState.Clear

                UpdateClearScreenTimer()

        End Select

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

            Case AppState.Clear

                DrawClearScreen()

        End Select

    End Sub

    Private Sub UpdateControllerData()

        UpdateControllerPosition()

    End Sub

    Private Sub UpdateDeltaTime()
        'Delta time (Δt) is the elapsed time since the last frame.

        CurrentFrame = Now

        DeltaTime = CurrentFrame - LastFrame 'Calculate delta time

        LastFrame = CurrentFrame 'Update last frame time

    End Sub

    Private Sub UpdateEditorDeltaTime()
        'Delta time (Δt) is the elapsed time since the last frame.

        EditorCurrentFrame = Now

        EditorDeltaTime = EditorCurrentFrame - EditorLastFrame 'Calculate delta time

        EditorLastFrame = EditorCurrentFrame 'Update last frame time

    End Sub

    Private Sub UpdateOurHero()

        If IsOnBlock() > -1 Then

            UpdateBlocks()

        ElseIf IsOnPlatform() > -1 Then

            'UpdatePlatform

        Else

            If OurHero.Velocity.Y >= 0 Then
                'Apply gravity to our hero. FALLING.

                If OurHero.Velocity.Y <= OurHero.MaxVelocity.Y Then

                    OurHero.Velocity.Y += Gravity * DeltaTime.TotalSeconds

                Else

                    OurHero.Velocity.Y = OurHero.MaxVelocity.Y

                End If

                ''Skydive steering
                'If RightArrowDown = True Or ControllerRight = True Then

                '    OurHero.Velocity.X += 25.5F * DeltaTime.TotalSeconds

                'ElseIf LeftArrowDown = True Or ControllerLeft = True Then

                '    OurHero.Velocity.X += -25.5F * DeltaTime.TotalSeconds

                'End If

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

        If IsOnBill() > -1 Then

            If Cash(IsOnBill).Collected = False Then

                Cash(IsOnBill).Collected = True

                CashCollected += 100

            End If

        End If

        If OurHero.Rect.IntersectsWith(Goal.Rect) = True Then

            DoGoalCollision()

        End If

        If IsOnEnemy() > -1 Then

            DoEnemyCollision()

        End If

        FellOffLevel()

        UpdateHeroMovement()

    End Sub

    Private Sub UpdateCamera()

        LookAhead()

        KeepCameraOnTheLevel()

    End Sub

    Private Sub DoGoalCollision()

        If GameState = AppState.Playing Then

            ClearScreenTimerStart = Now

            StopClearScreenTimer = False

            If IsBackgroundLoopPlaying = True Then

                My.Computer.Audio.Stop()

                IsBackgroundLoopPlaying = False

            End If

            GameState = AppState.Clear

        End If

    End Sub

    Private Sub DoEnemyCollision()

        If Enemies IsNot Nothing Then

            For Each Enemy In Enemies

                If Enemy.Eliminated = False Then

                    Dim Index As Integer = Array.IndexOf(Enemies, Enemy)

                    'Is our hero colliding with the Enemy?
                    If OurHero.Rect.IntersectsWith(Enemy.Rect) = True Then
                        'Yes, our hero is colliding with the Enemy.

                        'Is our hero falling?
                        If OurHero.Velocity.Y > 0 Then
                            'Yes, our hero is falling.

                            'Is our hero above the Enemy?
                            If OurHero.Position.Y <= Enemy.Rect.Top - OurHero.Rect.Height \ 2 Then
                                'Yes, our hero is above the enemy.

                                Enemies(Index).Eliminated = True

                            End If

                        Else

                            Camera.Rect.X = 0
                            Camera.Rect.Y = 0

                            BufferGridLines()

                            ResetCash()

                            ResurrectEnemies()

                            ResetOurHero()

                        End If

                    End If

                End If

            Next

        End If

    End Sub

    Private Sub DrawStartScreen()

        DrawBackground(Color.LightSkyBlue)

        DrawClouds()

        DrawBushes()

        DrawBlocks()

        DrawCash()

        DrawGoal()

        DrawEnemies()

        DrawOurHero()

        DrawCollectedCash()

        DrawTitle()

        DrawStartScreenNewButtonBackground()

        DrawStartScreenNewButton()

        DrawStartScreenOpenButtonBackground()

        DrawStartScreenOpenButton()

    End Sub

    Private Sub DrawPlaying()

        DrawBackground(Color.LightSkyBlue)

        DrawClouds()

        DrawBushes()

        DrawBlocks()

        DrawCash()

        DrawGoal()

        DrawEnemies()

        DrawOurHero()

        DrawCollectedCash()

        DrawFPS()

        DrawEditButton()

    End Sub

    Private Sub DrawEditing()

        DrawBackground(Color.LightSkyBlue)

        DrawClouds()

        DrawBushes()

        DrawBlocks()

        DrawCash()

        DrawGoal()

        DrawEnemies()

        DrawOurHero()

        DrawGridLines()

        DrawToolPreview()

        DrawToolBar()

        DrawPlayButton()

        DrawMenuButton()

        DrawFPS()

        If ShowMenu = True Then

            DrawMenuBackground()

            DrawSaveButton()

            DrawNewButton()

            DrawOpenButton()

            DrawExitButton()

        End If

    End Sub

    Private Sub DrawToolBar()

        DrawToolbarBackground()

        DrawPointerToolButton()

        DrawBlockToolButton()

        DrawBillToolButton()

        DrawCloudToolButton()

        DrawBushesToolButton()

        DrawGoalToolButton()

        DrawEnemyToolButton()

    End Sub

    Private Sub DrawClearScreen()

        DrawBackground(Color.Black)

        DrawClearTitle()

        DrawOurHero()

    End Sub

    Private Sub ResetOurHero()

        OurHero.Rect = New Rectangle(128, 769, 64, 64)

        OurHero.Position = New PointF(OurHero.Rect.X, OurHero.Rect.Y)

        OurHero.Velocity = New PointF(0, 0)

    End Sub

    Private Sub ResetCash()

        CashCollected = 0

        If Cash IsNot Nothing Then

            For Each Bill In Cash

                Cash(Array.IndexOf(Cash, Bill)).Collected = False

            Next

        End If

    End Sub

    Private Sub ResurrectEnemies()

        If Enemies IsNot Nothing Then

            For Each Enemy In Enemies

                Enemies(Array.IndexOf(Enemies, Enemy)).Eliminated = False

            Next

        End If

    End Sub

    Private Sub UpdateClearScreenTimer()

        If StopClearScreenTimer = False Then

            ClearScreenTimer = Now - ClearScreenTimerStart

            If ClearScreenTimer.TotalMilliseconds > 2000 Then

                StopClearScreenTimer = True

                Camera.Rect.X = 0
                Camera.Rect.Y = 0

                BufferGridLines()

                ResetCash()

                ResurrectEnemies()

                ResetOurHero()

                MovePointerOffScreen()

                LastFrame = Now

                GameState = AppState.Playing

                PlayLevelMusic()

            End If

        End If

    End Sub

    Private Sub PlayLevelMusic()

        If IsMuted = False Then

            Try

                My.Computer.Audio.Play(My.Resources.level,
                                       AudioPlayMode.BackgroundLoop)

                IsBackgroundLoopPlaying = True

            Catch ex As Exception

                IsBackgroundLoopPlaying = False

            End Try

        End If

    End Sub

    Private Sub UpdateControllerPosition()

        For ControllerNumber = 0 To 3 'Up to 4 controllers

            Try

                ' Check if the function call was successful
                If XInputGetState(ControllerNumber, ControllerPosition) = 0 Then
                    ' The function call was successful, so you can access the controller state now

                    Connected(ControllerNumber) = True

                    'Is controller zero connected?
                    If Connected(0) = True AndAlso ControllerNumber = 0 Then
                        'Yes, controller zero is connected.
                        'Use controller zero.

                        UpdateButtonPosition()

                        UpdateLeftThumbstickPosition()

                        UpdateRightThumbstickPosition()

                        UpdateRightTriggerPosition()

                    End If

                    'Is controller zero disconnected and controller one connected?
                    If Connected(0) = False AndAlso Connected(1) = True AndAlso ControllerNumber = 1 Then
                        'Yes, controller zero is disconnected and controller one is connected.
                        'Use controller one.

                        UpdateButtonPosition()

                        UpdateLeftThumbstickPosition()

                        UpdateRightThumbstickPosition()

                        UpdateRightTriggerPosition()

                    End If

                Else
                    ' The function call failed, so you cannot access the controller state

                    Connected(ControllerNumber) = False

                End If

            Catch ex As Exception

                MsgBox(ex.ToString)

                Exit Sub

            End Try

        Next

    End Sub

    Private Sub UpdateLeftThumbstickPosition()
        'The range on the X-axis is -32,768 through 32,767. Signed 16-bit (2-byte) integer.
        'The range on the Y-axis is -32,768 through 32,767. Signed 16-bit (2-byte) integer.

        'What position is the left thumbstick in on the X-axis?
        If ControllerPosition.Gamepad.sThumbLX <= NeutralStart Then
            'The left thumbstick is in the left position.

            Select Case GameState

                Case AppState.Start

                    If ControllerPosition.Gamepad.sThumbLX < -32000 Then

                        Cursor.Position = New Point(Cursor.Position.X - 14, Cursor.Position.Y)

                    Else

                        Cursor.Position = New Point(Cursor.Position.X - 1, Cursor.Position.Y)

                    End If

                Case AppState.Playing

                    ControllerLeft = True

                    ControllerRight = False

                Case AppState.Editing

                    If ControllerPosition.Gamepad.sThumbLX < -32000 Then

                        Cursor.Position = New Point(Cursor.Position.X - 14, Cursor.Position.Y)

                    Else

                        Cursor.Position = New Point(Cursor.Position.X - 1, Cursor.Position.Y)

                    End If

            End Select

        ElseIf ControllerPosition.Gamepad.sThumbLX >= NeutralEnd Then
            'The left thumbstick is in the right position.

            Select Case GameState

                Case AppState.Start

                    If ControllerPosition.Gamepad.sThumbLX > 32000 Then

                        Cursor.Position = New Point(Cursor.Position.X + 14, Cursor.Position.Y)

                    Else

                        Cursor.Position = New Point(Cursor.Position.X + 1, Cursor.Position.Y)

                    End If

                Case AppState.Playing

                    ControllerLeft = False

                    ControllerRight = True

                Case AppState.Editing

                    If ControllerPosition.Gamepad.sThumbLX > 32000 Then

                        Cursor.Position = New Point(Cursor.Position.X + 14, Cursor.Position.Y)

                    Else

                        Cursor.Position = New Point(Cursor.Position.X + 1, Cursor.Position.Y)

                    End If

            End Select

        Else
            'The left thumbstick is in the neutral position.

        End If

        'What position is the left thumbstick in on the Y-axis?
        If ControllerPosition.Gamepad.sThumbLY <= NeutralStart Then
            'The left thumbstick is in the down position.

            If GameState = AppState.Start Or GameState = AppState.Editing Then

                If ControllerPosition.Gamepad.sThumbLY < -32000 Then

                    Cursor.Position = New Point(Cursor.Position.X, Cursor.Position.Y + 10)

                Else

                    Cursor.Position = New Point(Cursor.Position.X, Cursor.Position.Y + 1)

                End If

            End If

        ElseIf ControllerPosition.Gamepad.sThumbLY >= NeutralEnd Then
            'The left thumbstick is in the up position.

            If GameState = AppState.Start Or GameState = AppState.Editing Then

                If ControllerPosition.Gamepad.sThumbLY > 32000 Then

                    Cursor.Position = New Point(Cursor.Position.X, Cursor.Position.Y - 10)

                Else

                    Cursor.Position = New Point(Cursor.Position.X, Cursor.Position.Y - 1)

                End If

            End If

        Else
            'The left thumbstick is in the neutral position.

        End If

    End Sub

    Private Sub UpdateRightTriggerPosition()
        'The range of right trigger is 0 to 255. Unsigned 8-bit (1-byte) integer.
        'The trigger position must be greater than the trigger threshold to register as pressed.

        'What position is the right trigger in?
        If ControllerPosition.Gamepad.bRightTrigger > TriggerThreshold Then
            'The right trigger is in the down position. Trigger Break. Bang!

            If GameState = AppState.Editing Then

                If ShowMenu = True Then

                    'Move cursor over the new button.
                    Cursor.Position = New Point(ScreenOffset.X + NewButton.Rect.X, ScreenOffset.Y + NewButton.Rect.Y)

                    If IsMouseDown = False Then

                        IsMouseDown = True

                        DoMouseLeftDown()

                    End If

                End If

            End If

        Else
            'The right trigger is in the neutral position. Pre-Travel.

        End If

    End Sub

    Private Sub UpdateRightThumbstickPosition()
        'The range on the X-axis is -32,768 through 32,767. Signed 16-bit (2-byte) integer.
        'The range on the Y-axis is -32,768 through 32,767. Signed 16-bit (2-byte) integer.

        'What position is the right thumbstick in on the X-axis?
        If ControllerPosition.Gamepad.sThumbRX <= NeutralStart Then
            'The right thumbstick is in the left position.

            If GameState = AppState.Editing Then

                If ShowMenu = False Then

                    'Move Camera to the left.
                    Camera.Rect.X += 10

                    BufferGridLines()

                End If

            End If

        ElseIf ControllerPosition.Gamepad.sThumbRX >= NeutralEnd Then
            'The right thumbstick is in the right position.

            If GameState = AppState.Editing Then

                If ShowMenu = False Then

                    'Move Camera to the left.
                    Camera.Rect.X -= 10

                    BufferGridLines()

                End If

            End If

        Else
            'The right thumbstick is in the neutral position.

        End If

        'What position is the right thumbstick in on the Y-axis?
        If ControllerPosition.Gamepad.sThumbRY <= NeutralStart Then
            'The right thumbstick is in the up position.

            If GameState = AppState.Editing Then

                If ShowMenu = False Then

                    'Move Camera to the up.
                    Camera.Rect.Y -= 10

                    BufferGridLines()

                End If

            End If

        ElseIf ControllerPosition.Gamepad.sThumbRY >= NeutralEnd Then
            'The right thumbstick is in the down position.

            If GameState = AppState.Editing Then

                If ShowMenu = False Then

                    'Move Camera to the down.
                    Camera.Rect.Y += 10

                    BufferGridLines()

                End If

            End If

        Else
            'The right thumbstick is in the neutral position.

        End If

    End Sub


    Private Sub LookAhead()

        'Is our hero near the right side of the frame?
        If OurHero.Rect.X > (Camera.Rect.X * -1) + Camera.Rect.Width / 1.5 Then
            'Yes, our hero is near the right side of the frame.

            'Move camera to the right.
            Camera.Rect.X = OurHero.Rect.Left * -1 + Camera.Rect.Width / 1.5

        End If

        'Is our hero near the left side of the frame?
        If OurHero.Rect.X < (Camera.Rect.X * -1) + Camera.Rect.Width / 4 Then
            'Yes, our hero is near the left side of the frame.

            'Move camera to the left.
            Camera.Rect.X = OurHero.Rect.Left * -1 + Camera.Rect.Width / 4

        End If

        'Is our hero near the bottom side of the frame?
        If OurHero.Rect.Y > (Camera.Rect.Y * -1) + Camera.Rect.Height / 1.25 Then
            'Yes, our hero is near the bottom side of the frame.

            'Move camera down.
            Camera.Rect.Y = OurHero.Rect.Top * -1 + Camera.Rect.Height / 1.25

        End If

        'Is our hero near the top side of the frame?
        If OurHero.Rect.Y < (Camera.Rect.Y * -1) + Camera.Rect.Height / 6 Then
            'Yes, our hero is near the top side of the frame.

            'Move camera up.
            Camera.Rect.Y = OurHero.Rect.Top * -1 + Camera.Rect.Height / 6

        End If

    End Sub

    Private Sub KeepCameraOnTheLevel()

        'Is the Camera off to left side of the level? checked
        If (Camera.Rect.X * -1) < Level.Rect.Left Then
            'Yes, the Camera is off the level.

            'Aline camera to the left side of the level.
            Camera.Rect.X = Level.Rect.Left * -1

        End If

        'Is the Camera off to right side of the level? checked
        If (Camera.Rect.X * -1) + Camera.Rect.Width > Level.Rect.Right Then
            'Yes, the Camera is off the level.

            'Aline camera to the right side of the level.
            Camera.Rect.X = Level.Rect.Right * -1 + Camera.Rect.Width

        End If

        'Is the Camera off to top side of the level? checked
        If (Camera.Rect.Y * -1) < Level.Rect.Top Then
            'Yes, the Camera is off the level.

            'Aline camera to the top side of the level. 
            Camera.Rect.Y = Level.Rect.Top * -1

        End If

        'Is the Camera off to bottom side of the level? checked
        If (Camera.Rect.Y * -1) + Camera.Rect.Height > Level.Rect.Bottom Then
            'Yes, the Camera is off the level.

            'Aline camera to the bottom side of the level.
            Camera.Rect.Y = (Level.Rect.Bottom * -1) + Camera.Rect.Height

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

    Private Sub UpdateEnemies()

        If Enemies IsNot Nothing Then

            For Each Enemy In Enemies

                If Enemy.Eliminated = False Then

                    Dim Index As Integer = Array.IndexOf(Enemies, Enemy)

                    If Enemy.PatrolDirection = Direction.Right Then

                        'Move Enemy to the right.
                        Enemies(Index).Velocity.X += Enemy.Acceleration.X * DeltaTime.TotalSeconds

                        'Limit Enemy velocity to the max.
                        If Enemy.Velocity.X > Enemy.MaxVelocity.X Then

                            Enemies(Index).Velocity.X = Enemy.MaxVelocity.X

                        End If

                    Else

                        'Move Enemy to the left.
                        Enemies(Index).Velocity.X += -Enemy.Acceleration.X * DeltaTime.TotalSeconds

                        'Limit Enemy velocity to the max.
                        If Enemy.Velocity.X < -Enemy.MaxVelocity.X Then

                            Enemies(Index).Velocity.X = -Enemy.MaxVelocity.X

                        End If

                    End If

                    Enemies(Index).Position.X += Enemy.Velocity.X * DeltaTime.TotalSeconds

                    If Enemy.Position.X >= Enemy.PatrolB.X Then

                        'Is Enemy moving to the right?
                        If Enemy.Velocity.X > 0 Then

                            'Stop the move before change in direction.
                            Enemies(Index).Velocity.X = 0 'Zero speed.

                            'Aline the enemy to the patrol b point.
                            Enemy.Position.X = Enemy.PatrolB.X

                            Enemies(Index).PatrolDirection = Direction.Left

                        End If

                    End If

                    If Enemy.Position.X <= Enemy.PatrolA.X Then

                        'Is Enemy moving to the left?
                        If Enemy.Velocity.X < 0 Then

                            'Stop the move before change in direction.
                            Enemies(Index).Velocity.X = 0 'Zero speed.

                            'Aline the enemy to the patrol a point.
                            Enemy.Position.X = Enemy.PatrolA.X

                            Enemies(Index).PatrolDirection = Direction.Right

                        End If

                    End If

                    Enemies(Index).Rect.X = Math.Round(Enemy.Position.X)

                End If

            Next

        End If

    End Sub

    Private Sub UpdateBlocks()

        If Blocks IsNot Nothing Then

            For Each Block In Blocks

                'Is our hero colliding with the block?
                If OurHero.Rect.IntersectsWith(Block.Rect) = True Then
                    'Yes, our hero is colliding with the block.

                    'Is our hero falling?
                    If OurHero.Velocity.Y > 0 Then
                        'Yes, our hero is falling.

                        'Stop the fall.
                        OurHero.Velocity.Y = 0

                        'Is our hero above the block?
                        If OurHero.Position.Y <= Block.Rect.Top - OurHero.Rect.Height \ 2 Then
                            'Yes, our hero is above the block.

                            'Is our hero on top of the block?
                            If OurHero.Position.Y <> Block.Rect.Top - OurHero.Rect.Height + 1 Then
                                'No, our hero is NOT on top of the block.

                                'Place our hero on top of the block.
                                OurHero.Position.Y = Block.Rect.Top - OurHero.Rect.Height + 1

                            End If

                        End If

                        'Is our hero jumping?
                    ElseIf OurHero.Velocity.Y < 0 Then
                        'Yes, our hero is jumping.

                        'Stop the jump.
                        OurHero.Velocity.Y = 0
                        'OurHero.Velocity.X = 0

                        'VibrateRight(0, 65535)

                        If OurHero.Position.Y > Block.Rect.Bottom - 10 Then
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
                        'NOT FALLING OR JUMPING

                        'Is our hero on top of the block.
                        If OurHero.Position.Y = Block.Rect.Top - OurHero.Rect.Height + 1 Then
                            'Yes, our hero is on top of the block.

                            'Is the player holding down the right arrow key?
                            If RightArrowDown = True Or ControllerRight = True Then
                                'Yes, the player is holding down the right arrow key.

                                'Is our hero moving to the left?
                                If OurHero.Velocity.X < 0 Then

                                    'Stop the move before change in direction.
                                    OurHero.Velocity.X = 0 'Zero speed.

                                End If

                                'Move our hero the right.
                                OurHero.Velocity.X += OurHero.Acceleration.X * DeltaTime.TotalSeconds

                                'Limit our heros velocity to the max.
                                If OurHero.Velocity.X > OurHero.MaxVelocity.X Then OurHero.Velocity.X = OurHero.MaxVelocity.X

                                'Is the player holding down the left arrow key?
                            ElseIf LeftArrowDown = True Or ControllerLeft = True Then
                                'Yes, the player is holding down the left arrow key.

                                'Is our hero moving to the right?
                                If OurHero.Velocity.X > 0F Then
                                    'Yes, our hero is moving to the right.

                                    'Stop the move before change in direction.
                                    OurHero.Velocity.X = 0F 'Zero speed.

                                End If

                                'Move our hero the left.
                                OurHero.Velocity.X += -OurHero.Acceleration.X * DeltaTime.TotalSeconds

                                'Limit our heros velocity to the max.
                                If OurHero.Velocity.X < -OurHero.MaxVelocity.X Then OurHero.Velocity.X = -OurHero.MaxVelocity.X

                            Else
                                'No,the player is NOT holding down the right arrow key.
                                'No, the player is NOT holding down the left arrow key.

                                'Is our hero moving to the right?
                                If OurHero.Velocity.X > 0F Then
                                    'Yes, our hero is moving to the right.

                                    'Slow our hero down.
                                    OurHero.Velocity.X += -Friction * DeltaTime.TotalSeconds

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

                                    OurHero.Velocity.Y += -1100.0F

                                    Jumped = True

                                End If

                            End If

                            If ControllerA = True Then

                                If ControllerJumped = False Then

                                    OurHero.Velocity.Y += -1100.0F

                                    ControllerJumped = True

                                End If

                            End If

                        Else
                            'No, our hero is NOT on top of the block.

                            'Stop the move
                            OurHero.Velocity.X = 0

                            'VibrateRight(0, 65535)


                            'Is our hero on the right side of the block?
                            If OurHero.Position.X > Block.Rect.Left Then
                                'Yes, our hero is on the right side of the block.

                                'Aline our hero to the right of the block.
                                OurHero.Position.X = Block.Rect.Right

                            Else
                                'No, our hero is on the left side of the block.

                                'Aline our hero to the left of the block.
                                OurHero.Position.X = Block.Rect.Left - OurHero.Rect.Width

                            End If

                        End If

                    End If

                End If

            Next

        End If

    End Sub

    Private Sub DrawOurHero()

        With Buffer.Graphics

            Dim rectOffset As Rectangle = OurHero.Rect

            rectOffset.Offset(Camera.Rect.Location)

            .FillRectangle(Brushes.Red, rectOffset)

            .DrawString("Hero", CWJFont, Brushes.White, rectOffset, AlineCenterMiddle)

            'Draw hero position
            .DrawString("X: " & OurHero.Position.X.ToString & vbCrLf & "Y: " & OurHero.Position.Y.ToString,
                        CWJFont,
                        Brushes.White,
                        rectOffset.X,
                        rectOffset.Y - 50,
                        New StringFormat With {.Alignment = StringAlignment.Near})

        End With

    End Sub

    Private Sub DrawEnemies()

        With Buffer.Graphics

            If Enemies IsNot Nothing Then

                For Each Enemy In Enemies

                    Select Case GameState

                        Case AppState.Playing

                            If Enemy.Eliminated = False Then

                                Dim rectOffset As Rectangle = Enemy.Rect

                                rectOffset.Offset(Camera.Rect.Location)

                                .FillRectangle(Brushes.Chocolate, rectOffset)

                                .DrawString("E", EnemyFont, Brushes.PaleGoldenrod, rectOffset, AlineCenterMiddle)

                            End If

                        Case AppState.Editing

                            Dim PatrolAOffset As New Rectangle(New Point(Enemy.PatrolA.X, Enemy.PatrolA.Y), New Drawing.Size(GridSize, GridSize))

                            PatrolAOffset.Offset(Camera.Rect.Location)

                            .FillRectangle(Brushes.Chocolate, PatrolAOffset)

                            .DrawString("E", EnemyFont, Brushes.PaleGoldenrod, PatrolAOffset, AlineCenterMiddle)

                            Dim PatrolBOffset As New Rectangle(New Point(Enemy.PatrolB.X, Enemy.PatrolB.Y), New Drawing.Size(GridSize, GridSize))

                            PatrolBOffset.Offset(Camera.Rect.Location)

                            .FillRectangle(New SolidBrush(Color.FromArgb(128, Color.Chocolate)), PatrolBOffset)

                            .DrawString("E", EnemyFont, New SolidBrush(Color.FromArgb(128, Color.PaleGoldenrod)), PatrolBOffset, AlineCenterMiddle)

                            Dim SpanWidth As Integer = Enemy.PatrolB.X - Enemy.PatrolA.X - GridSize

                            Dim SpanOffset As New Rectangle(New Point(Enemy.PatrolA.X + GridSize, Enemy.PatrolA.Y), New Drawing.Size(SpanWidth, GridSize))

                            SpanOffset.Offset(Camera.Rect.Location)

                            .FillRectangle(New SolidBrush(Color.FromArgb(128, Color.Chocolate)), SpanOffset)

                            If SelectedEnemy = Array.IndexOf(Enemies, Enemy) Then

                                Dim SelectionSize As New Size((Enemy.PatrolB.X + GridSize) - Enemy.PatrolA.X, GridSize)

                                Dim SelectionOffset As New Rectangle(New Point(Enemy.PatrolA.X, Enemy.PatrolA.Y), SelectionSize)

                                SelectionOffset.Offset(Camera.Rect.Location)

                                'Draw selection rectangle.
                                .DrawRectangle(New Pen(Color.Red, 6), SelectionOffset)

                                'Position sizing handle.
                                SizingHandle.X = SelectionOffset.Right - SizingHandle.Width \ 2
                                SizingHandle.Y = SelectionOffset.Bottom - SizingHandle.Height \ 2

                                'Draw sizing handle.
                                .FillRectangle(Brushes.Black,
                                               SizingHandle)

                            End If

                    End Select

                Next

            End If

        End With

    End Sub

    Private Sub DrawGoal()

        If Buffer.Graphics IsNot Nothing Then

            With Buffer.Graphics

                Dim rectOffset As Rectangle = Goal.Rect

                rectOffset.Offset(Camera.Rect.Location)

                .FillRectangle(Brushes.White, rectOffset)

                ' Define the rectangle to be filled
                Dim rect As RectangleF = rectOffset

                rect.Inflate(rect.Width / 6.4F, rect.Height / 6.4F)

                ' Define the center point of the gradient
                Dim center As New PointF(rect.Left + rect.Width / 2.0F, rect.Top + rect.Height / 2.0F)

                ' Define the colors for the gradient stops
                Dim colors() As Color = {Color.Yellow, Color.White}

                ' Create the path for the gradient brush
                Dim GradPath As New GraphicsPath()
                GradPath.AddEllipse(rect)

                ' Create the gradient brush
                Dim GradBrush As New PathGradientBrush(GradPath) With {
                    .CenterPoint = center,
                    .CenterColor = colors(0),
                    .SurroundColors = New Color() {colors(1)}
                }

                .FillRectangle(GradBrush, rectOffset)

                If Goal.Rect.Width <= Goal.Rect.Height Then
                    Dim Font As New Font(New FontFamily("Wingdings"), Goal.Rect.Width \ 2, FontStyle.Regular)

                    .DrawString("«",
                            Font,
                            Brushes.Green,
                            rectOffset,
                            AlineCenterMiddle)

                Else
                    Dim Font As New Font(New FontFamily("Wingdings"), Goal.Rect.Height \ 2, FontStyle.Regular)

                    .DrawString("«",
                            Font,
                            Brushes.Green,
                            rectOffset,
                            AlineCenterMiddle)

                End If

                If GameState = AppState.Editing Then

                    If GoalSelected = True Then

                        'Draw selection rectangle.
                        .DrawRectangle(New Pen(Color.Red, 6), rectOffset)

                        'Position sizing handle.
                        SizingHandle.X = rectOffset.Right - SizingHandle.Width \ 2
                        SizingHandle.Y = rectOffset.Bottom - SizingHandle.Height \ 2

                        'Draw sizing handle.
                        .FillRectangle(Brushes.Black,
                                       SizingHandle)

                    End If

                End If

            End With

        End If

    End Sub

    Private Sub DrawBlocks()

        With Buffer.Graphics

            If Blocks IsNot Nothing Then

                For Each Block In Blocks

                    Dim rectOffset As Rectangle = Block.Rect

                    rectOffset.Offset(Camera.Rect.Location)

                    .FillRectangle(Brushes.Chocolate, rectOffset)

                    If GameState = AppState.Editing Then

                        If SelectedBlock = Array.IndexOf(Blocks, Block) Then

                            'Draw selection rectangle.
                            .DrawRectangle(New Pen(Color.Red, 6), rectOffset)

                            'Position sizing handle.
                            SizingHandle.X = rectOffset.Right - SizingHandle.Width \ 2
                            SizingHandle.Y = rectOffset.Bottom - SizingHandle.Height \ 2

                            'Draw sizing handle.
                            .FillRectangle(Brushes.Black,
                                           SizingHandle)

                        End If

                    End If

                Next

            End If

        End With

    End Sub

    Private Sub DrawBushes()

        With Buffer.Graphics

            If Bushes IsNot Nothing Then

                For Each Bush In Bushes

                    Dim rectOffset As Rectangle = Bush.Rect

                    rectOffset.Offset(Camera.Rect.Location)

                    .FillRectangle(Brushes.GreenYellow, rectOffset)

                    .DrawLine(SeaGreenPen, rectOffset.Right - 10, rectOffset.Top + 10, rectOffset.Right - 10, rectOffset.Bottom - 10)

                    .DrawLine(SeaGreenPen, rectOffset.Left + 10, rectOffset.Bottom - 10, rectOffset.Right - 10, rectOffset.Bottom - 10)

                    .DrawRectangle(OutinePen, rectOffset)

                    If GameState = AppState.Editing Then

                        If SelectedBush = Array.IndexOf(Bushes, Bush) Then

                            'Draw selection rectangle.
                            .DrawRectangle(New Pen(Color.Red, 6), rectOffset)

                            'Position sizing handle.
                            SizingHandle.X = rectOffset.Right - SizingHandle.Width \ 2
                            SizingHandle.Y = rectOffset.Bottom - SizingHandle.Height \ 2

                            'Draw sizing handle.
                            .FillRectangle(Brushes.Black,
                                           SizingHandle)

                        End If

                    End If

                Next

            End If

        End With

    End Sub

    Private Sub DrawClouds()

        With Buffer.Graphics

            If Clouds IsNot Nothing Then

                For Each Cloud In Clouds

                    Dim rectOffset As Rectangle = Cloud.Rect

                    rectOffset.Offset(Camera.Rect.Location)

                    .FillRectangle(Brushes.White, rectOffset)

                    .DrawLine(LightSkyBluePen, rectOffset.Right - 10, rectOffset.Top + 10, rectOffset.Right - 10, rectOffset.Bottom - 10)

                    .DrawLine(LightSkyBluePen, rectOffset.Left + 10, rectOffset.Bottom - 10, rectOffset.Right - 10, rectOffset.Bottom - 10)

                    .DrawRectangle(OutinePen, rectOffset)

                    If GameState = AppState.Editing Then

                        If SelectedCloud = Array.IndexOf(Clouds, Cloud) Then

                            'Draw selection rectangle.
                            .DrawRectangle(New Pen(Color.Red, 6), rectOffset)

                            'Position sizing handle.
                            SizingHandle.X = rectOffset.Right - SizingHandle.Width \ 2
                            SizingHandle.Y = rectOffset.Bottom - SizingHandle.Height \ 2

                            'Draw sizing handle.
                            .FillRectangle(Brushes.Black,
                                           SizingHandle)

                        End If

                    End If

                Next

            End If

        End With

    End Sub

    Private Sub DrawCash()

        With Buffer.Graphics

            If Cash IsNot Nothing Then

                For Each Bill In Cash

                    Dim rectOffset As Rectangle = Bill.Rect

                    rectOffset.Offset(Camera.Rect.Location)

                    Select Case GameState

                        Case AppState.Start

                            If Bill.Collected = False Then

                                .FillRectangle(Brushes.Goldenrod, rectOffset)

                                .DrawString("$", FPSFont, Brushes.OrangeRed, rectOffset, AlineCenterMiddle)

                            End If

                        Case AppState.Playing

                            If Bill.Collected = False Then

                                .FillRectangle(Brushes.Goldenrod, rectOffset)

                                .DrawString("$", FPSFont, Brushes.OrangeRed, rectOffset, AlineCenterMiddle)

                            End If

                        Case AppState.Editing

                            .FillRectangle(Brushes.Goldenrod, rectOffset)

                            .DrawString("$", FPSFont, Brushes.OrangeRed, rectOffset, AlineCenterMiddle)

                            If SelectedBill = Array.IndexOf(Cash, Bill) Then

                                'Draw selection rectangle.
                                .DrawRectangle(New Pen(Color.Red, 6), rectOffset)

                            End If

                    End Select

                Next

            End If

        End With

    End Sub

    Private Sub DrawCollectedCash()

        With Buffer.Graphics

            'Draw drop shadow.
            .DrawString("$" & CashCollected.ToString,
                    FPSFont,
                    New SolidBrush(Color.FromArgb(255, Color.Black)),
                    CashCollectedPostion.X + 2,
                    CashCollectedPostion.Y + 2)

            .DrawString("$" & CashCollected.ToString, FPSFont, Brushes.White, CashCollectedPostion)

        End With

    End Sub

    Private Sub DrawToolPreview()

        With Buffer.Graphics

            If ShowToolPreview = True Then

                Dim rectOffset As Rectangle = ToolPreview

                rectOffset.Offset(Camera.Rect.Location)

                Select Case SelectedTool

                    Case Tools.Block

                        .FillRectangle(Brushes.Chocolate, rectOffset)

                    Case Tools.Bill

                        .FillRectangle(Brushes.Goldenrod, rectOffset)

                        .DrawString("$", FPSFont, Brushes.OrangeRed, rectOffset, AlineCenterMiddle)

                    Case Tools.Cloud

                        .FillRectangle(Brushes.White, rectOffset)

                        .DrawLine(LightSkyBluePen, rectOffset.Right - 10, rectOffset.Top + 10, rectOffset.Right - 10, rectOffset.Bottom - 10)

                        .DrawLine(LightSkyBluePen, rectOffset.Left + 10, rectOffset.Bottom - 10, rectOffset.Right - 10, rectOffset.Bottom - 10)

                        .DrawRectangle(OutinePen, rectOffset)

                    Case Tools.Bush

                        .FillRectangle(Brushes.GreenYellow, rectOffset)

                        .DrawLine(SeaGreenPen, rectOffset.Right - 10, rectOffset.Top + 10, rectOffset.Right - 10, rectOffset.Bottom - 10)

                        .DrawLine(SeaGreenPen, rectOffset.Left + 10, rectOffset.Bottom - 10, rectOffset.Right - 10, rectOffset.Bottom - 10)

                        .DrawRectangle(OutinePen, rectOffset)

                    Case Tools.Enemy

                        .FillRectangle(Brushes.Chocolate, rectOffset)

                        .DrawString("E", EnemyFont, Brushes.PaleGoldenrod, rectOffset, AlineCenterMiddle)

                        Dim PatrolB As New Rectangle(rectOffset.X + GridSize, rectOffset.Y, GridSize, GridSize)

                        .FillRectangle(New SolidBrush(Color.FromArgb(128, Color.Chocolate)), PatrolB)

                        .DrawString("E",
                                    EnemyFont,
                                    New SolidBrush(Color.FromArgb(128, Color.PaleGoldenrod)),
                                    PatrolB,
                                    AlineCenterMiddle)

                    Case Tools.Goal

                        .FillRectangle(Brushes.White, rectOffset)

                        ' Define the rectangle to be filled
                        Dim rect As RectangleF = rectOffset

                        rect.Inflate(rect.Width / 6.4F, rect.Height / 6.4F)

                        ' Define the center point of the gradient
                        Dim center As New PointF(rect.Left + rect.Width / 2.0F, rect.Top + rect.Height / 2.0F)

                        ' Define the colors for the gradient stops
                        Dim colors() As Color = {Color.Yellow, Color.White}

                        ' Create the path for the gradient brush
                        Dim GradPath As New GraphicsPath()
                        GradPath.AddEllipse(rect)

                        ' Create the gradient brush
                        Dim GradBrush As New PathGradientBrush(GradPath) With {
                            .CenterPoint = center,
                            .CenterColor = colors(0),
                            .SurroundColors = New Color() {colors(1)}
                        }

                        .FillRectangle(GradBrush, rectOffset)

                        If rectOffset.Width <= rectOffset.Height Then

                            Dim Font As New Font(New FontFamily("Wingdings"), rectOffset.Width \ 2, FontStyle.Regular)

                            .DrawString("«",
                                    Font,
                                    Brushes.Green,
                                    rectOffset,
                                    AlineCenterMiddle)

                        Else

                            Dim Font As New Font(New FontFamily("Wingdings"), rectOffset.Height \ 2, FontStyle.Regular)

                            .DrawString("«",
                                    Font,
                                    Brushes.Green,
                                    rectOffset,
                                    AlineCenterMiddle)

                        End If

                End Select

            End If

        End With

    End Sub

    Private Sub DrawToolbarBackground()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black, ToolBarBackground.Rect)

        End With

    End Sub

    Private Sub DrawMenuBackground()

        With Buffer.Graphics

            Dim Shadow As Rectangle = MenuBackground.Rect

            Shadow.Offset(9, 9)

            '.FillRectangle(New SolidBrush(Color.FromArgb(128, Color.Black)), Shadow)

            .DrawRectangle(MenuShadowPen, Shadow)

            .DrawRectangle(MenuOutinePen, MenuBackground.Rect)

            .FillRectangle(Brushes.Black, MenuBackground.Rect)

        End With

    End Sub

    Private Sub DrawStartScreenNewButtonBackground()

        With Buffer.Graphics

            Dim Shadow As Rectangle = StartScreenNewButton.Rect

            Shadow.Offset(9, 9)

            .DrawRectangle(MenuShadowPen, Shadow)

            .DrawRectangle(MenuOutinePen, StartScreenNewButton.Rect)

            .FillRectangle(Brushes.Black, StartScreenNewButton.Rect)

        End With

    End Sub

    Private Sub DrawStartScreenOpenButtonBackground()

        With Buffer.Graphics

            Dim Shadow As Rectangle = StartScreenOpenButton.Rect

            Shadow.Offset(9, 9)

            .DrawRectangle(MenuShadowPen, Shadow)

            .DrawRectangle(MenuOutinePen, StartScreenOpenButton.Rect)

            .FillRectangle(Brushes.Black, StartScreenOpenButton.Rect)

        End With

    End Sub

    Private Sub DrawPointerToolButton()

        With Buffer.Graphics

            If SelectedTool = Tools.Pointer Then

                .FillRectangle(DarkCharcoalGreyBrush, PointerToolButton.Rect)

                .DrawString("ë",
                            PointerToolFont,
                            Brushes.White,
                            PointerToolButton.Rect,
                            AlineCenterMiddle)

            Else

                .FillRectangle(Brushes.Black, PointerToolButton.Rect)

                .DrawString("ë",
                            PointerToolFont,
                            Brushes.White,
                            PointerToolButton.Rect,
                            AlineCenterMiddle)

            End If

        End With

    End Sub

    Private Sub DrawBlockToolButton()

        With Buffer.Graphics

            If SelectedTool = Tools.Block Then

                .FillRectangle(DarkCharcoalGreyBrush, BlockToolButton.Rect)

                .FillRectangle(Brushes.Chocolate, BlockToolIcon.Rect)

            Else

                .FillRectangle(Brushes.Black, BlockToolButton.Rect)

                .FillRectangle(Brushes.Chocolate, BlockToolIcon.Rect)

            End If

        End With

    End Sub

    Private Sub DrawBillToolButton()

        With Buffer.Graphics

            If SelectedTool = Tools.Bill Then

                .FillRectangle(DarkCharcoalGreyBrush, BillToolButton.Rect)

                .FillRectangle(Brushes.Goldenrod, BillToolIcon.Rect)

                .DrawString("$",
                            BillIconFont,
                            Brushes.OrangeRed,
                            BillToolIcon.Rect,
                            AlineCenterMiddle)

            Else

                .FillRectangle(Brushes.Black, BillToolButton.Rect)

                .FillRectangle(Brushes.Goldenrod, BillToolIcon.Rect)

                .DrawString("$",
                            BillIconFont,
                            Brushes.OrangeRed,
                            BillToolIcon.Rect,
                            AlineCenterMiddle)

            End If

        End With

    End Sub

    Private Sub DrawCloudToolButton()

        With Buffer.Graphics

            If SelectedTool = Tools.Cloud Then

                .FillRectangle(DarkCharcoalGreyBrush, CloudToolButton.Rect)

                .FillRectangle(Brushes.White, CloundToolIcon.Rect)

                .DrawLine(CloundToolIconPen,
                          CloundToolIcon.Rect.Right - 6,
                          CloundToolIcon.Rect.Top + 6,
                          CloundToolIcon.Rect.Right - 6,
                          CloundToolIcon.Rect.Bottom - 6)

                .DrawLine(CloundToolIconPen,
                          CloundToolIcon.Rect.Left + 6,
                          CloundToolIcon.Rect.Bottom - 6,
                          CloundToolIcon.Rect.Right - 6,
                          CloundToolIcon.Rect.Bottom - 6)

                .DrawRectangle(CloundToolIconOutinePen, CloundToolIcon.Rect)

            Else

                .FillRectangle(Brushes.Black, CloudToolButton.Rect)

                .FillRectangle(Brushes.White, CloundToolIcon.Rect)

                .DrawLine(CloundToolIconPen,
                          CloundToolIcon.Rect.Right - 6,
                          CloundToolIcon.Rect.Top + 6,
                          CloundToolIcon.Rect.Right - 6,
                          CloundToolIcon.Rect.Bottom - 6)

                .DrawLine(CloundToolIconPen,
                          CloundToolIcon.Rect.Left + 6,
                          CloundToolIcon.Rect.Bottom - 6,
                          CloundToolIcon.Rect.Right - 6,
                          CloundToolIcon.Rect.Bottom - 6)

                .DrawRectangle(CloundToolIconOutinePen, CloundToolIcon.Rect)

            End If

        End With

    End Sub

    Private Sub DrawBushesToolButton()

        With Buffer.Graphics

            If SelectedTool = Tools.Bush Then

                .FillRectangle(DarkCharcoalGreyBrush, BushToolButton.Rect)

                .FillRectangle(Brushes.GreenYellow, BushToolIcon.Rect)

                .DrawLine(BushToolIconPen,
                          BushToolIcon.Rect.Right - 6,
                          BushToolIcon.Rect.Top + 6,
                          BushToolIcon.Rect.Right - 6,
                          BushToolIcon.Rect.Bottom - 6)

                .DrawLine(BushToolIconPen,
                          BushToolIcon.Rect.Left + 6,
                          BushToolIcon.Rect.Bottom - 6,
                          BushToolIcon.Rect.Right - 6,
                          BushToolIcon.Rect.Bottom - 6)

                .DrawRectangle(BushToolIconOutinePen, BushToolIcon.Rect)

            Else

                .FillRectangle(Brushes.Black, BushToolButton.Rect)

                .FillRectangle(Brushes.GreenYellow, BushToolIcon.Rect)

                .DrawLine(BushToolIconPen,
                          BushToolIcon.Rect.Right - 6,
                          BushToolIcon.Rect.Top + 6,
                          BushToolIcon.Rect.Right - 6,
                          BushToolIcon.Rect.Bottom - 6)

                .DrawLine(BushToolIconPen,
                          BushToolIcon.Rect.Left + 6,
                          BushToolIcon.Rect.Bottom - 6,
                          BushToolIcon.Rect.Right - 6,
                          BushToolIcon.Rect.Bottom - 6)

                .DrawRectangle(BushToolIconOutinePen, BushToolIcon.Rect)

            End If

        End With

    End Sub

    Private Sub DrawEnemyToolButton()

        With Buffer.Graphics

            If SelectedTool = Tools.Enemy Then

                .FillRectangle(DarkCharcoalGreyBrush, EnemyToolButton.Rect)

                .FillRectangle(Brushes.Chocolate, EnemyToolIcon.Rect)

                .DrawString("E", EnemyIconFont, Brushes.PaleGoldenrod, EnemyToolIcon.Rect, AlineCenterMiddle)

            Else

                .FillRectangle(Brushes.Black, EnemyToolButton.Rect)

                .FillRectangle(Brushes.Chocolate, EnemyToolIcon.Rect)

                .DrawString("E", EnemyIconFont, Brushes.PaleGoldenrod, EnemyToolIcon.Rect, AlineCenterMiddle)

            End If

        End With

    End Sub

    Private Sub DrawGoalToolButton()

        With Buffer.Graphics

            If SelectedTool = Tools.Goal Then

                .FillRectangle(DarkCharcoalGreyBrush, GoalToolButton.Rect)

                .FillRectangle(Brushes.White, GoalToolIcon.Rect)

                ' Define the rectangle to be filled
                Dim rect As RectangleF = GoalToolIcon.Rect

                rect.Inflate(rect.Width / 6.4F, rect.Height / 6.4F)

                ' Define the center point of the gradient
                Dim center As New PointF(rect.Left + rect.Width / 2.0F, rect.Top + rect.Height / 2.0F)

                ' Define the colors for the gradient stops
                Dim colors() As Color = {Color.Yellow, Color.White}

                ' Create the path for the gradient brush
                Dim GradPath As New GraphicsPath()
                GradPath.AddEllipse(rect)

                ' Create the gradient brush
                Dim GradBrush As New PathGradientBrush(GradPath) With {
                    .CenterPoint = center,
                    .CenterColor = colors(0),
                    .SurroundColors = New Color() {colors(1)}
                }

                .FillRectangle(GradBrush, GoalToolIcon.Rect)

                Dim Font As New Font(New FontFamily("Wingdings"), GoalToolIcon.Rect.Width \ 2, FontStyle.Regular)

                .DrawString("«",
                        Font,
                        Brushes.Green,
                        GoalToolIcon.Rect,
                        AlineCenterMiddle)

            Else

                .FillRectangle(Brushes.Black, GoalToolButton.Rect)

                .FillRectangle(Brushes.White, GoalToolIcon.Rect)

                ' Define the rectangle to be filled
                Dim rect As RectangleF = GoalToolIcon.Rect

                rect.Inflate(rect.Width / 6.4F, rect.Height / 6.4F)

                ' Define the center point of the gradient
                Dim center As New PointF(rect.Left + rect.Width / 2.0F, rect.Top + rect.Height / 2.0F)

                ' Define the colors for the gradient stops
                Dim colors() As Color = {Color.Yellow, Color.White}

                ' Create the path for the gradient brush
                Dim GradPath As New GraphicsPath()
                GradPath.AddEllipse(rect)

                ' Create the gradient brush
                Dim GradBrush As New PathGradientBrush(GradPath) With {
                    .CenterPoint = center,
                    .CenterColor = colors(0),
                    .SurroundColors = New Color() {colors(1)}
                }

                .FillRectangle(GradBrush, GoalToolIcon.Rect)

                Dim Font As New Font(New FontFamily("Wingdings"), GoalToolIcon.Rect.Width \ 2, FontStyle.Regular)

                .DrawString("«",
                        Font,
                        Brushes.Green,
                        GoalToolIcon.Rect,
                        AlineCenterMiddle)

            End If

        End With

    End Sub

    Private Sub DrawPlayButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black, EditPlayButton.Rect)

            .DrawString("Play",
                        FPSFont,
                        Brushes.White,
                        EditPlayButton.Rect,
                        AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawEditButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black, EditPlayButton.Rect)

            .DrawString("Edit",
                        FPSFont,
                        Brushes.White,
                        EditPlayButton.Rect,
                        AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawMenuButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black, MenuButton.Rect)

            .DrawString("Menu",
                        FPSFont,
                        Brushes.White,
                        MenuButton.Rect,
                        AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawSaveButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black, SaveButton.Rect)

            .DrawEllipse(BButtonIconOutinePen,
                         New Rectangle(SaveButton.Rect.X + 197,
                                       SaveButton.Rect.Y + 20,
                                       52,
                                       52))

            .DrawString("Save",
                        FPSFont,
                        Brushes.White,
                        New Rectangle(SaveButton.Rect.X + 22,
                                      SaveButton.Rect.Y + 24,
                                      145,
                                      50),
                        AlineCenterMiddle)

            .DrawString("B",
                        RightTriggerIconFont,
                        Brushes.White,
                        New Rectangle(SaveButton.Rect.X + 175,
                                      SaveButton.Rect.Y + 24,
                                      100,
                                      50),
                        AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawOpenButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black, OpenButton.Rect)

            .DrawEllipse(YButtonIconOutinePen,
                         New Rectangle(OpenButton.Rect.X + 197,
                                       OpenButton.Rect.Y + 20,
                                       52,
                                       52))

            .DrawString("Open",
                        FPSFont,
                        Brushes.White,
                        New Rectangle(OpenButton.Rect.X + 24,
                                      OpenButton.Rect.Y + 24,
                                      145,
                                      50),
                        AlineCenterMiddle)

            .DrawString("Y",
                        ButtonIconFont,
                        Brushes.White,
                        New Rectangle(OpenButton.Rect.X + 174,
                                      OpenButton.Rect.Y + 24,
                                      100,
                                      50),
                        AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawNewButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black, NewButton.Rect)

            .DrawString("New", FPSFont,
                        Brushes.White,
                        New Rectangle(NewButton.Rect.X + 25,
                                      NewButton.Rect.Y + 24,
                                      120,
                                      50),
                        AlineCenterMiddle)

            .DrawString("RT",
                        RightTriggerIconFont,
                        Brushes.White,
                        New Rectangle(NewButton.Rect.X + 175,
                                      NewButton.Rect.Y + 24,
                                      100,
                                      50),
                        AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawExitButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black, ExitButton.Rect)

            .DrawEllipse(XButtonIconOutinePen,
                         New Rectangle(ExitButton.Rect.X + 124,
                                       ExitButton.Rect.Y + 16,
                                       52,
                                       52))

            .DrawString("X",
                        ButtonIconFont,
                        Brushes.White,
                        ExitButton.Rect,
                        AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawFPS()

        With Buffer.Graphics

            .DrawString(FPS.ToString & " FPS", FPSFont, Brushes.White, FPS_Postion)

        End With

    End Sub

    Private Sub AddBlock(Rect As Rectangle)

        If Blocks IsNot Nothing Then

            Array.Resize(Blocks, Blocks.Length + 1)

        Else

            ReDim Blocks(0)

        End If

        Dim Index As Integer = Blocks.Length - 1

        'Init block
        Blocks(Index).Rect = Rect

        Blocks(Index).Position.X = Rect.X
        Blocks(Index).Position.Y = Rect.Y

        AutoSizeLevel(Blocks(Index).Rect)

    End Sub

    Private Sub AutoSizeLevel(Rect As Rectangle)

        If Rect.Right > Level.Rect.Right Then

            Level.Rect.Width = Rect.Right

            BufferGridLines()

        End If

        If Rect.Bottom > Level.Rect.Bottom Then

            Level.Rect.Height = Rect.Bottom

            BufferGridLines()

        End If

    End Sub

    Private Sub AddBill(Location As Point)

        If Cash IsNot Nothing Then

            Array.Resize(Cash, Cash.Length + 1)

        Else

            ReDim Cash(0)

        End If

        Dim Index As Integer = Cash.Length - 1

        'Init Bill
        Cash(Index).Rect.Location = Location

        Cash(Index).Rect.Size = New Size(GridSize, GridSize)

        Cash(Index).Position.X = Location.X
        Cash(Index).Position.Y = Location.Y

        Cash(Index).Collected = False

        AutoSizeLevel(Cash(Index).Rect)

    End Sub

    Private Sub AddEnemy(Location As Point, PatrolA As Point, PatrolB As Point)

        If Enemies IsNot Nothing Then

            Array.Resize(Enemies, Enemies.Length + 1)

        Else

            ReDim Enemies(0)

        End If

        Dim Index As Integer = Enemies.Length - 1

        'Init Enemy
        Enemies(Index).Rect.Location = Location

        Enemies(Index).Rect.Size = New Size(GridSize, GridSize)

        Enemies(Index).Position.X = Location.X
        Enemies(Index).Position.Y = Location.Y

        Enemies(Index).PatrolA.X = PatrolA.X
        Enemies(Index).PatrolA.Y = PatrolA.Y

        Enemies(Index).PatrolB.X = PatrolB.X
        Enemies(Index).PatrolB.Y = PatrolB.Y

        Enemies(Index).PatrolDirection = Direction.Right

        Enemies(Index).Eliminated = False

        Enemies(Index).Acceleration.X = 100
        Enemies(Index).MaxVelocity.X = 75
        Enemies(Index).Velocity.X = 0


        AutoSizeLevel(New Rectangle(Enemies(Index).PatrolB.X,
                                    Enemies(Index).PatrolB.Y,
                                    GridSize,
                                    GridSize))

    End Sub

    Private Sub AddCloud(Rect As Rectangle)

        'Add the cloud to clouds
        If Clouds IsNot Nothing Then

            Array.Resize(Clouds, Clouds.Length + 1)

        Else

            ReDim Clouds(0)

        End If

        Dim Index As Integer = Clouds.Length - 1

        'Init the cloud
        Clouds(Index).Rect = Rect

        Clouds(Index).Position.X = Rect.X
        Clouds(Index).Position.Y = Rect.Y

        AutoSizeLevel(Clouds(Index).Rect)

    End Sub

    Private Sub AddBush(Rect As Rectangle)

        If Bushes IsNot Nothing Then

            Array.Resize(Bushes, Bushes.Length + 1)

        Else

            ReDim Bushes(0)

        End If

        Dim Index As Integer = Bushes.Length - 1

        'Init Bush
        Bushes(Index).Rect = Rect

        Bushes(Index).Position.X = Rect.X
        Bushes(Index).Position.Y = Rect.Y

        AutoSizeLevel(Bushes(Index).Rect)

    End Sub

    Private Sub RemoveBlock(Index As Integer)

        'Remove the block from blocks.
        Blocks = Blocks.Where(Function(e, i) i <> Index).ToArray()

    End Sub

    Private Sub RemoveBill(Index As Integer)

        'Remove the bill from cash.
        Cash = Cash.Where(Function(e, i) i <> Index).ToArray()

    End Sub

    Private Sub RemoveBush(Index As Integer)

        'Remove the bush from bushes.
        Bushes = Bushes.Where(Function(e, i) i <> Index).ToArray()

    End Sub

    Private Sub RemoveEnemy(Index As Integer)

        'Remove the Enemy from Enemies.
        Enemies = Enemies.Where(Function(e, i) i <> Index).ToArray()

    End Sub

    Private Sub RemoveCloud(Index As Integer)

        'Remove the cloud from clouds.
        Clouds = Clouds.Where(Function(e, i) i <> Index).ToArray()

    End Sub

    Private Sub DrawStartScreenOpenButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black,
                           StartScreenOpenButton.Rect)

            .DrawEllipse(YButtonIconOutinePen,
                         New Rectangle(StartScreenOpenButton.Rect.X + 142,
                                       StartScreenOpenButton.Rect.Y + 20,
                                       52,
                                       52))

            .DrawString("Open",
                        FPSFont,
                        Brushes.White,
                        New Rectangle(StartScreenOpenButton.Rect.X,
                                      StartScreenOpenButton.Rect.Y + 24,
                                      150,
                                      50),
                        AlineCenterMiddle)

            .DrawString("Y",
                        ButtonIconFont,
                        Brushes.White,
                        New Rectangle(StartScreenOpenButton.Rect.X + 144,
                                      StartScreenOpenButton.Rect.Y + 25,
                                      50,
                                      50),
                        AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawStartScreenNewButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black,
                           StartScreenNewButton.Rect)

            .DrawEllipse(BButtonIconOutinePen, New Rectangle(StartScreenNewButton.Rect.X + 140,
                                                             StartScreenNewButton.Rect.Y + 20,
                                                             52,
                                                             52))

            .DrawString("New",
                        FPSFont,
                        Brushes.White,
                        New Rectangle(StartScreenNewButton.Rect.X + 5,
                                      StartScreenNewButton.Rect.Y + 24,
                                      120,
                                      50),
                        AlineCenterMiddle)

            .DrawString("B",
                        ButtonIconFont,
                        Brushes.White,
                        New Rectangle(StartScreenNewButton.Rect.X + 143,
                                      StartScreenNewButton.Rect.Y + 24,
                                      50,
                                      50),
                        AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawTitle()

        With Buffer.Graphics

            'Draw drop shadow.
            .DrawString(Title.Text,
                    TitleFont,
                    New SolidBrush(Color.FromArgb(128, Color.Black)),
                    New Rectangle(Title.Rect.X + 5,
                                  Title.Rect.Y + 5,
                                  Title.Rect.Width,
                                  Title.Rect.Height),
                                  AlineCenterMiddle)

            'Draw title.
            .DrawString(Title.Text,
                    TitleFont,
                    Brushes.Black,
                    Title.Rect,
                    AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawClearTitle()

        With Buffer.Graphics

            'Draw drop shadow.
            .DrawString("Level Clear!",
                    TitleFont,
                    New SolidBrush(Color.FromArgb(128, Color.White)),
                    New Rectangle(Title.Rect.X + 5,
                                  Title.Rect.Y + 5,
                                  Title.Rect.Width,
                                  Title.Rect.Height),
                                  AlineCenterMiddle)

            'Draw title.
            .DrawString("Level Clear!",
                    TitleFont,
                    Brushes.White,
                    Title.Rect,
                    AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawBackground(Color As Color)

        With Buffer.Graphics

            .Clear(Color) 'LightSkyBlue

        End With

    End Sub

    Private Sub DrawGridLines()

        With Buffer.Graphics

            .DrawImageUnscaled(GridLineBitmap, 0, 0)

        End With

    End Sub

    Private Sub BufferGridLines()

        GridLineBuffer.Clear(Color.Transparent)

        ' Draw vertical lines  |
        For x As Integer = Camera.Rect.X To Camera.Rect.X + Level.Rect.Width Step GridSize

            GridLineBuffer.DrawLine(Pens.Black, x, Camera.Rect.Y, x, Camera.Rect.Y + Level.Rect.Height)

        Next

        ' Draw horizontal lines ---
        For y As Integer = Camera.Rect.Y To Camera.Rect.Y + Level.Rect.Height Step GridSize

            GridLineBuffer.DrawLine(Pens.Black, Camera.Rect.X, y, Camera.Rect.X + Level.Rect.Width, y)

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

    Private Sub InitializeApp()

        InitializeToolBarButtons()

        InitializeForm()

        InitializeBuffer()

        Title.Text = "Platformer" & vbCrLf & "with Level Editor"

        OutinePen.LineJoin = Drawing2D.LineJoin.Round

        MenuOutinePen.LineJoin = Drawing2D.LineJoin.Round

        MenuShadowPen.LineJoin = Drawing2D.LineJoin.Round

        InitializeObjects()

        'CreateNewLevel()

        CreateStartScreenLevel()

        CashCollected = 0

    End Sub

    Private Sub InitializeToolBarButtons()

        EditPlayButton.Rect = New Rectangle(ClientRectangle.Left + 210,
                                            ClientRectangle.Bottom - 90,
                                            120,
                                            100)

        SaveButton.Rect = New Rectangle(ClientRectangle.Right - 210,
                                        ClientRectangle.Bottom - 90,
                                        120,
                                        100)
    End Sub

    Private Sub InitializeObjects()

        Camera.Rect.Location = New Point(0, 0)

        SetMinLevelSize()

        OurHero.Rect = New Rectangle(128, 769, 64, 64)

        OurHero.Position = New PointF(OurHero.Rect.X, OurHero.Rect.Y)

        OurHero.Velocity = New PointF(0, 0)

        OurHero.MaxVelocity = New PointF(400, 1000)

        OurHero.Acceleration = New PointF(300, 25)

        BufferGridLines()

    End Sub

    Private Sub InitializeForm()

        Me.WindowState = FormWindowState.Maximized

        Text = "Platformer with Level Editor - Code with Joe"

        SetStyle(ControlStyles.UserPaint, True)

        SetStyle(ControlStyles.OptimizedDoubleBuffer, True)

        SetStyle(ControlStyles.AllPaintingInWmPaint, True)

    End Sub

    Private Sub InitializeBuffer()

        'Set context to the context of this app.
        Context = BufferedGraphicsManager.Current

        'Set buffer size to the primary working area.
        Context.MaximumBuffer = Screen.PrimaryScreen.WorkingArea.Size

        'Create buffer.
        Buffer = Context.Allocate(CreateGraphics(), ClientRectangle)

    End Sub

    Private Sub CreateNewLevel()

        Goal.Rect = New Rectangle(1472, 768, 64, 64)

        AddBlock(New Rectangle(0, 832, 1920, 64))

        AddBlock(New Rectangle(1088, 576, 64, 64))

        AddBlock(New Rectangle(1344, 576, 320, 64))

        AddBlock(New Rectangle(1472, 320, 64, 64))

        AddCloud(New Rectangle(512, 64, 192, 128))

        AddCloud(New Rectangle(1728, 64, 128, 64))

        AddBush(New Rectangle(768, 768, 320, 64))

        AddBush(New Rectangle(1600, 768, 64, 64))

        AddBill(New Point(1088, 320))

        AddBill(New Point(1472, 64))

        AddEnemy(New Point(500, 769), New Point(512, 769), New Point(576, 769))

    End Sub

    Private Sub CreateStartScreenLevel()

        Goal.Rect = New Rectangle(-100, -100, 64, 64)

        AddBlock(New Rectangle(0, 832, 1920, 64))

        AddBlock(New Rectangle(1472, 576, 384, 64))

        AddBlock(New Rectangle(1536, 256, 256, 64))

        AddBlock(New Rectangle(0, 896, 1920, 64))

        AddBlock(New Rectangle(0, 960, 1920, 128))

        AddBush(New Rectangle(256, 768, 320, 64))

        AddBush(New Rectangle(1408, 768, 512, 64))

        AddCloud(New Rectangle(64, 128, 192, 64))

        AddCloud(New Rectangle(1728, 64, 128, 64))

    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize

        'Place the FPS display at the bottom of the client area.
        FPS_Postion.Y = ClientRectangle.Bottom - 75

        CashCollectedPostion.Y = ClientRectangle.Top + 5

        EditPlayButton.Rect = New Rectangle(ClientRectangle.Left + 210, ClientRectangle.Bottom - 90, 120, 90)

        ToolBarBackground.Rect = New Rectangle(ClientRectangle.Left, ClientRectangle.Bottom - 90, ClientRectangle.Width, 100)

        MenuBackground.Rect = New Rectangle(ClientRectangle.Width \ 2 - MenuBackground.Rect.Width \ 2,
                                            (ClientRectangle.Height \ 2) - MenuBackground.Rect.Height \ 2,
                                            300,
                                            92 * 4)

        SaveButton.Rect = New Rectangle(MenuBackground.Rect.Left,
                                        MenuBackground.Rect.Top,
                                        300,
                                        90)

        OpenButton.Rect = New Rectangle(MenuBackground.Rect.Left,
                                        MenuBackground.Rect.Top + 92,
                                        300,
                                        90)

        NewButton.Rect = New Rectangle(MenuBackground.Rect.Left,
                                       MenuBackground.Rect.Top + 92 * 2,
                                       300,
                                       90)


        ExitButton.Rect = New Rectangle(MenuBackground.Rect.Left,
                                       MenuBackground.Rect.Top + 92 * 3,
                                       300,
                                       90)

        MenuButton.Rect = New Rectangle(ClientRectangle.Right - 152,
                                        ClientRectangle.Bottom - 90,
                                        150,
                                        100)

        PointerToolButton.Rect = New Rectangle(ClientRectangle.Left + 331, ClientRectangle.Bottom - 90, 90, 90)

        BlockToolButton.Rect = New Rectangle(ClientRectangle.Left + 422, ClientRectangle.Bottom - 90, 90, 90)

        BlockToolIcon.Rect = New Rectangle(ClientRectangle.Left + 447, ClientRectangle.Bottom - 65, 40, 40)

        BillToolButton.Rect = New Rectangle(ClientRectangle.Left + 513, ClientRectangle.Bottom - 90, 90, 90)

        BillToolIcon.Rect = New Rectangle(ClientRectangle.Left + 538, ClientRectangle.Bottom - 65, 40, 40)

        CloudToolButton.Rect = New Rectangle(ClientRectangle.Left + 604, ClientRectangle.Bottom - 90, 90, 90)

        CloundToolIcon.Rect = New Rectangle(ClientRectangle.Left + 629, ClientRectangle.Bottom - 65, 40, 40)

        BushToolButton.Rect = New Rectangle(ClientRectangle.Left + 695, ClientRectangle.Bottom - 90, 90, 90)

        BushToolIcon.Rect = New Rectangle(ClientRectangle.Left + 720, ClientRectangle.Bottom - 65, 40, 40)

        GoalToolButton.Rect = New Rectangle(ClientRectangle.Left + 786, ClientRectangle.Bottom - 90, 90, 90)

        GoalToolIcon.Rect = New Rectangle(ClientRectangle.Left + 811, ClientRectangle.Bottom - 65, 40, 40)

        EnemyToolButton.Rect = New Rectangle(ClientRectangle.Left + 877, ClientRectangle.Bottom - 90, 90, 90)

        EnemyToolIcon.Rect = New Rectangle(ClientRectangle.Left + 902, ClientRectangle.Bottom - 65, 40, 40)

        Title.Rect = New Rectangle(ClientRectangle.Width \ 2 - 425, ClientRectangle.Height \ 2 - 175, 850, 245)

        StartScreenNewButton.Rect = New Rectangle(ClientRectangle.Width \ 2 - 230, ClientRectangle.Height \ 2 + 70, 210, 90)

        StartScreenOpenButton.Rect = New Rectangle(ClientRectangle.Width \ 2 + 20, ClientRectangle.Height \ 2 + 70, 210, 90)

        Camera.Rect.Size = ClientRectangle.Size

        BufferGridLines()

    End Sub

    Private Sub Form1_MouseDown(sender As Object, e As MouseEventArgs) Handles MyBase.MouseDown

        Select Case GameState

            Case AppState.Start

                MouseDownStart(e)

            Case AppState.Playing

                If EditPlayButton.Rect.Contains(e.Location) Then

                    'Remember the cameras in game position before opening the editor.
                    CameraPlayPostion.X = Camera.Rect.X
                    CameraPlayPostion.Y = Camera.Rect.Y


                    GameState = AppState.Editing

                    BufferGridLines()

                End If

            Case AppState.Editing

                MouseDownEditing(e.Location)

        End Select

    End Sub

    Private Sub MouseDownStart(e As MouseEventArgs)

        'Open Button
        If StartScreenOpenButton.Rect.Contains(e.Location) Then

            OpenFileDialog1.FileName = ""
            OpenFileDialog1.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
            OpenFileDialog1.FilterIndex = 1
            OpenFileDialog1.RestoreDirectory = True

            If OpenFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then

                If My.Computer.FileSystem.FileExists(OpenFileDialog1.FileName) = True Then

                    InitializeObjects()

                    OpenTestLevelFile(OpenFileDialog1.FileName)

                    If IsFileLoaded = True Then

                        LevelName = Path.GetFileName(OpenFileDialog1.FileName)
                        Text = LevelName & " - Platformer with Level Editor - Code with Joe"

                        CashCollected = 0

                        MovePointerOffScreen()

                        LastFrame = Now

                        GameState = AppState.Playing

                        PlayLevelMusic()

                    End If

                End If

            End If

        End If

        'Is the player selecting the new button?
        If StartScreenNewButton.Rect.Contains(e.Location) Then
            'Yes, the player is selecting the new button.

            ClearObjects()

            InitializeObjects()

            CreateNewLevel()

            CashCollected = 0

            MovePointerOffScreen()

            LastFrame = Now

            GameState = AppState.Playing

            LevelName = "Untitled"
            Text = LevelName & " - Platformer with Level Editor - Code with Joe"

            PlayLevelMusic()

        End If

    End Sub

    Private Shared Sub MovePointerOffScreen()
        'Move mouse pointer off screen.

        Cursor.Position = New Point(Screen.PrimaryScreen.WorkingArea.Right, Screen.PrimaryScreen.WorkingArea.Height \ 2)

    End Sub

    Private Sub MovePointerCenterMenu()
        'Move mouse pointer to the center of the menu.

        Cursor.Position = New Point(MenuBackground.Rect.X + MenuBackground.Rect.Width / 2,
                                    MenuBackground.Rect.Y + MenuBackground.Rect.Height / 2)

    End Sub

    Private Sub MouseDownEditing(e As Point)

        If ShowMenu = False Then

            MouseDownEditingSelection(e)

            MouseDownEditingButtons(e)

        Else

            MouseDownEditingMenuButtons(e)

        End If

    End Sub

    Private Sub MouseDownEditingMenuButtons(e As Point)

        'Is the player clicking the save button?
        If SaveButton.Rect.Contains(e) Then
            'Yes, the player is clicking the save button.

            ShowSaveLevelDialog()

            ShowMenu = False

        End If

        'Is the player clicking the open button?
        If OpenButton.Rect.Contains(e) Then
            'Yes, the player is clicking the open button.

            'Does the player want to save this level before opening a level?
            If MsgBox("Save " & LevelName & " before opening a different level?", MsgBoxStyle.YesNo, "Platformer with Level Editor") = MsgBoxResult.No Then
                'No, the player doesn't want to save this level before opening a level?

                ShowOpenLevelDialog()

                ShowMenu = False

            End If

        End If

        'Is the player clicking the new button?
        If NewButton.Rect.Contains(e) Then
            'Yes, the player is clicking the new button.

            'Does the player want to save this level before creating a new level?
            If MsgBox("Save " & LevelName & " before creating a new level?", MsgBoxStyle.YesNo, "Platformer with Level Editor") = MsgBoxResult.No Then
                'No, the player doesn't want to save this level before creating a new level?

                InitAndCreateNewLevel()

                ShowMenu = False

            End If

        End If

        'Is the player clicking the exit button?
        If ExitButton.Rect.Contains(e) Then
            'Yes, the player is clicking the exit button.

            ShowMenu = False

        End If

    End Sub

    Private Sub InitAndCreateNewLevel()

        ClearObjects()

        SetMinLevelSize()

        OurHero.Rect = New Rectangle(128, 769, 64, 64)

        OurHero.Position = New PointF(OurHero.Rect.X, OurHero.Rect.Y)

        OurHero.Velocity = New PointF(0, 0)

        CreateNewLevel()

        BufferGridLines()

        CashCollected = 0

        LevelName = "Untitled"

        Text = LevelName & " - Platformer with Level Editor - Code with Joe"

    End Sub

    Private Sub ShowOpenLevelDialog()

        OpenFileDialog1.FileName = ""
        OpenFileDialog1.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
        OpenFileDialog1.FilterIndex = 1
        OpenFileDialog1.RestoreDirectory = True

        If OpenFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then

            If My.Computer.FileSystem.FileExists(OpenFileDialog1.FileName) = True Then

                OpenTestLevelFile(OpenFileDialog1.FileName)

                If IsFileLoaded = True Then

                    ShowMenu = False

                    LevelName = Path.GetFileName(OpenFileDialog1.FileName)

                    Text = LevelName & " - Platformer with Level Editor - Code with Joe"

                Else

                    Text = "Platformer with Level Editor - Code with Joe"

                End If

                CashCollected = 0

            End If

        End If

    End Sub

    Private Sub ShowSaveLevelDialog()

        SaveFileDialog1.FileName = LevelName
        SaveFileDialog1.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
        SaveFileDialog1.FilterIndex = 1
        SaveFileDialog1.RestoreDirectory = True

        If SaveFileDialog1.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK Then

            SaveTestLevelFile(SaveFileDialog1.FileName)

            LevelName = Path.GetFileName(SaveFileDialog1.FileName)
            Text = LevelName & " - Platformer with Level Editor - Code with Joe"

        End If

    End Sub

    Private Sub SetMinLevelSize()

        Level.Rect.Width = ClientRectangle.Width

        Level.Rect.Height = ClientRectangle.Height

    End Sub

    Private Sub ClearObjects()

        Blocks = Nothing

        Cash = Nothing

        Bushes = Nothing

        Clouds = Nothing

        Enemies = Nothing

    End Sub

    Private Sub MouseDownEditingButtons(e As Point)

        Dim pointOffset As Point = e

        pointOffset.X = (Camera.Rect.X * -1) + e.X

        pointOffset.Y = (Camera.Rect.Y * -1) + e.Y

        'Is the player clicking the play button?
        If EditPlayButton.Rect.Contains(e) Then
            'Yes, the player is clicking the play button.

            DeselectObjects()

            'Restore the cameras in game position.
            Camera.Rect.X = CameraPlayPostion.X
            Camera.Rect.Y = CameraPlayPostion.Y

            MovePointerOffScreen()

            LastFrame = Now

            GameState = AppState.Playing

        End If

        'Is the player clicking the pointer tool button?
        If PointerToolButton.Rect.Contains(e) Then
            'Yes, the player is clicking the pointer tool button.

            SelectedTool = Tools.Pointer

            ShowToolPreview = False

        End If

        'Is the player clicking the block tool button?
        If BlockToolButton.Rect.Contains(e) Then
            'Yes, the player is clicking the block tool button.

            DeselectObjects()

            'Snap preview to grid.
            ToolPreview.X = CInt(Math.Round(pointOffset.X / GridSize) * GridSize)
            ToolPreview.Y = CInt(Math.Round(pointOffset.Y / GridSize) * GridSize)

            ToolPreview.Width = GridSize
            ToolPreview.Height = GridSize

            SelectedTool = Tools.Block

            ShowToolPreview = True

        End If

        If BillToolButton.Rect.Contains(e) Then

            DeselectObjects()

            'Snap preview to grid.
            ToolPreview.X = CInt(Math.Round(pointOffset.X / GridSize) * GridSize)
            ToolPreview.Y = CInt(Math.Round(pointOffset.Y / GridSize) * GridSize)

            ToolPreview.Width = GridSize
            ToolPreview.Height = GridSize

            SelectedTool = Tools.Bill

            ShowToolPreview = True

        End If

        If CloudToolButton.Rect.Contains(e) Then

            DeselectObjects()

            'Snap preview to grid.
            ToolPreview.X = CInt(Math.Round(pointOffset.X / GridSize) * GridSize)
            ToolPreview.Y = CInt(Math.Round(pointOffset.Y / GridSize) * GridSize)

            ToolPreview.Width = GridSize
            ToolPreview.Height = GridSize

            SelectedTool = Tools.Cloud

            ShowToolPreview = True

        End If

        If BushToolButton.Rect.Contains(e) Then

            DeselectObjects()

            'Snap preview to grid.
            ToolPreview.X = CInt(Math.Round(pointOffset.X / GridSize) * GridSize)
            ToolPreview.Y = CInt(Math.Round(pointOffset.Y / GridSize) * GridSize)

            ToolPreview.Width = GridSize
            ToolPreview.Height = GridSize

            SelectedTool = Tools.Bush

            ShowToolPreview = True

        End If

        If EnemyToolButton.Rect.Contains(e) Then

            DeselectObjects()

            'Snap preview to grid.
            ToolPreview.X = CInt(Math.Round(pointOffset.X / GridSize) * GridSize)
            ToolPreview.Y = CInt(Math.Round(pointOffset.Y / GridSize) * GridSize)

            ToolPreview.Width = GridSize
            ToolPreview.Height = GridSize

            SelectedTool = Tools.Enemy

            ShowToolPreview = True

        End If

        If GoalToolButton.Rect.Contains(e) Then

            DeselectObjects()

            'Snap preview to grid.
            ToolPreview.X = CInt(Math.Round(pointOffset.X / GridSize) * GridSize)
            ToolPreview.Y = CInt(Math.Round(pointOffset.Y / GridSize) * GridSize)

            ToolPreview.Width = GridSize
            ToolPreview.Height = GridSize

            SelectedTool = Tools.Goal

            ShowToolPreview = True

        End If

        'Is the player clicking the menu button?
        If MenuButton.Rect.Contains(e) Then
            'Yes, the player is clicking the menu button.

            ShowMenu = True

            MovePointerCenterMenu()

        End If

    End Sub

    Private Sub DeselectObjects()
        'Deselect game objects.

        SelectedBlock = -1

        SelectedBill = -1

        SelectedCloud = -1

        SelectedBush = -1

        SelectedEnemy = -1

        GoalSelected = False

        LevelSelected = False

    End Sub

    Private Sub MouseDownEditingSelection(e As Point)

        'Is the player over the toolbar?
        If ToolBarBackground.Rect.Contains(e) = False Then
            'No, the player is NOT over the toolbar.

            Dim PointOffset As Point = e

            PointOffset.X = (Camera.Rect.X * -1) + e.X
            PointOffset.Y = (Camera.Rect.Y * -1) + e.Y

            If SizingHandle.Contains(e) Then

                SizingHandleSelected = True

            Else

                SizingHandleSelected = False

                'Is the player selecting a Enemy?
                If CheckEnemySelection(PointOffset) > -1 Then
                    'Yes, the player is selecting a Enemy.

                    SelectedEnemy = CheckEnemySelection(PointOffset)

                    SelectionOffset.X = PointOffset.X - Enemies(SelectedEnemy).PatrolA.X
                    SelectionOffset.Y = PointOffset.Y - Enemies(SelectedEnemy).PatrolA.Y

                    'Deselect other game objects.
                    SelectedBlock = -1
                    SelectedBill = -1
                    SelectedCloud = -1
                    SelectedBush = -1
                    GoalSelected = False
                    LevelSelected = False

                ElseIf Goal.Rect.Contains(PointOffset) Then

                    GoalSelected = True

                    SelectionOffset.X = PointOffset.X - Goal.Rect.X
                    SelectionOffset.Y = PointOffset.Y - Goal.Rect.Y

                    'Deselect other game objects.
                    SelectedBlock = -1
                    SelectedBill = -1
                    SelectedCloud = -1
                    SelectedBush = -1
                    SelectedEnemy = -1
                    LevelSelected = False

                    'Is the player selecting a block?
                ElseIf CheckBlockSelection(PointOffset) > -1 Then
                    'Yes, the player is selecting a block.

                    SelectedBlock = CheckBlockSelection(PointOffset)

                    SelectionOffset.X = PointOffset.X - Blocks(SelectedBlock).Rect.X
                    SelectionOffset.Y = PointOffset.Y - Blocks(SelectedBlock).Rect.Y

                    'Deselect other game objects.
                    SelectedBill = -1
                    SelectedCloud = -1
                    SelectedBush = -1
                    SelectedEnemy = -1
                    GoalSelected = False
                    LevelSelected = False

                    'Is the player selecting a bill?
                ElseIf CheckBillSelection(PointOffset) > -1 Then
                    'Yes, the player is selecting a bill.

                    SelectedBill = CheckBillSelection(PointOffset)

                    SelectionOffset.X = PointOffset.X - Cash(SelectedBill).Rect.X
                    SelectionOffset.Y = PointOffset.Y - Cash(SelectedBill).Rect.Y

                    'Deselect other game objects.
                    SelectedBlock = -1
                    SelectedCloud = -1
                    SelectedBush = -1
                    SelectedEnemy = -1
                    GoalSelected = False
                    LevelSelected = False

                    'Is the player selecting a cloud?
                ElseIf CheckCloudSelection(PointOffset) > -1 Then
                    'Yes, the player is selecting a cloud.

                    SelectedCloud = CheckCloudSelection(PointOffset)

                    SelectionOffset.X = PointOffset.X - Clouds(SelectedCloud).Rect.X
                    SelectionOffset.Y = PointOffset.Y - Clouds(SelectedCloud).Rect.Y

                    'Deselect other game objects.
                    SelectedBlock = -1
                    SelectedBill = -1
                    SelectedBush = -1
                    SelectedEnemy = -1
                    GoalSelected = False
                    LevelSelected = False

                    'Is the player selecting a bush?
                ElseIf CheckBushSelection(PointOffset) > -1 Then
                    'Yes, the player is selecting a bush.

                    SelectedBush = CheckBushSelection(PointOffset)

                    SelectionOffset.X = PointOffset.X - Bushes(SelectedBush).Rect.X
                    SelectionOffset.Y = PointOffset.Y - Bushes(SelectedBush).Rect.Y

                    'Deselect other game objects.
                    SelectedBlock = -1
                    SelectedBill = -1
                    SelectedCloud = -1
                    SelectedEnemy = -1
                    GoalSelected = False
                    LevelSelected = False

                Else
                    'No, the player is not selecting a game object.

                    Select Case SelectedTool

                        Case Tools.Block

                            'Snap block to grid.
                            Dim SnapPoint As New Point(CInt(Math.Round(PointOffset.X / GridSize) * GridSize),
                                                           CInt(Math.Round(PointOffset.Y / GridSize) * GridSize))

                            AddBlock(New Rectangle(SnapPoint, New Drawing.Size(GridSize, GridSize)))

                            'Change tool to the mouse pointer.
                            SelectedTool = Tools.Pointer

                            'Turn tool preview off.
                            ShowToolPreview = False

                            'Select the newly created block.
                            SelectedBlock = Blocks.Length - 1

                        Case Tools.Bill

                            'Snap block to grid.
                            AddBill(New Point(CInt(Math.Round(PointOffset.X / GridSize) * GridSize),
                                           CInt(Math.Round(PointOffset.Y / GridSize) * GridSize)))

                            'Change tool to the mouse pointer.
                            SelectedTool = Tools.Pointer

                            'Turn tool preview off.
                            ShowToolPreview = False

                            'Select the newly created bill.
                            SelectedBill = Cash.Length - 1

                        Case Tools.Cloud

                            'Snap block to grid.
                            Dim SnapPoint As New Point(CInt(Math.Round(PointOffset.X / GridSize) * GridSize),
                                                           CInt(Math.Round(PointOffset.Y / GridSize) * GridSize))

                            AddCloud(New Rectangle(SnapPoint, New Drawing.Size(GridSize, GridSize)))

                            'Change tool to the mouse pointer.
                            SelectedTool = Tools.Pointer

                            'Turn tool preview off.
                            ShowToolPreview = False

                            'Select the newly created cloud.
                            SelectedCloud = Clouds.Length - 1

                        Case Tools.Bush

                            'Snap block to grid.
                            Dim SnapPoint As New Point(CInt(Math.Round(PointOffset.X / GridSize) * GridSize),
                                                           CInt(Math.Round(PointOffset.Y / GridSize) * GridSize))

                            AddBush(New Rectangle(SnapPoint, New Drawing.Size(GridSize, GridSize)))

                            'Change tool to the mouse pointer.
                            SelectedTool = Tools.Pointer

                            'Turn tool preview off.
                            ShowToolPreview = False

                            'Select the newly created bill.
                            SelectedBush = Bushes.Length - 1

                        Case Tools.Enemy

                            'Snap block to grid.
                            Dim SnapPoint As New Point(CInt(Math.Round(PointOffset.X / GridSize) * GridSize),
                                                       CInt(Math.Round(PointOffset.Y / GridSize) * GridSize))

                            Dim SnapPointB As New Point(SnapPoint.X + GridSize, SnapPoint.Y)

                            AddEnemy(SnapPoint, SnapPoint, SnapPointB)

                            'Change tool to the mouse pointer.
                            SelectedTool = Tools.Pointer

                            'Turn tool preview off.
                            ShowToolPreview = False

                            'Select the newly created enemy.
                            SelectedEnemy = Enemies.Length - 1

                        Case Tools.Goal

                            Goal.Rect.Location = New Point(CInt(Math.Round(PointOffset.X / GridSize) * GridSize),
                                                               CInt(Math.Round(PointOffset.Y / GridSize) * GridSize))

                            Goal.Rect.Size = New Size(GridSize, GridSize)

                            AutoSizeLevel(Goal.Rect)

                            'Change tool to the mouse pointer.
                            SelectedTool = Tools.Pointer

                            'Turn tool preview off.
                            ShowToolPreview = False

                            'Select the goal.
                            GoalSelected = True

                        Case Tools.Pointer

                            LevelSelected = True

                            SelectionOffset.X = PointOffset.X - Level.Rect.X
                            SelectionOffset.Y = PointOffset.Y - Level.Rect.Y

                            'Deselect game objects.
                            SelectedBlock = -1
                            SelectedBill = -1
                            SelectedCloud = -1
                            SelectedBush = -1
                            GoalSelected = False
                            SelectedEnemy = -1

                    End Select

                End If

            End If

        Else
            'Yes, the player is over the toolbar.

            'Deselect game objects.
            SelectedBlock = -1
            SelectedBill = -1
            SelectedCloud = -1
            SelectedBush = -1
            GoalSelected = False
            LevelSelected = False
            SelectedEnemy = -1

        End If

    End Sub

    Private Function CheckCloudSelection(e As Point) As Integer

        If Clouds IsNot Nothing Then

            For Each Cloud In Clouds

                'Has the player selected a cloud?
                If Cloud.Rect.Contains(e) Then
                    'Yes, the player has selected a cloud.

                    Return Array.IndexOf(Clouds, Cloud)

                    Exit Function

                End If

            Next

        End If

        Return -1

    End Function

    Private Function CheckBlockSelection(e As Point) As Integer

        If Blocks IsNot Nothing Then

            For Each Block In Blocks

                'Has the player selected a cloud?
                If Block.Rect.Contains(e) Then
                    'Yes, the player has selected a cloud.

                    Return Array.IndexOf(Blocks, Block)

                    Exit Function

                End If

            Next

        End If

        Return -1

    End Function

    Private Function CheckBillSelection(e As Point) As Integer

        If Cash IsNot Nothing Then

            For Each Bill In Cash

                'Has the player selected a cloud?
                If Bill.Rect.Contains(e) Then
                    'Yes, the player has selected a cloud.

                    Return Array.IndexOf(Cash, Bill)

                    Exit Function

                End If

            Next

        End If

        Return -1

    End Function

    Private Function CheckBushSelection(e As Point) As Integer

        If Bushes IsNot Nothing Then

            For Each Bush In Bushes

                'Has the player selected a cloud?
                If Bush.Rect.Contains(e) Then
                    'Yes, the player has selected a cloud.

                    Return Array.IndexOf(Bushes, Bush)

                    Exit Function

                End If

            Next

        End If

        Return -1

    End Function

    Private Function CheckEnemySelection(e As Point) As Integer

        If Enemies IsNot Nothing Then

            For Each Enemy In Enemies

                Dim PatrolRect As New Rectangle(Enemy.PatrolA.X,
                                                Enemy.PatrolA.Y,
                                                Enemy.PatrolB.X + GridSize - Enemy.PatrolA.X,
                                                GridSize)

                'Has the player selected a Enemy?
                If PatrolRect.Contains(e) Then
                    'Yes, the player has selected a Enemy.

                    Return Array.IndexOf(Enemies, Enemy)

                    Exit Function

                End If

            Next

        End If

        Return -1

    End Function

    Private Sub SaveTestLevelFile(FilePath As String)

        Dim File_Number As Integer = FreeFile()

        FileOpen(File_Number, FilePath, OpenMode.Output)

        'Write Level to File
        'Write ID
        Write(File_Number, ObjectID.Level)

        'Write Position
        Write(File_Number, Level.Rect.X)
        Write(File_Number, Level.Rect.Y)

        'Write Size
        Write(File_Number, Level.Rect.Width)
        Write(File_Number, Level.Rect.Height)

        'Write PatrolA
        Write(File_Number, Level.PatrolA.X)
        Write(File_Number, Level.PatrolA.Y)

        'Write PatrolB
        Write(File_Number, Level.PatrolB.X)
        Write(File_Number, Level.PatrolB.Y)

        Write(File_Number, "Level")

        'Write Blocks to File
        If Blocks IsNot Nothing Then

            For Each Block In Blocks

                'Write ID
                Write(File_Number, ObjectID.Block)

                'Write Position
                Write(File_Number, Block.Rect.X)
                Write(File_Number, Block.Rect.Y)

                'Write Size
                Write(File_Number, Block.Rect.Width)
                Write(File_Number, Block.Rect.Height)

                'Write PatrolA
                Write(File_Number, Block.PatrolA.X)
                Write(File_Number, Block.PatrolA.Y)

                'Write PatrolB
                Write(File_Number, Block.PatrolB.X)
                Write(File_Number, Block.PatrolB.Y)

                Write(File_Number, "Block")

            Next

        End If

        'Write Cash to File
        If Cash IsNot Nothing Then

            For Each Bill In Cash

                'Write ID
                Write(File_Number, ObjectID.Bill)

                'Write Position
                Write(File_Number, Bill.Rect.X)
                Write(File_Number, Bill.Rect.Y)

                'Write Size
                Write(File_Number, Bill.Rect.Width)
                Write(File_Number, Bill.Rect.Height)

                'Write PatrolA
                Write(File_Number, Bill.PatrolA.X)
                Write(File_Number, Bill.PatrolA.Y)

                'Write PatrolB
                Write(File_Number, Bill.PatrolB.X)
                Write(File_Number, Bill.PatrolB.Y)

                Write(File_Number, "Bill")

            Next

        End If

        'Write Bushes to File
        If Bushes IsNot Nothing Then

            For Each Bush In Bushes

                'Write ID
                Write(File_Number, ObjectID.Bush)

                'Write Position
                Write(File_Number, Bush.Rect.X)
                Write(File_Number, Bush.Rect.Y)

                'Write Size
                Write(File_Number, Bush.Rect.Width)
                Write(File_Number, Bush.Rect.Height)

                'Write PatrolA
                Write(File_Number, Bush.PatrolA.X)
                Write(File_Number, Bush.PatrolA.Y)

                'Write PatrolB
                Write(File_Number, Bush.PatrolB.X)
                Write(File_Number, Bush.PatrolB.Y)

                Write(File_Number, "Bush")

            Next

        End If

        'Write Clouds to File
        If Clouds IsNot Nothing Then

            For Each Cloud In Clouds

                'Write ID
                Write(File_Number, ObjectID.Cloud)

                'Write Position
                Write(File_Number, Cloud.Rect.X)
                Write(File_Number, Cloud.Rect.Y)

                'Write Size
                Write(File_Number, Cloud.Rect.Width)
                Write(File_Number, Cloud.Rect.Height)

                'Write PatrolA
                Write(File_Number, Cloud.PatrolA.X)
                Write(File_Number, Cloud.PatrolA.Y)

                'Write PatrolB
                Write(File_Number, Cloud.PatrolB.X)
                Write(File_Number, Cloud.PatrolB.Y)

                Write(File_Number, "Cloud")

            Next

        End If

        'Write Enemies to File
        If Enemies IsNot Nothing Then

            For Each Enemy In Enemies

                'Write ID
                Write(File_Number, ObjectID.Enemy)

                'Write Position
                Write(File_Number, Enemy.PatrolA.X)
                Write(File_Number, Enemy.PatrolA.Y)

                'Write Size
                Write(File_Number, Enemy.Rect.Width)
                Write(File_Number, Enemy.Rect.Height)

                'Write PatrolA
                Write(File_Number, Enemy.PatrolA.X)
                Write(File_Number, Enemy.PatrolA.Y)

                'Write PatrolB
                Write(File_Number, Enemy.PatrolB.X)
                Write(File_Number, Enemy.PatrolB.Y)

                Write(File_Number, "Enemy")

            Next

        End If

        'Write Goal to File
        'Write ID
        Write(File_Number, ObjectID.Goal)

        'Write Position
        Write(File_Number, Goal.Rect.X)
        Write(File_Number, Goal.Rect.Y)

        'Write Size
        Write(File_Number, Goal.Rect.Width)
        Write(File_Number, Goal.Rect.Height)

        'Write PatrolA
        Write(File_Number, Goal.PatrolA.X)
        Write(File_Number, Goal.PatrolA.Y)

        'Write PatrolB
        Write(File_Number, Goal.PatrolB.X)
        Write(File_Number, Goal.PatrolB.Y)

        Write(File_Number, "Goal")

        FileClose(File_Number)

    End Sub

    Private Sub OpenTestLevelFile(FilePath As String)

        Dim File_Number As Integer = FreeFile()

        Dim Index As Integer = -1

        FileObjects = Nothing

        FileOpen(File_Number, FilePath, OpenMode.Input)

        IsFileLoaded = True

        'Read Objects from File
        Do Until EOF(File_Number)

            Index += 1

            ReDim Preserve FileObjects(Index)

            With FileObjects(Index)

                Try

                    'Read ID
                    FileSystem.Input(File_Number, .ID)

                    'Read Position
                    FileSystem.Input(File_Number, .Rect.X)
                    FileSystem.Input(File_Number, .Rect.Y)

                    'Read Size
                    FileSystem.Input(File_Number, .Rect.Width)
                    FileSystem.Input(File_Number, .Rect.Height)

                    'Read PatrolA
                    FileSystem.Input(File_Number, .PatrolA.X)
                    FileSystem.Input(File_Number, .PatrolA.Y)

                    'Read PatrolB
                    FileSystem.Input(File_Number, .PatrolB.X)
                    FileSystem.Input(File_Number, .PatrolB.Y)

                    'Read Text
                    FileSystem.Input(File_Number, .Text)

                Catch ex As Exception

                    IsFileLoaded = False

                    MsgBox("Invaild File Structure", MsgBoxStyle.Critical, "File Error")

                    Exit Do

                End Try

            End With

        Loop

        FileClose(File_Number)

        If IsFileLoaded = True Then

            ClearObjects()

            SetMinLevelSize()

            If FileObjects IsNot Nothing Then

                LoadGameObjects()

            End If

            BufferGridLines()

        End If

    End Sub

    Private Sub LoadGameObjects()

        For Each FileObject In FileObjects

            Select Case FileObject.ID

                Case ObjectID.Block

                    AddBlock(FileObject.Rect)

                Case ObjectID.Bill

                    AddBill(FileObject.Rect.Location)

                Case ObjectID.Bush

                    AddBush(FileObject.Rect)

                Case ObjectID.Cloud

                    AddCloud(FileObject.Rect)

                Case ObjectID.Enemy

                    AddEnemy(FileObject.Rect.Location,
                             New Point(FileObject.PatrolA.X, FileObject.PatrolA.Y),
                             New Point(FileObject.PatrolB.X, FileObject.PatrolB.Y))

                Case ObjectID.Goal

                    'Load ID
                    Goal.ID = FileObject.ID

                    'Load Rect Position
                    Goal.Rect.X = FileObject.Rect.X
                    Goal.Rect.Y = FileObject.Rect.Y

                    'Load Vec2 Position
                    Goal.Position.X = FileObject.Rect.X
                    Goal.Position.Y = FileObject.Rect.Y

                    'Load Rect Size
                    Goal.Rect.Width = FileObject.Rect.Width
                    Goal.Rect.Height = FileObject.Rect.Height

                    'Load Text
                    Goal.Text = FileObject.Text

            End Select

        Next

    End Sub

    Private Sub Form1_MouseMove(sender As Object, e As MouseEventArgs) Handles MyBase.MouseMove

        If GameState = AppState.Editing Then

            MouseMoveEditing(e)

        End If

    End Sub

    Private Sub MouseMoveEditing(e As MouseEventArgs)

        Dim pointOffset As Point = e.Location

        pointOffset.X = (Camera.Rect.X * -1) + e.X

        pointOffset.Y = (Camera.Rect.Y * -1) + e.Y

        If e.Button = MouseButtons.None Then

            If SelectedTool <> Tools.Pointer Then

                If ToolBarBackground.Rect.Contains(e.Location) = False Then

                    ToolPreview.X = CInt(Math.Round(pointOffset.X / GridSize) * GridSize)
                    ToolPreview.Y = CInt(Math.Round(pointOffset.Y / GridSize) * GridSize)

                    ShowToolPreview = True

                Else

                    ShowToolPreview = False

                End If

            End If

        End If

        If SelectedCloud > -1 Then

            If e.Button = MouseButtons.Left Then

                If SizingHandleSelected = True Then

                    'Snap cloud width to grid.
                    Clouds(SelectedCloud).Rect.Width = CInt(Math.Round((pointOffset.X - Clouds(SelectedCloud).Rect.X) / GridSize)) * GridSize

                    'Limit smallest cloud width to one grid width.
                    If Clouds(SelectedCloud).Rect.Width < GridSize Then Clouds(SelectedCloud).Rect.Width = GridSize

                    'Snap cloud height to grid.
                    Clouds(SelectedCloud).Rect.Height = CInt(Math.Round((pointOffset.Y - Clouds(SelectedCloud).Rect.Y) / GridSize)) * GridSize

                    'Limit smallest cloud height to one grid height.
                    If Clouds(SelectedCloud).Rect.Height < GridSize Then Clouds(SelectedCloud).Rect.Height = GridSize

                    AutoSizeLevel(Clouds(SelectedCloud).Rect)

                Else

                    'Snap cloud to grid
                    Clouds(SelectedCloud).Rect.X = CInt(Math.Round((pointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                    Clouds(SelectedCloud).Rect.Y = CInt(Math.Round((pointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

                    AutoSizeLevel(Clouds(SelectedCloud).Rect)

                End If

            End If

        End If

        If SelectedBlock > -1 Then

            If e.Button = MouseButtons.Left Then

                'Is the player resizing the block?
                If SizingHandleSelected = True Then
                    'Yes, the player is resizing the block.

                    'Snap block width to grid.
                    Blocks(SelectedBlock).Rect.Width = CInt(Math.Round((pointOffset.X - Blocks(SelectedBlock).Rect.X) / GridSize)) * GridSize

                    'Limit smallest block width to one grid width.
                    If Blocks(SelectedBlock).Rect.Width < GridSize Then Blocks(SelectedBlock).Rect.Width = GridSize

                    'Snap block height to grid.
                    Blocks(SelectedBlock).Rect.Height = CInt(Math.Round((pointOffset.Y - Blocks(SelectedBlock).Rect.Y) / GridSize)) * GridSize

                    'Limit smallest block height to one grid height.
                    If Blocks(SelectedBlock).Rect.Height < GridSize Then Blocks(SelectedBlock).Rect.Height = GridSize

                    AutoSizeLevel(Blocks(SelectedBlock).Rect)

                Else

                    'Snap block to grid
                    Blocks(SelectedBlock).Rect.X = CInt(Math.Round((pointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                    Blocks(SelectedBlock).Rect.Y = CInt(Math.Round((pointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

                    AutoSizeLevel(Blocks(SelectedBlock).Rect)

                End If

            End If

        End If

        If SelectedBill > -1 Then

            If e.Button = MouseButtons.Left Then

                'Move bill, snap to grid.
                Cash(SelectedBill).Rect.X = CInt(Math.Round((pointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                Cash(SelectedBill).Rect.Y = CInt(Math.Round((pointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

                AutoSizeLevel(Cash(SelectedBill).Rect)

            End If

        End If

        'Has the player selected a bush?
        If SelectedBush > -1 Then
            'Yes, the player has selected a bush.

            If e.Button = MouseButtons.Left Then

                'Is the player resizing the bush?
                If SizingHandleSelected = True Then
                    'Yes, the player is resizing the bush.

                    'Snap bush width to grid.
                    Bushes(SelectedBush).Rect.Width = CInt(Math.Round((pointOffset.X - Bushes(SelectedBush).Rect.X) / GridSize)) * GridSize

                    'Limit smallest bush width to one grid width.
                    If Bushes(SelectedBush).Rect.Width < GridSize Then Bushes(SelectedBush).Rect.Width = GridSize

                    'Snap bush height to grid.
                    Bushes(SelectedBush).Rect.Height = CInt(Math.Round((pointOffset.Y - Bushes(SelectedBush).Rect.Y) / GridSize)) * GridSize

                    'Limit smallest bush height to one grid height.
                    If Bushes(SelectedBush).Rect.Height < GridSize Then Bushes(SelectedBush).Rect.Height = GridSize

                    AutoSizeLevel(Bushes(SelectedBush).Rect)

                Else
                    'No, the player is not resizing the bush.
                    'The player is moving the bush.

                    'Move bush, snap to grid
                    Bushes(SelectedBush).Rect.X = CInt(Math.Round((pointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                    Bushes(SelectedBush).Rect.Y = CInt(Math.Round((pointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

                    AutoSizeLevel(Bushes(SelectedBush).Rect)

                End If

            End If

        End If

        If GoalSelected = True Then

            If e.Button = MouseButtons.Left Then

                If SizingHandleSelected = True Then

                    'Snap bush width to grid.
                    Goal.Rect.Width = CInt(Math.Round((pointOffset.X - Goal.Rect.X) / GridSize)) * GridSize

                    'Limit smallest bush width to one grid width.
                    If Goal.Rect.Width < GridSize Then Goal.Rect.Width = GridSize

                    'Snap bush height to grid.
                    Goal.Rect.Height = CInt(Math.Round((pointOffset.Y - Goal.Rect.Y) / GridSize)) * GridSize

                    'Limit smallest bush height to one grid height.
                    If Goal.Rect.Height < GridSize Then Goal.Rect.Height = GridSize

                    AutoSizeLevel(Goal.Rect)

                Else

                    'Move Goal, snap to grid
                    Goal.Rect.X = CInt(Math.Round((pointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                    Goal.Rect.Y = CInt(Math.Round((pointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

                    AutoSizeLevel(Goal.Rect)

                End If

            End If

        End If

        'Has the player selected a Enemy?
        If SelectedEnemy > -1 Then
            'Yes, the player has selected a Enemy.

            If e.Button = MouseButtons.Left Then

                'Is the player resizing the enemy Patrol?
                If SizingHandleSelected = True Then
                    'Yes, the player is resizing the enemy patrol width.

                    'W = B + G - A
                    Dim PatrolWidth As Integer = Enemies(SelectedEnemy).PatrolB.X + GridSize - Enemies(SelectedEnemy).PatrolA.X

                    Dim PatrolRect As New Rectangle(Enemies(SelectedEnemy).PatrolA.X,
                                                    Enemies(SelectedEnemy).PatrolA.Y,
                                                    PatrolWidth,
                                                    GridSize)

                    'Snap patrol width to grid.
                    PatrolRect.Width = CInt(Math.Round((pointOffset.X - PatrolRect.X) / GridSize)) * GridSize

                    'Limit smallest patrol width to two grid widths.
                    If PatrolRect.Width < GridSize * 2 Then PatrolRect.Width = GridSize * 2

                    'Move patrol point B.
                    Enemies(SelectedEnemy).PatrolB.X = PatrolRect.Right - GridSize

                    AutoSizeLevel(PatrolRect)

                Else
                    'No, the player is not resizing the enemy.

                    'The player is moving the enemy patrol.

                    Dim PatrolWidth As Integer = Enemies(SelectedEnemy).PatrolB.X + GridSize - Enemies(SelectedEnemy).PatrolA.X

                    'Move enemy patrol, snap to grid
                    Enemies(SelectedEnemy).Rect.X = CInt(Math.Round((pointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                    Enemies(SelectedEnemy).Rect.Y = CInt(Math.Round((pointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

                    Enemies(SelectedEnemy).Position.X = CInt(Math.Round((pointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                    Enemies(SelectedEnemy).Position.Y = CInt(Math.Round((pointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

                    Enemies(SelectedEnemy).PatrolA.X = CInt(Math.Round((pointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                    Enemies(SelectedEnemy).PatrolA.Y = CInt(Math.Round((pointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

                    'Move patrol point B.
                    Enemies(SelectedEnemy).PatrolB.X = Enemies(SelectedEnemy).PatrolA.X + PatrolWidth - GridSize
                    Enemies(SelectedEnemy).PatrolB.Y = Enemies(SelectedEnemy).PatrolA.Y

                    Dim PatrolRect As New Rectangle(Enemies(SelectedEnemy).PatrolA.X,
                                                    Enemies(SelectedEnemy).PatrolA.Y,
                                                    PatrolWidth,
                                                    GridSize)

                    AutoSizeLevel(PatrolRect)

                End If

            End If

        End If

        If LevelSelected = True Then

            If e.Button = MouseButtons.Left Then

                Camera.Rect.X = e.X - SelectionOffset.X

                Camera.Rect.Y = e.Y - SelectionOffset.Y

                BufferGridLines()

            End If

        End If

    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown

        Select Case e.KeyCode

            'Has the player pressed the right arrow key down?
            Case Keys.Right
                'Yes, the player has pressed the right arrow key down.

                If GameState = AppState.Playing Then

                    RightArrowDown = True

                    LeftArrowDown = False

                End If

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        'Move Camera to the right.
                        Camera.Rect.X -= 10

                        BufferGridLines()

                    Else

                        'Move mouse pointer right.
                        Cursor.Position = New Point(Cursor.Position.X + 2, Cursor.Position.Y)

                    End If

                End If

            'Has the player pressed the left arrow key down?
            Case Keys.Left
                'Yes, the player has pressed the left arrow key down.

                If GameState = AppState.Playing Then

                    LeftArrowDown = True

                    RightArrowDown = False

                End If

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        'Move Camera to the left.
                        Camera.Rect.X += 10

                        BufferGridLines()

                    Else

                        'Move mouse pointer left.
                        Cursor.Position = New Point(Cursor.Position.X - 2, Cursor.Position.Y)

                    End If

                End If

            Case Keys.Up

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        'Move Camera up.
                        Camera.Rect.Y += 10

                        BufferGridLines()

                    Else

                        'Move mouse pointer up.
                        Cursor.Position = New Point(Cursor.Position.X, Cursor.Position.Y - 2)

                    End If

                End If

            Case Keys.Down

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        'Move Camera down.
                        Camera.Rect.Y -= 10

                        BufferGridLines()

                    Else

                        'Move mouse pointer up.
                        Cursor.Position = New Point(Cursor.Position.X, Cursor.Position.Y + 2)

                    End If

                End If

            Case Keys.A

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        If IsMouseDown = False Then

                            IsMouseDown = True

                            DoMouseLeftDown()

                        End If

                    End If

                End If

            Case Keys.Enter

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        If IsMouseDown = False Then

                            IsMouseDown = True

                            DoMouseLeftDown()

                        End If

                    End If

                End If

            'Has the player pressed the B key down?
            Case Keys.B
                'Yes, the player has pressed the B key down.

                BDown = True

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        ShowSaveLevelDialog()

                        ShowMenu = False

                    End If

                End If

            Case Keys.Y

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        ShowOpenLevelDialog()

                        ShowMenu = False

                    End If

                End If

            Case Keys.N

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        InitAndCreateNewLevel()

                        ShowMenu = False

                    End If

                End If

            Case Keys.R

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        InitAndCreateNewLevel()

                        ShowMenu = False

                    End If

                End If

            Case Keys.O

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        ShowOpenLevelDialog()

                        ShowMenu = False

                    End If

                End If

            Case Keys.S

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        ShowSaveLevelDialog()

                        ShowMenu = False

                    End If

                End If

            Case Keys.C

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        ShowMenu = False

                    End If

                End If

            Case Keys.Escape

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        ShowMenu = False

                    End If

                End If

                'Has the player pressed the delete key down?
            Case Keys.Delete
                'Yes, the player has pressed the delete key down.

                If GameState = AppState.Editing Then

                    If SelectedBlock > -1 Then

                        RemoveBlock(SelectedBlock)

                        SelectedBlock = -1

                    End If

                    If SelectedBill > -1 Then

                        RemoveBill(SelectedBill)

                        SelectedBill = -1

                    End If

                    If SelectedBush > -1 Then

                        RemoveBush(SelectedBush)

                        SelectedBush = -1

                    End If

                    If SelectedCloud > -1 Then

                        RemoveCloud(SelectedCloud)

                        SelectedCloud = -1

                    End If

                    If SelectedEnemy > -1 Then

                        RemoveEnemy(SelectedEnemy)

                        SelectedEnemy = -1

                    End If

                    If GoalSelected = True Then

                        'Place goal off level.
                        Goal.Rect.X = -100
                        Goal.Rect.Y = -100

                        GoalSelected = False

                    End If

                End If

            Case Keys.M 'Mute

                If IsBackgroundLoopPlaying = True Then

                    IsMuted = True

                    My.Computer.Audio.Stop()

                    IsBackgroundLoopPlaying = False

                Else

                    IsMuted = False

                    PlayLevelMusic()

                End If

            Case Keys.X

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        ShowMenu = False

                    End If

                End If

            Case 93 'Context Menu

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        ShowMenu = True

                        MovePointerCenterMenu()

                    End If

                End If

        End Select

    End Sub

    Private Sub Form1_KeyUp(sender As Object, e As KeyEventArgs) Handles MyBase.KeyUp

        Select Case e.KeyCode

            Case Keys.Right

                RightArrowDown = False

            Case Keys.Left

                LeftArrowDown = False

            Case Keys.B

                If Jumped = True Then Jumped = False

                BDown = False

                'Has the player let the delete key up?
            Case Keys.Delete
                'Yes, the player has let the delete key up.

                DeleteDown = False

        End Select

    End Sub

    Private Sub UpdateButtonPosition()
        'The range of buttons is 0 to 65,535. Unsigned 16-bit (2-byte) integer.

        'What buttons are down?
        Select Case ControllerPosition.Gamepad.wButtons

            Case 0 'All the buttons are up.

                If ControllerJumped = True Then ControllerJumped = False

                ControllerA = False

                ControllerB = False

                ControllerRight = False

                ControllerLeft = False

                If GameState = AppState.Start Or GameState = AppState.Editing Then

                    If IsMouseDown = True Then

                        IsMouseDown = False

                        DoMouseLeftUp()

                    End If

                    If IsBDown = True Then

                        IsBDown = False

                    End If

                End If

                If IsStartDown = True Then

                    IsStartDown = False

                End If

            Case 1 'Up

                ControllerLeft = False

                ControllerRight = False

                If ControllerJumped = True Then ControllerJumped = False

                ControllerA = False

                ControllerB = False

                If GameState = AppState.Start Or GameState = AppState.Editing Then

                    If IsMouseDown = True Then

                        IsMouseDown = False

                        DoMouseLeftUp()

                    End If

                    'Move mouse pointer up.
                    Cursor.Position = New Point(Cursor.Position.X, Cursor.Position.Y - 2)

                End If

            Case 2 'Down

                ControllerLeft = False

                ControllerRight = False

                If ControllerJumped = True Then ControllerJumped = False

                ControllerA = False

                ControllerB = False

                If GameState = AppState.Start Or GameState = AppState.Editing Then

                    If IsMouseDown = True Then

                        IsMouseDown = False

                        DoMouseLeftUp()

                    End If

                    'Move mouse pointer down.
                    Cursor.Position = New Point(Cursor.Position.X, Cursor.Position.Y + 2)

                End If

            Case 4 'Left

                ControllerLeft = True

                ControllerRight = False

                If ControllerJumped = True Then ControllerJumped = False

                ControllerA = False

                ControllerB = False

                If GameState = AppState.Start Or GameState = AppState.Editing Then

                    If IsMouseDown = True Then

                        IsMouseDown = False

                        DoMouseLeftUp()

                    End If

                    'Move mouse pointer to the left.
                    Cursor.Position = New Point(Cursor.Position.X - 2, Cursor.Position.Y)

                End If

            Case 5 'Up+Left

                ControllerLeft = True

                ControllerRight = False

                If ControllerJumped = True Then ControllerJumped = False

                ControllerA = False

                ControllerB = False

                If GameState = AppState.Start Or GameState = AppState.Editing Then

                    If IsMouseDown = True Then

                        IsMouseDown = False

                        DoMouseLeftUp()

                    End If

                    'Move mouse pointer up and left.
                    Cursor.Position = New Point(Cursor.Position.X - 2, Cursor.Position.Y - 2)

                End If

            Case 6 'Down+Left

                ControllerLeft = True

                ControllerRight = False

                If ControllerJumped = True Then ControllerJumped = False

                ControllerA = False

                ControllerB = False

                If GameState = AppState.Start Or GameState = AppState.Editing Then

                    If IsMouseDown = True Then

                        IsMouseDown = False

                        DoMouseLeftUp()

                    End If

                    'Move mouse pointer down and to the left.
                    Cursor.Position = New Point(Cursor.Position.X - 2, Cursor.Position.Y + 2)

                End If

            Case 8 'Right

                ControllerRight = True

                ControllerLeft = False

                If ControllerJumped = True Then ControllerJumped = False

                ControllerA = False

                ControllerB = False

                If GameState = AppState.Start Or GameState = AppState.Editing Then

                    If IsMouseDown = True Then

                        IsMouseDown = False

                        DoMouseLeftUp()

                    End If

                    'Move mouse pointer to the right.
                    Cursor.Position = New Point(Cursor.Position.X + 2, Cursor.Position.Y)

                End If

            Case 9 'Up+Right

                ControllerLeft = False

                ControllerRight = True

                If ControllerJumped = True Then ControllerJumped = False

                ControllerA = False

                ControllerB = False

                If GameState = AppState.Start Or GameState = AppState.Editing Then

                    If IsMouseDown = True Then

                        IsMouseDown = False

                        DoMouseLeftUp()

                    End If

                    'Move mouse pointer up and to the right.
                    Cursor.Position = New Point(Cursor.Position.X + 2, Cursor.Position.Y - 2)

                End If

            Case 10 'Down+Right

                ControllerLeft = False

                ControllerRight = True

                If ControllerJumped = True Then ControllerJumped = False

                ControllerA = False

                ControllerB = False

                If GameState = AppState.Start Or GameState = AppState.Editing Then

                    If IsMouseDown = True Then

                        IsMouseDown = False

                        DoMouseLeftUp()

                    End If

                    'Move mouse pointer down and to the right.
                    Cursor.Position = New Point(Cursor.Position.X + 2, Cursor.Position.Y + 2)

                End If

            Case 16 'Start

                Select Case GameState

                    Case AppState.Playing

                        If IsStartDown = False Then

                            IsStartDown = True

                            'Remember the cameras in game position before opening the editor.
                            CameraPlayPostion.X = Camera.Rect.X
                            CameraPlayPostion.Y = Camera.Rect.Y

                            'Move mouse pointer to the center of the client rctangle.
                            Cursor.Position = New Point(ClientRectangle.X + ClientRectangle.Width / 2,
                                                       ClientRectangle.Y + ClientRectangle.Height / 2)

                            GameState = AppState.Editing

                            BufferGridLines()

                        End If

                    Case AppState.Editing

                        If IsStartDown = False Then

                            IsStartDown = True

                            DeselectObjects()

                            'Restore the cameras in game position.
                            Camera.Rect.X = CameraPlayPostion.X
                            Camera.Rect.Y = CameraPlayPostion.Y

                            MovePointerOffScreen()

                            LastFrame = Now

                            GameState = AppState.Playing

                        End If

                End Select

            Case 32 'Back

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        ShowMenu = True

                        MovePointerCenterMenu()

                    End If

                End If

            Case 64 'Left Stick
            Case 128 'Right Stick
            Case 256 'Left bumper

                If GameState = AppState.Editing Then

                    If SelectedBlock > -1 Then

                        RemoveBlock(SelectedBlock)

                        SelectedBlock = -1

                    End If

                    If SelectedBill > -1 Then

                        RemoveBill(SelectedBill)

                        SelectedBill = -1

                    End If

                    If SelectedBush > -1 Then

                        RemoveBush(SelectedBush)

                        SelectedBush = -1

                    End If

                    If SelectedCloud > -1 Then

                        RemoveCloud(SelectedCloud)

                        SelectedCloud = -1

                    End If

                    If GoalSelected = True Then

                        'Place goal off level.
                        Goal.Rect.X = -100
                        Goal.Rect.Y = -100

                        GoalSelected = False

                    End If

                End If

            Case 512 'Right bumper
                'The player pushed the right bumper down.

                If GameState = AppState.Editing Then

                    If SelectedBlock > -1 Then

                        RemoveBlock(SelectedBlock)

                        SelectedBlock = -1

                    End If

                    If SelectedBill > -1 Then

                        RemoveBill(SelectedBill)

                        SelectedBill = -1

                    End If

                    If SelectedBush > -1 Then

                        RemoveBush(SelectedBush)

                        SelectedBush = -1

                    End If

                    If SelectedCloud > -1 Then

                        RemoveCloud(SelectedCloud)

                        SelectedCloud = -1

                    End If

                    If SelectedEnemy > -1 Then

                        RemoveEnemy(SelectedEnemy)

                        SelectedEnemy = -1

                    End If

                    If GoalSelected = True Then

                        'Place goal off level.
                        Goal.Rect.X = -100
                        Goal.Rect.Y = -100

                        GoalSelected = False

                    End If

                End If

            Case 4096 'A

                ControllerA = True

                ControllerLeft = False

                ControllerRight = False

                ControllerB = False

                If GameState = AppState.Start Or GameState = AppState.Editing Then

                    If IsMouseDown = False Then

                        IsMouseDown = True

                        DoMouseLeftDown()

                    End If

                End If

            Case 8192 'B

                ControllerB = True

                If ControllerJumped = True Then ControllerJumped = False

                ControllerA = False

                ControllerLeft = False

                ControllerRight = False

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        Cursor.Position = New Point(ScreenOffset.X + SaveButton.Rect.X, ScreenOffset.Y + SaveButton.Rect.Y)

                        If IsMouseDown = False Then

                            IsMouseDown = True

                            DoMouseLeftDown()

                        End If

                    End If

                End If

                If GameState = AppState.Start Then

                    Cursor.Position = New Point(ScreenOffset.X + StartScreenNewButton.Rect.X, ScreenOffset.Y + StartScreenNewButton.Rect.Y)

                    If IsMouseDown = False Then

                        IsMouseDown = True

                        DoMouseLeftDown()

                    End If

                End If

            Case 16384 'X

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        ShowMenu = False

                    End If

                End If

            Case 32768 'Y

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        Cursor.Position = New Point(ScreenOffset.X + OpenButton.Rect.X, ScreenOffset.Y + OpenButton.Rect.Y)

                        If IsMouseDown = False Then

                            IsMouseDown = True

                            DoMouseLeftDown()

                        End If

                    End If

                End If

                If GameState = AppState.Start Then

                    Cursor.Position = New Point(ScreenOffset.X + StartScreenOpenButton.Rect.X, ScreenOffset.Y + StartScreenOpenButton.Rect.Y)

                    If IsMouseDown = False Then

                        IsMouseDown = True

                        DoMouseLeftDown()

                    End If

                End If

            Case 48 'Start+Back
            Case 192 'Left+Right Sticks
            Case 768 'Left+Right Bumpers
            Case 12288 'A+B
            Case 20480 'A+X
            Case 36864 'A+Y
            Case 24576 'B+X
            Case 40960 'B+Y
            Case 49152 'X+Y
            Case 28672 'A+B+X
            Case 45056 'A+B+Y
            Case 53248 'A+X+Y
            Case 57344 'B+X+Y
            Case 61440 'A+B+X+Y
            Case 4097 'Up+A

                ControllerA = True

                ControllerLeft = True

                ControllerRight = False

                ControllerB = False

                If GameState = AppState.Editing Then

                    If IsMouseDown = False Then

                        IsMouseDown = True

                        DoMouseLeftDown()

                    End If

                    'Move mouse pointer down.
                    Cursor.Position = New Point(Cursor.Position.X, Cursor.Position.Y - 2)

                End If

            Case 4098 'Down+A

                ControllerA = True

                ControllerLeft = True

                ControllerRight = False

                ControllerB = False

                If GameState = AppState.Editing Then

                    If IsMouseDown = False Then

                        IsMouseDown = True

                        DoMouseLeftDown()

                    End If

                    'Move mouse pointer down.
                    Cursor.Position = New Point(Cursor.Position.X, Cursor.Position.Y + 2)

                End If

            Case 4100 'Left+A

                ControllerA = True

                ControllerLeft = True

                ControllerRight = False

                ControllerB = False

                If GameState = AppState.Editing Then

                    If IsMouseDown = False Then

                        IsMouseDown = True

                        DoMouseLeftDown()

                    End If

                    'Move mouse pointer to the left.
                    Cursor.Position = New Point(Cursor.Position.X - 2, Cursor.Position.Y)

                End If

            Case 4104 'Right+A

                ControllerA = True

                ControllerRight = True

                ControllerLeft = False

                ControllerB = False

                If GameState = AppState.Editing Then

                    If IsMouseDown = False Then

                        IsMouseDown = True

                        DoMouseLeftDown()

                    End If

                    'Move mouse pointer to the right.
                    Cursor.Position = New Point(Cursor.Position.X + 2, Cursor.Position.Y)

                End If

            Case 4105 'Up+Right+A

                ControllerA = True

                ControllerRight = True

                ControllerLeft = False

                ControllerB = False

                If GameState = AppState.Editing Then

                    If IsMouseDown = False Then

                        IsMouseDown = True

                        DoMouseLeftDown()

                    End If

                    'Move mouse pointer to the right.
                    Cursor.Position = New Point(Cursor.Position.X + 2, Cursor.Position.Y - 2)

                End If

            Case 4101 'Up+Left+A

                ControllerA = True

                ControllerLeft = True

                ControllerRight = False

                ControllerB = False

                If GameState = AppState.Editing Then

                    If IsMouseDown = False Then

                        IsMouseDown = True

                        DoMouseLeftDown()

                    End If

                    'Move mouse pointer to the left.
                    Cursor.Position = New Point(Cursor.Position.X - 2, Cursor.Position.Y - 2)

                End If

            Case 4106 'Down+Right+A

                ControllerA = True

                ControllerRight = True

                ControllerLeft = False

                ControllerB = False

                If GameState = AppState.Editing Then

                    If IsMouseDown = False Then

                        IsMouseDown = True

                        DoMouseLeftDown()

                    End If

                    'Move mouse pointer to the right.
                    Cursor.Position = New Point(Cursor.Position.X + 2, Cursor.Position.Y + 2)

                End If

            Case 4102 'Down+Left+A

                ControllerA = True

                ControllerLeft = True

                ControllerRight = False

                ControllerB = False

                If GameState = AppState.Editing Then

                    If IsMouseDown = False Then

                        IsMouseDown = True

                        DoMouseLeftDown()

                    End If

                    'Move mouse pointer to the left.
                    Cursor.Position = New Point(Cursor.Position.X - 2, Cursor.Position.Y + 2)

                End If

            Case 8196 'Left+B

                ControllerLeft = True

                ControllerRight = False

                ControllerA = False

                ControllerB = True

            Case 8200 'Right+B

                ControllerRight = True

                ControllerLeft = False

                ControllerA = False

                ControllerB = True

            Case 8198 'Left+Down+B

                ControllerRight = False

                ControllerLeft = True

                ControllerB = True
            Case 8202 'Right+Down+B
                ControllerRight = True

                ControllerLeft = False

                ControllerA = False

                ControllerB = True

            Case 8201 'Right+Up+B
                ControllerRight = True

                ControllerLeft = False

                ControllerB = True

            Case 8197 'Left+Up+B
                ControllerRight = False

                ControllerLeft = True

                ControllerA = False

                ControllerB = True
            Case 8194 'Down+B
                ControllerRight = False

                ControllerLeft = False

                ControllerA = False

                ControllerB = True
            Case 8193 'Up+B
                ControllerRight = False

                ControllerLeft = False

                ControllerA = False

                ControllerB = True
            Case Else 'Any buttons not handled yet.

                Debug.Print(ControllerPosition.Gamepad.wButtons.ToString)

        End Select

    End Sub

    Private Sub VibrateLeft(ControllerNumber As Integer, Speed As UShort)
        'The range of speed is 0 through 65,535. Unsigned 16-bit (2-byte) integer.
        'The left motor is the low-frequency rumble motor.

        'Turn right motor off (set zero speed).
        Vibration.wRightMotorSpeed = 0

        'Set left motor speed.
        Vibration.wLeftMotorSpeed = Speed

        Vibrate(ControllerNumber)

    End Sub

    Private Sub VibrateRight(ControllerNumber As Integer, Speed As UShort)
        'The range of speed is 0 through 65,535. Unsigned 16-bit (2-byte) integer.
        'The right motor is the high-frequency rumble motor.

        'Turn left motor off (set zero speed).
        Vibration.wLeftMotorSpeed = 0

        'Set right motor speed.
        Vibration.wRightMotorSpeed = Speed

        Vibrate(ControllerNumber)

    End Sub

    Private Sub Vibrate(ControllerNumber As Integer)

        Try

            'Turn motor on.
            If XInputSetState(ControllerNumber, Vibration) = 0 Then
                'Success
                'Text = XInputSetState(ControllerNumber, vibration).ToString
            Else
                'Fail
                'Text = XInputSetState(ControllerNumber, vibration).ToString
            End If

        Catch ex As Exception

            MsgBox(ex.ToString)

            Exit Sub

        End Try

    End Sub

    Private Function IsOnPlatform() As Integer

        If Platforms IsNot Nothing Then

            For Each Platform In Platforms

                'Is our hero colliding with the platform?
                If OurHero.Rect.IntersectsWith(Platform.Rect) = True Then
                    'Yes, our hero is colliding with the platform.

                    Return Array.IndexOf(Platforms, Platform)

                End If

            Next

        End If

        Return -1

    End Function

    Private Function IsOnBlock() As Integer

        If Blocks IsNot Nothing Then

            For Each Block In Blocks

                'Is our hero colliding with the block?
                If OurHero.Rect.IntersectsWith(Block.Rect) = True Then
                    'Yes, our hero is colliding with the block.

                    Return Array.IndexOf(Blocks, Block)

                End If

            Next

        End If

        Return -1

    End Function

    Private Function IsOnBill() As Integer

        If Cash IsNot Nothing Then

            For Each Bill In Cash

                'Is our hero colliding with the bill?
                If OurHero.Rect.IntersectsWith(Bill.Rect) = True Then
                    'Yes, our hero is colliding with the bill.

                    Return Array.IndexOf(Cash, Bill)

                End If

            Next

        End If

        Return -1

    End Function

    Private Function IsOnEnemy() As Integer

        If Enemies IsNot Nothing Then

            For Each Enemy In Enemies

                'Is our hero colliding with the enemy?
                If OurHero.Rect.IntersectsWith(Enemy.Rect) = True Then
                    'Yes, our hero is colliding with the enemy.

                    Return Array.IndexOf(Enemies, Enemy)

                End If

            Next

        End If

        Return -1

    End Function

    Private Sub Wraparound()

        'When our hero exits the bottom side of the level.
        If OurHero.Position.Y > Level.Rect.Bottom Then

            OurHero.Velocity.Y = 0F
            OurHero.Velocity.X = 0F

            OurHero.Position.X = 1500.0F

            'Our hero reappears on the top side the level.
            OurHero.Position.Y = Level.Rect.Top - OurHero.Rect.Height

        End If

    End Sub

    Private Sub FellOffLevel()

        'When our hero exits the bottom side of the level.
        If OurHero.Position.Y > Level.Rect.Bottom Then

            Camera.Rect.X = 0
            Camera.Rect.Y = 0

            BufferGridLines()

            ResetCash()

            ResurrectEnemies()

            ResetOurHero()

        End If

    End Sub

    Private Sub Form1_Move(sender As Object, e As EventArgs) Handles Me.Move

        ScreenOffset = PointToScreen(New Point(0, 0))

    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles MyBase.Closing

        GameLoopCancellationToken.Cancel(True)

    End Sub

    Protected Overrides Sub OnPaintBackground(ByVal e As PaintEventArgs)

        'Intentionally left blank. Do not remove.

    End Sub

End Class
