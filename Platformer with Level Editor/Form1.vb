'Platformer with Level Editor

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

'Testing Joseph Lumbley Jr
'Level music by Joseph Lumbley Jr.
'Level clear music by Joseph Lumbley Jr.

Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Numerics
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading

Public Class Form1

    'Import mciSendStringW for playback of multiple audio files simultaneously.
    <DllImport("winmm.dll", EntryPoint:="mciSendStringW")>
    Private Shared Function mciSendStringW(<MarshalAs(UnmanagedType.LPTStr)> ByVal lpszCommand As String,
                                           <MarshalAs(UnmanagedType.LPWStr)> ByVal lpszReturnString As StringBuilder,
                                           ByVal cchReturn As UInteger, ByVal hwndCallback As IntPtr) As Integer
    End Function

    'Import GetState to get the controller button positions.
    <DllImport("XInput1_4.dll")>
    Private Shared Function XInputGetState(dwUserIndex As Integer, ByRef pState As XINPUT_STATE) As Integer
    End Function

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

    'Import SetState to vibrate the controller.
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

    Private IsContextDown As Boolean = False

    Private RightTriggerDown As Boolean = False

    Private LeftTriggerDown As Boolean = False

    Private Const DPadUp As Integer = 1
    Private Const DPadDown As Integer = 2
    Private Const DPadLeft As Integer = 4
    Private Const DPadRight As Integer = 8

    Private Const StartButton As Integer = 16
    Private Const BackButton As Integer = 32

    Private Const LeftStickButton As Integer = 64
    Private Const RightStickButton As Integer = 128

    Private Const LeftBumperButton As Integer = 256
    Private Const RightBumperButton As Integer = 512

    Private Const AButton As Integer = 4096
    Private Const BButton As Integer = 8192
    Private Const XButton As Integer = 16384
    Private Const YButton As Integer = 32768

    Private DPadUpPressed As Boolean = False
    Private DPadDownPressed As Boolean = False
    Private DPadLeftPressed As Boolean = False
    Private DPadRightPressed As Boolean = False

    Private StartButtonPressed As Boolean = False
    Private BackButtonPressed As Boolean = False

    Private LeftStickButtonPressed As Boolean = False
    Private RightStickButtonPressed As Boolean = False

    Private LeftBumperButtonPressed As Boolean = False
    Private RightBumperButtonPressed As Boolean = False

    Private AButtonPressed As Boolean = False
    Private BButtonPressed As Boolean = False
    Private XButtonPressed As Boolean = False
    Private YButtonPressed As Boolean = False

    Private ADown As Boolean = False
    Private BDown As Boolean = False

    'Import SendInput to simulate mouse input from the controller.
    <DllImport("user32.dll")>
    Private Shared Function SendInput(nInputs As UInteger, pInputs As INPUTStruc(), cbSize As Integer) As UInteger
    End Function

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
    Private Const MOUSEEVENTF_LEFTDOWN As UInteger = 2
    Private Const MOUSEEVENTF_LEFTUP As UInteger = 4

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

    Private Enum Tools As Integer
        Pointer
        Block
        Bill
        Bush
        Cloud
        Goal
        Enemy
    End Enum

    Private GameState As AppState = AppState.Start

    Private Context As New BufferedGraphicsContext

    Private Buffer As BufferedGraphics

    Private FrameCount As Integer = 0

    Private StartTime As DateTime = Now 'Get current time.

    Private TimeElapsed As TimeSpan

    Private SecondsElapsed As Double = 0

    Private FPS As Integer = 0

    Private ReadOnly FPSFont As New Font(FontFamily.GenericSansSerif, 25)

    Private ReadOnly MenuButtonFont As New Font(FontFamily.GenericSansSerif, 40)

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

    Private Hero As GameObject

    Private MousePointer As GameObject

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

    Private MenuButtonHover As Boolean = False

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

    Private UpArrowDown As Boolean = False

    Private DownArrowDown As Boolean = False

    Private DeleteDown As Boolean = False

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

    Private OutinePen As New Pen(Color.Black, 4)

    Private MenuOutinePen As New Pen(Color.White, 9)

    Private MenuShadowPen As New Pen(Color.FromArgb(128, Color.Black), 16)

    Private MenuShadowBrush As New SolidBrush(Color.FromArgb(128, Color.Black))

    Private BButtonIconOutinePen As New Pen(Color.Tomato, 3)

    Private YButtonIconOutinePen As New Pen(Color.Yellow, 3)

    Private XButtonIconOutinePen As New Pen(Color.DeepSkyBlue, 3)

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

    Private IsBackDown As Boolean = False

    Private ClearScreenTimer As TimeSpan

    Private ClearScreenTimerStart As DateTime

    Private StopClearScreenTimer As Boolean = True

    Private GoalSelected As Boolean = False

    Private LevelSelected As Boolean = False

    Private LevelName As String = "Untitled"

    Private ScreenOffset As Point

    Private IsMuted As Boolean = False

    Private CameraOffset As New Point(0, 0)

    'Create array for sounds.
    Private Sounds() As String

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        InitializeApp()

    End Sub

    Private Sub GameTimer_Tick(sender As Object, e As EventArgs) Handles GameTimer.Tick

        UpdateFrame()

        Refresh()

    End Sub

    Private Sub UpdateFrame()

        Select Case GameState

            Case AppState.Start

                UpdateControllerData()

                UpdateEditorDeltaTime()

                UpdateMousePointerMovement()

            Case AppState.Playing

                UpdateControllerData()

                UpdateDeltaTime()

                UpdateEnemies()

                UpdateOurHero()

                UpdateCamera()

            Case AppState.Editing

                UpdateControllerData()

                UpdateEditorDeltaTime()

                UpdateMousePointerMovement()

                UpdateCameraMovement()

            Case AppState.Clear

                UpdateClearScreenTimer()

        End Select

    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)

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

            If Hero.Velocity.Y >= 0 Then
                'Apply gravity to our hero. FALLING.

                If Hero.Velocity.Y <= Hero.MaxVelocity.Y Then

                    Hero.Velocity.Y += Gravity * DeltaTime.TotalSeconds

                Else

                    Hero.Velocity.Y = Hero.MaxVelocity.Y

                End If

                ''Skydive steering
                'If RightArrowDown = True Or ControllerRight = True Then

                '    OurHero.Velocity.X += 25.5F * DeltaTime.TotalSeconds

                'ElseIf LeftArrowDown = True Or ControllerLeft = True Then

                '    OurHero.Velocity.X += -25.5F * DeltaTime.TotalSeconds

                'End If

            Else
                'Apply gravity to our hero. JUMPING.

                Hero.Velocity.Y += Gravity * DeltaTime.TotalSeconds

                'Max falling speed.
                If Hero.Velocity.Y > Hero.MaxVelocity.Y Then Hero.Velocity.Y = Hero.MaxVelocity.Y

                'air resistance
                If Hero.Velocity.X >= 0 Then

                    Hero.Velocity.X += -AirResistance * DeltaTime.TotalSeconds

                    If Hero.Velocity.X < 0 Then Hero.Velocity.X = 0

                Else

                    Hero.Velocity.X += AirResistance * DeltaTime.TotalSeconds

                    If Hero.Velocity.X > 0 Then Hero.Velocity.X = 0

                End If

            End If

        End If

        UpdateHeroMovement()

        UpdateCash()

        DoEnemyCollision()

        FellOffLevel()

        If Hero.Rect.IntersectsWith(Goal.Rect) = True Then

            DoGoalCollision()

        End If

    End Sub

    Private Sub UpdateCash()

        If Cash IsNot Nothing Then

            For Each Bill In Cash

                If Bill.Collected = False Then

                    'Is our hero colliding with the bill?
                    If Hero.Rect.IntersectsWith(Bill.Rect) = True Then
                        'Yes, our hero is colliding with the bill.

                        If IsMuted = False Then

                            PlayOverlapping("CashCollected")

                        End If

                        CashCollected += 100

                        Cash(Array.IndexOf(Cash, Bill)).Collected = True

                    End If

                End If

            Next

        End If

    End Sub

    Private Sub DoGoalCollision()

        If GameState = AppState.Playing Then

            ClearScreenTimerStart = Now

            StopClearScreenTimer = False

            If IsPlaying("Music") = True Then

                PauseSound("Music")

            End If

            If IsMuted = False Then

                PlaySound("clear")

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
                    If Hero.Rect.IntersectsWith(Enemy.Rect) = True Then
                        'Yes, our hero is colliding with the Enemy.

                        'Is our hero falling?
                        If Hero.Velocity.Y > 0 Then
                            'Yes, our hero is falling.

                            'Is our hero above the Enemy?
                            If Hero.Position.Y <= Enemy.Rect.Top - Hero.Rect.Height \ 2 Then
                                'Yes, our hero is above the enemy.

                                If IsMuted = False Then

                                    PlayOverlapping("eliminated")

                                End If

                                Enemies(Index).Eliminated = True

                            End If

                        Else
                            'Our Hero died.

                            'Restart level.
                            Camera.Position.X = 0
                            Camera.Position.Y = 0

                            UpdateCameraOffset()

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

        DrawBackground(Color.SkyBlue)

        DrawClouds()

        DrawBushes()

        DrawBlocks()

        DrawOurHero()

        DrawTitle()

        DrawStartScreenNewButton()

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

            DrawMenuOutline()

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

        Hero.Rect = New Rectangle(128, 769, 64, 64)

        Hero.Position = New PointF(Hero.Rect.X, Hero.Rect.Y)

        Hero.Velocity = New PointF(0, 0)

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

            If ClearScreenTimer.TotalMilliseconds > 3000 Then

                StopClearScreenTimer = True

                Camera.Position.X = 0
                Camera.Position.Y = 0

                UpdateCameraOffset()

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

            If IsPlaying("Music") = False Then

                LoopSound("Music")

            End If

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

                        DoButtonLogic()

                        UpdateLeftThumbstickPosition()

                        UpdateLeftTriggerPosition()

                        UpdateRightThumbstickPosition()

                        UpdateRightTriggerPosition()

                    End If

                    'Is controller zero disconnected and controller one connected?
                    If Connected(0) = False AndAlso Connected(1) = True AndAlso ControllerNumber = 1 Then
                        'Yes, controller zero is disconnected and controller one is connected.
                        'Use controller one.

                        UpdateButtonPosition()

                        DoButtonLogic()

                        UpdateLeftThumbstickPosition()

                        UpdateLeftTriggerPosition()

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

            If GameState = AppState.Start Or GameState = AppState.Editing Then

                MovePointerLeft()

            End If

            If GameState = AppState.Playing Then

                ControllerLeft = True

                ControllerRight = False

            End If

        ElseIf ControllerPosition.Gamepad.sThumbLX >= NeutralEnd Then
            'The left thumbstick is in the right position.

            If GameState = AppState.Start Or GameState = AppState.Editing Then

                MovePointerRight()

            End If

            If GameState = AppState.Playing Then

                ControllerLeft = False

                ControllerRight = True

            End If

        Else
            'The left thumbstick is in the neutral position.

            If GameState = AppState.Start Or GameState = AppState.Editing Then

                If DPadLeftPressed = False AndAlso DPadRightPressed = False AndAlso LeftArrowDown = False AndAlso RightArrowDown = False Then

                    DeceleratePointerXAxis()

                End If

            End If

        End If

        'What position is the left thumbstick in on the Y-axis?
        If ControllerPosition.Gamepad.sThumbLY <= NeutralStart Then
            'The left thumbstick is in the down position.

            If GameState = AppState.Start Or GameState = AppState.Editing Then

                MovePointerDown()

            End If

        ElseIf ControllerPosition.Gamepad.sThumbLY >= NeutralEnd Then
            'The left thumbstick is in the up position.

            If GameState = AppState.Start Or GameState = AppState.Editing Then

                MovePointerUp()

            End If

        Else
            'The left thumbstick is in the neutral position.

            If GameState = AppState.Start Or GameState = AppState.Editing Then

                If DPadUpPressed = False AndAlso DPadDownPressed = False AndAlso UpArrowDown = False AndAlso DownArrowDown = False Then

                    DeceleratePointerYAxis()

                End If

            End If

        End If

    End Sub

    Private Sub MovePointerLeft()

        'Is the pointer moving right?
        If MousePointer.Velocity.X > 0 Then
            'Yes, the pointer is moving right.

            'Stop move before changing direction.
            MousePointer.Velocity.X = 0 'Zero speed.

        End If

        'Move pointer left.
        MousePointer.Velocity.X += -MousePointer.Acceleration.X * EditorDeltaTime.TotalSeconds

        'Limit pointer velocity to the max.
        If MousePointer.Velocity.X < -MousePointer.MaxVelocity.X Then MousePointer.Velocity.X = -MousePointer.MaxVelocity.X

    End Sub

    Private Sub MovePointerRight()

        'Is the pointer moving left?
        If MousePointer.Velocity.X < 0 Then
            'Yes, the pointer is moving left.

            'Stop move before changing direction.
            MousePointer.Velocity.X = 0 'Zero speed.

        End If

        'Move pointer right.
        MousePointer.Velocity.X += MousePointer.Acceleration.X * EditorDeltaTime.TotalSeconds

        'Limit pointer velocity to the max.
        If MousePointer.Velocity.X > MousePointer.MaxVelocity.X Then MousePointer.Velocity.X = MousePointer.MaxVelocity.X

    End Sub

    Private Sub DeceleratePointerXAxis()

        If MousePointer.Velocity.X < 0 Then

            'Decelerate pointer.
            MousePointer.Velocity.X += MousePointer.Acceleration.X * 8 * EditorDeltaTime.TotalSeconds

            'Limit decelerate to zero speed.
            If MousePointer.Velocity.X > 0 Then MousePointer.Velocity.X = 0 'Zero speed.

        End If

        If MousePointer.Velocity.X > 0 Then

            'Decelerate pointer.
            MousePointer.Velocity.X += -MousePointer.Acceleration.X * 8 * EditorDeltaTime.TotalSeconds

            'Limit decelerate to zero speed.
            If MousePointer.Velocity.X < 0 Then MousePointer.Velocity.X = 0 'Zero speed.

        End If

    End Sub

    Private Sub DeceleratePointerYAxis()

        'Is the pointer moving up?
        If MousePointer.Velocity.Y < 0 Then
            'Yes, the pointer is moving up.

            'Decelerate pointer.
            MousePointer.Velocity.Y += MousePointer.Acceleration.Y * 8 * EditorDeltaTime.TotalSeconds

            'Limit decelerate to zero speed.
            If MousePointer.Velocity.Y > 0 Then MousePointer.Velocity.Y = 0 'Zero speed.

        End If

        'Is the pointer moving down?
        If MousePointer.Velocity.Y > 0 Then
            'Yes, the pointer is moving down.

            'Decelerate pointer.
            MousePointer.Velocity.Y += -MousePointer.Acceleration.Y * 8 * EditorDeltaTime.TotalSeconds

            'Limit decelerate to zero speed.
            If MousePointer.Velocity.Y < 0 Then MousePointer.Velocity.Y = 0 'Zero speed.

        End If

    End Sub

    Private Sub MovePointerUp()

        'Is the pointer moving down?
        If MousePointer.Velocity.Y > 0 Then
            'Yes, the pointer is moving down.

            'Stop move before changing direction.
            MousePointer.Velocity.Y = 0 'Zero speed.

        End If

        'Move pointer up.
        MousePointer.Velocity.Y += -MousePointer.Acceleration.Y * EditorDeltaTime.TotalSeconds

        'Limit pointer velocity to the max.
        If MousePointer.Velocity.Y < -MousePointer.MaxVelocity.Y Then MousePointer.Velocity.Y = -MousePointer.MaxVelocity.Y

    End Sub

    Private Sub MovePointerDown()

        'Is the pointer moving up?
        If MousePointer.Velocity.Y < 0 Then
            'Yes, the pointer is moving up.

            'Stop move before changing direction.
            MousePointer.Velocity.Y = 0 'Zero speed.

        End If

        'Move pointer down.
        MousePointer.Velocity.Y += MousePointer.Acceleration.Y * EditorDeltaTime.TotalSeconds

        'Limit pointer velocity to the max.
        If MousePointer.Velocity.Y > MousePointer.MaxVelocity.Y Then MousePointer.Velocity.Y = MousePointer.MaxVelocity.Y

    End Sub

    Private Sub UpdateRightTriggerPosition()
        'The range of right trigger is 0 to 255. Unsigned 8-bit (1-byte) integer.
        'The trigger position must be greater than the trigger threshold to register as pressed.

        'What position is the right trigger in?
        If ControllerPosition.Gamepad.bRightTrigger > TriggerThreshold Then
            'The right trigger is in the down position. Trigger Break. Bang!

            If GameState = AppState.Editing Then

                If ShowMenu = True Then

                    'Move mouse pointer over the new level button.
                    Cursor.Position = New Point(ScreenOffset.X + NewButton.Rect.X + NewButton.Rect.Width \ 2,
                                                ScreenOffset.Y + NewButton.Rect.Y + NewButton.Rect.Height \ 2)

                    If IsMouseDown = False Then

                        DoMouseLeftDown()

                        IsMouseDown = True

                    End If

                Else

                    If RightTriggerDown = False Then

                        RightTriggerDown = True

                        'Has the player reached the right end of the toolbar?
                        If SelectedTool = Tools.Enemy Then
                            'Yes, the player has reached the right end of the toolbar.

                            'Select the first tool.
                            SelectedTool = Tools.Pointer

                            ShowToolPreview = False

                        Else

                            DeselectObjects()

                            ToolPreview.Width = GridSize
                            ToolPreview.Height = GridSize

                            SelectedTool += 1

                            ShowToolPreview = True

                        End If

                    End If

                End If

            End If

        Else
            'The right trigger is in the neutral position. Pre-Travel.

            RightTriggerDown = False

        End If

    End Sub

    Private Sub UpdateLeftTriggerPosition()

        'The range of left trigger is 0 to 255. Unsigned 8-bit (1-byte) integer.
        'The trigger position must be greater than the trigger threshold to register as pressed.

        'What position is the left trigger in?
        If ControllerPosition.Gamepad.bLeftTrigger > TriggerThreshold Then
            'The left trigger is in the down position. Trigger Break. Bang!

            If GameState = AppState.Editing Then

                If ShowMenu = False Then

                    If LeftTriggerDown = False Then

                        LeftTriggerDown = True

                        'Has the player reached the left end of the toolbar?
                        If SelectedTool = Tools.Pointer Then
                            'Yes, the player has reached the left end of the toolbar.

                            DeselectObjects()

                            ToolPreview.Width = GridSize
                            ToolPreview.Height = GridSize

                            'Select the last tool.
                            SelectedTool = Tools.Enemy

                            ShowToolPreview = True

                        Else

                            DeselectObjects()

                            ToolPreview.Width = GridSize
                            ToolPreview.Height = GridSize

                            SelectedTool -= 1

                            ShowToolPreview = True

                        End If

                    End If

                End If

            End If

        Else
            'The left trigger is in the neutral position. Pre-Travel.

            LeftTriggerDown = False

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

                    'Is the camera moving right?
                    If Camera.Velocity.X > 0 Then
                        'Yes, the camera is moving right.

                        'Stop move before changing direction.
                        Camera.Velocity.X = 0 'Zero speed.

                    End If
                    'Move camera left.
                    Camera.Velocity.X += -Camera.Acceleration.X * EditorDeltaTime.TotalSeconds

                    'Limit camera velocity to the max.
                    If Camera.Velocity.X < -Camera.MaxVelocity.X Then Camera.Velocity.X = -Camera.MaxVelocity.X

                    UpdateCameraOffset()

                    BufferGridLines()

                End If

            End If

        ElseIf ControllerPosition.Gamepad.sThumbRX >= NeutralEnd Then
            'The right thumbstick is in the right position.

            If GameState = AppState.Editing Then

                If ShowMenu = False Then

                    'Is the camera moving left?
                    If Camera.Velocity.X < 0 Then
                        'Yes, the camera is moving left.

                        'Stop move before changing direction.
                        Camera.Velocity.X = 0 'Zero speed.

                    End If

                    'Move camera right.
                    Camera.Velocity.X += Camera.Acceleration.X * EditorDeltaTime.TotalSeconds

                    'Limit camera velocity to the max.
                    If Camera.Velocity.X > Camera.MaxVelocity.X Then Camera.Velocity.X = Camera.MaxVelocity.X

                    UpdateCameraOffset()

                    BufferGridLines()

                End If

            End If

        Else
            'The right thumbstick is in the neutral position.

            If GameState = AppState.Editing Then

                If LeftArrowDown = False And RightArrowDown = False Then

                    If Camera.Velocity.X < 0 Then

                        'Decelerate camera.
                        Camera.Velocity.X += Camera.Acceleration.X * 8 * EditorDeltaTime.TotalSeconds

                        'Limit decelerate to zero speed.
                        If Camera.Velocity.X > 0 Then Camera.Velocity.X = 0 'Zero speed.

                        UpdateCameraOffset()

                        BufferGridLines()

                    End If

                    If Camera.Velocity.X > 0 Then

                        'Decelerate camera.
                        Camera.Velocity.X += -Camera.Acceleration.X * 8 * EditorDeltaTime.TotalSeconds

                        'Limit decelerate to zero speed.
                        If Camera.Velocity.X < 0 Then Camera.Velocity.X = 0 'Zero speed.

                        UpdateCameraOffset()

                        BufferGridLines()

                    End If

                End If

            End If

        End If

        'What position is the right thumbstick in on the Y-axis?
        If ControllerPosition.Gamepad.sThumbRY <= NeutralStart Then
            'The right thumbstick is in the down position.

            If GameState = AppState.Editing Then

                If ShowMenu = False Then

                    'Is the camera moving up?
                    If Camera.Velocity.Y < 0 Then
                        'Yes, the camera is moving up.

                        'Stop move before changing direction.
                        Camera.Velocity.Y = 0 'Zero speed.

                    End If

                    'Move camera down.
                    Camera.Velocity.Y += Camera.Acceleration.Y * EditorDeltaTime.TotalSeconds

                    'Limit camera velocity to the max.
                    If Camera.Velocity.Y > Camera.MaxVelocity.Y Then Camera.Velocity.Y = Camera.MaxVelocity.Y

                    UpdateCameraOffset()

                    BufferGridLines()

                End If

            End If

        ElseIf ControllerPosition.Gamepad.sThumbRY >= NeutralEnd Then
            'The right thumbstick is in the up position.

            If GameState = AppState.Editing Then

                If ShowMenu = False Then

                    'Is the camera moving down?
                    If Camera.Velocity.Y > 0 Then
                        'Yes, the camera is moving down.

                        'Stop move before changing direction.
                        Camera.Velocity.Y = 0 'Zero speed.

                    End If

                    'Move camera up.
                    Camera.Velocity.Y += -Camera.Acceleration.Y * EditorDeltaTime.TotalSeconds

                    'Limit camera velocity to the max.
                    If Camera.Velocity.Y < -Camera.MaxVelocity.Y Then Camera.Velocity.Y = -Camera.MaxVelocity.Y

                    UpdateCameraOffset()

                    BufferGridLines()

                End If

            End If

        Else
            'The right thumbstick is in the neutral position.

            If GameState = AppState.Editing Then


                If UpArrowDown = False And DownArrowDown = False Then

                    'Is the camera moving up?
                    If Camera.Velocity.Y < 0 Then
                        'Yes, the camera is moving up.

                        'Decelerate camera.
                        Camera.Velocity.Y += Camera.Acceleration.Y * 8 * EditorDeltaTime.TotalSeconds

                        'Limit decelerate to zero speed.
                        If Camera.Velocity.Y > 0 Then Camera.Velocity.Y = 0 'Zero speed.

                        UpdateCameraOffset()

                        BufferGridLines()

                    End If

                    'Is the camera moving down?
                    If Camera.Velocity.Y > 0 Then
                        'Yes, the camera is moving down.

                        'Decelerate camera.
                        Camera.Velocity.Y += -Camera.Acceleration.Y * 8 * EditorDeltaTime.TotalSeconds

                        'Limit decelerate to zero speed.
                        If Camera.Velocity.Y < 0 Then Camera.Velocity.Y = 0 'Zero speed.

                        UpdateCameraOffset()

                        BufferGridLines()

                    End If

                End If

            End If

        End If

    End Sub

    Private Sub UpdateHeroMovement()

        'Move our hero horizontally.
        Hero.Position.X += Hero.Velocity.X * DeltaTime.TotalSeconds 'Δs = V * Δt
        'Displacement = Velocity x Delta Time

        Hero.Rect.X = Math.Round(Hero.Position.X)

        'Move our hero vertically.
        Hero.Position.Y += Hero.Velocity.Y * DeltaTime.TotalSeconds 'Δs = V * Δt
        'Displacement = Velocity x Delta Time

        Hero.Rect.Y = Math.Round(Hero.Position.Y)

    End Sub

    Private Sub UpdateMousePointerMovement()

        MousePointer.Position.X = Cursor.Position.X
        MousePointer.Position.Y = Cursor.Position.Y

        'Move pointer horizontally.
        MousePointer.Position.X += MousePointer.Velocity.X * EditorDeltaTime.TotalSeconds 'Δs = V * Δt
        'Displacement = Velocity x Delta Time

        MousePointer.Rect.X = Math.Round(MousePointer.Position.X)

        'Move pointer vertically.
        MousePointer.Position.Y += MousePointer.Velocity.Y * EditorDeltaTime.TotalSeconds 'Δs = V * Δt
        'Displacement = Velocity x Delta Time

        MousePointer.Rect.Y = Math.Round(MousePointer.Position.Y)

        Cursor.Position = New Point(MousePointer.Rect.X, MousePointer.Rect.Y)

    End Sub

    Private Sub UpdateCameraMovement()

        'Move camera horizontally.
        Camera.Position.X += Camera.Velocity.X * EditorDeltaTime.TotalSeconds 'Δs = V * Δt
        'Displacement = Velocity x Delta Time

        Camera.Rect.X = Math.Round(Camera.Position.X)

        'Move camera vertically.
        Camera.Position.Y += Camera.Velocity.Y * EditorDeltaTime.TotalSeconds 'Δs = V * Δt
        'Displacement = Velocity x Delta Time

        Camera.Rect.Y = Math.Round(Camera.Position.Y)

    End Sub

    Private Sub UpdateHeroPosition()

        Hero.Rect.X = Math.Round(Hero.Position.X)

        Hero.Rect.Y = Math.Round(Hero.Position.Y)

    End Sub

    Private Sub UpdateEnemies()

        If Enemies IsNot Nothing Then

            For Each Enemy In Enemies

                If Enemy.Eliminated = False Then

                    Dim EnemyIndex As Integer = Array.IndexOf(Enemies, Enemy)

                    If Enemy.PatrolDirection = Direction.Right Then

                        'Move Enemy to the right.
                        Enemies(EnemyIndex).Velocity.X += Enemy.Acceleration.X * DeltaTime.TotalSeconds

                        'Limit Enemy velocity to the max.
                        If Enemy.Velocity.X > Enemy.MaxVelocity.X Then

                            Enemies(EnemyIndex).Velocity.X = Enemy.MaxVelocity.X

                        End If

                    Else

                        'Move Enemy to the left.
                        Enemies(EnemyIndex).Velocity.X += -Enemy.Acceleration.X * DeltaTime.TotalSeconds

                        'Limit Enemy velocity to the max.
                        If Enemy.Velocity.X < -Enemy.MaxVelocity.X Then

                            Enemies(EnemyIndex).Velocity.X = -Enemy.MaxVelocity.X

                        End If

                    End If

                    Enemies(EnemyIndex).Position.X += Enemy.Velocity.X * DeltaTime.TotalSeconds

                    If Enemy.Position.X >= Enemy.PatrolB.X Then

                        'Is Enemy moving to the right?
                        If Enemy.Velocity.X > 0 Then

                            'Stop the move before changing direction.
                            Enemies(EnemyIndex).Velocity.X = 0 'Zero speed.

                            'Aline the enemy to the patrol b point.
                            Enemies(EnemyIndex).Position.X = Enemy.PatrolB.X

                            Enemies(EnemyIndex).PatrolDirection = Direction.Left

                        End If

                    End If

                    If Enemy.Position.X <= Enemy.PatrolA.X Then

                        'Is Enemy moving to the left?
                        If Enemy.Velocity.X < 0 Then

                            'Stop the move before changing direction.
                            Enemies(EnemyIndex).Velocity.X = 0 'Zero speed.

                            'Aline the enemy to the patrol a point.
                            Enemies(EnemyIndex).Position.X = Enemy.PatrolA.X

                            Enemies(EnemyIndex).PatrolDirection = Direction.Right

                        End If

                    End If

                    Enemies(EnemyIndex).Rect.X = Math.Round(Enemy.Position.X)

                End If

            Next

        End If

    End Sub
    Private Sub UpdateBlocks()

        If Blocks IsNot Nothing Then

            For Each Block In Blocks

                'Is our hero colliding with the block?
                If Hero.Rect.IntersectsWith(Block.Rect) = True Then
                    'Yes, our hero is colliding with the block.

                    'Is our hero on top of the block.
                    If Hero.Rect.Y = Block.Rect.Top - Hero.Rect.Height + 1 Then
                        'Yes, our hero is on top of the block.

                        'Is the player holding down the right arrow key?
                        If RightArrowDown = True Or ControllerRight = True Then
                            'Yes, the player is holding down the right arrow key.

                            'Is our hero moving to the left?
                            If Hero.Velocity.X < 0 Then

                                'Stop the move before change in direction.
                                Hero.Velocity.X = 0 'Zero speed.

                            End If

                            'Move our hero the right.
                            Hero.Velocity.X += Hero.Acceleration.X * DeltaTime.TotalSeconds

                            'Limit our heros velocity to the max.
                            If Hero.Velocity.X > Hero.MaxVelocity.X Then Hero.Velocity.X = Hero.MaxVelocity.X

                            'Is the player holding down the left arrow key?
                        ElseIf LeftArrowDown = True Or ControllerLeft = True Then
                            'Yes, the player is holding down the left arrow key.

                            'Is our hero moving to the right?
                            If Hero.Velocity.X > 0F Then
                                'Yes, our hero is moving to the right.

                                'Stop the move before change in direction.
                                Hero.Velocity.X = 0F 'Zero speed.

                            End If

                            'Move our hero the left.
                            Hero.Velocity.X += -Hero.Acceleration.X * DeltaTime.TotalSeconds

                            'Limit our heros velocity to the max.
                            If Hero.Velocity.X < -Hero.MaxVelocity.X Then Hero.Velocity.X = -Hero.MaxVelocity.X

                        Else
                            'No,the player is NOT holding down the right arrow key.
                            'No, the player is NOT holding down the left arrow key.

                            'Is our hero moving to the right?
                            If Hero.Velocity.X > 0F Then
                                'Yes, our hero is moving to the right.

                                'Slow our hero down.
                                Hero.Velocity.X += -Friction * DeltaTime.TotalSeconds

                                If Hero.Velocity.X < 0F Then
                                    Hero.Velocity.X = 0F
                                End If

                            ElseIf Hero.Velocity.X < 0F Then

                                Hero.Velocity.X += Friction * DeltaTime.TotalSeconds

                                If Hero.Velocity.X > 0F Then
                                    Hero.Velocity.X = 0F
                                End If

                            End If

                        End If

                        If ADown = True Then

                            If Jumped = False Then

                                Hero.Velocity.Y += -1100.0F

                                Jumped = True

                            End If

                        End If

                        If ControllerA = True Then

                            If ControllerJumped = False Then

                                Hero.Velocity.Y += -1100.0F

                                ControllerJumped = True

                            End If

                        End If

                    Else

                        DoBlockCollision(Block.Rect)

                    End If

                End If

            Next

        End If

    End Sub

    Private Sub DoBlockCollision(Block As Rectangle)

        Dim CombinedHalfWidths As Single = (Hero.Rect.Width + Block.Width) / 2
        Dim CombinedHalfHeights As Single = ((Hero.Rect.Height - 1) + Block.Height) / 2

        Dim DeltaX As Single = (Block.X + Block.Width / 2) - (Hero.Rect.X + Hero.Rect.Width / 2)
        Dim DeltaY As Single = (Block.Y + Block.Height / 2) - (Hero.Rect.Y + (Hero.Rect.Height - 1) / 2)

        Dim OverlapX As Single = CombinedHalfWidths - Math.Abs(DeltaX)
        Dim OverlapY As Single = CombinedHalfHeights - Math.Abs(DeltaY)

        If OverlapX > 0 And OverlapY > 0 Then
            ' Collision detected, resolve it

            Dim ResolveX As Single = If(OverlapX <= OverlapY, OverlapX * Math.Sign(DeltaX), 0)
            Dim ResolveY As Single = If(OverlapY <= OverlapX, OverlapY * Math.Sign(DeltaY), 0)

            If ResolveX <> 0 Then
                Hero.Velocity.X = 0F
            End If

            If ResolveY <> 0 Then
                Hero.Velocity.Y = 0F
            End If

            Hero.Position.X -= ResolveX

            Hero.Position.Y -= ResolveY

            UpdateHeroPosition()

        End If

    End Sub

    Private Sub DrawOurHero()

        With Buffer.Graphics

            Dim RectOffset As Rectangle = Hero.Rect

            RectOffset.Offset(CameraOffset)

            .FillRectangle(Brushes.Red, RectOffset)

            .DrawString("Hero", CWJFont, Brushes.White, RectOffset, AlineCenterMiddle)

            'Draw hero position
            .DrawString("X: " & Hero.Position.X.ToString & vbCrLf & "Y: " & Hero.Position.Y.ToString,
                        CWJFont,
                        Brushes.White,
                        RectOffset.X,
                        RectOffset.Y - 50,
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

                                Dim RectOffset As Rectangle = Enemy.Rect

                                RectOffset.Offset(CameraOffset)

                                If RectOffset.IntersectsWith(ClientRectangle) Then

                                    .FillRectangle(Brushes.Chocolate, RectOffset)

                                    .DrawString("E", EnemyFont, Brushes.PaleGoldenrod, RectOffset, AlineCenterMiddle)

                                End If

                            End If

                        Case AppState.Editing

                            Dim PatrolAOffset As New Rectangle(New Point(Enemy.PatrolA.X, Enemy.PatrolA.Y), New Drawing.Size(GridSize, GridSize))

                            PatrolAOffset.Offset(CameraOffset)

                            .FillRectangle(Brushes.Chocolate, PatrolAOffset)

                            .DrawString("E", EnemyFont, Brushes.PaleGoldenrod, PatrolAOffset, AlineCenterMiddle)

                            Dim PatrolBOffset As New Rectangle(New Point(Enemy.PatrolB.X, Enemy.PatrolB.Y), New Drawing.Size(GridSize, GridSize))

                            PatrolBOffset.Offset(CameraOffset)

                            .FillRectangle(New SolidBrush(Color.FromArgb(128, Color.Chocolate)), PatrolBOffset)

                            .DrawString("E", EnemyFont, New SolidBrush(Color.FromArgb(128, Color.PaleGoldenrod)), PatrolBOffset, AlineCenterMiddle)

                            Dim SpanWidth As Integer = Enemy.PatrolB.X - Enemy.PatrolA.X - GridSize

                            Dim SpanOffset As New Rectangle(New Point(Enemy.PatrolA.X + GridSize, Enemy.PatrolA.Y), New Drawing.Size(SpanWidth, GridSize))

                            SpanOffset.Offset(CameraOffset)

                            .FillRectangle(New SolidBrush(Color.FromArgb(128, Color.Chocolate)), SpanOffset)

                            If SelectedEnemy = Array.IndexOf(Enemies, Enemy) Then

                                Dim SelectionSize As New Size(Enemy.PatrolB.X + GridSize - Enemy.PatrolA.X, GridSize)

                                Dim SelectionOffset As New Rectangle(New Point(Enemy.PatrolA.X, Enemy.PatrolA.Y), SelectionSize)

                                SelectionOffset.Offset(CameraOffset)

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

                Dim RectOffset As Rectangle = Goal.Rect

                RectOffset.Offset(CameraOffset)

                If RectOffset.IntersectsWith(ClientRectangle) Then

                    .FillRectangle(Brushes.White, RectOffset)

                    ' Define the rectangle to be filled
                    Dim Rect As RectangleF = RectOffset

                    Rect.Inflate(Rect.Width / 6.4F, Rect.Height / 6.4F)

                    ' Define the center point of the gradient
                    Dim Center As New PointF(Rect.Left + Rect.Width / 2.0F, Rect.Top + Rect.Height / 2.0F)

                    ' Define the colors for the gradient stops
                    Dim Colors() As Color = {Color.Yellow, Color.White}

                    ' Create the path for the gradient brush
                    Dim GradPath As New GraphicsPath()
                    GradPath.AddEllipse(Rect)

                    ' Create the gradient brush
                    Dim GradBrush As New PathGradientBrush(GradPath) With {
                        .CenterPoint = Center,
                        .CenterColor = Colors(0),
                        .SurroundColors = New Color() {Colors(1)}
                    }

                    .FillRectangle(GradBrush, RectOffset)

                    If Goal.Rect.Width <= Goal.Rect.Height Then

                        Dim Font As New Font(New FontFamily("Wingdings"), Goal.Rect.Width \ 2, FontStyle.Regular)

                        .DrawString("«",
                                Font,
                                Brushes.Green,
                                RectOffset,
                                AlineCenterMiddle)

                    Else

                        Dim Font As New Font(New FontFamily("Wingdings"), Goal.Rect.Height \ 2, FontStyle.Regular)

                        .DrawString("«",
                                Font,
                                Brushes.Green,
                                RectOffset,
                                AlineCenterMiddle)

                    End If

                End If

                If GameState = AppState.Editing Then

                    If GoalSelected = True Then

                        'Draw selection rectangle.
                        .DrawRectangle(New Pen(Color.Red, 6), RectOffset)

                        'Position sizing handle.
                        SizingHandle.X = RectOffset.Right - SizingHandle.Width \ 2
                        SizingHandle.Y = RectOffset.Bottom - SizingHandle.Height \ 2

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

                    Dim RectOffset As Rectangle = Block.Rect

                    RectOffset.Offset(CameraOffset)

                    If RectOffset.IntersectsWith(ClientRectangle) Then

                        .FillRectangle(Brushes.Chocolate, RectOffset)

                    End If

                    If GameState = AppState.Editing Then

                        If SelectedBlock = Array.IndexOf(Blocks, Block) Then

                            'Draw selection rectangle.
                            .DrawRectangle(New Pen(Color.Red, 6), RectOffset)

                            'Position sizing handle.
                            SizingHandle.X = RectOffset.Right - SizingHandle.Width \ 2
                            SizingHandle.Y = RectOffset.Bottom - SizingHandle.Height \ 2

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

                    Dim RectOffset As Rectangle = Bush.Rect

                    RectOffset.Offset(CameraOffset)

                    If RectOffset.IntersectsWith(ClientRectangle) Then

                        .FillRectangle(Brushes.GreenYellow, RectOffset)

                        .DrawLine(SeaGreenPen, RectOffset.Right - 10, RectOffset.Top + 10, RectOffset.Right - 10, RectOffset.Bottom - 10)

                        .DrawLine(SeaGreenPen, RectOffset.Left + 10, RectOffset.Bottom - 10, RectOffset.Right - 10, RectOffset.Bottom - 10)

                        .DrawRectangle(OutinePen, RectOffset)

                    End If

                    If GameState = AppState.Editing Then

                        If SelectedBush = Array.IndexOf(Bushes, Bush) Then

                            'Draw selection rectangle.
                            .DrawRectangle(New Pen(Color.Red, 6), RectOffset)

                            'Position sizing handle.
                            SizingHandle.X = RectOffset.Right - SizingHandle.Width \ 2
                            SizingHandle.Y = RectOffset.Bottom - SizingHandle.Height \ 2

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

                    Dim RectOffset As Rectangle = Cloud.Rect

                    RectOffset.Offset(CameraOffset)

                    If RectOffset.IntersectsWith(ClientRectangle) Then

                        .FillRectangle(Brushes.White, RectOffset)

                        .DrawLine(LightSkyBluePen, RectOffset.Right - 10,
                                  RectOffset.Top + 10,
                                  RectOffset.Right - 10,
                                  RectOffset.Bottom - 10)

                        .DrawLine(LightSkyBluePen, RectOffset.Left + 10, RectOffset.Bottom - 10, RectOffset.Right - 10, RectOffset.Bottom - 10)

                        .DrawRectangle(OutinePen, RectOffset)

                    End If

                    If GameState = AppState.Editing Then

                        If SelectedCloud = Array.IndexOf(Clouds, Cloud) Then

                            'Draw selection rectangle.
                            .DrawRectangle(New Pen(Color.Red, 6), RectOffset)

                            'Position sizing handle.
                            SizingHandle.X = RectOffset.Right - SizingHandle.Width \ 2
                            SizingHandle.Y = RectOffset.Bottom - SizingHandle.Height \ 2

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

                    Dim RectOffset As Rectangle = Bill.Rect

                    RectOffset.Offset(CameraOffset)

                    Select Case GameState

                        Case AppState.Start

                            If Bill.Collected = False Then

                                .FillRectangle(Brushes.Goldenrod, RectOffset)

                                .DrawString("$", FPSFont, Brushes.OrangeRed, RectOffset, AlineCenterMiddle)

                            End If

                        Case AppState.Playing

                            If RectOffset.IntersectsWith(ClientRectangle) Then

                                If Bill.Collected = False Then

                                    .FillRectangle(Brushes.Goldenrod, RectOffset)

                                    .DrawString("$", FPSFont, Brushes.OrangeRed, RectOffset, AlineCenterMiddle)

                                End If

                            End If

                        Case AppState.Editing

                            If RectOffset.IntersectsWith(ClientRectangle) Then

                                .FillRectangle(Brushes.Goldenrod, RectOffset)

                                .DrawString("$", FPSFont, Brushes.OrangeRed, RectOffset, AlineCenterMiddle)


                            End If

                            If SelectedBill = Array.IndexOf(Cash, Bill) Then

                                'Draw selection rectangle.
                                .DrawRectangle(New Pen(Color.Red, 6), RectOffset)

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

            If ShowToolPreview = True AndAlso ShowMenu = False Then

                Dim RectOffset As Rectangle = ToolPreview

                RectOffset.Offset(CameraOffset)

                Select Case SelectedTool

                    Case Tools.Block

                        .FillRectangle(Brushes.Chocolate, RectOffset)

                    Case Tools.Bill

                        .FillRectangle(Brushes.Goldenrod, RectOffset)

                        .DrawString("$", FPSFont, Brushes.OrangeRed, RectOffset, AlineCenterMiddle)

                    Case Tools.Cloud

                        .FillRectangle(Brushes.White, RectOffset)

                        .DrawLine(LightSkyBluePen, RectOffset.Right - 10, RectOffset.Top + 10, RectOffset.Right - 10, RectOffset.Bottom - 10)

                        .DrawLine(LightSkyBluePen, RectOffset.Left + 10, RectOffset.Bottom - 10, RectOffset.Right - 10, RectOffset.Bottom - 10)

                        .DrawRectangle(OutinePen, RectOffset)

                    Case Tools.Bush

                        .FillRectangle(Brushes.GreenYellow, RectOffset)

                        .DrawLine(SeaGreenPen, RectOffset.Right - 10, RectOffset.Top + 10, RectOffset.Right - 10, RectOffset.Bottom - 10)

                        .DrawLine(SeaGreenPen, RectOffset.Left + 10, RectOffset.Bottom - 10, RectOffset.Right - 10, RectOffset.Bottom - 10)

                        .DrawRectangle(OutinePen, RectOffset)

                    Case Tools.Enemy

                        .FillRectangle(Brushes.Chocolate, RectOffset)

                        .DrawString("E", EnemyFont, Brushes.PaleGoldenrod, RectOffset, AlineCenterMiddle)

                        Dim PatrolB As New Rectangle(RectOffset.X + GridSize, RectOffset.Y, GridSize, GridSize)

                        .FillRectangle(New SolidBrush(Color.FromArgb(128, Color.Chocolate)), PatrolB)

                        .DrawString("E",
                                    EnemyFont,
                                    New SolidBrush(Color.FromArgb(128, Color.PaleGoldenrod)),
                                    PatrolB,
                                    AlineCenterMiddle)

                    Case Tools.Goal

                        .FillRectangle(Brushes.White, RectOffset)

                        ' Define the rectangle to be filled
                        Dim Rect As RectangleF = RectOffset

                        Rect.Inflate(Rect.Width / 6.4F, Rect.Height / 6.4F)

                        ' Define the center point of the gradient
                        Dim Center As New PointF(Rect.Left + Rect.Width / 2.0F, Rect.Top + Rect.Height / 2.0F)

                        ' Define the colors for the gradient stops
                        Dim Colors() As Color = {Color.Yellow, Color.White}

                        ' Create the path for the gradient brush
                        Dim GradPath As New GraphicsPath()
                        GradPath.AddEllipse(Rect)

                        ' Create the gradient brush
                        Dim GradBrush As New PathGradientBrush(GradPath) With {
                            .CenterPoint = Center,
                            .CenterColor = Colors(0),
                            .SurroundColors = New Color() {Colors(1)}
                        }

                        .FillRectangle(GradBrush, RectOffset)

                        If RectOffset.Width <= RectOffset.Height Then

                            Dim Font As New Font(New FontFamily("Wingdings"), RectOffset.Width \ 2, FontStyle.Regular)

                            .DrawString("«",
                                    Font,
                                    Brushes.Green,
                                    RectOffset,
                                    AlineCenterMiddle)

                        Else

                            Dim Font As New Font(New FontFamily("Wingdings"), RectOffset.Height \ 2, FontStyle.Regular)

                            .DrawString("«",
                                    Font,
                                    Brushes.Green,
                                    RectOffset,
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

            Shadow.Inflate(2, 2)

            Shadow.Offset(15, 15)

            FillRoundedRectangle(MenuShadowBrush, Shadow, 20, Buffer.Graphics)

            FillRoundedRectangle(Brushes.Black, MenuBackground.Rect, 10, Buffer.Graphics)

        End With

    End Sub

    Private Sub DrawMenuOutline()

        Dim OutLineRect As Rectangle = MenuBackground.Rect

        OutLineRect.Inflate(2, 2)

        DrawRoundedRectangle(MenuOutinePen, OutLineRect, 20, Buffer.Graphics)

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

            If MenuButtonHover = True Then

                .FillRectangle(Brushes.White, MenuButton.Rect)

                .DrawString("≡",
                        MenuButtonFont,
                        Brushes.Black,
                        MenuButton.Rect,
                        AlineCenterMiddle)

            Else

                .FillRectangle(Brushes.Black, MenuButton.Rect)

                .DrawString("≡",
                        MenuButtonFont,
                        Brushes.White,
                        MenuButton.Rect,
                        AlineCenterMiddle)

            End If

        End With

    End Sub

    Private Sub DrawSaveButton()

        With Buffer.Graphics

            '.FillRectangle(Brushes.Black, SaveButton.Rect)

            FillRoundedRectangle(Brushes.Black, SaveButton.Rect, 20, Buffer.Graphics)


            .DrawEllipse(BButtonIconOutinePen,
                         New Rectangle(SaveButton.Rect.X + 197,
                                       SaveButton.Rect.Y + SaveButton.Rect.Height \ 2 - 52 \ 2,
                                       52,
                                       52))

            .DrawString("Save",
                        FPSFont,
                        Brushes.White,
                        New Rectangle(SaveButton.Rect.X + 25,
                                      SaveButton.Rect.Y + SaveButton.Rect.Height \ 2 - 45 \ 2,
                                      140,
                                      50),
                        AlineCenterMiddle)

            .DrawString("B",
                        RightTriggerIconFont,
                        Brushes.White,
                        New Rectangle(SaveButton.Rect.X + 175,
                                      SaveButton.Rect.Y + SaveButton.Rect.Height \ 2 - 45 \ 2,
                                      100,
                                      50),
                        AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawOpenButton()

        With Buffer.Graphics

            '.FillRectangle(Brushes.Black, OpenButton.Rect)

            FillRoundedRectangle(Brushes.Black, OpenButton.Rect, 20, Buffer.Graphics)


            .DrawEllipse(YButtonIconOutinePen,
                         New Rectangle(OpenButton.Rect.X + 197,
                                       OpenButton.Rect.Y + OpenButton.Rect.Height \ 2 - 52 \ 2,
                                       52,
                                       52))

            .DrawString("Open",
                        FPSFont,
                        Brushes.White,
                        New Rectangle(OpenButton.Rect.X + 25,
                                      OpenButton.Rect.Y + OpenButton.Rect.Height \ 2 - 45 \ 2,
                                      145,
                                      50),
                        AlineCenterMiddle)

            .DrawString("Y",
                        ButtonIconFont,
                        Brushes.White,
                        New Rectangle(OpenButton.Rect.X + 174,
                                      OpenButton.Rect.Y + OpenButton.Rect.Height \ 2 - 45 \ 2,
                                      100,
                                      50),
                        AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawNewButton()

        With Buffer.Graphics

            '.FillRectangle(Brushes.Black, NewButton.Rect)

            FillRoundedRectangle(Brushes.Black, NewButton.Rect, 20, Buffer.Graphics)


            .DrawString("New", FPSFont,
                        Brushes.White,
                        New Rectangle(NewButton.Rect.X + 25,
                                      NewButton.Rect.Y + SaveButton.Rect.Height \ 2 - 45 \ 2,
                                      120,
                                      50),
                        AlineCenterMiddle)

            .DrawString("RT",
                        RightTriggerIconFont,
                        Brushes.White,
                        New Rectangle(NewButton.Rect.X + 175,
                                      NewButton.Rect.Y + NewButton.Rect.Height \ 2 - 52 \ 2,
                                      100,
                                      50),
                        AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawExitButton()

        With Buffer.Graphics

            '.FillRectangle(Brushes.Black, ExitButton.Rect)

            FillRoundedRectangle(Brushes.Black, ExitButton.Rect, 20, Buffer.Graphics)


            .DrawEllipse(XButtonIconOutinePen,
                         New Rectangle(ExitButton.Rect.X + ExitButton.Rect.Width \ 2 - 52 \ 2,
                                       ExitButton.Rect.Y + ExitButton.Rect.Height \ 2 - 52 \ 2,
                                       52,
                                       52))

            .DrawString("X",
                        ButtonIconFont,
                        Brushes.White,
                        New Rectangle(ExitButton.Rect.X + ExitButton.Rect.Width \ 2 - 52 \ 2,
                                      ExitButton.Rect.Y + 16,
                                      52,
                                      52),
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

            Dim Shadow As Rectangle = StartScreenOpenButton.Rect

            Shadow.Offset(12, 12)

            FillRoundedRectangle(MenuShadowBrush, Shadow, 10, Buffer.Graphics)

            FillRoundedRectangle(Brushes.Black, StartScreenOpenButton.Rect, 10, Buffer.Graphics)

            DrawRoundedRectangle(MenuOutinePen, StartScreenOpenButton.Rect, 10, Buffer.Graphics)

            .DrawEllipse(YButtonIconOutinePen,
                         New Rectangle(StartScreenOpenButton.Rect.X + 142,
                                       StartScreenOpenButton.Rect.Y + StartScreenOpenButton.Rect.Height \ 2 - 52 \ 2,
                                       52,
                                       52))

            .DrawString("Open",
                        FPSFont,
                        Brushes.White,
                        New Rectangle(StartScreenOpenButton.Rect.X,
                                      StartScreenOpenButton.Rect.Y + StartScreenOpenButton.Rect.Height \ 2 - 45 \ 2,
                                      150,
                                      50),
                                      AlineCenterMiddle)

            .DrawString("Y",
                        ButtonIconFont,
                        Brushes.White,
                        New Rectangle(StartScreenOpenButton.Rect.X + 143,
                                      StartScreenOpenButton.Rect.Y + StartScreenOpenButton.Rect.Height \ 2 - 45 \ 2,
                                      50,
                                      50),
                                      AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawStartScreenNewButton()

        With Buffer.Graphics

            Dim Shadow As Rectangle = StartScreenNewButton.Rect

            Shadow.Offset(12, 12)

            FillRoundedRectangle(MenuShadowBrush, Shadow, 10, Buffer.Graphics)

            FillRoundedRectangle(Brushes.Black, StartScreenNewButton.Rect, 10, Buffer.Graphics)

            DrawRoundedRectangle(MenuOutinePen, StartScreenNewButton.Rect, 10, Buffer.Graphics)

            .DrawEllipse(BButtonIconOutinePen,
                         New Rectangle(StartScreenNewButton.Rect.X + 140,
                                       StartScreenNewButton.Rect.Y + StartScreenNewButton.Rect.Height \ 2 - 52 \ 2,
                                       52,
                                       52))

            .DrawString("New",
                        FPSFont,
                        Brushes.White,
                        New Rectangle(StartScreenNewButton.Rect.X + 5,
                                      StartScreenNewButton.Rect.Y + StartScreenNewButton.Rect.Height \ 2 - 45 \ 2,
                                      120,
                                      50),
                        AlineCenterMiddle)

            .DrawString("B",
                        ButtonIconFont,
                        Brushes.White,
                        New Rectangle(StartScreenNewButton.Rect.X + 143,
                                      StartScreenNewButton.Rect.Y + StartScreenNewButton.Rect.Height \ 2 - 45 \ 2,
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

            .Clear(Color)

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
        For x As Integer = CameraOffset.X To CameraOffset.X + Level.Rect.Width Step GridSize

            GridLineBuffer.DrawLine(Pens.Black, x, CameraOffset.Y, x, CameraOffset.Y + Level.Rect.Height)

        Next

        ' Draw horizontal lines ---
        For y As Integer = CameraOffset.Y To CameraOffset.Y + Level.Rect.Height Step GridSize

            GridLineBuffer.DrawLine(Pens.Black, CameraOffset.X, y, CameraOffset.X + Level.Rect.Width, y)

        Next

    End Sub

    Private Sub DrawRoundedRectangle(pen As Pen, Rect As Rectangle, radius As Integer, g As Graphics)

        Dim path As New Drawing2D.GraphicsPath()

        'Add top line inside the top left and top right corners.
        path.AddLine(Rect.Left + radius, Rect.Top, Rect.Right - radius, Rect.Top)

        'Add top right corner.
        path.AddArc(Rect.Right - radius, Rect.Top, radius, radius, 270, 90)

        'Add right line inside the top right and bottom right corners.
        path.AddLine(Rect.Right, Rect.Top + radius, Rect.Right, Rect.Bottom - radius)

        'Add bottom right corner.
        path.AddArc(Rect.Right - radius, Rect.Bottom - radius, radius, radius, 0, 90)

        'Add bottom line inside the bottom left and the bottom right corners.
        path.AddLine(Rect.Right - radius, Rect.Bottom, Rect.Left + radius, Rect.Bottom)

        'Add bottom left corner.
        path.AddArc(Rect.Left, Rect.Bottom - radius, radius, radius, 90, 90)

        'Add left line inside the top left and bottom left corners.
        path.AddLine(Rect.Left, Rect.Bottom - radius, Rect.Left, Rect.Top + radius)

        'Add top left corner.
        path.AddArc(Rect.Left, Rect.Top, radius, radius, 180, 90)

        path.CloseFigure()

        g.DrawPath(pen, path)

    End Sub

    Private Sub FillRoundedRectangle(brush As Brush, Rect As Rectangle, radius As Integer, e As Graphics)

        Dim Path As New Drawing2D.GraphicsPath()

        'Add top line inside the top left and top right corners.
        Path.AddLine(Rect.Left + radius, Rect.Top, Rect.Right - radius, Rect.Top)

        'Add top right corner.
        Path.AddArc(Rect.Right - radius, Rect.Top, radius, radius, 270, 90)

        'Add right line inside the top right and bottom right corners.
        Path.AddLine(Rect.Right, Rect.Top + radius, Rect.Right, Rect.Bottom - radius)

        'Add bottom right corner.
        Path.AddArc(Rect.Right - radius, Rect.Bottom - radius, radius, radius, 0, 90)

        'Add bottom line inside the bottom left and the bottom right corners.
        Path.AddLine(Rect.Right - radius, Rect.Bottom, Rect.Left + radius, Rect.Bottom)

        'Add bottom left corner.
        Path.AddArc(Rect.Left, Rect.Bottom - radius, radius, radius, 90, 90)

        'Add left line inside the top left and bottom left corners.
        Path.AddLine(Rect.Left, Rect.Bottom - radius, Rect.Left, Rect.Top + radius)

        'Add top left corner.
        Path.AddArc(Rect.Left, Rect.Top, radius, radius, 180, 90)

        Path.CloseFigure()

        e.FillPath(brush, Path)

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

        CreateSoundFileFromResource()

        AddSound("Music", Application.StartupPath & "level.mp3")

        SetVolume("Music", 50)

        AddOverlapping("CashCollected", Application.StartupPath & "CashCollected.mp3")

        SetVolumeOverlapping("CashCollected", 700)

        AddOverlapping("eliminated", Application.StartupPath & "eliminated.mp3")

        SetVolumeOverlapping("eliminated", 700)

        AddSound("clear", Application.StartupPath & "clear.mp3")

        SetVolume("clear", 1000)

        InitializeToolBarButtons()

        InitializeForm()

        InitializeBuffer()

        Title.Text = "Platformer" & vbCrLf & "with Level Editor"

        OutinePen.LineJoin = Drawing2D.LineJoin.Round

        InitializeObjects()

        CreateStartScreenLevel()

        CashCollected = 0

        GameTimer.Start()

        If IsPlaying("Music") = False Then

            LoopSound("Music")

        End If

        MovePointerToStartScreenNewButton()

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

        Camera.Position.X = 0
        Camera.Position.Y = 0

        UpdateCameraOffset()

        SetMinLevelSize()

        Hero.Rect = New Rectangle(128, 769, 64, 64)

        Hero.Position = New PointF(Hero.Rect.X, Hero.Rect.Y)

        Hero.Velocity = New PointF(0, 0)

        Hero.MaxVelocity = New PointF(400, 1000)

        Hero.Acceleration = New PointF(300, 25)

        MousePointer.Velocity = New PointF(0, 0)

        MousePointer.MaxVelocity = New PointF(1500, 1500)

        MousePointer.Acceleration = New PointF(400, 300)

        Camera.Velocity = New PointF(0, 0)

        Camera.MaxVelocity = New PointF(2500, 2500)

        Camera.Acceleration = New PointF(400, 300)

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

        Goal.Rect = New Rectangle(2176, 768, 64, 64)

        AddBlock(New Rectangle(0, 832, 2496, 128))

        AddBlock(New Rectangle(1088, 576, 64, 64))

        AddBlock(New Rectangle(1344, 576, 320, 64))

        AddBlock(New Rectangle(1472, 320, 64, 64))

        AddBlock(New Rectangle(-128, 384, 128, 576))

        AddCloud(New Rectangle(512, 64, 192, 128))

        AddCloud(New Rectangle(1728, 64, 128, 64))

        AddBush(New Rectangle(768, 768, 320, 64))

        AddBush(New Rectangle(1600, 768, 64, 64))

        AddBill(New Point(1088, 320))

        AddBill(New Point(1472, 64))

        AddEnemy(New Point(512, 769), New Point(512, 769), New Point(1280, 769))

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

    Private Sub InitAndCreateNewLevel()

        ClearObjects()

        SetMinLevelSize()

        Hero.Rect = New Rectangle(128, 769, 64, 64)

        Hero.Position = New PointF(Hero.Rect.X, Hero.Rect.Y)

        Hero.Velocity = New PointF(0, 0)

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
        OpenFileDialog1.InitialDirectory = Application.StartupPath

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
        SaveFileDialog1.InitialDirectory = Application.StartupPath

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

        Dim PointOffset As Point = e

        PointOffset.X = Camera.Position.X + e.X

        PointOffset.Y = Camera.Position.Y + e.Y

        'Is the player clicking the play button?
        If EditPlayButton.Rect.Contains(e) Then
            'Yes, the player is clicking the play button.

            DeselectObjects()

            'Restore the cameras in game position.
            Camera.Position.X = CameraPlayPostion.X
            Camera.Position.Y = CameraPlayPostion.Y

            UpdateCameraOffset()

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
            ToolPreview.X = CInt(Math.Round(PointOffset.X / GridSize) * GridSize)
            ToolPreview.Y = CInt(Math.Round(PointOffset.Y / GridSize) * GridSize)

            ToolPreview.Width = GridSize
            ToolPreview.Height = GridSize

            SelectedTool = Tools.Block

            ShowToolPreview = True

        End If

        If BillToolButton.Rect.Contains(e) Then

            DeselectObjects()

            'Snap preview to grid.
            ToolPreview.X = CInt(Math.Round(PointOffset.X / GridSize) * GridSize)
            ToolPreview.Y = CInt(Math.Round(PointOffset.Y / GridSize) * GridSize)

            ToolPreview.Width = GridSize
            ToolPreview.Height = GridSize

            SelectedTool = Tools.Bill

            ShowToolPreview = True

        End If

        If CloudToolButton.Rect.Contains(e) Then

            DeselectObjects()

            'Snap preview to grid.
            ToolPreview.X = CInt(Math.Round(PointOffset.X / GridSize) * GridSize)
            ToolPreview.Y = CInt(Math.Round(PointOffset.Y / GridSize) * GridSize)

            ToolPreview.Width = GridSize
            ToolPreview.Height = GridSize

            SelectedTool = Tools.Cloud

            ShowToolPreview = True

        End If

        If BushToolButton.Rect.Contains(e) Then

            DeselectObjects()

            'Snap preview to grid.
            ToolPreview.X = CInt(Math.Round(PointOffset.X / GridSize) * GridSize)
            ToolPreview.Y = CInt(Math.Round(PointOffset.Y / GridSize) * GridSize)

            ToolPreview.Width = GridSize
            ToolPreview.Height = GridSize

            SelectedTool = Tools.Bush

            ShowToolPreview = True

        End If

        If EnemyToolButton.Rect.Contains(e) Then

            DeselectObjects()

            'Snap preview to grid.
            ToolPreview.X = CInt(Math.Round(PointOffset.X / GridSize) * GridSize)
            ToolPreview.Y = CInt(Math.Round(PointOffset.Y / GridSize) * GridSize)

            ToolPreview.Width = GridSize
            ToolPreview.Height = GridSize

            SelectedTool = Tools.Enemy

            ShowToolPreview = True

        End If

        If GoalToolButton.Rect.Contains(e) Then

            DeselectObjects()

            'Snap preview to grid.
            ToolPreview.X = CInt(Math.Round(PointOffset.X / GridSize) * GridSize)
            ToolPreview.Y = CInt(Math.Round(PointOffset.Y / GridSize) * GridSize)

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

            PointOffset.X = Camera.Rect.X + e.X
            PointOffset.Y = Camera.Rect.Y + e.Y

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

        Dim PointOffset As Point = e.Location

        PointOffset.X = Camera.Position.X + e.X

        PointOffset.Y = Camera.Position.Y + e.Y

        If e.Button = MouseButtons.None Then

            If SelectedTool <> Tools.Pointer Then

                If ToolBarBackground.Rect.Contains(e.Location) = False Then

                    ToolPreview.X = CInt(Math.Round(PointOffset.X / GridSize) * GridSize)
                    ToolPreview.Y = CInt(Math.Round(PointOffset.Y / GridSize) * GridSize)

                    ShowToolPreview = True

                Else

                    ShowToolPreview = False

                End If

            End If

            If MenuButton.Rect.Contains(e.Location) Then

                If ShowMenu = False Then

                    MenuButtonHover = True

                End If

            Else

                MenuButtonHover = False

            End If

        End If

        If SelectedCloud > -1 Then

            If e.Button = MouseButtons.Left Then

                If SizingHandleSelected = True Then

                    'Snap cloud width to grid.
                    Clouds(SelectedCloud).Rect.Width = CInt(Math.Round((PointOffset.X - Clouds(SelectedCloud).Rect.X) / GridSize)) * GridSize

                    'Limit smallest cloud width to one grid width.
                    If Clouds(SelectedCloud).Rect.Width < GridSize Then Clouds(SelectedCloud).Rect.Width = GridSize

                    'Snap cloud height to grid.
                    Clouds(SelectedCloud).Rect.Height = CInt(Math.Round((PointOffset.Y - Clouds(SelectedCloud).Rect.Y) / GridSize)) * GridSize

                    'Limit smallest cloud height to one grid height.
                    If Clouds(SelectedCloud).Rect.Height < GridSize Then Clouds(SelectedCloud).Rect.Height = GridSize

                    AutoSizeLevel(Clouds(SelectedCloud).Rect)

                Else

                    'Snap cloud to grid
                    Clouds(SelectedCloud).Rect.X = CInt(Math.Round((PointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                    Clouds(SelectedCloud).Rect.Y = CInt(Math.Round((PointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

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
                    Blocks(SelectedBlock).Rect.Width = CInt(Math.Round((PointOffset.X - Blocks(SelectedBlock).Rect.X) / GridSize)) * GridSize

                    'Limit smallest block width to one grid width.
                    If Blocks(SelectedBlock).Rect.Width < GridSize Then Blocks(SelectedBlock).Rect.Width = GridSize

                    'Snap block height to grid.
                    Blocks(SelectedBlock).Rect.Height = CInt(Math.Round((PointOffset.Y - Blocks(SelectedBlock).Rect.Y) / GridSize)) * GridSize

                    'Limit smallest block height to one grid height.
                    If Blocks(SelectedBlock).Rect.Height < GridSize Then Blocks(SelectedBlock).Rect.Height = GridSize

                    AutoSizeLevel(Blocks(SelectedBlock).Rect)

                Else

                    'Snap block to grid
                    Blocks(SelectedBlock).Rect.X = CInt(Math.Round((PointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                    Blocks(SelectedBlock).Rect.Y = CInt(Math.Round((PointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

                    AutoSizeLevel(Blocks(SelectedBlock).Rect)

                End If

            End If

        End If

        If SelectedBill > -1 Then

            If e.Button = MouseButtons.Left Then

                'Move bill, snap to grid.
                Cash(SelectedBill).Rect.X = CInt(Math.Round((PointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                Cash(SelectedBill).Rect.Y = CInt(Math.Round((PointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

                Cash(SelectedBill).Position.X = Cash(SelectedBill).Rect.X
                Cash(SelectedBill).Position.Y = Cash(SelectedBill).Rect.Y

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
                    Bushes(SelectedBush).Rect.Width = CInt(Math.Round((PointOffset.X - Bushes(SelectedBush).Rect.X) / GridSize)) * GridSize

                    'Limit smallest bush width to one grid width.
                    If Bushes(SelectedBush).Rect.Width < GridSize Then Bushes(SelectedBush).Rect.Width = GridSize

                    'Snap bush height to grid.
                    Bushes(SelectedBush).Rect.Height = CInt(Math.Round((PointOffset.Y - Bushes(SelectedBush).Rect.Y) / GridSize)) * GridSize

                    'Limit smallest bush height to one grid height.
                    If Bushes(SelectedBush).Rect.Height < GridSize Then Bushes(SelectedBush).Rect.Height = GridSize

                    AutoSizeLevel(Bushes(SelectedBush).Rect)

                Else
                    'No, the player is not resizing the bush.
                    'The player is moving the bush.

                    'Move bush, snap to grid
                    Bushes(SelectedBush).Rect.X = CInt(Math.Round((PointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                    Bushes(SelectedBush).Rect.Y = CInt(Math.Round((PointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

                    AutoSizeLevel(Bushes(SelectedBush).Rect)

                End If

            End If

        End If

        If GoalSelected = True Then

            If e.Button = MouseButtons.Left Then

                If SizingHandleSelected = True Then

                    'Snap bush width to grid.
                    Goal.Rect.Width = CInt(Math.Round((PointOffset.X - Goal.Rect.X) / GridSize)) * GridSize

                    'Limit smallest bush width to one grid width.
                    If Goal.Rect.Width < GridSize Then Goal.Rect.Width = GridSize

                    'Snap bush height to grid.
                    Goal.Rect.Height = CInt(Math.Round((PointOffset.Y - Goal.Rect.Y) / GridSize)) * GridSize

                    'Limit smallest bush height to one grid height.
                    If Goal.Rect.Height < GridSize Then Goal.Rect.Height = GridSize

                    AutoSizeLevel(Goal.Rect)

                Else

                    'Move Goal, snap to grid
                    Goal.Rect.X = CInt(Math.Round((PointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                    Goal.Rect.Y = CInt(Math.Round((PointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

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
                    PatrolRect.Width = CInt(Math.Round((PointOffset.X - PatrolRect.X) / GridSize)) * GridSize

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
                    Enemies(SelectedEnemy).Rect.X = CInt(Math.Round((PointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                    Enemies(SelectedEnemy).Rect.Y = CInt(Math.Round((PointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

                    Enemies(SelectedEnemy).Position.X = CInt(Math.Round((PointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                    Enemies(SelectedEnemy).Position.Y = CInt(Math.Round((PointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

                    Enemies(SelectedEnemy).PatrolA.X = CInt(Math.Round((PointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                    Enemies(SelectedEnemy).PatrolA.Y = CInt(Math.Round((PointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

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

                Camera.Position.X = SelectionOffset.X - e.X

                Camera.Position.Y = SelectionOffset.Y - e.Y

                UpdateCameraOffset()

                BufferGridLines()

            End If

        End If

    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown

        Select Case e.KeyCode

            Case Keys.E

                If GameState = AppState.Playing Then

                    'Remember the cameras in game position before opening the editor.
                    CameraPlayPostion.X = Camera.Position.X
                    CameraPlayPostion.Y = Camera.Position.Y

                    GameState = AppState.Editing

                    EditorLastFrame = Now

                    BufferGridLines()

                End If

            Case Keys.P

                If GameState = AppState.Editing Then

                    DeselectObjects()

                    'Restore the cameras in game position.
                    Camera.Position.X = CameraPlayPostion.X
                    Camera.Position.Y = CameraPlayPostion.Y

                    UpdateCameraOffset()

                    MovePointerOffScreen()

                    LastFrame = Now

                    GameState = AppState.Playing

                End If

            'Has the player pressed the right arrow key down?
            Case Keys.Right
                'Yes, the player has pressed the right arrow key down.

                RightArrowDown = True

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        MoveCameraRight()

                        UpdateCameraOffset()

                        BufferGridLines()

                    Else

                        MovePointerRight()

                    End If

                End If

                If GameState = AppState.Start Then

                    MovePointerRight()

                End If

            'Has the player pressed the left arrow key down?
            Case Keys.Left
                'Yes, the player has pressed the left arrow key down.

                LeftArrowDown = True

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        MoveCameraLeft()

                        UpdateCameraOffset()

                        BufferGridLines()

                    Else

                        MovePointerLeft()

                    End If

                End If

                If GameState = AppState.Start Then

                    MovePointerLeft()

                End If


            Case Keys.Up

                UpArrowDown = True

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        MoveCameraUp()

                        UpdateCameraOffset()

                        BufferGridLines()

                    Else

                        MovePointerUp()

                    End If

                End If

                If GameState = AppState.Start Then

                    MovePointerUp()

                End If

            Case Keys.Down

                DownArrowDown = True

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        MoveCameraDown()

                        UpdateCameraOffset()

                        BufferGridLines()

                    Else

                        MovePointerDown()

                    End If

                End If

                If GameState = AppState.Start Then

                    MovePointerDown()

                End If

            Case Keys.A

                ADown = True

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        If IsMouseDown = False Then

                            DoMouseLeftDown()

                            IsMouseDown = True

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

                    End If

                End If

                If GameState = AppState.Start Then

                    ClearObjects()

                    InitializeObjects()

                    CreateNewLevel()

                    LevelName = "Untitled"

                    Text = LevelName & " - Platformer with Level Editor - Code with Joe"

                    CashCollected = 0

                    LastFrame = Now

                    GameState = AppState.Playing

                    MovePointerOffScreen()

                    PlayLevelMusic()

                End If

            Case Keys.Enter

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        If IsMouseDown = False Then

                            DoMouseLeftDown()

                            IsMouseDown = True

                        End If

                    End If

                End If

            Case Keys.Y

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        'Does the player want to save this level before opening a level?
                        If MsgBox("Changes to " & LevelName & " may be lost." & vbCrLf & "Open a level anyway?",
                                  MsgBoxStyle.Question Or MsgBoxStyle.OkCancel,
                                  "Open Level - Platformer with Level Editor") = MsgBoxResult.Ok Then
                            'No, the player doesn't want to save this level before opening a level?

                            ShowOpenLevelDialog()

                        End If

                    End If

                End If

                If GameState = AppState.Start Then

                    OpenFileDialog1.FileName = ""
                    OpenFileDialog1.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
                    OpenFileDialog1.FilterIndex = 1
                    OpenFileDialog1.InitialDirectory = Application.StartupPath

                    If OpenFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then

                        If My.Computer.FileSystem.FileExists(OpenFileDialog1.FileName) = True Then

                            InitializeObjects()

                            OpenTestLevelFile(OpenFileDialog1.FileName)

                            If IsFileLoaded = True Then

                                LevelName = Path.GetFileName(OpenFileDialog1.FileName)

                                Text = LevelName & " - Platformer with Level Editor - Code with Joe"

                                CashCollected = 0

                                LastFrame = Now

                                GameState = AppState.Playing

                                MovePointerOffScreen()

                                PlayLevelMusic()

                            End If

                        End If

                    End If

                End If

            Case Keys.N

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        'Does the player want to save this level before creating a new level?
                        If MsgBox("Changes to " & LevelName & " may be lost." & vbCrLf & "Create a new level anyway?",
                                  MsgBoxStyle.Question Or MsgBoxStyle.OkCancel,
                                  "New Level - Platformer with Level Editor") = MsgBoxResult.Ok Then
                            'No, the player doesn't want to save this level before creating a new level?

                            InitAndCreateNewLevel()

                        End If

                    End If

                End If

                If GameState = AppState.Start Then

                    ClearObjects()

                    InitializeObjects()

                    CreateNewLevel()

                    LevelName = "Untitled"

                    Text = LevelName & " - Platformer with Level Editor - Code with Joe"

                    CashCollected = 0

                    LastFrame = Now

                    GameState = AppState.Playing

                    MovePointerOffScreen()

                    PlayLevelMusic()

                End If


            Case Keys.R

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        'Does the player want to save this level before creating a new level?
                        If MsgBox("Changes to " & LevelName & " may be lost." & vbCrLf & "Create a new level anyway?",
                                  MsgBoxStyle.Question Or MsgBoxStyle.OkCancel,
                                  "New Level - Platformer with Level Editor") = MsgBoxResult.Ok Then
                            'No, the player doesn't want to save this level before creating a new level?

                            InitAndCreateNewLevel()

                        End If

                    End If

                End If

            Case Keys.O

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        'Does the player want to save this level before opening a level?
                        If MsgBox("Changes to " & LevelName & " may be lost." & vbCrLf & "Open a level anyway?",
                                  MsgBoxStyle.Question Or MsgBoxStyle.OkCancel,
                                  "Open Level - Platformer with Level Editor") = MsgBoxResult.Ok Then
                            'No, the player doesn't want to save this level before opening a level?

                            ShowOpenLevelDialog()

                        End If

                    End If

                End If

                If GameState = AppState.Start Then

                    OpenFileDialog1.FileName = ""
                    OpenFileDialog1.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
                    OpenFileDialog1.FilterIndex = 1
                    OpenFileDialog1.InitialDirectory = Application.StartupPath

                    If OpenFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then

                        If My.Computer.FileSystem.FileExists(OpenFileDialog1.FileName) = True Then

                            InitializeObjects()

                            OpenTestLevelFile(OpenFileDialog1.FileName)

                            If IsFileLoaded = True Then

                                LevelName = Path.GetFileName(OpenFileDialog1.FileName)

                                Text = LevelName & " - Platformer with Level Editor - Code with Joe"

                                CashCollected = 0

                                LastFrame = Now

                                GameState = AppState.Playing

                                MovePointerOffScreen()

                                PlayLevelMusic()

                            End If

                        End If

                    End If

                End If


            Case Keys.S

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        ShowSaveLevelDialog()

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

                If IsPlaying("Music") = True Then

                    PauseSound("Music")

                    IsMuted = True

                Else

                    LoopSound("Music")

                    IsMuted = False

                End If

            Case Keys.X

                If GameState = AppState.Editing Then

                    If ShowMenu = True Then

                        ShowMenu = False

                    End If

                End If

            Case 93 'Context Menu ≡

                If GameState = AppState.Editing Then

                    If IsContextDown = False Then

                        IsContextDown = True

                        If ShowMenu = False Then

                            ShowMenu = True

                            MovePointerCenterMenu()

                        Else

                            ShowMenu = False

                        End If

                    End If

                End If

        End Select

    End Sub

    Private Sub MoveCameraDown()
        'Is the camera moving up?
        If Camera.Velocity.Y < 0 Then
            'Yes, the camera is moving up.

            'Stop move before changing direction.
            Camera.Velocity.Y = 0 'Zero speed.

        End If

        'Move camera down.
        Camera.Velocity.Y += Camera.Acceleration.Y * EditorDeltaTime.TotalSeconds

        'Limit camera velocity to the max.
        If Camera.Velocity.Y > Camera.MaxVelocity.Y Then Camera.Velocity.Y = Camera.MaxVelocity.Y
    End Sub

    Private Sub MoveCameraUp()
        'Is the camera moving down?
        If Camera.Velocity.Y > 0 Then
            'Yes, the camera is moving down.

            'Stop move before changing direction.
            Camera.Velocity.Y = 0 'Zero speed.

        End If

        'Move camera up.
        Camera.Velocity.Y += -Camera.Acceleration.Y * EditorDeltaTime.TotalSeconds

        'Limit camera velocity to the max.
        If Camera.Velocity.Y < -Camera.MaxVelocity.Y Then Camera.Velocity.Y = -Camera.MaxVelocity.Y
    End Sub

    Private Sub MoveCameraLeft()
        'Is the camera moving right?
        If Camera.Velocity.X > 0 Then
            'Yes, the camera is moving right.

            'Stop move before changing direction.
            Camera.Velocity.X = 0 'Zero speed.

        End If
        'Move camera left.
        Camera.Velocity.X += -Camera.Acceleration.X * EditorDeltaTime.TotalSeconds

        'Limit camera velocity to the max.
        If Camera.Velocity.X < -Camera.MaxVelocity.X Then Camera.Velocity.X = -Camera.MaxVelocity.X
    End Sub

    Private Sub MoveCameraRight()
        'Is the camera moving left?
        If Camera.Velocity.X < 0 Then
            'Yes, the camera is moving left.

            'Stop move before changing direction.
            Camera.Velocity.X = 0 'Zero speed.

        End If

        'Move camera right.
        Camera.Velocity.X += Camera.Acceleration.X * EditorDeltaTime.TotalSeconds

        'Limit camera velocity to the max.
        If Camera.Velocity.X > Camera.MaxVelocity.X Then Camera.Velocity.X = Camera.MaxVelocity.X
    End Sub

    Private Sub Form1_KeyUp(sender As Object, e As KeyEventArgs) Handles MyBase.KeyUp

        Select Case e.KeyCode

            Case Keys.Right

                RightArrowDown = False

            Case Keys.Left

                LeftArrowDown = False

            Case Keys.Up

                UpArrowDown = False

            Case Keys.Down

                DownArrowDown = False

            Case Keys.A

                If Jumped = True Then Jumped = False

                ADown = False

            Case Keys.B

                If Jumped = True Then Jumped = False

                BDown = False

                'Has the player let the delete key up?
            Case Keys.Delete
                'Yes, the player has let the delete key up.

                DeleteDown = False

            Case 93 'Context Menu ≡

                IsContextDown = False

        End Select

    End Sub

    Private Sub UpdateButtonPosition()
        'The range of buttons is 0 to 65,535. Unsigned 16-bit (2-byte) integer.

        If (ControllerPosition.Gamepad.wButtons And DPadUp) <> 0 Then
            DPadUpPressed = True
        Else
            DPadUpPressed = False
        End If

        If (ControllerPosition.Gamepad.wButtons And DPadDown) <> 0 Then
            DPadDownPressed = True
        Else
            DPadDownPressed = False
        End If

        If (ControllerPosition.Gamepad.wButtons And DPadLeft) <> 0 Then
            DPadLeftPressed = True
        Else
            DPadLeftPressed = False
        End If

        If (ControllerPosition.Gamepad.wButtons And DPadRight) <> 0 Then
            DPadRightPressed = True
        Else
            DPadRightPressed = False
        End If

        If (ControllerPosition.Gamepad.wButtons And StartButton) <> 0 Then
            StartButtonPressed = True
        Else
            StartButtonPressed = False
        End If

        If (ControllerPosition.Gamepad.wButtons And BackButton) <> 0 Then
            BackButtonPressed = True
        Else
            BackButtonPressed = False
        End If

        If (ControllerPosition.Gamepad.wButtons And LeftStickButton) <> 0 Then
            LeftStickButtonPressed = True
        Else
            LeftStickButtonPressed = False
        End If

        If (ControllerPosition.Gamepad.wButtons And RightStickButton) <> 0 Then
            RightStickButtonPressed = True
        Else
            RightStickButtonPressed = False
        End If

        If (ControllerPosition.Gamepad.wButtons And LeftBumperButton) <> 0 Then
            LeftBumperButtonPressed = True
        Else
            LeftBumperButtonPressed = False
        End If

        If (ControllerPosition.Gamepad.wButtons And RightBumperButton) <> 0 Then
            RightBumperButtonPressed = True
        Else
            RightBumperButtonPressed = False
        End If

        If (ControllerPosition.Gamepad.wButtons And AButton) <> 0 Then
            AButtonPressed = True
        Else
            AButtonPressed = False
        End If

        If (ControllerPosition.Gamepad.wButtons And BButton) <> 0 Then
            BButtonPressed = True
        Else
            BButtonPressed = False
        End If

        If (ControllerPosition.Gamepad.wButtons And XButton) <> 0 Then
            XButtonPressed = True
        Else
            XButtonPressed = False
        End If

        If (ControllerPosition.Gamepad.wButtons And YButton) <> 0 Then
            YButtonPressed = True
        Else
            YButtonPressed = False
        End If

    End Sub

    Private Sub DoButtonLogic()

        DoLetterButtonLogic()

        DoDPadLogic()

        DoStartBackLogic()

        DoBumperLogic()

    End Sub

    Private Sub DoBumperLogic()

        If LeftBumperButtonPressed = True Then

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

        End If

        If RightBumperButtonPressed = True Then

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

        End If

    End Sub

    Private Sub DoStartBackLogic()

        If StartButtonPressed = True Then

            Select Case GameState

                Case AppState.Playing
                    'Start is the controller shortcut to edit the level.

                    If IsStartDown = False Then

                        IsStartDown = True

                        'Remember the cameras in game position before opening the editor.
                        CameraPlayPostion.X = Camera.Position.X
                        CameraPlayPostion.Y = Camera.Position.Y

                        'Move mouse pointer to the center of the client rectangle.
                        Cursor.Position = New Point(ClientRectangle.X + ClientRectangle.Width / 2,
                                                       ClientRectangle.Y + ClientRectangle.Height / 2)

                        GameState = AppState.Editing

                        BufferGridLines()

                    End If

                Case AppState.Editing
                    'Start is the controller shortcut to play the level.

                    If IsStartDown = False Then

                        IsStartDown = True

                        DeselectObjects()

                        'Restore the cameras in game position.
                        Camera.Position.X = CameraPlayPostion.X
                        Camera.Position.Y = CameraPlayPostion.Y

                        UpdateCameraOffset()

                        MovePointerOffScreen()

                        LastFrame = Now

                        GameState = AppState.Playing

                    End If

            End Select

        Else

            IsStartDown = False

        End If

        If BackButtonPressed = True Then

            If GameState = AppState.Editing Then

                If ShowMenu = False Then
                    'Back is the controller shortcut to show the menu.

                    If IsBackDown = False Then

                        ShowMenu = True

                        MovePointerCenterMenu()

                        IsBackDown = True

                    End If

                Else
                    'Back is the controller shortcut to hide the menu.

                    If IsBackDown = False Then

                        ShowMenu = False

                        IsBackDown = True

                    End If

                End If

            End If

        Else

            IsBackDown = False

        End If

    End Sub

    Private Sub DoDPadLogic()

        If DPadLeftPressed = True Then

            ControllerLeft = True

            If GameState = AppState.Start Or GameState = AppState.Editing Then

                MovePointerLeftDPad()

            End If

        Else

            ControllerLeft = False

        End If

        If DPadRightPressed = True Then

            ControllerRight = True

            If GameState = AppState.Start Or GameState = AppState.Editing Then

                MovePointerRightDPad()

            End If

        Else

            ControllerRight = False

        End If

        If DPadUpPressed = True Then

            If GameState = AppState.Start Or GameState = AppState.Editing Then

                MovePointerUpDPad()

            End If

        End If

        If DPadDownPressed = True Then

            If GameState = AppState.Start Or GameState = AppState.Editing Then

                MovePointerDownDPad()

            End If

        End If

    End Sub

    Private Sub MovePointerLeftDPad()

        'Is the pointer moving right?
        If MousePointer.Velocity.X > 0 Then
            'Yes, the pointer is moving right.

            'Stop move before changing direction.
            MousePointer.Velocity.X = 0 'Zero speed.

        End If

        'Move pointer left.
        MousePointer.Velocity.X += (-MousePointer.Acceleration.X \ 3) * EditorDeltaTime.TotalSeconds

        'Limit pointer velocity to the max.
        If MousePointer.Velocity.X < -MousePointer.MaxVelocity.X Then MousePointer.Velocity.X = -MousePointer.MaxVelocity.X

    End Sub

    Private Sub MovePointerRightDPad()

        'Is the pointer moving left?
        If MousePointer.Velocity.X < 0 Then
            'Yes, the pointer is moving left.

            'Stop move before changing direction.
            MousePointer.Velocity.X = 0 'Zero speed.

        End If

        'Move pointer right.
        MousePointer.Velocity.X += (MousePointer.Acceleration.X \ 3) * EditorDeltaTime.TotalSeconds

        'Limit pointer velocity to the max.
        If MousePointer.Velocity.X > MousePointer.MaxVelocity.X Then MousePointer.Velocity.X = MousePointer.MaxVelocity.X

    End Sub

    Private Sub MovePointerUpDPad()

        'Is the pointer moving down?
        If MousePointer.Velocity.Y > 0 Then
            'Yes, the pointer is moving down.

            'Stop move before changing direction.
            MousePointer.Velocity.Y = 0 'Zero speed.

        End If

        'Move pointer up.
        MousePointer.Velocity.Y += (-MousePointer.Acceleration.Y \ 3) * EditorDeltaTime.TotalSeconds

        'Limit pointer velocity to the max.
        If MousePointer.Velocity.Y < -MousePointer.MaxVelocity.Y Then MousePointer.Velocity.Y = -MousePointer.MaxVelocity.Y

    End Sub

    Private Sub MovePointerDownDPad()

        'Is the pointer moving up?
        If MousePointer.Velocity.Y < 0 Then
            'Yes, the pointer is moving up.

            'Stop move before changing direction.
            MousePointer.Velocity.Y = 0 'Zero speed.

        End If

        'Move pointer down.
        MousePointer.Velocity.Y += (MousePointer.Acceleration.Y \ 3) * EditorDeltaTime.TotalSeconds

        'Limit pointer velocity to the max.
        If MousePointer.Velocity.Y > MousePointer.MaxVelocity.Y Then MousePointer.Velocity.Y = MousePointer.MaxVelocity.Y

    End Sub

    Private Sub DoLetterButtonLogic()

        If AButtonPressed = True Then

            ControllerA = True

            If GameState = AppState.Start Or GameState = AppState.Editing Then
                'A on the controller simulates the left mouse button down.

                If IsMouseDown = False Then

                    DoMouseLeftDown()

                    IsMouseDown = True

                End If

            End If

        Else

            ControllerA = False

            If ControllerJumped = True Then ControllerJumped = False

            If GameState = AppState.Start Or GameState = AppState.Editing Then
                'A on the controller simulates the left mouse button up.

                If IsMouseDown = True Then

                    DoMouseLeftUp()

                    IsMouseDown = False

                End If

            End If

        End If

        If BButtonPressed = True Then

            ControllerB = True

            If GameState = AppState.Editing Then

                If ShowMenu = True Then
                    'B is the controller shortcut to save the level.

                    'Move mouse pointer over the save level button.
                    Cursor.Position = New Point(ScreenOffset.X + SaveButton.Rect.X + SaveButton.Rect.Width \ 2,
                                                ScreenOffset.Y + SaveButton.Rect.Y + SaveButton.Rect.Height \ 2)

                    If IsMouseDown = False Then

                        DoMouseLeftDown()

                        IsMouseDown = True

                    End If

                End If

            End If

            If GameState = AppState.Start Then
                'B is the controller shortcut to create a new level.

                'Move mouse pointer over the new level button.
                Cursor.Position = New Point(ScreenOffset.X + StartScreenNewButton.Rect.X + StartScreenNewButton.Rect.Width \ 2,
                                            ScreenOffset.Y + StartScreenNewButton.Rect.Y + StartScreenNewButton.Rect.Height \ 2)

                If IsMouseDown = False Then

                    DoMouseLeftDown()

                    IsMouseDown = True

                End If

            End If

        Else

            ControllerB = False

        End If

        If XButtonPressed = True Then

            If GameState = AppState.Editing Then

                If ShowMenu = True Then
                    'X is the controller shortcut to close the menu.

                    ShowMenu = False

                End If

            End If

        End If

        If YButtonPressed = True Then

            If GameState = AppState.Editing Then

                If ShowMenu = True Then
                    'Y is the controller shortcut to open a level.

                    'Move mouse pointer over the open level button.
                    Cursor.Position = New Point(ScreenOffset.X + OpenButton.Rect.X + OpenButton.Rect.Width \ 2,
                                                ScreenOffset.Y + OpenButton.Rect.Y + OpenButton.Rect.Height \ 2)

                    If IsMouseDown = False Then

                        DoMouseLeftDown()

                        IsMouseDown = True

                    End If

                End If

            End If

            If GameState = AppState.Start Then
                'Y is the controller shortcut to open a level.

                'Move mouse pointer over the open button.
                Cursor.Position = New Point(ScreenOffset.X + StartScreenOpenButton.Rect.X + StartScreenOpenButton.Rect.Width \ 2,
                                            ScreenOffset.Y + StartScreenOpenButton.Rect.Y + StartScreenOpenButton.Rect.Height \ 2)

                If IsMouseDown = False Then

                    DoMouseLeftDown()

                    IsMouseDown = True

                End If

            End If

        End If

    End Sub

    Private Function IsOnPlatform() As Integer

        If Platforms IsNot Nothing Then

            For Each Platform In Platforms

                'Is our hero colliding with the platform?
                If Hero.Rect.IntersectsWith(Platform.Rect) = True Then
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
                If Hero.Rect.IntersectsWith(Block.Rect) = True Then
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
                If Hero.Rect.IntersectsWith(Bill.Rect) = True Then
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
                If Hero.Rect.IntersectsWith(Enemy.Rect) = True Then
                    'Yes, our hero is colliding with the enemy.

                    Return Array.IndexOf(Enemies, Enemy)

                End If

            Next

        End If

        Return -1

    End Function

    Private Sub Wraparound()

        'When our hero exits the bottom side of the level.
        If Hero.Position.Y > Level.Rect.Bottom Then

            Hero.Velocity.Y = 0F
            Hero.Velocity.X = 0F

            Hero.Position.X = 1500.0F

            'Our hero reappears on the top side the level.
            Hero.Position.Y = Level.Rect.Top - Hero.Rect.Height

        End If

    End Sub

    Private Sub FellOffLevel()

        'When our hero exits the bottom side of the level.
        If Hero.Position.Y > Level.Rect.Bottom Then

            Camera.Position.X = 0
            Camera.Position.Y = 0

            UpdateCameraOffset()

            BufferGridLines()

            ResetCash()

            ResurrectEnemies()

            ResetOurHero()

        End If

    End Sub

    Private Sub Form1_MouseDown(sender As Object, e As MouseEventArgs) Handles MyBase.MouseDown

        Select Case GameState

            Case AppState.Start

                MouseDownStart(e)

            Case AppState.Playing

                If EditPlayButton.Rect.Contains(e.Location) Then

                    'Remember the cameras in game position before opening the editor.
                    CameraPlayPostion.X = Camera.Position.X
                    CameraPlayPostion.Y = Camera.Position.Y

                    GameState = AppState.Editing

                    EditorLastFrame = Now

                    BufferGridLines()

                End If

            Case AppState.Editing

                MouseDownEditing(e)

        End Select

    End Sub

    Private Sub MouseDownStart(e As MouseEventArgs)

        'Open Button
        If StartScreenOpenButton.Rect.Contains(e.Location) Then

            OpenFileDialog1.FileName = ""
            OpenFileDialog1.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
            OpenFileDialog1.FilterIndex = 1
            OpenFileDialog1.InitialDirectory = Application.StartupPath

            If OpenFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then

                If My.Computer.FileSystem.FileExists(OpenFileDialog1.FileName) = True Then

                    InitializeObjects()

                    OpenTestLevelFile(OpenFileDialog1.FileName)

                    If IsFileLoaded = True Then

                        LevelName = Path.GetFileName(OpenFileDialog1.FileName)

                        Text = LevelName & " - Platformer with Level Editor - Code with Joe"

                        CashCollected = 0

                        LastFrame = Now

                        GameState = AppState.Playing

                        MovePointerOffScreen()

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

            LevelName = "Untitled"

            Text = LevelName & " - Platformer with Level Editor - Code with Joe"

            CashCollected = 0

            LastFrame = Now

            GameState = AppState.Playing

            MovePointerOffScreen()

            PlayLevelMusic()

        End If

    End Sub

    Private Sub MouseDownEditing(e As MouseEventArgs)

        If ShowMenu = False Then

            If e.Button = MouseButtons.Left Then

                MouseDownEditingSelection(e.Location)

                MouseDownEditingButtons(e.Location)

            End If

            If e.Button = MouseButtons.Right Then

                ShowMenu = True

                MovePointerCenterMenu()

            End If

        Else

            If e.Button = MouseButtons.Left Then

                MouseDownEditingMenuButtons(e.Location)

            End If

            If e.Button = MouseButtons.Right Then

                ShowMenu = False

            End If

        End If

    End Sub

    Private Sub MouseDownEditingMenuButtons(e As Point)

        'Is the player selecting the save button?
        If SaveButton.Rect.Contains(e) Then
            'Yes, the player is selecting the save button.

            ShowSaveLevelDialog()

        End If

        'Is the player selecting the open button?
        If OpenButton.Rect.Contains(e) Then
            'Yes, the player is selecting the open button.

            'Does the player want to save this level before opening a level?
            If MsgBox("Changes to " & LevelName & " may be lost." & vbCrLf & "Open a level anyway?",
                      MsgBoxStyle.Question Or MsgBoxStyle.OkCancel,
                      "Open Level - Platformer with Level Editor") = MsgBoxResult.Ok Then
                'No, the player doesn't want to save this level before opening a level?

                ShowOpenLevelDialog()

            End If

        End If

        'Is the player selecting the new button?
        If NewButton.Rect.Contains(e) Then
            'Yes, the player is selecting the new button.

            'Does the player want to save this level before creating a new level?
            If MsgBox("Changes to " & LevelName & " may be lost." & vbCrLf & "Create a new level anyway?",
                      MsgBoxStyle.Question Or MsgBoxStyle.OkCancel,
                      "New Level - Platformer with Level Editor") = MsgBoxResult.Ok Then
                'No, the player doesn't want to save this level before creating a new level?

                InitAndCreateNewLevel()

                ShowMenu = False

            End If

        End If

        'Is the player selecting the exit button?
        If ExitButton.Rect.Contains(e) Then
            'Yes, the player is selecting the exit button.

            LevelSelected = False

            ShowMenu = False

        End If

    End Sub

    Private Sub Form1_Move(sender As Object, e As EventArgs) Handles MyBase.Move

        ScreenOffset = PointToScreen(New Point(0, 0))

    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize

        'Is the form minimized?
        If Not Me.WindowState = FormWindowState.Minimized Then
            'No, the form is not minimized.

            DoResize()

            BufferGridLines()

        End If

    End Sub

    Private Sub DoResize()

        ResizeCamera()

        ResizeHUD()

        ResizeMenu()

        ResizeToolBar()

        ResizeStartScreen()

    End Sub

    Private Sub ResizeCamera()

        Camera.Rect.Size = ClientRectangle.Size

    End Sub

    Private Sub ResizeStartScreen()

        Title.Rect = New Rectangle(ClientRectangle.Width \ 2 - 425, ClientRectangle.Height \ 2 - 175, 850, 245)

        StartScreenNewButton.Rect = New Rectangle(ClientRectangle.Width \ 2 - 230, ClientRectangle.Height \ 2 + 70, 210, 90)

        StartScreenOpenButton.Rect = New Rectangle(ClientRectangle.Width \ 2 + 20, ClientRectangle.Height \ 2 + 70, 210, 90)

    End Sub

    Private Sub ResizeHUD()

        CashCollectedPostion.Y = ClientRectangle.Top + 5

        EditPlayButton.Rect = New Rectangle(ClientRectangle.Left + 210, ClientRectangle.Bottom - 90, 120, 90)

        'Place the FPS display at the bottom of the client area.
        FPS_Postion.Y = ClientRectangle.Bottom - 75

    End Sub

    Private Sub ResizeMenu()

        MenuBackground.Rect = New Rectangle(ClientRectangle.Width \ 2 - MenuBackground.Rect.Width \ 2,
                                        (ClientRectangle.Height \ 2) - MenuBackground.Rect.Height \ 2,
                                        300,
                                        86 * 4)

        SaveButton.Rect = New Rectangle(MenuBackground.Rect.Left + 5,
                                    MenuBackground.Rect.Top + 5,
                                    290,
                                    80)

        OpenButton.Rect = New Rectangle(MenuBackground.Rect.Left + 5,
                                    MenuBackground.Rect.Top + 90,
                                    290,
                                    80)

        NewButton.Rect = New Rectangle(MenuBackground.Rect.Left + 5,
                                   MenuBackground.Rect.Top + 87 * 2,
                                   290,
                                   80)


        ExitButton.Rect = New Rectangle(MenuBackground.Rect.Left + 5,
                                   MenuBackground.Rect.Top + 86 * 3,
                                   290,
                                   80)

        MenuButton.Rect = New Rectangle(ClientRectangle.Right - 90,
                                    ClientRectangle.Bottom - 90,
                                    90,
                                    90)

    End Sub

    Private Sub ResizeToolBar()

        ToolBarBackground.Rect = New Rectangle(ClientRectangle.Left, ClientRectangle.Bottom - 90, ClientRectangle.Width, 100)

        PointerToolButton.Rect = New Rectangle(ClientRectangle.Left + 331, ClientRectangle.Bottom - 90, 90, 90)

        BlockToolButton.Rect = New Rectangle(ClientRectangle.Left + 422, ClientRectangle.Bottom - 90, 90, 90)

        BlockToolIcon.Rect = New Rectangle(ClientRectangle.Left + 447, ClientRectangle.Bottom - 65, 40, 40)

        BillToolButton.Rect = New Rectangle(ClientRectangle.Left + 513, ClientRectangle.Bottom - 90, 90, 90)

        BillToolIcon.Rect = New Rectangle(ClientRectangle.Left + 538, ClientRectangle.Bottom - 65, 40, 40)

        BushToolButton.Rect = New Rectangle(ClientRectangle.Left + 604, ClientRectangle.Bottom - 90, 90, 90)

        BushToolIcon.Rect = New Rectangle(ClientRectangle.Left + 629, ClientRectangle.Bottom - 65, 40, 40)

        CloudToolButton.Rect = New Rectangle(ClientRectangle.Left + 695, ClientRectangle.Bottom - 90, 90, 90)

        CloundToolIcon.Rect = New Rectangle(ClientRectangle.Left + 720, ClientRectangle.Bottom - 65, 40, 40)

        GoalToolButton.Rect = New Rectangle(ClientRectangle.Left + 786, ClientRectangle.Bottom - 90, 90, 90)

        GoalToolIcon.Rect = New Rectangle(ClientRectangle.Left + 811, ClientRectangle.Bottom - 65, 40, 40)

        EnemyToolButton.Rect = New Rectangle(ClientRectangle.Left + 877, ClientRectangle.Bottom - 90, 90, 90)

        EnemyToolIcon.Rect = New Rectangle(ClientRectangle.Left + 902, ClientRectangle.Bottom - 65, 40, 40)

    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles MyBase.Closing

        GameLoopCancellationToken.Cancel(True)

        CloseSounds()

    End Sub

    Private Sub UpdateCamera()

        LookAhead()

        KeepCameraOnTheLevel()

    End Sub

    Private Sub LookAhead()

        'Is our hero near the right side of the frame?
        If Hero.Position.X > Camera.Position.X + Camera.Rect.Width / 1.5 Then
            'If Hero.X > Camera.X + Camera.Width / 1.5 Then
            'Yes, our hero is near the right side of the frame.

            'Move camera to the right.
            Camera.Position.X = Hero.Rect.Left - Camera.Rect.Width / 1.5
            'Camera.X = Hero.Left - Camera.Width / 1.5

            UpdateCameraOffset()

        End If

        'Is our hero near the left side of the frame?
        If Hero.Rect.X < Camera.Position.X + Camera.Rect.Width / 4 Then
            'If Hero.X < Camera.X + Camera.Width / 4 Then
            'Yes, our hero is near the left side of the frame.

            'Move camera to the left.
            Camera.Position.X = Hero.Rect.Left - Camera.Rect.Width / 4
            'Camera.X = Hero.Left - Camera.Width / 4

            UpdateCameraOffset()

        End If

        'Is our hero near the bottom side of the frame?
        If Hero.Rect.Y > Camera.Position.Y + Camera.Rect.Height / 1.25 Then
            'If Hero.Y > Camera.Y + Camera.Height / 1.25 Then
            'Yes, our hero is near the bottom side of the frame.

            'Move camera down.
            Camera.Position.Y = Hero.Rect.Top - Camera.Rect.Height / 1.25
            'Camera.Y = Hero.Top - Camera.Height / 1.25

            UpdateCameraOffset()

        End If

        'Is our hero near the top side of the frame?
        If Hero.Rect.Y < Camera.Position.Y + Camera.Rect.Height / 6 Then
            'Yes, our hero is near the top side of the frame.

            'Move camera up.
            Camera.Position.Y = Hero.Rect.Top - Camera.Rect.Height / 6

            UpdateCameraOffset()

        End If

    End Sub

    Private Sub KeepCameraOnTheLevel()

        'Has the camera moved passed the left side of the level?
        If Camera.Position.X < Level.Rect.Left Then
            'Yes, the camera has moved pass the left side of the level.

            'Limit the camera movement to the left side of the level.
            Camera.Position.X = Level.Rect.Left

            UpdateCameraOffset()

        End If

        'Has the camera moved passed the right side of the level?
        If Camera.Position.X + Camera.Rect.Width > Level.Rect.Right Then
            'Yes, the camera has moved pass the right side of the level.

            'Limit the camera movement to the right side of the level.
            Camera.Position.X = Level.Rect.Right - Camera.Rect.Width

            UpdateCameraOffset()

        End If

        'Has the camera moved passed the top side of the level?
        If Camera.Position.Y < Level.Rect.Top Then
            'Yes, the camera has moved passed the top side of the level.

            'Limit camera movement to the top side of the level.
            Camera.Position.Y = Level.Rect.Top

            UpdateCameraOffset()

        End If

        'Has the camera moved passed the bottom side of the level?
        If Camera.Position.Y + Camera.Rect.Height > Level.Rect.Bottom Then
            'Yes, the camera has moved pass the bottom side of the level.

            'Limit camera movement to the bottom of the level.
            Camera.Position.Y = Level.Rect.Bottom - Camera.Rect.Height

            UpdateCameraOffset()

        End If

    End Sub

    Private Sub UpdateCameraOffset()

        CameraOffset.X = Camera.Position.X * -1

        CameraOffset.Y = Camera.Position.Y * -1

    End Sub

    Private Function AddSound(SoundName As String, FilePath As String) As Boolean

        Dim CommandOpen As String = "open " & Chr(34) & FilePath & Chr(34) & " alias " & SoundName

        'Do we have a name and does the file exist?
        If Not SoundName.Trim = String.Empty And IO.File.Exists(FilePath) Then
            'Yes, we have a name and the file exists.

            'Do we have sounds?
            If Sounds IsNot Nothing Then
                'Yes, we have sounds.

                'Is the sound in the array already?
                If Not Sounds.Contains(SoundName) Then
                    'No, the sound is not in the array.

                    'Did the sound file open?
                    If mciSendStringW(CommandOpen, Nothing, 0, IntPtr.Zero) = 0 Then
                        'Yes, the sound file did open.

                        'Add the sound to the Sounds array.
                        Array.Resize(Sounds, Sounds.Length + 1)
                        Sounds(Sounds.Length - 1) = SoundName

                        Return True 'The sound was added.

                    End If

                End If

            Else
                'No, we do not have sounds.

                'Did the sound file open?
                If mciSendStringW(CommandOpen, Nothing, 0, IntPtr.Zero) = 0 Then
                    'Yes, the sound file did open.

                    'Start the Sounds array with the sound.
                    ReDim Sounds(0)
                    Sounds(0) = SoundName

                    Return True 'The sound was added.

                End If

            End If

        End If

        Return False 'The sound was not added.

    End Function

    Private Function LoopSound(SoundName As String) As Boolean

        Dim CommandSeekToStart As String = "seek " & SoundName & " to start"

        Dim CommandPlayRepete As String = "play " & SoundName & " repeat"

        'Do we have sounds?
        If Sounds IsNot Nothing Then
            'Yes, we have sounds.

            'Is the sound in the array?
            If Not Sounds.Contains(SoundName) Then
                'No, the sound is not in the array.

                Return False 'The sound is not playing.

            End If

            mciSendStringW(CommandSeekToStart, Nothing, 0, IntPtr.Zero)

            If mciSendStringW(CommandPlayRepete, Nothing, 0, Me.Handle) <> 0 Then

                Return False 'The sound is not playing.

            End If

        End If

        Return True 'The sound is playing.

    End Function

    Private Function PlaySound(SoundName As String) As Boolean

        Dim CommandSeekToStart As String = "seek " & SoundName & " to start"

        Dim CommandPlay As String = "play " & SoundName & " notify"

        'Do we have sounds?
        If Sounds IsNot Nothing Then
            'Yes, we have sounds.

            'Is the sound in the array?
            If Sounds.Contains(SoundName) Then
                'Yes, the sound is in the array.

                mciSendStringW(CommandSeekToStart, Nothing, 0, IntPtr.Zero)

                If mciSendStringW(CommandPlay, Nothing, 0, Me.Handle) = 0 Then

                    Return True 'The sound is playing.

                End If

            End If

        End If

        Return False 'The sound is not playing.

    End Function

    Private Function PauseSound(SoundName As String) As Boolean

        Dim CommandPause As String = "pause " & SoundName & " notify"

        'Do we have sounds?
        If Sounds IsNot Nothing Then
            'Yes, we have sounds.

            'Is the sound in the array?
            If Sounds.Contains(SoundName) Then
                'Yes, the sound is in the array.

                If mciSendStringW(CommandPause, Nothing, 0, Me.Handle) = 0 Then

                    Return True 'The sound is playing.

                End If

            End If

        End If

        Return False 'The sound is not playing.

    End Function

    Private Function SetVolume(SoundName As String, Level As Integer) As Boolean

        Dim CommandVolume As String = "setaudio " & SoundName & " volume to " & Level.ToString

        'Do we have sounds?
        If Sounds IsNot Nothing Then
            'Yes, we have sounds.

            'Is the sound in the sounds array?
            If Sounds.Contains(SoundName) Then
                'Yes, the sound is the sounds array.

                'Is the level in the valid range?
                If Level >= 0 And Level <= 1000 Then
                    'Yes, the level is in range.

                    'Was the volume set?
                    If mciSendStringW(CommandVolume, Nothing, 0, IntPtr.Zero) = 0 Then

                        Return True 'Yes, the volume was set.

                    End If

                End If

            End If

        End If

        Return False 'The volume was not set.

    End Function

    Private Function IsPlaying(SoundName As String) As Boolean

        Return GetStatus(SoundName, "mode") = "playing"

    End Function

    Private Sub AddOverlapping(SoundName As String, FilePath As String)

        For Each Suffix As String In {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L"}

            AddSound(SoundName & Suffix, FilePath)

        Next

    End Sub

    Private Sub PlayOverlapping(SoundName As String)

        For Each Suffix As String In {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L"}

            If Not IsPlaying(SoundName & Suffix) Then

                PlaySound(SoundName & Suffix)

                Exit Sub

            End If

        Next

    End Sub

    Private Sub SetVolumeOverlapping(SoundName As String, Level As Integer)

        For Each Suffix As String In {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L"}

            SetVolume(SoundName & Suffix, Level)

        Next

    End Sub

    Private Function GetStatus(SoundName As String, StatusType As String) As String

        Dim CommandStatus As String = "status " & SoundName & " " & StatusType

        Dim StatusReturn As New System.Text.StringBuilder(128)

        If Sounds IsNot Nothing Then

            If Sounds.Contains(SoundName) Then

                mciSendStringW(CommandStatus, StatusReturn, 128, IntPtr.Zero)

                Return StatusReturn.ToString.Trim.ToLower

            End If

        End If

        Return String.Empty

    End Function

    Private Sub CloseSounds()

        Dim CommandClose As String

        If Sounds IsNot Nothing Then

            For Each Sound In Sounds

                CommandClose = "close " & Sound

                mciSendStringW(CommandClose, Nothing, 0, IntPtr.Zero)

            Next

        End If

        Sounds = Nothing

    End Sub

    Private Sub CreateSoundFileFromResource()

        Dim File As String = Path.Combine(Application.StartupPath, "level.mp3")

        If Not IO.File.Exists(File) Then

            IO.File.WriteAllBytes(File, My.Resources.level)

        End If

        File = Path.Combine(Application.StartupPath, "CashCollected.mp3")

        If Not IO.File.Exists(File) Then

            IO.File.WriteAllBytes(File, My.Resources.CashCollected)

        End If

        File = Path.Combine(Application.StartupPath, "eliminated.mp3")

        If Not IO.File.Exists(File) Then

            IO.File.WriteAllBytes(File, My.Resources.eliminated)

        End If

        File = Path.Combine(Application.StartupPath, "clear.mp3")

        If Not IO.File.Exists(File) Then

            IO.File.WriteAllBytes(File, My.Resources.clear)

        End If

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

    Private Sub DoMouseLeftDown()
        'Simulate a left mouse button down event

        Dim InputDown As New INPUTStruc With {
            .type = INPUT_MOUSE
        }

        InputDown.union.mi.dwFlags = MOUSEEVENTF_LEFTDOWN

        Dim Inputs As INPUTStruc() = {InputDown}

        SendInput(CUInt(Inputs.Length), Inputs, Marshal.SizeOf(GetType(INPUTStruc)))

    End Sub

    Private Sub DoMouseLeftUp()
        'Simulate a left mouse button up event.

        Dim InputUp As New INPUTStruc With {
            .type = INPUT_MOUSE
        }

        InputUp.union.mi.dwFlags = MOUSEEVENTF_LEFTUP

        Dim Inputs As INPUTStruc() = {InputUp}

        SendInput(CUInt(Inputs.Length), Inputs, Marshal.SizeOf(GetType(INPUTStruc)))

    End Sub

    Private Sub ClickMouseLeft()
        ' Simulate a left mouse button down event
        Dim InputDown As New INPUTStruc()
        InputDown.type = INPUT_MOUSE
        InputDown.union.mi.dwFlags = MOUSEEVENTF_LEFTDOWN

        ' Simulate a left mouse button up event
        Dim InputUp As New INPUTStruc()
        InputUp.type = INPUT_MOUSE
        InputUp.union.mi.dwFlags = MOUSEEVENTF_LEFTUP

        ' Send the input events using SendInput
        Dim Inputs As INPUTStruc() = {InputDown, InputUp}
        SendInput(CUInt(Inputs.Length), Inputs, Marshal.SizeOf(GetType(INPUTStruc)))

    End Sub

    Private Shared Sub MovePointerOffScreen()
        'Move mouse pointer off screen.

        Cursor.Position = New Point(Screen.PrimaryScreen.WorkingArea.Right,
                                    Screen.PrimaryScreen.WorkingArea.Height \ 2)

    End Sub

    Private Sub MovePointerCenterMenu()
        'Move mouse pointer to the center of the menu.

        Cursor.Position = New Point(MenuBackground.Rect.X + MenuBackground.Rect.Width \ 2,
                                    MenuBackground.Rect.Y + MenuBackground.Rect.Height \ 2)

    End Sub

    Private Sub MovePointerToStartScreenNewButton()
        'Move mouse pointer over the new level button on the start screen.

        Cursor.Position = New Point(ScreenOffset.X + StartScreenNewButton.Rect.X + StartScreenNewButton.Rect.Width \ 2,
                                    ScreenOffset.Y + StartScreenNewButton.Rect.Y + StartScreenNewButton.Rect.Height \ 2)

    End Sub

    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)

        'Intentionally left blank. Do not remove.

    End Sub

End Class


'Monica is our an AI assistant.
'https://monica.im/


'I also make coding videos on my YouTube channel.
'https://www.youtube.com/@codewithjoe6074

