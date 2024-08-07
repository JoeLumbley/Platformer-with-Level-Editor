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

    'Import GetState to get the controller button, stick and trigger positions.
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

    'Set the start of the thumbstick neutral zone to 1/2 over.
    Private Const NeutralStart As Short = -16384 '-16,384 = -32,768 / 2
    'The thumbstick position must be more than 1/2 over the neutral start to register as moved.
    'A short is a signed 16-bit (2-byte) integer range -32,768 through 32,767. This gives us 65,536 values.

    'Set the end of the thumbstick neutral zone to 1/2 over.
    Private Const NeutralEnd As Short = 16384 '16,383.5 = 32,767 / 2
    'The thumbstick position must be more than 1/2 over the neutral end to register as moved.
    'A short is a signed 16-bit (2-byte) integer range -32,768 through 32,767. This gives us 65,536 values.

    'Set the trigger threshold to 1/4 pull.
    Private Const TriggerThreshold As Byte = 64 '64 = 256 / 4
    'The trigger position must be greater than the trigger threshold to register as pulled.
    'A byte is a unsigned 8-bit (1-byte) integer range 0 through 255. This gives us 256 values.

    Private ReadOnly Connected(0 To 3) As Boolean 'A boolean is a logical value that is either true or false.

    Private ControllerPosition As XINPUT_STATE

    Private Vibration As XINPUT_VIBRATION

    Private ControllerA As Boolean = False

    Private ControllerB As Boolean = False

    Private ControllerRight As Boolean = False

    Private ControllerLeft As Boolean = False

    Private ControllerUp As Boolean = False

    Private ControllerJumped As Boolean = False

    Private IsMouseDown As Boolean = False

    Private IsLeftStickDown As Boolean = False

    Private IsStartDown As Boolean = False

    Private IsContextDown As Boolean = False

    Private IsMKeyDown As Boolean = False

    Private RightTriggerDown As Boolean = False

    Private LeftTriggerDown As Boolean = False

    Private LeftThumbstickDown As Boolean = False

    Private LeftThumbstickUp As Boolean = False

    Private RightThumbstickDown As Boolean = False

    Private RightThumbstickUp As Boolean = False

    Private RightThumbstickLeft As Boolean = False

    Private RightThumbstickRight As Boolean = False

    Private IsDPadUp As Boolean = False

    Private IsDPadDown As Boolean = False

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
        Spawn
        Backdrop
        Portal
    End Enum

    Private Enum Direction As Integer
        Right
        Left
    End Enum

    Private Structure GameObject

        Public ID As ObjectID

        Public Position As Vector2 'A vector 2 is composed of two floating-point values called X and Y.

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

        Public Color As Integer

    End Structure

    Private Enum Tools As Integer
        Pointer
        Block
        Bill
        Bush
        Cloud
        Goal
        Enemy
        Backdrop
        Portal
    End Enum

    Private GameState As AppState = AppState.Start

    Private Context As New BufferedGraphicsContext

    Private Buffer As BufferedGraphics

    Private FrameCount As Integer = 0

    Private StartTime As DateTime = Now

    Private TimeElapsed As TimeSpan

    Private SecondsElapsed As Double = 0

    Private FPS As Integer = 0

    Private ReadOnly FPSFont As New Font(FontFamily.GenericSansSerif, 25)

    Private ReadOnly ControllerHintFont As New Font(FontFamily.GenericSansSerif, 15, FontStyle.Bold)

    Private ReadOnly ControllerHintFontSmall As New Font(New FontFamily("Wingdings"), 12, FontStyle.Bold)

    Private ReadOnly BillFont As New Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold)

    Private ReadOnly MenuButtonFont As New Font(FontFamily.GenericSansSerif, 45)

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

    Private Spawn As GameObject

    Private Platforms() As GameObject

    Private Blocks() As GameObject

    Private Clouds() As GameObject

    Private Bushes() As GameObject

    Private Cash() As GameObject

    Private Enemies() As GameObject

    Private Backdrops() As GameObject

    Private Portals() As GameObject

    Private FileObjects() As GameObject

    Private EditPlayButton As GameObject

    Private EditPlayButtonHover As Boolean = False

    Private ToolBarBackground As GameObject

    Private MenuBackground As GameObject

    Private MenuButton As GameObject

    Private MenuButtonHover As Boolean = False

    Private PointerToolButton As GameObject

    Private PointerToolButtonHover As Boolean = False

    Private BlockToolButton As GameObject

    Private BlockToolButtonHover As Boolean = False

    Private BlockToolIcon As GameObject

    Private BackdropToolButton As GameObject

    Private BackdropToolButtonHover As Boolean = False

    Private BackdropToolIcon As GameObject

    Private BillToolButton As GameObject

    Private BillToolButtonHover As Boolean = False

    Private BillToolIcon As GameObject

    Private CloudToolButton As GameObject

    Private CloudToolButtonHover As Boolean = False

    Private CloundToolIcon As GameObject

    Private BushToolButton As GameObject

    Private BushToolIcon As GameObject

    Private BushToolButtonHover As Boolean = False

    Private GoalToolButton As GameObject

    Private GoalToolButtonHover As Boolean = False

    Private GoalToolIcon As GameObject

    Private PortalToolButton As GameObject

    Private PortalToolButtonHover As Boolean = False

    Private PortalToolIcon As GameObject

    Private EnemyToolButton As GameObject

    Private EnemyToolButtonHover As Boolean = False

    Private EnemyToolIcon As GameObject

    Private SelectedTool As Tools = Tools.Pointer

    Private ShowToolPreview As Boolean = False

    Private ShowMenu As Boolean = False

    Private Title As GameObject

    Private StartScreenOpenButton As GameObject

    Private StartScreenOpenButtonHover As Boolean = False

    Private StartScreenNewButton As GameObject

    Private StartScreenNewButtonHover As Boolean = False

    Private SaveButton As GameObject

    Private SaveButtonHover As Boolean = False

    Private OpenButton As GameObject

    Private OpenButtonHover As Boolean = False

    Private NewButton As GameObject

    Private NewButtonHover As Boolean = False

    Private ExitButton As GameObject

    Private ExitButtonHover As Boolean = False

    Private ScoreIndicators As GameObject

    Private Level As GameObject

    Private Camera As GameObject

    Private CameraPlayPostion As Point

    Private ToolPreview As Rectangle

    Private SelectedCloud As Integer = -1

    Private SelectedBlock As Integer = -1

    Private SelectedBackdrop As Integer = -1

    Private SelectedPortal As Integer = -1

    Private PortalEntranceSelected As Boolean = False

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

    'Private GridLineBitmap As New Bitmap(Screen.PrimaryScreen.WorkingArea.Size.Width, Screen.PrimaryScreen.WorkingArea.Size.Height)

    'Private GridLineBuffer As Graphics = Graphics.FromImage(GridLineBitmap)

    Private SizingHandle As New Rectangle(0, 0, 25, 25)

    Private SizingHandleSelected As Boolean = False

    Private SelectionOffset As Point

    Private CashCollected As Integer = 0

    Private CashCollectedPostion As New Point(0, 0)

    Private ReadOnly PointerToolFont As New Font(New FontFamily("Wingdings"), 32, FontStyle.Bold)

    Private ReadOnly GoalToolFont As New Font(New FontFamily("Wingdings"), 25, FontStyle.Bold)

    Private ReadOnly GoalFont As New Font(New FontFamily("Wingdings"), 35, FontStyle.Bold)

    Private ReadOnly BillIconFont As New Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold)

    Private ReadOnly EnemyIconFont As New Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold)

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

    Private CloundToolIconOutinePen As New Pen(Color.Black, 4)

    Private BushToolIconOutinePen As New Pen(Color.Black, 4)

    Private LightSkyBluePen As New Pen(Color.LightSkyBlue, 4)

    Private CloundToolIconPen As New Pen(Color.LightSkyBlue, 4)

    Private BushToolIconPen As New Pen(Color.SeaGreen, 4)

    Private LawnGreenPen As New Pen(Color.LawnGreen, 4)

    Private SeaGreenPen As New Pen(Color.SeaGreen, 4)

    Private CharcoalGrey As Color = Color.FromArgb(255, 60, 65, 66)

    Private DarkCharcoalGrey As Color = Color.FromArgb(255, 48, 52, 53)

    Private CharcoalGreyBrush As New SolidBrush(CharcoalGrey)

    Private DarkCharcoalGreyBrush As New SolidBrush(DarkCharcoalGrey)

    Private HoverColor As Color = Color.FromArgb(255, 39, 39, 39)

    Private HoverBrush As New SolidBrush(HoverColor)

    Private StartScreenButtonOutlineHoverPen As New Pen(Color.Orange, 15)

    Private StartScreenButtonOutlinePen As New Pen(Color.White, 9)

    Private SelectedColor As Color = Color.FromArgb(255, 51, 51, 51)

    Private SelectedBrush As New SolidBrush(SelectedColor)

    Private SelectedHoverColor As Color = Color.FromArgb(255, 71, 71, 71)

    Private SelectedHoverBrush As New SolidBrush(SelectedHoverColor)

    Private SelectionHighlightColor As Color = Color.Orange

    Private SelectionHighlightPen As New Pen(SelectionHighlightColor, 3)

    Private SelectionHighlightHoverColor As Color = Color.DarkOrange

    Private SelectionHighlightHoverPen As New Pen(SelectionHighlightHoverColor, 3)

    Private SelectionRectangleOutlinePen As New Pen(Color.Black, 14)

    Private SelectionRectangleDashPen As New Pen(Color.Orange, 8) With {
                        .DashCap = DashCap.Flat,
                        .DashPattern = New Single() {4.0F, 4.0F}
                    }

    Private SizingHandleOuterOutlinePen As New Pen(Color.Black, 14)

    Private SizingHandleInnerOutlinePen As New Pen(Color.OrangeRed, 8)

    Private ReadOnly AlineCenter As New StringFormat With {.Alignment = StringAlignment.Center}

    Private ReadOnly AlineCenterMiddle As New StringFormat With {.Alignment = StringAlignment.Center,
                                                                 .LineAlignment = StringAlignment.Center}

    Private ReadOnly CWJFont As New Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold)

    Private ReadOnly SpawnFont As New Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold)

    Private SpawnColor As Color = Color.FromArgb(255, Color.Red)

    Private SpawnBrush As New SolidBrush(SpawnColor)

    Private ReadOnly EnemyFont As New Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold)

    Private ReadOnly PortalFont As New Font(FontFamily.GenericSansSerif, 20, FontStyle.Bold)

    Private ReadOnly ToolTipFont As New Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold)

    Private ToolTipColor As Color = Color.Black

    Private ToolTipBrush As New SolidBrush(ToolTipColor)



    Private GameLoopCancellationToken As New CancellationTokenSource()

    Private IsFileLoaded As Boolean = False

    Private IsBackDown As Boolean = False

    Private ClearScreenTimer As TimeSpan

    Private ClearScreenTimerStart As DateTime

    Private StopClearScreenTimer As Boolean = True

    Private GoalSelected As Boolean = False

    Private LevelSelected As Boolean = False

    Private SpawnSelected As Boolean = False

    Private ShowOpenFileDialog As Boolean = False

    Private ShowSaveFileDialog As Boolean = False

    Private ShowSaveWarning As Boolean = False

    Private LevelName As String = "Untitled"

    Private ScreenOffset As Point

    Private IsMuted As Boolean = False

    Private CameraOffset As New Point(0, 0)

    Private Sounds() As String

    Private LeftVibrateStart As DateTime

    Private RightVibrateStart As DateTime

    Private IsLeftVibrating As Boolean = False

    Private IsRightVibrating As Boolean = False

    Private GridLinesOutlinePen As New Pen(Color.Black, 1)

    Private GridLinesDashPen As New Pen(Color.Gray, 1) With {
                        .DashCap = DashCap.Flat,
                        .DashPattern = New Single() {32.0F, 32.0F}
                    }
    Private Structure Line

        Public X1 As Integer
        Public Y1 As Integer
        Public X2 As Integer
        Public Y2 As Integer

    End Structure

    Private Gridlines() As Line

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        InitializeApp()

    End Sub

    Private Sub GameTimer_Tick(sender As Object, e As EventArgs) Handles GameTimer.Tick

        UpdateFrame()

        Refresh() 'Triggers the OnPaint sub which draws the frame.

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

                UpdateCameraOffset()

            Case AppState.Editing

                UpdateControllerData()

                UpdateEditorDeltaTime()

                UpdateMousePointerMovement()

                UpdateCameraMovement()

                UpdateCameraOffset()

                UpdateGridLines()

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

            .CompositingMode = CompositingMode.SourceCopy
            .CompositingQuality = CompositingQuality.HighSpeed
            .TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit
            .PixelOffsetMode = PixelOffsetMode.None
            .SmoothingMode = SmoothingMode.None
            .InterpolationMode = InterpolationMode.Bilinear

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

        For ControllerNumber As Integer = 0 To 3 'Up to 4 controllers

            Try

                If IsControllerConnected(ControllerNumber) = True Then

                    UpdateControllerState(ControllerNumber)

                    Connected(ControllerNumber) = True

                Else

                    Connected(ControllerNumber) = False

                End If

            Catch ex As Exception
                'Something went wrong (An exception occured).

                DisplayError(ex)

                Exit Sub

            End Try

        Next

        UpdateVibrateTimer()

    End Sub

    Private Sub UpdateControllerState(controllerNumber As Integer)

        'Is controller zero connected?
        If Connected(0) = True AndAlso controllerNumber = 0 Then
            'Yes, controller zero is connected.
            'Use controller zero.

            UpdateButtonPosition()

            UpdateLeftThumbstickPosition()

            UpdateLeftTriggerPosition()

            UpdateRightThumbstickPosition()

            UpdateRightTriggerPosition()

        End If

        'Is controller zero disconnected and controller one connected?
        If Connected(0) = False AndAlso Connected(1) = True AndAlso controllerNumber = 1 Then
            'Yes, controller zero is disconnected and controller one is connected.
            'Use controller one.

            UpdateButtonPosition()

            UpdateLeftThumbstickPosition()

            UpdateLeftTriggerPosition()

            UpdateRightThumbstickPosition()

            UpdateRightTriggerPosition()

        End If

    End Sub

    Private Sub DisplayError(ex As Exception)

        MsgBox(ex.ToString()) ' Display the exception message in a message box.

    End Sub

    Private Function IsControllerConnected(controllerNumber As Integer) As Boolean

        Return XInputGetState(controllerNumber, ControllerPosition) = 0 '0 means the controller is connected.
        'Anything else (a non-zero value) means the controller is not connected.

    End Function

    Private Sub UpdateVibrateTimer()

        UpdateLeftVibrateTimer()

        UpdateRightVibrateTimer()

    End Sub

    Private Sub UpdateLeftVibrateTimer()

        If IsLeftVibrating = True Then

            Dim ElapsedTime As TimeSpan = Now - LeftVibrateStart

            If ElapsedTime.TotalSeconds >= 1 Then

                IsLeftVibrating = False

                'Turn left motor off (set zero speed).
                Vibration.wLeftMotorSpeed = 0

                If Connected(0) = True Then

                    SendVibrationMotorCommand(0)

                End If

                If Connected(0) = False AndAlso Connected(1) = True Then

                    SendVibrationMotorCommand(1)

                End If

            End If

        End If

    End Sub

    Private Sub UpdateRightVibrateTimer()

        If IsRightVibrating = True Then

            Dim ElapsedTime As TimeSpan = Now - RightVibrateStart

            If ElapsedTime.TotalSeconds >= 1 Then

                IsRightVibrating = False

                'Turn right motor off (set zero speed).
                Vibration.wRightMotorSpeed = 0

                If Connected(0) = True Then

                    SendVibrationMotorCommand(0)

                End If

                If Connected(0) = False AndAlso Connected(1) = True Then

                    SendVibrationMotorCommand(1)

                End If

            End If

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

        DrawBackground(Color.FromArgb(Level.Color))

        DrawBackdrops()

        DrawClouds()

        DrawBushes()

        DrawBlocks()

        DrawCash()

        DrawGoal()

        DrawEnemies()

        DrawPortals()

        DrawOurHero()

        DrawCollectedCash()

        DrawFPS()

        DrawEditButton()

    End Sub

    Private Sub DrawEditing()

        DrawBackground(Color.FromArgb(Level.Color))

        DrawBackdrops()

        DrawToolPreviewBackdrop()

        DrawClouds()

        DrawBushes()

        DrawBlocks()

        DrawCash()

        DrawGoal()

        DrawEnemies()

        DrawSpawn()

        DrawPortals()

        DrawGridLines()

        DrawSelectionAndSizingHandle()

        DrawOurHero()

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

        DrawBackdropToolButton()

        DrawPortalToolButton()

    End Sub

    Private Sub DrawClearScreen()

        DrawBackground(Color.Black)

        DrawClearTitle()

        DrawOurHero()

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

        If Portals IsNot Nothing Then

            For Each Portal In Portals

                Dim PortalEntrance As New Rectangle(New Point(Portal.PatrolA.X, Portal.PatrolA.Y), New Drawing.Size(GridSize, GridSize))

                If Hero.Rect.IntersectsWith(PortalEntrance) = True Then

                    If UpArrowDown = True Or ControllerUp = True Then

                        If IsPlaying("Portal") = False Then

                            PlaySound("Portal")

                        End If

                        Dim PortalExit As New Rectangle(New Point(Portal.PatrolB.X, Portal.PatrolB.Y), New Drawing.Size(GridSize, GridSize))

                        Hero.Rect = PortalExit
                        Hero.Position.X = Hero.Rect.X
                        Hero.Position.Y = Hero.Rect.Y

                        FrameHero()

                    End If

                End If

            Next

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

                            DeathRumble()

                            'Restart level.
                            Camera.Position.X = 0
                            Camera.Position.Y = 0

                            UpdateCameraOffset()

                            'BufferGridLines()

                            ResetCash()

                            ResurrectEnemies()

                            ResetOurHero()

                        End If

                    End If

                End If

            Next

        End If

    End Sub

    Private Sub DeathRumble()

        If Connected(0) = True Then

            VibrateLeft(0, 65535)

            VibrateRight(0, 65535)

        End If

        If Connected(0) = False AndAlso Connected(1) = True Then

            VibrateLeft(1, 65535)

            VibrateRight(1, 65535)

        End If

    End Sub

    Private Sub ResetOurHero()

        Hero.Rect.X = Spawn.Rect.X
        Hero.Rect.Y = Spawn.Rect.Y

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

                'BufferGridLines()

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

    Private Sub UpdateLeftThumbstickPosition()
        'The range on the X-axis is -32,768 through 32,767. Signed 16-bit (2-byte) integer.
        'The range on the Y-axis is -32,768 through 32,767. Signed 16-bit (2-byte) integer.

        'What position is the left thumbstick in on the X-axis?
        If ControllerPosition.Gamepad.sThumbLX <= NeutralStart Then
            'The left thumbstick is in the left position.

            DoLeftThumbstickLeftLogic()

        ElseIf ControllerPosition.Gamepad.sThumbLX >= NeutralEnd Then
            'The left thumbstick is in the right position.

            DoLeftThumbstickRightLogic()

        Else
            'The left thumbstick is in the neutral position.

            DoLeftThumbstickXAxisNeutralLogic()

        End If

        'What position is the left thumbstick in on the Y-axis?
        If ControllerPosition.Gamepad.sThumbLY <= NeutralStart Then
            'The left thumbstick is in the down position.

            DoLeftThumbstickDownLogic()

        ElseIf ControllerPosition.Gamepad.sThumbLY >= NeutralEnd Then
            'The left thumbstick is in the up position.

            DoLeftThumbstickUpLogic()

        Else
            'The left thumbstick is in the neutral position.

            DoLeftThumbstickYAxisNeutralLogic()

        End If

    End Sub

    Private Sub DoLeftThumbstickLeftLogic()

        If GameState = AppState.Editing Then

            If ShowMenu = False Then

                MovePointerLeft()

            Else

                If ShowSaveWarning = True Or ShowSaveFileDialog = True Then

                    MovePointerLeft()

                End If

            End If

        End If

        If GameState = AppState.Playing Then

            ControllerLeft = True

            ControllerRight = False

        End If

        If GameState = AppState.Start Then

            If ShowOpenFileDialog = True Then

                MovePointerLeft()

            Else

                MovePointerToStartScreenNewButton()

            End If

        End If

    End Sub

    Private Sub DoLeftThumbstickRightLogic()

        If GameState = AppState.Editing Then

            If ShowMenu = False Then

                MovePointerRight()

            Else

                If ShowSaveWarning = True Or ShowSaveFileDialog = True Then

                    MovePointerRight()

                End If

            End If

        End If

        If GameState = AppState.Playing Then

            ControllerLeft = False

            ControllerRight = True

        End If

        If GameState = AppState.Start Then

            If ShowOpenFileDialog = True Then

                MovePointerRight()

            Else

                MovePointerToStartScreenOpenButton()

            End If

        End If

    End Sub

    Private Sub DoLeftThumbstickXAxisNeutralLogic()

        If GameState = AppState.Start Or GameState = AppState.Editing Then

            If DPadLeftPressed = False AndAlso
              DPadRightPressed = False AndAlso
                 LeftArrowDown = False AndAlso
                RightArrowDown = False AndAlso
           RightThumbstickLeft = False AndAlso
          RightThumbstickRight = False Then

                DeceleratePointerXAxis()

            End If

        End If

    End Sub

    Private Sub DoLeftThumbstickDownLogic()

        If GameState = AppState.Editing Then

            'Is the menu open?
            If ShowMenu = True Then
                'Yes, the menu is open.

                'Are dialog windows open?
                If ShowSaveWarning = True Or ShowSaveFileDialog = True Then
                    'Yes, dialog windows are open.

                    MovePointerDown()

                Else
                    'No, dialog windows are not open.

                    DoLeftThumbstickDownMenuLogic()

                End If

            Else
                'No, the menu is not open.

                MovePointerDown()

            End If

        End If

        If GameState = AppState.Start Then

            If ShowOpenFileDialog = True Then

                MovePointerDown()

            Else

                MovePointerToStartScreenOpenButton()

            End If

        End If

    End Sub

    Private Sub DoLeftThumbstickUpLogic()

        If GameState = AppState.Editing Then

            'Is the menu open?
            If ShowMenu = True Then
                'Yes, the menu is open.

                'Are dialog windows open?
                If ShowSaveWarning = True Or ShowSaveFileDialog = True Then
                    'Yes, dialog windows are open.

                    MovePointerUp()

                Else
                    'No, dialog windows are not open.

                    DoLeftThumbstickUpMenuLogic()

                End If

            Else
                'No, the menu is not open.

                MovePointerUp()

            End If

        End If

        If GameState = AppState.Start Then

            If ShowOpenFileDialog = True Then

                MovePointerUp()

            Else

                MovePointerToStartScreenNewButton()

            End If

        End If

        If GameState = AppState.Playing Then

            ControllerUp = True

        End If

    End Sub

    Private Sub DoLeftThumbstickYAxisNeutralLogic()

        If GameState = AppState.Start Or GameState = AppState.Editing Then

            LeftThumbstickDown = False

            LeftThumbstickUp = False

            If DPadUpPressed = False AndAlso DPadDownPressed = False AndAlso UpArrowDown = False AndAlso DownArrowDown = False AndAlso RightThumbstickDown = False AndAlso RightThumbstickUp = False Then

                DeceleratePointerYAxis()

            End If

        End If

        If GameState = AppState.Playing AndAlso DPadUpPressed = False Then

            ControllerUp = False

        End If

    End Sub

    Private Sub DoLeftThumbstickDownMenuLogic()

        If LeftThumbstickDown = False Then

            LeftThumbstickDown = True

            DoMenuLogicDown()

        End If

    End Sub

    Private Sub DoLeftThumbstickUpMenuLogic()

        If LeftThumbstickUp = False Then

            LeftThumbstickUp = True

            DoMenuLogicUp()

        End If

    End Sub

    Private Sub DoMenuLogicUp()

        Dim MousePointerOffset As Point = MousePointer.Rect.Location

        'Convert from screen to client.
        MousePointerOffset.X -= ScreenOffset.X
        MousePointerOffset.Y -= ScreenOffset.Y

        'Is the mouse pointer over any button?
        If Not OpenButton.Rect.Contains(MousePointerOffset) AndAlso
           Not NewButton.Rect.Contains(MousePointerOffset) AndAlso
           Not ExitButton.Rect.Contains(MousePointerOffset) AndAlso
           Not SaveButton.Rect.Contains(MousePointerOffset) Then
            'No, the mouse pointer is not over any button.

            MovePointerOverSaveButton()

        End If

        'Is the mouse pointer on the save button?
        If SaveButton.Rect.Contains(MousePointerOffset) Then
            'Yes, the mouse pointer is on the save button.

            MovePointerOverExitButton()

        End If

        'Is the mouse pointer on the exit button?
        If ExitButton.Rect.Contains(MousePointerOffset) Then
            'Yes, the mouse pointer is on the exit button.

            MovePointerOverNewButton()

        End If

        'Is the mouse pointer on the new button?
        If NewButton.Rect.Contains(MousePointerOffset) Then
            'Yes, the mouse pointer is on the new button.

            MovePointerOverOpenButton()

        End If

        'Is the mouse pointer on the open button?
        If OpenButton.Rect.Contains(MousePointerOffset) Then
            'Yes, the mouse pointer is on the open button.

            MovePointerOverSaveButton()

        End If

    End Sub

    Private Sub DoMenuLogicDown()

        Dim MousePointerOffset As Point = MousePointer.Rect.Location

        MousePointerOffset.X -= ScreenOffset.X
        MousePointerOffset.Y -= ScreenOffset.Y

        'Is the mouse pointer over any button?
        If Not OpenButton.Rect.Contains(MousePointerOffset) AndAlso
               Not NewButton.Rect.Contains(MousePointerOffset) AndAlso
               Not ExitButton.Rect.Contains(MousePointerOffset) AndAlso
               Not SaveButton.Rect.Contains(MousePointerOffset) Then
            'No, the mouse pointer is not over any button.

            MovePointerOverSaveButton()

        End If

        'Is the mouse pointer on the save button?
        If SaveButton.Rect.Contains(MousePointerOffset) Then
            'Yes, the mouse pointer is on the save button.

            MovePointerOverOpenButton()

        End If

        'Is the mouse pointer on the open button?
        If OpenButton.Rect.Contains(MousePointerOffset) Then
            'Yes, the mouse pointer is on the open button.

            MovePointerOverNewButton()

        End If

        'Is the mouse pointer on the new button?
        If NewButton.Rect.Contains(MousePointerOffset) Then
            'Yes, the mouse pointer is on the new button.

            MovePointerOverExitButton()

        End If

        'Is the mouse pointer on the exit button?
        If ExitButton.Rect.Contains(MousePointerOffset) Then
            'Yes, the mouse pointer is on the exit button.

            MovePointerOverSaveButton()

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

                    MovePointerOverNewButton()

                    If IsMouseDown = False Then

                        DoMouseLeftDown()

                        IsMouseDown = True

                    End If

                Else

                    If RightTriggerDown = False Then

                        RightTriggerDown = True

                        SelectNextToolToTheRight()

                    End If

                End If

            End If

            If GameState = AppState.Start Then

                If ShowOpenFileDialog = False Then

                    MovePointerToStartScreenOpenButton()

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

                        SelectNextToolToTheLeft()

                    End If

                End If

            End If

            If GameState = AppState.Start Then

                If ShowOpenFileDialog = False Then

                    MovePointerToStartScreenNewButton()

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

            DoRightThumbstickLeftLogic()

        ElseIf ControllerPosition.Gamepad.sThumbRX >= NeutralEnd Then
            'The right thumbstick is in the right position.

            DoRightThumbstickRightLogic()

        Else
            'The right thumbstick is in the neutral position on the X-axis.

            DoRightThumbstickXAxisNeutralLogic()

        End If

        'What position is the right thumbstick in on the Y-axis?
        If ControllerPosition.Gamepad.sThumbRY <= NeutralStart Then
            'The right thumbstick is in the down position.

            DoRightThumbstickDownLogic()

        ElseIf ControllerPosition.Gamepad.sThumbRY >= NeutralEnd Then
            'The right thumbstick is in the up position.

            DoRightThumbstickUpLogic()

        Else
            'The right thumbstick is in the neutral position on the Y-axis.

            DoRightThumbstickYAxisNeutralLogic()

        End If

    End Sub

    Private Sub DoRightThumbstickUpLogic()

        If GameState = AppState.Editing Then

            'Is the menu open?
            If ShowMenu = False Then
                'No, the menu is not open.

                MoveCameraUp()

            Else
                'Yes, the menu is open.

                'Are dialog windows open?
                If ShowSaveWarning = True Or ShowSaveFileDialog = True Then
                    'Yes, dialog windows are open.

                    RightThumbstickUp = True

                    MovePointerUp()

                Else
                    'No, dialog windows are not open.

                    DoRightThumbstickUpMenuLogic()

                End If

            End If

        End If

        If GameState = AppState.Start Then

            'Is the open file dialog open?
            If ShowOpenFileDialog = False Then
                'No, the open file dialog is not open.

                MovePointerToStartScreenNewButton()

            End If

        End If

    End Sub

    Private Sub DoRightThumbstickDownLogic()

        If GameState = AppState.Editing Then

            'Is the menu open?
            If ShowMenu = False Then
                'No, the menu is not open.

                MoveCameraDown()

            Else
                'Yes, the menu is open.

                'Are dialog windows open?
                If ShowSaveWarning = True Or ShowSaveFileDialog = True Then
                    'Yes, dialog windows are open.

                    RightThumbstickDown = True

                    MovePointerDown()

                Else
                    'No, dialog windows are not open.

                    DoRightThumbstickDownMenuLogic()

                End If

            End If

        End If

        If GameState = AppState.Start Then

            'Is the open file dialog open?
            If ShowOpenFileDialog = False Then
                'No, the open file dialog is not open.

                MovePointerToStartScreenOpenButton()

            End If

        End If

    End Sub

    Private Sub DoRightThumbstickYAxisNeutralLogic()

        RightThumbstickUp = False

        RightThumbstickDown = False

        If GameState = AppState.Editing Then

            If UpArrowDown = False And DownArrowDown = False Then

                DecelerateCameraYAxis()

            End If

        End If

    End Sub

    Private Sub DoRightThumbstickLeftLogic()

        RightThumbstickLeft = True

        If GameState = AppState.Editing Then

            If ShowMenu = False Then

                MoveCameraLeft()

            Else

                If ShowSaveWarning = True Or ShowSaveFileDialog = True Then

                    MovePointerLeft()

                End If

            End If

        End If

        If GameState = AppState.Start Then

            If ShowOpenFileDialog = False Then

                MovePointerToStartScreenNewButton()

            End If

        End If

    End Sub

    Private Sub DoRightThumbstickRightLogic()

        RightThumbstickRight = True

        If GameState = AppState.Editing Then

            If ShowMenu = False Then

                MoveCameraRight()

            Else

                If ShowSaveWarning = True Or ShowSaveFileDialog = True Then

                    MovePointerRight()

                End If

            End If

        End If

        If GameState = AppState.Start Then

            If ShowOpenFileDialog = False Then

                MovePointerToStartScreenOpenButton()

            End If

        End If

    End Sub

    Private Sub DoRightThumbstickXAxisNeutralLogic()

        RightThumbstickLeft = False

        RightThumbstickRight = False

        If GameState = AppState.Editing Then

            If ShowMenu = False Then

                If LeftArrowDown = False AndAlso RightArrowDown = False Then

                    DecelerateCameraXAxis()

                End If

            End If

        End If

    End Sub

    Private Sub DoRightThumbstickUpMenuLogic()

        If RightThumbstickUp = False Then

            RightThumbstickUp = True

            DoMenuLogicUp()


            'Dim MousePointerOffset As Point = MousePointer.Rect.Location

            ''Convert from screen to client.
            'MousePointerOffset.X -= ScreenOffset.X
            'MousePointerOffset.Y -= ScreenOffset.Y

            ''Is the mouse pointer over any button?
            'If Not OpenButton.Rect.Contains(MousePointerOffset) AndAlso
            '   Not NewButton.Rect.Contains(MousePointerOffset) AndAlso
            '   Not ExitButton.Rect.Contains(MousePointerOffset) AndAlso
            '   Not SaveButton.Rect.Contains(MousePointerOffset) Then
            '    'No, the mouse pointer is not over any button.

            '    MovePointerOverSaveButton()

            'End If

            ''Is the mouse pointer on the save button?
            'If SaveButton.Rect.Contains(MousePointerOffset) Then
            '    'Yes, the mouse pointer is on the save button.

            '    MovePointerOverExitButton()

            'End If

            ''Is the mouse pointer on the exit button?
            'If ExitButton.Rect.Contains(MousePointerOffset) Then
            '    'Yes, the mouse pointer is on the exit button.

            '    MovePointerOverNewButton()

            'End If

            ''Is the mouse pointer on the new button?
            'If NewButton.Rect.Contains(MousePointerOffset) Then
            '    'Yes, the mouse pointer is on the new button.

            '    MovePointerOverOpenButton()

            'End If

            ''Is the mouse pointer on the open button?
            'If OpenButton.Rect.Contains(MousePointerOffset) Then
            '    'Yes, the mouse pointer is on the open button.

            '    MovePointerOverSaveButton()

            'End If

        End If

    End Sub

    Private Sub DoRightThumbstickDownMenuLogic()

        If RightThumbstickDown = False Then

            RightThumbstickDown = True


            DoMenuLogicDown()


            'Dim MousePointerOffset As Point = MousePointer.Rect.Location

            ''Convert from screen to client.
            'MousePointerOffset.X -= ScreenOffset.X
            'MousePointerOffset.Y -= ScreenOffset.Y

            ''Is the mouse pointer over any button?
            'If Not OpenButton.Rect.Contains(MousePointerOffset) AndAlso
            '   Not NewButton.Rect.Contains(MousePointerOffset) AndAlso
            '   Not ExitButton.Rect.Contains(MousePointerOffset) AndAlso
            '   Not SaveButton.Rect.Contains(MousePointerOffset) Then
            '    'No, the mouse pointer is not over any button.

            '    MovePointerOverSaveButton()

            'End If

            ''Is the mouse pointer on the save button?
            'If SaveButton.Rect.Contains(MousePointerOffset) Then
            '    'Yes, the mouse pointer is on the save button.

            '    MovePointerOverOpenButton()

            'End If

            ''Is the mouse pointer on the open button?
            'If OpenButton.Rect.Contains(MousePointerOffset) Then
            '    'Yes, the mouse pointer is on the open button.

            '    MovePointerOverNewButton()

            'End If

            ''Is the mouse pointer on the new button?
            'If NewButton.Rect.Contains(MousePointerOffset) Then
            '    'Yes, the mouse pointer is on the new button.

            '    MovePointerOverExitButton()

            'End If

            ''Is the mouse pointer on the exit button?
            'If ExitButton.Rect.Contains(MousePointerOffset) Then
            '    'Yes, the mouse pointer is on the exit button.

            '    MovePointerOverSaveButton()

            'End If

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

    Private Sub DecelerateCameraXAxis()

        If Camera.Velocity.X < 0 Then

            'Decelerate camera.
            Camera.Velocity.X += Camera.Acceleration.X * 8 * EditorDeltaTime.TotalSeconds

            'Limit decelerate to zero speed.
            If Camera.Velocity.X > 0 Then

                Camera.Velocity.X = 0 'Zero speed.

            End If

        End If

        If Camera.Velocity.X > 0 Then

            'Decelerate camera.
            Camera.Velocity.X += -Camera.Acceleration.X * 8 * EditorDeltaTime.TotalSeconds

            'Limit decelerate to zero speed.
            If Camera.Velocity.X < 0 Then

                Camera.Velocity.X = 0 'Zero speed.

            End If

        End If

    End Sub

    Private Sub DecelerateCameraYAxis()

        'Is the camera moving up?
        If Camera.Velocity.Y < 0 Then
            'Yes, the camera is moving up.

            'Decelerate camera.
            Camera.Velocity.Y += Camera.Acceleration.Y * 8 * EditorDeltaTime.TotalSeconds

            'Limit decelerate to zero speed.
            If Camera.Velocity.Y > 0 Then

                Camera.Velocity.Y = 0 'Zero speed.

            End If

        End If

        'Is the camera moving down?
        If Camera.Velocity.Y > 0 Then
            'Yes, the camera is moving down.

            'Decelerate camera.
            Camera.Velocity.Y += -Camera.Acceleration.Y * 8 * EditorDeltaTime.TotalSeconds

            'Limit decelerate to zero speed.
            If Camera.Velocity.Y < 0 Then

                Camera.Velocity.Y = 0 'Zero speed.

            End If

        End If

    End Sub

    Private Sub UpdateHeroPosition()

        Hero.Rect.X = Math.Round(Hero.Position.X)

        Hero.Rect.Y = Math.Round(Hero.Position.Y)

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

            If Hero.Rect.IntersectsWith(Camera.Rect) Then

                Dim RectOffset As Rectangle = Hero.Rect

                RectOffset.Offset(CameraOffset)

                'Bug Fix Don't Change.
                .CompositingMode = CompositingMode.SourceOver
                'To fix draw string error with anti aliasing: "Parameters not valid."
                'Set the compositing mode to source over.

                .FillRectangle(Brushes.Red, RectOffset)

                .DrawString("Hero", CWJFont, Brushes.White, RectOffset, AlineCenterMiddle)

                'Draw hero position
                .DrawString("X: " & Hero.Position.X.ToString & vbCrLf & "Y: " & Hero.Position.Y.ToString,
                        CWJFont,
                        Brushes.White,
                        RectOffset.X,
                        RectOffset.Y - 50,
                        New StringFormat With {.Alignment = StringAlignment.Near})

                .CompositingMode = CompositingMode.SourceCopy

            End If

        End With

    End Sub

    Private Sub DrawSpawn()

        With Buffer.Graphics

            If Spawn.Rect.IntersectsWith(Camera.Rect) Then

                'Bug Fix Don't Change.
                .CompositingMode = CompositingMode.SourceOver
                'To fix draw string error with anti aliasing: "Parameters not valid."
                'Set the compositing mode to source over.

                Dim RectOffset As Rectangle = Spawn.Rect

                RectOffset.Offset(CameraOffset)

                .FillRectangle(SpawnBrush, RectOffset)

                .DrawString("Start", SpawnFont, Brushes.White, RectOffset, AlineCenterMiddle)

                .CompositingMode = CompositingMode.SourceCopy

            End If

        End With

    End Sub

    Private Sub DrawEnemies()

        With Buffer.Graphics

            If Enemies IsNot Nothing Then

                'Bug Fix Don't Change.
                .CompositingMode = CompositingMode.SourceOver
                'To fix draw string error with anti aliasing: "Parameters not valid."
                'Set the compositing mode to source over.

                For Each Enemy In Enemies

                    Select Case GameState

                        Case AppState.Playing

                            If Enemy.Eliminated = False Then

                                If Enemy.Rect.IntersectsWith(Camera.Rect) Then

                                    Dim RectOffset As Rectangle = Enemy.Rect

                                    RectOffset.Offset(CameraOffset)

                                    .FillRectangle(Brushes.Chocolate, RectOffset)

                                    .DrawString("E", EnemyFont, Brushes.PaleGoldenrod, RectOffset, AlineCenterMiddle)

                                End If

                            End If

                        Case AppState.Editing

                            'Draw Start of Patrol
                            Dim PatrolAOffset As New Rectangle(New Point(Enemy.PatrolA.X, Enemy.PatrolA.Y), New Drawing.Size(GridSize, GridSize))

                            If PatrolAOffset.IntersectsWith(Camera.Rect) Then

                                PatrolAOffset.Offset(CameraOffset)

                                .FillRectangle(Brushes.Chocolate, PatrolAOffset)

                                .DrawString("E", EnemyFont, Brushes.PaleGoldenrod, PatrolAOffset, AlineCenterMiddle)

                            End If

                            'Draw End of Patrol
                            Dim PatrolBOffset As New Rectangle(New Point(Enemy.PatrolB.X, Enemy.PatrolB.Y), New Drawing.Size(GridSize, GridSize))

                            If PatrolBOffset.IntersectsWith(Camera.Rect) Then

                                PatrolBOffset.Offset(CameraOffset)

                                .FillRectangle(Brushes.PaleGoldenrod, PatrolBOffset)

                                .DrawString("E", EnemyFont, Brushes.Chocolate, PatrolBOffset, AlineCenterMiddle)

                            End If

                            'Draw span of patrol.
                            Dim SpanWidth As Integer = Enemy.PatrolB.X - Enemy.PatrolA.X - GridSize

                            Dim SpanOffset As New Rectangle(New Point(Enemy.PatrolA.X + GridSize, Enemy.PatrolA.Y), New Drawing.Size(SpanWidth, GridSize))

                            If SpanOffset.IntersectsWith(Camera.Rect) Then

                                SpanOffset.Offset(CameraOffset)

                                .FillRectangle(Brushes.PaleGoldenrod, SpanOffset)

                            End If

                    End Select

                Next

                .CompositingMode = CompositingMode.SourceCopy

            End If

        End With

    End Sub

    Private Sub DrawGoal()

        If Buffer.Graphics IsNot Nothing Then

            With Buffer.Graphics

                If Goal.Rect.IntersectsWith(Camera.Rect) Then

                    Dim RectOffset As Rectangle = Goal.Rect

                    RectOffset.Offset(CameraOffset)

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

                    'Bug Fix Don't Change.
                    .CompositingMode = CompositingMode.SourceOver
                    'To fix draw string error with anti aliasing: "Parameters not valid."
                    'Set the compositing mode to source over.

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

                    If GameState = AppState.Editing Then

                        Dim ShadowOffset As Rectangle = RectOffset

                        ShadowOffset.Offset(2, 2)

                        .DrawString("End", SpawnFont, Brushes.White, ShadowOffset, AlineCenterMiddle)

                        .DrawString("End", SpawnFont, Brushes.Black, RectOffset, AlineCenterMiddle)

                    End If

                    .CompositingMode = CompositingMode.SourceCopy

                End If

            End With

        End If

    End Sub

    Private Sub DrawPortals()

        With Buffer.Graphics

            If Portals IsNot Nothing Then

                'Bug Fix Don't Change.
                .CompositingMode = CompositingMode.SourceOver
                'To fix draw string error with anti aliasing: "Parameters not valid."
                'Set the compositing mode to source over.

                For Each Portal In Portals

                    Dim RectOffset As Rectangle = Portal.Rect

                    RectOffset.Offset(CameraOffset)

                    Select Case GameState

                        Case AppState.Playing

                            If Portal.Rect.IntersectsWith(Camera.Rect) Then

                                'Draw portal entrance
                                Dim PortalEntrance As New Rectangle(New Point(Portal.PatrolA.X, Portal.PatrolA.Y), New Drawing.Size(GridSize, GridSize))
                                Dim PortalEntranceOffset As Rectangle = PortalEntrance
                                PortalEntranceOffset.Offset(CameraOffset)

                                If Hero.Rect.IntersectsWith(PortalEntrance) = True Then

                                    Dim ControllerHintOffset As Rectangle = PortalEntranceOffset

                                    ControllerHintOffset.Offset(16, -40)
                                    ControllerHintOffset.Width = 32
                                    ControllerHintOffset.Height = 32

                                    Dim ControllerHintTextOffset As Rectangle = ControllerHintOffset

                                    .FillRectangle(Brushes.White, ControllerHintOffset)

                                    .DrawString("é", ControllerHintFontSmall, Brushes.Black, ControllerHintTextOffset, AlineCenterMiddle)

                                End If

                                DrawPortal(PortalEntranceOffset)

                            End If

                            'Draw portal exit
                            Dim PortalExitOffset As New Rectangle(New Point(Portal.PatrolB.X, Portal.PatrolB.Y), New Drawing.Size(GridSize, GridSize))

                            If PortalExitOffset.IntersectsWith(Camera.Rect) Then

                                PortalExitOffset.Offset(CameraOffset)

                                DrawPortalExit(PortalExitOffset)

                            End If

                        Case AppState.Editing

                            'Draw portal entrance
                            Dim PortalEntranceOffset As New Rectangle(New Point(Portal.PatrolA.X, Portal.PatrolA.Y), New Drawing.Size(GridSize, GridSize))

                            If PortalEntranceOffset.IntersectsWith(Camera.Rect) Then

                                PortalEntranceOffset.Offset(CameraOffset)

                                DrawPortal(PortalEntranceOffset)

                                .DrawString(Array.IndexOf(Portals, Portal).ToString, PortalFont, Brushes.White, PortalEntranceOffset, AlineCenterMiddle)

                            End If

                            'Draw portal exit
                            Dim PortalExitOffset As New Rectangle(New Point(Portal.PatrolB.X, Portal.PatrolB.Y), New Drawing.Size(GridSize, GridSize))

                            If PortalExitOffset.IntersectsWith(Camera.Rect) Then

                                PortalExitOffset.Offset(CameraOffset)

                                DrawPortalExit(PortalExitOffset)

                                .DrawString(Array.IndexOf(Portals, Portal).ToString, PortalFont, Brushes.White, PortalExitOffset, AlineCenterMiddle)

                            End If

                    End Select

                Next

                .CompositingMode = CompositingMode.SourceCopy

            End If

        End With

    End Sub

    Private Sub DrawPortal(Rect As Rectangle)

        With Buffer.Graphics

            .FillRectangle(Brushes.Indigo, Rect)

            ' Define the rectangle to be filled
            Dim InflatedRect As RectangleF = Rect

            'rect.Inflate(rect.Width / 6.4F, rect.Height / 6.4F)
            InflatedRect.Inflate(15, 15)

            ' Define the center point of the gradient
            Dim center As New PointF(InflatedRect.Left + InflatedRect.Width / 2.0F, InflatedRect.Top + InflatedRect.Height / 2.0F)

            ' Define the colors for the gradient stops
            Dim colors() As Color = {Color.Cyan, Color.Indigo}

            ' Create the path for the gradient brush
            Dim GradPath As New GraphicsPath()
            GradPath.AddEllipse(InflatedRect)

            ' Create the gradient brush
            Dim GradBrush As New PathGradientBrush(GradPath) With
                {.CenterPoint = center,
                .CenterColor = colors(0),
                .SurroundColors = New Color() {colors(1)}}

            .FillRectangle(GradBrush, Rect)

            Dim DoorWay As Rectangle

            DoorWay.X = Rect.X + 20
            DoorWay.Y = Rect.Y + 8

            DoorWay.Width = 26
            DoorWay.Height = 48

            .FillRectangle(Brushes.Black, DoorWay)

        End With

    End Sub

    Private Sub DrawPortalExit(Rect As Rectangle)

        With Buffer.Graphics

            .FillRectangle(Brushes.Indigo, Rect)

            ' Define the rectangle to be filled
            Dim InflatedRect As RectangleF = Rect

            'rect.Inflate(rect.Width / 6.4F, rect.Height / 6.4F)
            InflatedRect.Inflate(15, 15)

            ' Define the center point of the gradient
            Dim center As New PointF(InflatedRect.Left + InflatedRect.Width / 2.0F, InflatedRect.Top + InflatedRect.Height / 2.0F)

            ' Define the colors for the gradient stops
            Dim colors() As Color = {Color.LightPink, Color.Maroon}

            ' Create the path for the gradient brush
            Dim GradPath As New GraphicsPath()
            GradPath.AddEllipse(InflatedRect)

            ' Create the gradient brush
            Dim GradBrush As New PathGradientBrush(GradPath) With
                {.CenterPoint = center,
                .CenterColor = colors(0),
                .SurroundColors = New Color() {colors(1)}}

            .FillRectangle(GradBrush, Rect)

            Dim DoorWay As Rectangle

            DoorWay.X = Rect.X + 20
            DoorWay.Y = Rect.Y + 8

            DoorWay.Width = 26
            DoorWay.Height = 48

            .FillRectangle(Brushes.Black, DoorWay)

        End With

    End Sub

    Private Sub DrawBackdrops()

        With Buffer.Graphics

            If Backdrops IsNot Nothing Then

                For Each Backdrop In Backdrops

                    If Backdrop.Rect.IntersectsWith(Camera.Rect) Then

                        Dim RectOffset As Rectangle = Backdrop.Rect

                        RectOffset.Offset(CameraOffset)

                        If RectOffset.IntersectsWith(ClientRectangle) Then

                            .FillRectangle(New SolidBrush(Color.FromArgb(Backdrop.Color)), RectOffset)

                        End If

                    End If

                Next

            End If

        End With

    End Sub

    Private Sub DrawSelectionAndSizingHandle()


        If GameState = AppState.Editing Then

            Dim RectOffset As Rectangle

            If SelectedPortal > -1 Then

                If PortalEntranceSelected = True Then

                    Dim PortalEntranceOffset As New Rectangle(New Point(Portals(SelectedPortal).PatrolA.X, Portals(SelectedPortal).PatrolA.Y), New Drawing.Size(GridSize, GridSize))

                    PortalEntranceOffset.Offset(CameraOffset)

                    DrawSelectionRectangle(PortalEntranceOffset, Buffer.Graphics)

                Else

                    Dim PortalExitOffset As New Rectangle(New Point(Portals(SelectedPortal).PatrolB.X, Portals(SelectedPortal).PatrolB.Y), New Drawing.Size(GridSize, GridSize))

                    PortalExitOffset.Offset(CameraOffset)

                    DrawSelectionRectangle(PortalExitOffset, Buffer.Graphics)

                End If

            End If

            If SelectedEnemy > -1 Then

                Dim SelectionSize As New Size(Enemies(SelectedEnemy).PatrolB.X + GridSize - Enemies(SelectedEnemy).PatrolA.X, GridSize)

                RectOffset = New Rectangle(New Point(Enemies(SelectedEnemy).PatrolA.X, Enemies(SelectedEnemy).PatrolA.Y), SelectionSize)

                RectOffset.Offset(CameraOffset)

                DrawSelectionRectangle(RectOffset, Buffer.Graphics)

                'Position sizing handle.
                SizingHandle.X = RectOffset.Right - SizingHandle.Width \ 2
                SizingHandle.Y = RectOffset.Bottom - SizingHandle.Height \ 2

                DrawSizingHandle(RectOffset, Buffer.Graphics)

            End If

            If SelectedBackdrop > -1 Then

                RectOffset = Backdrops(SelectedBackdrop).Rect

                RectOffset.Offset(CameraOffset)

                DrawSelectionRectangle(RectOffset, Buffer.Graphics)

                'Position sizing handle.
                SizingHandle.X = RectOffset.Right - SizingHandle.Width \ 2
                SizingHandle.Y = RectOffset.Bottom - SizingHandle.Height \ 2

                DrawSizingHandle(RectOffset, Buffer.Graphics)

            End If

            If SelectedBush > -1 Then

                RectOffset = Bushes(SelectedBush).Rect

                RectOffset.Offset(CameraOffset)

                DrawSelectionRectangle(RectOffset, Buffer.Graphics)

                'Position sizing handle.
                SizingHandle.X = RectOffset.Right - SizingHandle.Width \ 2
                SizingHandle.Y = RectOffset.Bottom - SizingHandle.Height \ 2

                DrawSizingHandle(RectOffset, Buffer.Graphics)

            End If

            If SpawnSelected = True Then

                RectOffset = Spawn.Rect

                RectOffset.Offset(CameraOffset)

                DrawSelectionRectangle(RectOffset, Buffer.Graphics)

            End If

            If SelectedBlock > -1 Then

                RectOffset = Blocks(SelectedBlock).Rect

                RectOffset.Offset(CameraOffset)

                DrawSelectionRectangle(RectOffset, Buffer.Graphics)

                'Position sizing handle.
                SizingHandle.X = RectOffset.Right - SizingHandle.Width \ 2
                SizingHandle.Y = RectOffset.Bottom - SizingHandle.Height \ 2

                DrawSizingHandle(RectOffset, Buffer.Graphics)

            End If

            If GoalSelected = True Then

                RectOffset = Goal.Rect

                RectOffset.Offset(CameraOffset)

                DrawSelectionRectangle(RectOffset, Buffer.Graphics)

                'Position sizing handle.
                SizingHandle.X = RectOffset.Right - SizingHandle.Width \ 2
                SizingHandle.Y = RectOffset.Bottom - SizingHandle.Height \ 2

                DrawSizingHandle(RectOffset, Buffer.Graphics)

            End If

            If SelectedCloud > -1 Then

                RectOffset = Clouds(SelectedCloud).Rect

                RectOffset.Offset(CameraOffset)

                DrawSelectionRectangle(RectOffset, Buffer.Graphics)

                'Position sizing handle.
                SizingHandle.X = RectOffset.Right - SizingHandle.Width \ 2
                SizingHandle.Y = RectOffset.Bottom - SizingHandle.Height \ 2

                DrawSizingHandle(RectOffset, Buffer.Graphics)

            End If

            If SelectedBill > -1 Then

                RectOffset = Cash(SelectedBill).Rect

                RectOffset.Offset(CameraOffset)

                DrawSelectionRectangle(RectOffset, Buffer.Graphics)

            End If

        End If

    End Sub

    Private Sub DrawSelectionRectangle(Rect As Rectangle, Grap As Graphics)

        Grap.CompositingMode = CompositingMode.SourceOver

        Grap.SmoothingMode = SmoothingMode.AntiAlias

        'Draw selection rectangle outline.
        DrawRoundedRectangle(SelectionRectangleOutlinePen, Rect, 5, Grap)

        Grap.SmoothingMode = SmoothingMode.None

        'Draw dash selection rectangle.
        Grap.DrawRectangle(SelectionRectangleDashPen, Rect)

        Grap.CompositingMode = CompositingMode.SourceCopy

    End Sub

    Private Sub DrawSizingHandle(Rect As Rectangle, Grap As Graphics)

        Grap.CompositingMode = CompositingMode.SourceOver

        'Draw sizing handle center.
        FillRoundedRectangle(Brushes.White, SizingHandle, 5, Grap)

        Grap.SmoothingMode = SmoothingMode.AntiAlias

        'Draw sizing handle outer outline.
        DrawRoundedRectangle(SizingHandleOuterOutlinePen, SizingHandle, 5, Grap)

        'Draw sizing handle inner outline.
        DrawRoundedRectangle(SizingHandleInnerOutlinePen, SizingHandle, 5, Grap)

        Grap.CompositingMode = CompositingMode.SourceCopy

        Grap.SmoothingMode = SmoothingMode.None

    End Sub

    Private Sub DrawBlocks()

        With Buffer.Graphics

            If Blocks IsNot Nothing Then

                .PixelOffsetMode = PixelOffsetMode.HighQuality

                For Each Block In Blocks

                    If Block.Rect.IntersectsWith(Camera.Rect) Then

                        Dim RectOffset As Rectangle = Block.Rect

                        RectOffset.Offset(CameraOffset)

                        .FillRectangle(Brushes.Chocolate, RectOffset)

                        .DrawLine(Pens.White,
                              RectOffset.Right,
                              RectOffset.Top + 1,
                              RectOffset.Left,
                              RectOffset.Top + 1)

                    End If

                Next

                .PixelOffsetMode = PixelOffsetMode.None

            End If

        End With

    End Sub

    Private Sub DrawBushes()

        With Buffer.Graphics

            If Bushes IsNot Nothing Then

                For Each Bush In Bushes

                    If Bush.Rect.IntersectsWith(Camera.Rect) Then

                        Dim RectOffset As Rectangle = Bush.Rect

                        RectOffset.Offset(CameraOffset)

                        .FillRectangle(Brushes.GreenYellow, RectOffset)

                        .DrawLine(SeaGreenPen,
                                  RectOffset.Right - 10,
                                  RectOffset.Top + 10,
                                  RectOffset.Right - 10,
                                  RectOffset.Bottom - 10)

                        .DrawLine(SeaGreenPen,
                                  RectOffset.Left + 10,
                                  RectOffset.Bottom - 10,
                                  RectOffset.Right - 10,
                                  RectOffset.Bottom - 10)

                        .DrawRectangle(OutinePen, RectOffset)

                    End If

                Next

            End If

        End With

    End Sub

    Private Sub DrawClouds()

        With Buffer.Graphics

            If Clouds IsNot Nothing Then

                For Each Cloud In Clouds

                    If Cloud.Rect.IntersectsWith(Camera.Rect) Then

                        Dim RectOffset As Rectangle = Cloud.Rect

                        RectOffset.Offset(CameraOffset)

                        .FillRectangle(Brushes.White, RectOffset)

                        .DrawLine(LightSkyBluePen,
                              RectOffset.Right - 10,
                              RectOffset.Top + 10,
                              RectOffset.Right - 10,
                              RectOffset.Bottom - 10)

                        .DrawLine(LightSkyBluePen,
                              RectOffset.Left + 10,
                              RectOffset.Bottom - 10,
                              RectOffset.Right - 10,
                              RectOffset.Bottom - 10)

                        .DrawRectangle(OutinePen, RectOffset)

                    End If

                Next

            End If

        End With

    End Sub

    Private Sub DrawCash()

        With Buffer.Graphics

            If Cash IsNot Nothing Then

                'Bug Fix Don't Change.
                .CompositingMode = CompositingMode.SourceOver
                'To fix draw string error with anti aliasing: "Parameters not valid."
                'Set the compositing mode to source over.

                For Each Bill In Cash

                    'Is the bill in the the frame?
                    If Bill.Rect.IntersectsWith(Camera.Rect) Then
                        'Yes, the bill is in the frame.

                        Dim RectOffset As Rectangle = Bill.Rect

                        RectOffset.Offset(CameraOffset)

                        Select Case GameState

                            Case AppState.Start

                                If Bill.Collected = False Then

                                    .FillRectangle(Brushes.Goldenrod, RectOffset)

                                    .DrawString("$", BillFont, Brushes.OrangeRed, RectOffset, AlineCenterMiddle)

                                End If

                            Case AppState.Playing

                                If Bill.Collected = False Then

                                    .FillRectangle(Brushes.Goldenrod, RectOffset)

                                    .DrawString("$", BillFont, Brushes.OrangeRed, RectOffset, AlineCenterMiddle)

                                End If

                            Case AppState.Editing

                                .FillRectangle(Brushes.Goldenrod, RectOffset)

                                .DrawString("$", BillFont, Brushes.OrangeRed, RectOffset, AlineCenterMiddle)

                        End Select

                    End If

                Next

                .CompositingMode = CompositingMode.SourceCopy

            End If

        End With

    End Sub

    Private Sub DrawCollectedCash()

        With Buffer.Graphics

            'Bug Fix Don't Change.
            .CompositingMode = CompositingMode.SourceOver
            'To fix draw string error with anti aliasing: "Parameters not valid."
            'Set the compositing mode to source over.

            'Draw drop shadow.
            .DrawString("$" & CashCollected.ToString,
                    FPSFont,
                    Brushes.Black,
                    CashCollectedPostion.X + 2,
                    CashCollectedPostion.Y + 2)

            .DrawString("$" & CashCollected.ToString,
                        FPSFont,
                        Brushes.White,
                        CashCollectedPostion)

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub DrawToolPreviewBackdrop()

        With Buffer.Graphics

            If ShowToolPreview = True AndAlso ShowMenu = False Then

                Dim RectOffset As Rectangle = ToolPreview

                RectOffset.Offset(CameraOffset)

                If SelectedTool = Tools.Backdrop Then

                    .FillRectangle(Brushes.Black, RectOffset)

                End If

            End If

        End With

    End Sub

    Private Sub DrawToolPreview()

        With Buffer.Graphics

            If ShowToolPreview = True AndAlso ShowMenu = False Then

                Dim RectOffset As Rectangle = ToolPreview

                RectOffset.Offset(CameraOffset)

                Select Case SelectedTool

                    Case Tools.Block

                        .CompositingMode = CompositingMode.SourceCopy

                        .PixelOffsetMode = PixelOffsetMode.HighQuality

                        .FillRectangle(Brushes.Chocolate, RectOffset)

                        .DrawLine(Pens.White,
                                  RectOffset.Right,
                                  RectOffset.Top + 1,
                                  RectOffset.Left,
                                  RectOffset.Top + 1)

                        .PixelOffsetMode = PixelOffsetMode.None

                    Case Tools.Bill

                        .FillRectangle(Brushes.Goldenrod, RectOffset)

                        .CompositingMode = CompositingMode.SourceOver

                        .DrawString("$", BillFont, Brushes.OrangeRed, RectOffset, AlineCenterMiddle)

                        .CompositingMode = CompositingMode.SourceCopy

                    Case Tools.Cloud

                        .FillRectangle(Brushes.White, RectOffset)

                        .DrawLine(LightSkyBluePen,
                                  RectOffset.Right - 10,
                                  RectOffset.Top + 10,
                                  RectOffset.Right - 10,
                                  RectOffset.Bottom - 10)

                        .DrawLine(LightSkyBluePen,
                                  RectOffset.Left + 10,
                                  RectOffset.Bottom - 10,
                                  RectOffset.Right - 10,
                                  RectOffset.Bottom - 10)

                        .DrawRectangle(OutinePen, RectOffset)

                    Case Tools.Bush

                        .FillRectangle(Brushes.GreenYellow, RectOffset)

                        .DrawLine(SeaGreenPen,
                                  RectOffset.Right - 10,
                                  RectOffset.Top + 10,
                                  RectOffset.Right - 10,
                                  RectOffset.Bottom - 10)

                        .DrawLine(SeaGreenPen,
                                  RectOffset.Left + 10,
                                  RectOffset.Bottom - 10,
                                  RectOffset.Right - 10,
                                  RectOffset.Bottom - 10)

                        .DrawRectangle(OutinePen, RectOffset)

                    Case Tools.Enemy

                        .FillRectangle(Brushes.Chocolate, RectOffset)

                        .CompositingMode = CompositingMode.SourceOver

                        .DrawString("E", EnemyFont, Brushes.PaleGoldenrod, RectOffset, AlineCenterMiddle)

                        Dim PatrolB As New Rectangle(RectOffset.X + GridSize, RectOffset.Y, GridSize, GridSize)

                        .FillRectangle(Brushes.PaleGoldenrod, PatrolB)

                        .DrawString("E",
                                    EnemyFont,
                                    Brushes.Chocolate,
                                    PatrolB,
                                    AlineCenterMiddle)

                        .CompositingMode = CompositingMode.SourceCopy

                    Case Tools.Goal

                        .CompositingMode = CompositingMode.SourceOver

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

                        .CompositingMode = CompositingMode.SourceCopy

                    Case Tools.Portal

                        DrawPortal(RectOffset)

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

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            FillRoundedRectangle(MenuShadowBrush, Shadow, 30, Buffer.Graphics)

            FillRoundedRectangle(Brushes.Black, MenuBackground.Rect, 30, Buffer.Graphics)

            .CompositingMode = CompositingMode.SourceCopy

            .SmoothingMode = SmoothingMode.None

        End With

    End Sub

    Private Sub DrawMenuOutline()

        With Buffer.Graphics

            Dim OutLineRect As Rectangle = MenuBackground.Rect

            OutLineRect.Inflate(2, 2)

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            DrawRoundedRectangle(MenuOutinePen, OutLineRect, 30, Buffer.Graphics)

            .CompositingMode = CompositingMode.SourceCopy

            .SmoothingMode = SmoothingMode.None

        End With

    End Sub

    Private Sub DrawPointerToolButton()

        With Buffer.Graphics

            Dim RoundedPointerToolButton As Rectangle = PointerToolButton.Rect

            RoundedPointerToolButton.Offset(2, 2)

            RoundedPointerToolButton.Width = PointerToolButton.Rect.Width - 4
            RoundedPointerToolButton.Height = PointerToolButton.Rect.Height - 4

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            If SelectedTool = Tools.Pointer Then

                If PointerToolButtonHover = True Then

                    FillRoundedRectangle(SelectedHoverBrush, RoundedPointerToolButton, 30, Buffer.Graphics)

                    .DrawLine(SelectionHighlightHoverPen,
                      RoundedPointerToolButton.Left + 11,
                      RoundedPointerToolButton.Bottom - 2,
                      RoundedPointerToolButton.Right - 11,
                      RoundedPointerToolButton.Bottom - 2)

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedPointerToolButton.Left
                    ToolTip.Y = RoundedPointerToolButton.Top - 30
                    ToolTip.Width = 75
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Pointer", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(SelectedBrush, RoundedPointerToolButton, 30, Buffer.Graphics)

                    .DrawLine(SelectionHighlightPen,
                      RoundedPointerToolButton.Left + 11,
                      RoundedPointerToolButton.Bottom - 2,
                      RoundedPointerToolButton.Right - 11,
                      RoundedPointerToolButton.Bottom - 2)

                End If

            Else

                If PointerToolButtonHover = True Then

                    FillRoundedRectangle(HoverBrush, RoundedPointerToolButton, 30, Buffer.Graphics)

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedPointerToolButton.Left
                    ToolTip.Y = RoundedPointerToolButton.Top - 30
                    ToolTip.Width = 75
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Pointer", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(Brushes.Black, RoundedPointerToolButton, 30, Buffer.Graphics)

                End If

            End If

            .SmoothingMode = SmoothingMode.None

            .DrawString("ë",
                        PointerToolFont,
                        Brushes.White,
                        New Rectangle(PointerToolButton.Rect.X,
                                      PointerToolButton.Rect.Y + 5,
                                      PointerToolButton.Rect.Width,
                                      PointerToolButton.Rect.Height),
                        AlineCenterMiddle)

            If SelectedTool = Tools.Block Then

                Dim ControllerHint As Rectangle

                ControllerHint.X = PointerToolButton.Rect.X + 18
                ControllerHint.Y = PointerToolButton.Rect.Y + 30
                ControllerHint.Width = PointerToolButton.Rect.Width
                ControllerHint.Height = PointerToolButton.Rect.Height

                Dim ControllerHintShadow As Rectangle = ControllerHint

                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            If SelectedTool = Tools.Portal Then

                Dim ControllerHint As Rectangle

                ControllerHint.X = PointerToolButton.Rect.X + 18
                ControllerHint.Y = PointerToolButton.Rect.Y + 30
                ControllerHint.Width = PointerToolButton.Rect.Width
                ControllerHint.Height = PointerToolButton.Rect.Height

                Dim ControllerHintShadow As Rectangle = ControllerHint

                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2

                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            .CompositingMode = CompositingMode.SourceCopy


        End With

    End Sub

    Private Sub DrawBlockToolButton()

        Dim RoundedBlockToolButton As Rectangle = BlockToolButton.Rect

        RoundedBlockToolButton.Offset(2, 2)

        RoundedBlockToolButton.Width = BlockToolButton.Rect.Width - 4
        RoundedBlockToolButton.Height = BlockToolButton.Rect.Height - 4

        With Buffer.Graphics

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            If SelectedTool = Tools.Block Then

                If BlockToolButtonHover = True Then

                    FillRoundedRectangle(SelectedHoverBrush, RoundedBlockToolButton, 30, Buffer.Graphics)

                    .DrawLine(SelectionHighlightHoverPen,
                              RoundedBlockToolButton.Left + 11,
                              RoundedBlockToolButton.Bottom - 2,
                              RoundedBlockToolButton.Right - 11,
                              RoundedBlockToolButton.Bottom - 2)

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedBlockToolButton.Left
                    ToolTip.Y = RoundedBlockToolButton.Top - 30
                    ToolTip.Width = 60
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Block", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(SelectedBrush, RoundedBlockToolButton, 30, Buffer.Graphics)

                    .DrawLine(SelectionHighlightPen,
                              RoundedBlockToolButton.Left + 11,
                              RoundedBlockToolButton.Bottom - 2,
                              RoundedBlockToolButton.Right - 11,
                              RoundedBlockToolButton.Bottom - 2)

                End If

            Else

                If BlockToolButtonHover = True Then

                    FillRoundedRectangle(HoverBrush, RoundedBlockToolButton, 30, Buffer.Graphics)

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedBlockToolButton.Left
                    ToolTip.Y = RoundedBlockToolButton.Top - 30
                    ToolTip.Width = 60
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Block", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(Brushes.Black, RoundedBlockToolButton, 30, Buffer.Graphics)

                End If

            End If

            .PixelOffsetMode = PixelOffsetMode.HighQuality

            .SmoothingMode = SmoothingMode.None

            .FillRectangle(Brushes.Chocolate, BlockToolIcon.Rect)

            .DrawLine(Pens.White,
                      BlockToolIcon.Rect.Right,
                      BlockToolIcon.Rect.Top + 1,
                      BlockToolIcon.Rect.Left,
                      BlockToolIcon.Rect.Top + 1)

            .PixelOffsetMode = PixelOffsetMode.None

            If SelectedTool = Tools.Pointer Then

                Dim ControllerHint As Rectangle

                ControllerHint.X = BlockToolButton.Rect.X + 18
                ControllerHint.Y = BlockToolButton.Rect.Y + 30
                ControllerHint.Width = BlockToolButton.Rect.Width
                ControllerHint.Height = BlockToolButton.Rect.Height

                Dim ControllerHintShadow As Rectangle = ControllerHint

                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2

                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            If SelectedTool = Tools.Bill Then

                Dim ControllerHint As Rectangle

                ControllerHint.X = BlockToolButton.Rect.X + 18
                ControllerHint.Y = BlockToolButton.Rect.Y + 30
                ControllerHint.Width = BlockToolButton.Rect.Width
                ControllerHint.Height = BlockToolButton.Rect.Height

                Dim ControllerHintShadow As Rectangle = ControllerHint

                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub DrawBackdropToolButton()

        Dim RoundedBackdropToolButton As Rectangle = BackdropToolButton.Rect

        RoundedBackdropToolButton.Offset(2, 2)

        RoundedBackdropToolButton.Width = BackdropToolButton.Rect.Width - 4
        RoundedBackdropToolButton.Height = BackdropToolButton.Rect.Height - 4

        With Buffer.Graphics

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            If SelectedTool = Tools.Backdrop Then

                If BackdropToolButtonHover = True Then

                    FillRoundedRectangle(SelectedHoverBrush, RoundedBackdropToolButton, 30, Buffer.Graphics)

                    .DrawLine(SelectionHighlightHoverPen,
                              RoundedBackdropToolButton.Left + 11,
                              RoundedBackdropToolButton.Bottom - 2,
                              RoundedBackdropToolButton.Right - 11,
                              RoundedBackdropToolButton.Bottom - 2)

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedBackdropToolButton.Left
                    ToolTip.Y = RoundedBackdropToolButton.Top - 30
                    ToolTip.Width = 98
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Backdrop", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(SelectedBrush, RoundedBackdropToolButton, 30, Buffer.Graphics)

                    .DrawLine(SelectionHighlightPen,
                              RoundedBackdropToolButton.Left + 11,
                              RoundedBackdropToolButton.Bottom - 2,
                              RoundedBackdropToolButton.Right - 11,
                              RoundedBackdropToolButton.Bottom - 2)

                End If

            Else

                If BackdropToolButtonHover = True Then

                    FillRoundedRectangle(HoverBrush, RoundedBackdropToolButton, 30, Buffer.Graphics)

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedBackdropToolButton.Left
                    ToolTip.Y = RoundedBackdropToolButton.Top - 30
                    ToolTip.Width = 98
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Backdrop", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(Brushes.Black, RoundedBackdropToolButton, 30, Buffer.Graphics)

                End If

            End If

            ' Define the start and end points of the gradient
            Dim StartPoint As New PointF(BackdropToolIcon.Rect.Left, BackdropToolIcon.Rect.Top)
            Dim EndPoint As New PointF(BackdropToolIcon.Rect.Right, BackdropToolIcon.Rect.Top)

            ' Define the colors for the gradient stops
            'Dim Colors() As Color = {Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet}
            Dim Colors() As Color = {Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.RoyalBlue, Color.Indigo, Color.Violet}

            ' Create the color blend for the gradient
            Dim ColorBlend As New ColorBlend()
            ColorBlend.Colors = Colors
            ColorBlend.Positions = New Single() {0.0F, 0.167F, 0.333F, 0.5F, 0.667F, 0.833F, 1.0F}
            'ColorBlend.Positions = New Single() {0.0F, 0.1F, 0.25F, 0.45F, 0.65F, 0.833F, 1.0F}

            ' Create the linear gradient brush
            Dim GradBrush As New LinearGradientBrush(StartPoint, EndPoint, Color.Empty, Color.Empty)
            GradBrush.InterpolationColors = ColorBlend

            ' Fill the rectangle with the gradient brush
            FillRoundedRectangle(GradBrush, BackdropToolIcon.Rect, 30, Buffer.Graphics)

            .SmoothingMode = SmoothingMode.None

            If SelectedTool = Tools.Portal Then

                Dim ControllerHint As Rectangle

                ControllerHint.X = BackdropToolButton.Rect.X + 18
                ControllerHint.Y = BackdropToolButton.Rect.Y + 30
                ControllerHint.Width = BackdropToolButton.Rect.Width
                ControllerHint.Height = BackdropToolButton.Rect.Height

                Dim ControllerHintShadow As Rectangle = ControllerHint

                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            If SelectedTool = Tools.Enemy Then

                Dim ControllerHint As Rectangle

                ControllerHint.X = BackdropToolButton.Rect.X + 18
                ControllerHint.Y = BackdropToolButton.Rect.Y + 30
                ControllerHint.Width = BackdropToolButton.Rect.Width
                ControllerHint.Height = BackdropToolButton.Rect.Height

                Dim ControllerHintShadow As Rectangle = ControllerHint

                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2

                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub DrawBillToolButton()

        Dim RoundedBillToolButton As Rectangle = BillToolButton.Rect

        RoundedBillToolButton.Offset(2, 2)

        RoundedBillToolButton.Width = BillToolButton.Rect.Width - 4
        RoundedBillToolButton.Height = BillToolButton.Rect.Height - 4

        With Buffer.Graphics

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            If SelectedTool = Tools.Bill Then

                If BillToolButtonHover = True Then

                    FillRoundedRectangle(SelectedHoverBrush, RoundedBillToolButton, 30, Buffer.Graphics)

                    .DrawLine(SelectionHighlightHoverPen,
                              RoundedBillToolButton.Left + 11,
                              RoundedBillToolButton.Bottom - 2,
                              RoundedBillToolButton.Right - 11,
                              RoundedBillToolButton.Bottom - 2)

                    .SmoothingMode = SmoothingMode.None

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedBillToolButton.Left
                    ToolTip.Y = RoundedBillToolButton.Top - 30
                    ToolTip.Width = 60
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Cash", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(SelectedBrush, RoundedBillToolButton, 30, Buffer.Graphics)

                    .CompositingMode = CompositingMode.SourceOver

                    .SmoothingMode = SmoothingMode.AntiAlias

                    .DrawLine(SelectionHighlightPen,
                              RoundedBillToolButton.Left + 11,
                              RoundedBillToolButton.Bottom - 2,
                              RoundedBillToolButton.Right - 11,
                              RoundedBillToolButton.Bottom - 2)

                    .SmoothingMode = SmoothingMode.None

                End If

            Else

                If BillToolButtonHover = True Then

                    FillRoundedRectangle(HoverBrush, RoundedBillToolButton, 30, Buffer.Graphics)

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedBillToolButton.Left
                    ToolTip.Y = RoundedBillToolButton.Top - 30
                    ToolTip.Width = 60
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Cash", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(Brushes.Black, RoundedBillToolButton, 30, Buffer.Graphics)

                End If

            End If

            .FillRectangle(Brushes.Goldenrod, BillToolIcon.Rect)

            .DrawString("$",
                        BillIconFont,
                        Brushes.OrangeRed,
                        BillToolIcon.Rect,
                        AlineCenterMiddle)

            If SelectedTool = Tools.Block Then

                Dim ControllerHint As Rectangle

                ControllerHint.X = BillToolButton.Rect.X + 18
                ControllerHint.Y = BillToolButton.Rect.Y + 30
                ControllerHint.Width = BillToolButton.Rect.Width
                ControllerHint.Height = BillToolButton.Rect.Height

                Dim ControllerHintShadow As Rectangle = ControllerHint

                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2

                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            If SelectedTool = Tools.Bush Then

                Dim ControllerHint As Rectangle

                ControllerHint.X = BillToolButton.Rect.X + 18
                ControllerHint.Y = BillToolButton.Rect.Y + 30
                ControllerHint.Width = BillToolButton.Rect.Width
                ControllerHint.Height = BillToolButton.Rect.Height

                Dim ControllerHintShadow As Rectangle = ControllerHint

                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub DrawCloudToolButton()

        Dim RoundedCloudToolButton As Rectangle = CloudToolButton.Rect

        RoundedCloudToolButton.Offset(2, 2)

        RoundedCloudToolButton.Width = CloudToolButton.Rect.Width - 4
        RoundedCloudToolButton.Height = CloudToolButton.Rect.Height - 4

        With Buffer.Graphics

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            If SelectedTool = Tools.Cloud Then

                If CloudToolButtonHover = True Then

                    FillRoundedRectangle(SelectedHoverBrush, RoundedCloudToolButton, 30, Buffer.Graphics)

                    .DrawLine(SelectionHighlightHoverPen,
                              RoundedCloudToolButton.Left + 11,
                              RoundedCloudToolButton.Bottom - 2,
                              RoundedCloudToolButton.Right - 11,
                              RoundedCloudToolButton.Bottom - 2)

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedCloudToolButton.Left
                    ToolTip.Y = RoundedCloudToolButton.Top - 30
                    ToolTip.Width = 65
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Cloud", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(SelectedBrush, RoundedCloudToolButton, 30, Buffer.Graphics)

                    .DrawLine(SelectionHighlightPen,
                              RoundedCloudToolButton.Left + 11,
                              RoundedCloudToolButton.Bottom - 2,
                              RoundedCloudToolButton.Right - 11,
                              RoundedCloudToolButton.Bottom - 2)

                End If

            Else

                If CloudToolButtonHover = True Then

                    FillRoundedRectangle(HoverBrush, RoundedCloudToolButton, 30, Buffer.Graphics)

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedCloudToolButton.Left
                    ToolTip.Y = RoundedCloudToolButton.Top - 30
                    ToolTip.Width = 65
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Cloud", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(Brushes.Black, RoundedCloudToolButton, 30, Buffer.Graphics)

                End If

            End If

            .SmoothingMode = SmoothingMode.None

            .FillRectangle(Brushes.White, CloundToolIcon.Rect)

            .DrawLine(CloundToolIconPen,
                      CloundToolIcon.Rect.Right - 10,
                      CloundToolIcon.Rect.Top + 10,
                      CloundToolIcon.Rect.Right - 10,
                      CloundToolIcon.Rect.Bottom - 10)

            .DrawLine(CloundToolIconPen,
                      CloundToolIcon.Rect.Left + 10,
                      CloundToolIcon.Rect.Bottom - 10,
                      CloundToolIcon.Rect.Right - 10,
                      CloundToolIcon.Rect.Bottom - 10)

            .DrawRectangle(CloundToolIconOutinePen, CloundToolIcon.Rect)

            If SelectedTool = Tools.Bush Then

                Dim ControllerHint As Rectangle
                ControllerHint.X = CloudToolButton.Rect.X + 18
                ControllerHint.Y = CloudToolButton.Rect.Y + 30
                ControllerHint.Width = CloudToolButton.Rect.Width
                ControllerHint.Height = CloudToolButton.Rect.Height

                Dim ControllerHintShadow As Rectangle = ControllerHint
                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2

                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            If SelectedTool = Tools.Goal Then

                Dim ControllerHint As Rectangle

                ControllerHint.X = CloudToolButton.Rect.X + 18
                ControllerHint.Y = CloudToolButton.Rect.Y + 30
                ControllerHint.Width = CloudToolButton.Rect.Width
                ControllerHint.Height = CloudToolButton.Rect.Height

                Dim ControllerHintShadow As Rectangle = ControllerHint

                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub DrawBushesToolButton()

        Dim RoundedBushToolButton As Rectangle = BushToolButton.Rect

        RoundedBushToolButton.Offset(2, 2)

        RoundedBushToolButton.Width = BushToolButton.Rect.Width - 4
        RoundedBushToolButton.Height = BushToolButton.Rect.Height - 4

        With Buffer.Graphics

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            If SelectedTool = Tools.Bush Then

                If BushToolButtonHover = True Then

                    FillRoundedRectangle(SelectedHoverBrush, RoundedBushToolButton, 30, Buffer.Graphics)

                    .DrawLine(SelectionHighlightHoverPen,
                              RoundedBushToolButton.Left + 11,
                              RoundedBushToolButton.Bottom - 2,
                              RoundedBushToolButton.Right - 11,
                              RoundedBushToolButton.Bottom - 2)

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedBushToolButton.Left
                    ToolTip.Y = RoundedBushToolButton.Top - 30
                    ToolTip.Width = 55
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Bush", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(SelectedBrush, RoundedBushToolButton, 30, Buffer.Graphics)

                    .DrawLine(SelectionHighlightPen,
                              RoundedBushToolButton.Left + 11,
                              RoundedBushToolButton.Bottom - 2,
                              RoundedBushToolButton.Right - 11,
                              RoundedBushToolButton.Bottom - 2)

                End If

            Else

                If BushToolButtonHover = True Then

                    FillRoundedRectangle(HoverBrush, RoundedBushToolButton, 30, Buffer.Graphics)

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedBushToolButton.Left
                    ToolTip.Y = RoundedBushToolButton.Top - 30
                    ToolTip.Width = 55
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Bush", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(Brushes.Black, RoundedBushToolButton, 30, Buffer.Graphics)

                End If

            End If

            .SmoothingMode = SmoothingMode.None

            .FillRectangle(Brushes.GreenYellow, BushToolIcon.Rect)

            .DrawLine(BushToolIconPen,
                      BushToolIcon.Rect.Right - 10,
                      BushToolIcon.Rect.Top + 10,
                      BushToolIcon.Rect.Right - 10,
                      BushToolIcon.Rect.Bottom - 10)

            .DrawLine(BushToolIconPen,
                      BushToolIcon.Rect.Left + 10,
                      BushToolIcon.Rect.Bottom - 10,
                      BushToolIcon.Rect.Right - 10,
                      BushToolIcon.Rect.Bottom - 10)

            .DrawRectangle(BushToolIconOutinePen, BushToolIcon.Rect)

            If SelectedTool = Tools.Bill Then

                Dim ControllerHint As Rectangle

                ControllerHint.X = BushToolIcon.Rect.X + 18
                ControllerHint.Y = BushToolIcon.Rect.Y + 30
                ControllerHint.Width = BushToolIcon.Rect.Width
                ControllerHint.Height = BushToolIcon.Rect.Height

                Dim ControllerHintShadow As Rectangle = ControllerHint

                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2

                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            If SelectedTool = Tools.Cloud Then

                Dim ControllerHint As Rectangle

                ControllerHint.X = BushToolIcon.Rect.X + 18
                ControllerHint.Y = BushToolIcon.Rect.Y + 30

                ControllerHint.Size = BushToolIcon.Rect.Size

                Dim ControllerHintShadow As Rectangle = ControllerHint

                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub DrawEnemyToolButton()

        Dim RoundedEnemyToolButton As Rectangle = EnemyToolButton.Rect

        RoundedEnemyToolButton.Offset(2, 2)

        RoundedEnemyToolButton.Width = EnemyToolButton.Rect.Width - 4
        RoundedEnemyToolButton.Height = EnemyToolButton.Rect.Height - 4

        With Buffer.Graphics

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            If SelectedTool = Tools.Enemy Then

                If EnemyToolButtonHover = True Then

                    FillRoundedRectangle(SelectedHoverBrush, RoundedEnemyToolButton, 30, Buffer.Graphics)

                    .DrawLine(SelectionHighlightHoverPen,
                              RoundedEnemyToolButton.Left + 11,
                              RoundedEnemyToolButton.Bottom - 2,
                              RoundedEnemyToolButton.Right - 11,
                              RoundedEnemyToolButton.Bottom - 2)

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedEnemyToolButton.Left
                    ToolTip.Y = RoundedEnemyToolButton.Top - 30
                    ToolTip.Width = 73
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Enemy", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(SelectedBrush, RoundedEnemyToolButton, 30, Buffer.Graphics)

                    .DrawLine(SelectionHighlightPen,
                              RoundedEnemyToolButton.Left + 11,
                              RoundedEnemyToolButton.Bottom - 2,
                              RoundedEnemyToolButton.Right - 11,
                              RoundedEnemyToolButton.Bottom - 2)

                End If

            Else

                If EnemyToolButtonHover = True Then

                    FillRoundedRectangle(HoverBrush, RoundedEnemyToolButton, 30, Buffer.Graphics)

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedEnemyToolButton.Left
                    ToolTip.Y = RoundedEnemyToolButton.Top - 30
                    ToolTip.Width = 73
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Enemy", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(Brushes.Black, RoundedEnemyToolButton, 30, Buffer.Graphics)

                End If

            End If

            .SmoothingMode = SmoothingMode.None

            .FillRectangle(Brushes.Chocolate, EnemyToolIcon.Rect)

            'Bug Fix Don't Change.
            .CompositingMode = CompositingMode.SourceOver
            'To fix draw string error with anti aliasing: "Parameters not valid."
            'Set the compositing mode to source over.

            .DrawString("E", EnemyIconFont, Brushes.PaleGoldenrod, EnemyToolIcon.Rect, AlineCenterMiddle)

            If SelectedTool = Tools.Goal Then

                Dim ControllerHint As Rectangle

                ControllerHint.X = EnemyToolButton.Rect.X + 18
                ControllerHint.Y = EnemyToolButton.Rect.Y + 30

                ControllerHint.Size = EnemyToolButton.Rect.Size

                Dim ControllerHintShadow As Rectangle = ControllerHint

                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2


                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            If SelectedTool = Tools.Backdrop Then

                Dim ControllerHint As Rectangle

                ControllerHint.X = EnemyToolButton.Rect.X + 18
                ControllerHint.Y = EnemyToolButton.Rect.Y + 30

                ControllerHint.Size = EnemyToolButton.Rect.Size

                Dim ControllerHintShadow As Rectangle = ControllerHint

                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub DrawGoalToolButton()

        Dim RoundedGoalToolButton As Rectangle = GoalToolButton.Rect

        RoundedGoalToolButton.Offset(2, 2)

        RoundedGoalToolButton.Width = GoalToolButton.Rect.Width - 4
        RoundedGoalToolButton.Height = GoalToolButton.Rect.Height - 4

        With Buffer.Graphics

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            If SelectedTool = Tools.Goal Then

                If GoalToolButtonHover = True Then

                    FillRoundedRectangle(SelectedHoverBrush, RoundedGoalToolButton, 30, Buffer.Graphics)

                    .DrawLine(SelectionHighlightHoverPen,
                              RoundedGoalToolButton.Left + 11,
                              RoundedGoalToolButton.Bottom - 2,
                              RoundedGoalToolButton.Right - 11,
                              RoundedGoalToolButton.Bottom - 2)

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedGoalToolButton.Left
                    ToolTip.Y = RoundedGoalToolButton.Top - 30
                    ToolTip.Width = 55
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Goal", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(SelectedBrush, RoundedGoalToolButton, 30, Buffer.Graphics)

                    .DrawLine(SelectionHighlightPen,
                              RoundedGoalToolButton.Left + 11,
                              RoundedGoalToolButton.Bottom - 2,
                              RoundedGoalToolButton.Right - 11,
                              RoundedGoalToolButton.Bottom - 2)

                End If

            Else

                If GoalToolButtonHover = True Then

                    FillRoundedRectangle(HoverBrush, RoundedGoalToolButton, 30, Buffer.Graphics)

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedGoalToolButton.Left
                    ToolTip.Y = RoundedGoalToolButton.Top - 30
                    ToolTip.Width = 55
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Goal", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(Brushes.Black, RoundedGoalToolButton, 30, Buffer.Graphics)

                End If

            End If

            .SmoothingMode = SmoothingMode.None

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
            Dim GradBrush As New PathGradientBrush(GradPath) With
                {.CenterPoint = center,
                .CenterColor = colors(0),
                .SurroundColors = New Color() {colors(1)}}

            .FillRectangle(GradBrush, GoalToolIcon.Rect)

            Dim Font As New Font(New FontFamily("Wingdings"), GoalToolIcon.Rect.Width \ 2, FontStyle.Regular)

            .DrawString("«",
                        Font,
                        Brushes.Green,
                        GoalToolIcon.Rect,
                        AlineCenterMiddle)

            If SelectedTool = Tools.Cloud Then

                Dim ControllerHint As Rectangle

                ControllerHint.X = GoalToolButton.Rect.X + 18
                ControllerHint.Y = GoalToolButton.Rect.Y + 30

                ControllerHint.Size = GoalToolButton.Rect.Size

                Dim ControllerHintShadow As Rectangle = ControllerHint

                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2

                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            If SelectedTool = Tools.Enemy Then

                Dim ControllerHint As Rectangle

                ControllerHint.X = GoalToolButton.Rect.X + 18
                ControllerHint.Y = GoalToolButton.Rect.Y + 30

                ControllerHint.Size = GoalToolButton.Rect.Size

                Dim ControllerHintShadow As Rectangle = ControllerHint

                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub DrawPortalToolButton()

        Dim RoundedPortalToolButton As Rectangle = PortalToolButton.Rect

        RoundedPortalToolButton.Offset(2, 2)

        RoundedPortalToolButton.Width = PortalToolButton.Rect.Width - 4
        RoundedPortalToolButton.Height = PortalToolButton.Rect.Height - 4

        With Buffer.Graphics

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            If SelectedTool = Tools.Portal Then

                'Draw hover effect for button
                If PortalToolButtonHover = True Then

                    FillRoundedRectangle(SelectedHoverBrush, RoundedPortalToolButton, 30, Buffer.Graphics)

                    .DrawLine(SelectionHighlightHoverPen,
                              RoundedPortalToolButton.Left + 11,
                              RoundedPortalToolButton.Bottom - 2,
                              RoundedPortalToolButton.Right - 11,
                              RoundedPortalToolButton.Bottom - 2)

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedPortalToolButton.Left
                    ToolTip.Y = RoundedPortalToolButton.Top - 30
                    ToolTip.Width = 65
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Portal", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(SelectedBrush, RoundedPortalToolButton, 30, Buffer.Graphics)

                    .DrawLine(SelectionHighlightPen,
                              RoundedPortalToolButton.Left + 11,
                              RoundedPortalToolButton.Bottom - 2,
                              RoundedPortalToolButton.Right - 11,
                              RoundedPortalToolButton.Bottom - 2)

                End If

            Else

                If PortalToolButtonHover = True Then

                    FillRoundedRectangle(HoverBrush, RoundedPortalToolButton, 30, Buffer.Graphics)

                    Dim ToolTip As Rectangle

                    ToolTip.X = RoundedPortalToolButton.Left
                    ToolTip.Y = RoundedPortalToolButton.Top - 30
                    ToolTip.Width = 65
                    ToolTip.Height = 28

                    .FillRectangle(Brushes.Yellow, ToolTip)

                    .DrawString("Portal", ToolTipFont, ToolTipBrush, ToolTip.Location)

                Else

                    FillRoundedRectangle(Brushes.Black, RoundedPortalToolButton, 30, Buffer.Graphics)

                End If

            End If

            .SmoothingMode = SmoothingMode.None

            DrawPortal(PortalToolIcon.Rect)

            'Draw Controller Hints
            If SelectedTool = Tools.Backdrop Then

                Dim ControllerHint As Rectangle

                ControllerHint.X = PortalToolIcon.Rect.X + 18
                ControllerHint.Y = PortalToolIcon.Rect.Y + 30

                ControllerHint.Size = PortalToolIcon.Rect.Size

                Dim ControllerHintShadow As Rectangle = ControllerHint

                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2

                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("RT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            If SelectedTool = Tools.Pointer Then

                Dim ControllerHint As Rectangle

                ControllerHint.X = PortalToolIcon.Rect.X + 18
                ControllerHint.Y = PortalToolIcon.Rect.Y + 30

                ControllerHint.Size = PortalToolIcon.Rect.Size

                Dim ControllerHintShadow As Rectangle = ControllerHint

                ControllerHintShadow.X = ControllerHint.X + 2
                ControllerHintShadow.Y = ControllerHint.Y + 2

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.Black,
                            ControllerHintShadow,
                            AlineCenterMiddle)

                .DrawString("LT",
                            ControllerHintFont,
                            Brushes.White,
                            ControllerHint,
                            AlineCenterMiddle)

            End If

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub DrawPlayButton()

        Dim RoundedEditPlayButton As Rectangle = EditPlayButton.Rect

        RoundedEditPlayButton.Offset(2, 2)

        RoundedEditPlayButton.Width = EditPlayButton.Rect.Width - 4
        RoundedEditPlayButton.Height = EditPlayButton.Rect.Height - 4

        With Buffer.Graphics

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            If EditPlayButtonHover = True Then

                FillRoundedRectangle(HoverBrush, RoundedEditPlayButton, 30, Buffer.Graphics)

            Else

                FillRoundedRectangle(Brushes.Black, RoundedEditPlayButton, 30, Buffer.Graphics)

            End If

            Dim ButtonCaption As Rectangle

            ButtonCaption.X = EditPlayButton.Rect.X + 2
            ButtonCaption.Y = EditPlayButton.Rect.Y

            ButtonCaption.Size = EditPlayButton.Rect.Size

            .DrawString("Play",
                        FPSFont,
                        Brushes.White,
                        ButtonCaption,
                        AlineCenterMiddle)

            Dim ControllerHint As Rectangle

            ControllerHint.X = EditPlayButton.Rect.X + 100
            ControllerHint.Y = EditPlayButton.Rect.Y + 65

            ControllerHint.Width = 15
            ControllerHint.Height = 7

            FillRoundedRectangle(Brushes.OrangeRed, ControllerHint, 7, Buffer.Graphics)

            ControllerHint.X = EditPlayButton.Rect.X + 85
            ControllerHint.Y = EditPlayButton.Rect.Y + 70

            ControllerHint.Width = 15
            ControllerHint.Height = 15

            FillRoundedRectangle(Brushes.Gray, ControllerHint, 15, Buffer.Graphics)

            ControllerHint.X = EditPlayButton.Rect.X + 70
            ControllerHint.Y = EditPlayButton.Rect.Y + 65

            ControllerHint.Width = 15
            ControllerHint.Height = 7

            FillRoundedRectangle(Brushes.Gray, ControllerHint, 7, Buffer.Graphics)

            .CompositingMode = CompositingMode.SourceCopy

            .SmoothingMode = SmoothingMode.None

        End With

    End Sub

    Private Sub DrawEditButton()

        Dim RoundedEditPlayButton As Rectangle = EditPlayButton.Rect

        RoundedEditPlayButton.Offset(2, 2)

        RoundedEditPlayButton.Width = EditPlayButton.Rect.Width - 4
        RoundedEditPlayButton.Height = EditPlayButton.Rect.Height - 4

        With Buffer.Graphics

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            If EditPlayButtonHover = True Then

                FillRoundedRectangle(HoverBrush, RoundedEditPlayButton, 30, Buffer.Graphics)

            Else

                FillRoundedRectangle(Brushes.Black, RoundedEditPlayButton, 30, Buffer.Graphics)

            End If

            .DrawString("Edit",
                        FPSFont,
                        Brushes.White,
                        EditPlayButton.Rect,
                        AlineCenterMiddle)

            Dim ControllerHint As Rectangle

            ControllerHint.X = EditPlayButton.Rect.X + 100
            ControllerHint.Y = EditPlayButton.Rect.Y + 65

            ControllerHint.Width = 15
            ControllerHint.Height = 7

            FillRoundedRectangle(Brushes.OrangeRed, ControllerHint, 7, Buffer.Graphics)

            ControllerHint.X = EditPlayButton.Rect.X + 85
            ControllerHint.Y = EditPlayButton.Rect.Y + 70

            ControllerHint.Width = 15
            ControllerHint.Height = 15

            FillRoundedRectangle(Brushes.Gray, ControllerHint, 15, Buffer.Graphics)

            ControllerHint.X = EditPlayButton.Rect.X + 70
            ControllerHint.Y = EditPlayButton.Rect.Y + 65

            ControllerHint.Width = 15
            ControllerHint.Height = 7

            FillRoundedRectangle(Brushes.Gray, ControllerHint, 7, Buffer.Graphics)

            .CompositingMode = CompositingMode.SourceCopy

            .SmoothingMode = SmoothingMode.None

        End With

    End Sub

    Private Sub DrawMenuButton()

        Dim RoundedMenuButton As Rectangle = MenuButton.Rect

        RoundedMenuButton.Offset(2, 2)

        RoundedMenuButton.Width = MenuButton.Rect.Width - 4
        RoundedMenuButton.Height = MenuButton.Rect.Height - 4

        With Buffer.Graphics

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            If MenuButtonHover = True Then

                FillRoundedRectangle(HoverBrush, RoundedMenuButton, 30, Buffer.Graphics)

            Else

                FillRoundedRectangle(Brushes.Black, RoundedMenuButton, 30, Buffer.Graphics)

            End If

            .DrawString("≡",
                        MenuButtonFont,
                        Brushes.White,
                        MenuButton.Rect,
                        AlineCenterMiddle)

            Dim ControllerHint As Rectangle

            ControllerHint.X = MenuButton.Rect.X + 70
            ControllerHint.Y = MenuButton.Rect.Y + 65

            ControllerHint.Width = 15
            ControllerHint.Height = 7

            FillRoundedRectangle(Brushes.Gray, ControllerHint, 7, Buffer.Graphics)

            ControllerHint.X = MenuButton.Rect.X + 55
            ControllerHint.Y = MenuButton.Rect.Y + 70

            ControllerHint.Width = 15
            ControllerHint.Height = 15

            FillRoundedRectangle(Brushes.Gray, ControllerHint, 15, Buffer.Graphics)

            ControllerHint.X = MenuButton.Rect.X + 40
            ControllerHint.Y = MenuButton.Rect.Y + 65

            ControllerHint.Width = 15
            ControllerHint.Height = 7

            FillRoundedRectangle(Brushes.OrangeRed, ControllerHint, 7, Buffer.Graphics)

            .CompositingMode = CompositingMode.SourceCopy

            .SmoothingMode = SmoothingMode.None

        End With

    End Sub

    Private Sub DrawSaveButton()

        With Buffer.Graphics

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            If SaveButtonHover = True Then

                FillRoundedRectangle(HoverBrush, SaveButton.Rect, 30, Buffer.Graphics)

            Else

                FillRoundedRectangle(Brushes.Black, SaveButton.Rect, 30, Buffer.Graphics)

            End If

            .DrawEllipse(BButtonIconOutinePen,
                         New Rectangle(SaveButton.Rect.X + 197,
                                       SaveButton.Rect.Y + SaveButton.Rect.Height \ 2 - 52 \ 2,
                                       52,
                                       52))

            .SmoothingMode = SmoothingMode.None

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

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub DrawOpenButton()

        With Buffer.Graphics

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            If OpenButtonHover = True Then

                FillRoundedRectangle(HoverBrush, OpenButton.Rect, 30, Buffer.Graphics)

            Else

                FillRoundedRectangle(Brushes.Black, OpenButton.Rect, 30, Buffer.Graphics)

            End If

            .DrawEllipse(YButtonIconOutinePen,
                         New Rectangle(OpenButton.Rect.X + 197,
                                       OpenButton.Rect.Y + OpenButton.Rect.Height \ 2 - 52 \ 2,
                                       52,
                                       52))

            .SmoothingMode = SmoothingMode.None

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

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub DrawNewButton()

        With Buffer.Graphics

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            If NewButtonHover = True Then

                FillRoundedRectangle(HoverBrush, NewButton.Rect, 30, Buffer.Graphics)

            Else

                FillRoundedRectangle(Brushes.Black, NewButton.Rect, 30, Buffer.Graphics)

            End If

            .SmoothingMode = SmoothingMode.None

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

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub DrawExitButton()

        With Buffer.Graphics

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            If ExitButtonHover = True Then

                FillRoundedRectangle(HoverBrush, ExitButton.Rect, 30, Buffer.Graphics)

            Else

                FillRoundedRectangle(Brushes.Black, ExitButton.Rect, 30, Buffer.Graphics)

            End If

            .DrawEllipse(XButtonIconOutinePen,
                         New Rectangle(ExitButton.Rect.X + ExitButton.Rect.Width \ 2 - 52 \ 2,
                                       ExitButton.Rect.Y + ExitButton.Rect.Height \ 2 - 52 \ 2,
                                       52,
                                       52))

            .SmoothingMode = SmoothingMode.None

            .DrawString("X",
                        ButtonIconFont,
                        Brushes.White,
                        New Rectangle(ExitButton.Rect.X + ExitButton.Rect.Width \ 2 - 52 \ 2,
                                      ExitButton.Rect.Y + 16,
                                      52,
                                      52),
                        AlineCenterMiddle)

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub DrawFPS()

        With Buffer.Graphics

            'Bug Fix Don't Change.
            .CompositingMode = CompositingMode.SourceOver
            'To fix draw string error with anti aliasing: "Parameters not valid."
            'Set the compositing mode to source over.

            .DrawString(FPS.ToString & " FPS", FPSFont, Brushes.White, FPS_Postion)

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub AddPortal(Rect As Rectangle, PatrolA As Point, PatrolB As Point)

        If Portals IsNot Nothing Then

            Array.Resize(Portals, Portals.Length + 1)

        Else

            ReDim Portals(0)

        End If

        Dim Index As Integer = Portals.Length - 1

        'Init portal
        Portals(Index).Rect = Rect

        Portals(Index).Position.X = Rect.X
        Portals(Index).Position.Y = Rect.Y

        Portals(Index).PatrolA.X = PatrolA.X
        Portals(Index).PatrolA.Y = PatrolA.Y

        Portals(Index).PatrolB.X = PatrolB.X
        Portals(Index).PatrolB.Y = PatrolB.Y

        AutoSizeLevel(Portals(Index).Rect)

    End Sub

    Private Sub AddBackdrop(Rect As Rectangle, Color As Color)

        If Backdrops IsNot Nothing Then

            Array.Resize(Backdrops, Backdrops.Length + 1)

        Else

            ReDim Backdrops(0)

        End If

        Dim Index As Integer = Backdrops.Length - 1

        'Init backdrop
        Backdrops(Index).Rect = Rect

        Backdrops(Index).Position.X = Rect.X
        Backdrops(Index).Position.Y = Rect.Y

        Backdrops(Index).Color = Color.ToArgb

        AutoSizeLevel(Backdrops(Index).Rect)

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

    Private Sub AddGridLine(X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer)
        'TODO

        If Gridlines IsNot Nothing Then

            Array.Resize(Gridlines, Gridlines.Length + 1)

        Else

            ReDim Gridlines(0)

        End If

        Dim Index As Integer = Gridlines.Length - 1

        'Init block
        Gridlines(Index).X1 = X1
        Gridlines(Index).Y1 = Y1
        Gridlines(Index).X2 = X2
        Gridlines(Index).Y2 = Y2

    End Sub


    Private Sub AutoSizeLevel(Rect As Rectangle)

        If Rect.Right > Level.Rect.Right Then

            Level.Rect.Width = Rect.Right

            'BufferGridLines()

        End If

        If Rect.Bottom > Level.Rect.Bottom Then

            Level.Rect.Height = Rect.Bottom

            'BufferGridLines()

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

    Private Sub RemoveBackdrop(Index As Integer)

        'Remove the backdrop from backdrops.
        Backdrops = Backdrops.Where(Function(e, i) i <> Index).ToArray()

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

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            Dim Shadow As Rectangle = StartScreenOpenButton.Rect

            Shadow.Offset(12, 12)

            FillRoundedRectangle(MenuShadowBrush, Shadow, 30, Buffer.Graphics)

            If StartScreenOpenButtonHover = True Then

                DrawRoundedRectangle(StartScreenButtonOutlineHoverPen, StartScreenOpenButton.Rect, 30, Buffer.Graphics)

                FillRoundedRectangle(HoverBrush, StartScreenOpenButton.Rect, 30, Buffer.Graphics)

            Else

                DrawRoundedRectangle(StartScreenButtonOutlinePen, StartScreenOpenButton.Rect, 30, Buffer.Graphics)

                FillRoundedRectangle(Brushes.Black, StartScreenOpenButton.Rect, 30, Buffer.Graphics)

            End If

            .DrawEllipse(YButtonIconOutinePen,
                         New Rectangle(StartScreenOpenButton.Rect.X + 142,
                                       StartScreenOpenButton.Rect.Y + StartScreenOpenButton.Rect.Height \ 2 - 52 \ 2,
                                       52,
                                       52))

            .SmoothingMode = SmoothingMode.None

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

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub DrawStartScreenNewButton()

        With Buffer.Graphics

            Dim Shadow As Rectangle = StartScreenNewButton.Rect

            Shadow.Offset(12, 12)

            .CompositingMode = CompositingMode.SourceOver

            .SmoothingMode = SmoothingMode.AntiAlias

            FillRoundedRectangle(MenuShadowBrush, Shadow, 30, Buffer.Graphics)

            If StartScreenNewButtonHover = True Then

                DrawRoundedRectangle(StartScreenButtonOutlineHoverPen, StartScreenNewButton.Rect, 30, Buffer.Graphics)

                FillRoundedRectangle(HoverBrush, StartScreenNewButton.Rect, 30, Buffer.Graphics)

            Else

                DrawRoundedRectangle(StartScreenButtonOutlinePen, StartScreenNewButton.Rect, 30, Buffer.Graphics)

                FillRoundedRectangle(Brushes.Black, StartScreenNewButton.Rect, 30, Buffer.Graphics)

            End If

            .DrawEllipse(BButtonIconOutinePen,
                         New Rectangle(StartScreenNewButton.Rect.X + 140,
                                       StartScreenNewButton.Rect.Y + StartScreenNewButton.Rect.Height \ 2 - 52 \ 2,
                                       52,
                                       52))

            .SmoothingMode = SmoothingMode.None

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

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub DrawTitle()

        With Buffer.Graphics

            'Bug Fix Don't Change.
            .CompositingMode = CompositingMode.SourceOver
            'To fix draw string error with anti aliasing: "Parameters not valid."
            'Set the compositing mode to source over.

            'Draw drop shadow.
            .DrawString(Title.Text,
                    TitleFont,
                    New SolidBrush(Color.FromArgb(255, Color.White)),
                    New Rectangle(Title.Rect.X + 3,
                                  Title.Rect.Y + 3,
                                  Title.Rect.Width,
                                  Title.Rect.Height),
                                  AlineCenterMiddle)

            'Draw title.
            .DrawString(Title.Text,
                        TitleFont,
                        Brushes.Black,
                        Title.Rect,
                        AlineCenterMiddle)

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub DrawClearTitle()

        With Buffer.Graphics

            'Bug Fix Don't Change.
            .CompositingMode = CompositingMode.SourceOver
            'To fix draw string error with anti aliasing: "Parameters not valid."
            'Set the compositing mode to source over.

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

            .CompositingMode = CompositingMode.SourceCopy

        End With

    End Sub

    Private Sub DrawBackground(Color As Color)

        With Buffer.Graphics

            .Clear(Color)

        End With

    End Sub

    Private Sub UpdateGridLines()

        Gridlines = Nothing

        'Update vertical grid lines  |
        For x As Integer = 0 To Level.Rect.Width Step GridSize

            If x >= Camera.Rect.Left AndAlso x <= Camera.Rect.Right Then

                AddGridLine(x, 0, x, Level.Rect.Height)

            End If

        Next

        'Update horizontal grid lines ---
        For y As Integer = 0 To Level.Rect.Height Step GridSize

            If y >= Camera.Rect.Top AndAlso y <= Camera.Rect.Bottom Then

                AddGridLine(0, y, Level.Rect.Width, y)

            End If

        Next

    End Sub

    Private Sub DrawGridLines()

        With Buffer.Graphics

            If Gridlines IsNot Nothing Then

                Dim OffsetLine As Line

                For Each line In Gridlines

                    OffsetLine.X1 = line.X1 + Camera.Position.X * -1
                    OffsetLine.Y1 = line.Y1 + Camera.Position.Y * -1
                    OffsetLine.X2 = line.X2 + Camera.Position.X * -1
                    OffsetLine.Y2 = line.Y2 + Camera.Position.Y * -1

                    .DrawLine(Pens.Gray, OffsetLine.X1, OffsetLine.Y1, OffsetLine.X2, OffsetLine.Y2)

                Next

            End If

        End With

    End Sub

    Private Sub DrawRoundedRectangle(pen As Pen, Rect As Rectangle, radius As Integer, g As Graphics)

        'g.CompositingMode = CompositingMode.SourceOver

        'g.SmoothingMode = SmoothingMode.AntiAlias

        Dim path As New GraphicsPath()

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

        'g.CompositingMode = CompositingMode.SourceCopy

        'g.SmoothingMode = SmoothingMode.None

    End Sub

    Private Sub FillRoundedRectangle(brush As Brush, Rect As Rectangle, radius As Integer, g As Graphics)

        'g.CompositingMode = CompositingMode.SourceOver

        'g.SmoothingMode = SmoothingMode.AntiAlias

        Dim Path As New GraphicsPath()

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

        g.FillPath(brush, Path)

        'g.CompositingMode = CompositingMode.SourceCopy

        'g.SmoothingMode = SmoothingMode.None

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

        CreateSoundFilesFromResources()

        CreateDemoFileFromResource()

        InitializeSounds()

        InitializeToolBarButtons()

        InitializeForm()

        InitializeBuffer()

        Title.Text = "Platformer" & vbCrLf & "with Level Editor"

        OutinePen.LineJoin = Drawing2D.LineJoin.Round

        CloundToolIconOutinePen.LineJoin = LineJoin.Round

        BushToolIconOutinePen.LineJoin = LineJoin.Round

        SelectionHighlightPen.StartCap = LineCap.Round

        SelectionHighlightPen.EndCap = LineCap.Round

        SelectionHighlightHoverPen.StartCap = LineCap.Round

        SelectionHighlightHoverPen.EndCap = LineCap.Round

        InitializeObjects()

        CreateStartScreenLevel()

        CashCollected = 0

        GameTimer.Start()

        If IsPlaying("Music") = False Then

            LoopSound("Music")

        End If

        MovePointerToStartScreenNewButton()

        Level.Color = Color.SkyBlue.ToArgb

    End Sub

    Private Sub InitializeSounds()

        AddSound("Music", Application.StartupPath & "level.mp3")

        SetVolume("Music", 50)

        AddOverlapping("CashCollected", Application.StartupPath & "CashCollected.mp3")

        SetVolumeOverlapping("CashCollected", 700)

        AddOverlapping("eliminated", Application.StartupPath & "eliminated.mp3")

        SetVolumeOverlapping("eliminated", 700)

        AddSound("clear", Application.StartupPath & "clear.mp3")

        SetVolume("clear", 1000)

        AddSound("Portal", Application.StartupPath & "Portal.mp3")

        SetVolume("Portal", 600)

    End Sub

    Private Sub InitializeToolBarButtons()

        EditPlayButton.Rect = New Rectangle(ClientRectangle.Left + 210,
                                            ClientRectangle.Bottom - 90,
                                            120,
                                            90)

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

        Spawn.Rect = New Rectangle(128, 769, 64, 64)

        Spawn.Position = New PointF(Spawn.Rect.X, Spawn.Rect.Y)

        MousePointer.Velocity = New PointF(0, 0)

        MousePointer.MaxVelocity = New PointF(1500, 1500)

        MousePointer.Acceleration = New PointF(400, 300)

        Camera.Velocity = New PointF(0, 0)

        Camera.MaxVelocity = New PointF(2500, 2500)

        Camera.Acceleration = New PointF(700, 700)

        'BufferGridLines()

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

        AddBlock(New Rectangle(0, 832, 2496, 192))

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

        AddBlock(New Rectangle(0, 832, 1920, 192))

        AddBlock(New Rectangle(1472, 576, 384, 64))

        AddBlock(New Rectangle(1536, 256, 256, 64))

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

        'BufferGridLines()

        CashCollected = 0

        LevelName = "Untitled"

        Text = LevelName & " - Platformer with Level Editor - Code with Joe"

    End Sub

    Private Sub ShowOpenLevelDialog()

        OpenFileDialog1.FileName = ""
        OpenFileDialog1.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
        OpenFileDialog1.FilterIndex = 1
        OpenFileDialog1.InitialDirectory = Application.StartupPath

        ShowOpenFileDialog = True

        If OpenFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then

            If My.Computer.FileSystem.FileExists(OpenFileDialog1.FileName) = True Then

                OpenTest2LevelFile(OpenFileDialog1.FileName)

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

        ShowOpenFileDialog = False

    End Sub

    Private Sub ShowSaveLevelDialog()

        SaveFileDialog1.FileName = LevelName
        SaveFileDialog1.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
        SaveFileDialog1.FilterIndex = 1
        SaveFileDialog1.InitialDirectory = Application.StartupPath

        ShowSaveFileDialog = True

        If SaveFileDialog1.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK Then

            SaveTest2LevelFile(SaveFileDialog1.FileName)

            LevelName = Path.GetFileName(SaveFileDialog1.FileName)
            Text = LevelName & " - Platformer with Level Editor - Code with Joe"

        End If

        ShowSaveFileDialog = False

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

        Backdrops = Nothing

        Portals = Nothing

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

        If BackdropToolButton.Rect.Contains(e) Then

            DeselectObjects()

            'Snap preview to grid.
            ToolPreview.X = CInt(Math.Round(PointOffset.X / GridSize) * GridSize)
            ToolPreview.Y = CInt(Math.Round(PointOffset.Y / GridSize) * GridSize)

            ToolPreview.Width = GridSize
            ToolPreview.Height = GridSize

            SelectedTool = Tools.Backdrop

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

        If PortalToolButton.Rect.Contains(e) Then

            DeselectObjects()

            'Snap preview to grid.
            ToolPreview.X = CInt(Math.Round(PointOffset.X / GridSize) * GridSize)
            ToolPreview.Y = CInt(Math.Round(PointOffset.Y / GridSize) * GridSize)

            ToolPreview.Width = GridSize
            ToolPreview.Height = GridSize

            SelectedTool = Tools.Portal

            ShowToolPreview = True

        End If

        'Is the player clicking the menu button?
        If MenuButton.Rect.Contains(e) Then
            'Yes, the player is clicking the menu button.

            DeselectObjects()

            ShowMenu = True

            'MovePointerCenterMenu()

            MovePointerOverSaveButton()

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

        SpawnSelected = False

        SelectedBackdrop = -1

        SelectedPortal = -1

    End Sub

    Private Sub MouseDownEditingSelection(e As Point)

        'Is the player over the toolbar?
        If ToolBarBackground.Rect.Contains(e) = False Then
            'No, the player is NOT over the toolbar.

            Dim PointOffset As Point

            PointOffset.X = Camera.Position.X + e.X
            PointOffset.Y = Camera.Position.Y + e.Y

            If SizingHandle.Contains(e) Then

                SizingHandleSelected = True

            Else

                SizingHandleSelected = False

                If SelectedTool = Tools.Pointer AndAlso CheckPortalSelection(PointOffset) = True Then

                    If Portals IsNot Nothing Then

                        For Each Portal In Portals

                            Dim PortalEntrance As New Rectangle(New Point(Portal.PatrolA.X, Portal.PatrolA.Y), New Drawing.Size(GridSize, GridSize))

                            If PortalEntrance.Contains(PointOffset) Then

                                SelectedPortal = Array.IndexOf(Portals, Portal)

                                PortalEntranceSelected = True

                                SelectionOffset.X = PointOffset.X - Portals(SelectedPortal).PatrolA.X
                                SelectionOffset.Y = PointOffset.Y - Portals(SelectedPortal).PatrolA.Y

                            End If

                            Dim PortalExit As New Rectangle(New Point(Portal.PatrolB.X, Portal.PatrolB.Y), New Drawing.Size(GridSize, GridSize))

                            If PortalExit.Contains(PointOffset) Then

                                SelectedPortal = Array.IndexOf(Portals, Portal)

                                PortalEntranceSelected = False

                                SelectionOffset.X = PointOffset.X - Portals(SelectedPortal).PatrolB.X
                                SelectionOffset.Y = PointOffset.Y - Portals(SelectedPortal).PatrolB.Y

                            End If

                        Next

                        'Deselect other game objects.
                        SelectedBlock = -1
                        SelectedBill = -1
                        SelectedCloud = -1
                        SelectedBush = -1
                        GoalSelected = False
                        LevelSelected = False
                        SpawnSelected = False
                        SelectedBackdrop = -1
                        SelectedEnemy = -1

                    End If

                    'Is the player selecting a Enemy?
                ElseIf SelectedTool = Tools.Pointer AndAlso CheckEnemySelection(PointOffset) > -1 Then

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
                    SpawnSelected = False
                    SelectedBackdrop = -1
                    SelectedPortal = -1

                ElseIf SelectedTool = Tools.Pointer AndAlso Goal.Rect.Contains(PointOffset) Then

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
                    SpawnSelected = False
                    SelectedBackdrop = -1
                    SelectedPortal = -1

                    'Is the player selecting a block?
                ElseIf SelectedTool = Tools.Pointer AndAlso CheckBlockSelection(PointOffset) > -1 Then
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
                    SpawnSelected = False
                    SelectedBackdrop = -1
                    SelectedPortal = -1

                    'Is the player selecting a bill?
                ElseIf SelectedTool = Tools.Pointer AndAlso CheckBillSelection(PointOffset) > -1 Then
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
                    SpawnSelected = False
                    SelectedBackdrop = -1
                    SelectedPortal = -1

                    'Is the player selecting a cloud?
                ElseIf SelectedTool = Tools.Pointer AndAlso CheckCloudSelection(PointOffset) > -1 Then
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
                    SpawnSelected = False
                    LevelSelected = False
                    SelectedBackdrop = -1
                    SelectedPortal = -1

                    'Is the player selecting a bush?
                ElseIf SelectedTool = Tools.Pointer AndAlso CheckBushSelection(PointOffset) > -1 Then
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
                    SpawnSelected = False
                    SelectedBackdrop = -1
                    SelectedPortal = -1

                ElseIf SelectedTool = Tools.Pointer AndAlso Spawn.Rect.Contains(PointOffset) Then

                    SpawnSelected = True

                    SelectionOffset.X = PointOffset.X - Spawn.Rect.X
                    SelectionOffset.Y = PointOffset.Y - Spawn.Rect.Y

                    'Deselect other game objects.
                    SelectedBlock = -1
                    SelectedBill = -1
                    SelectedCloud = -1
                    SelectedBush = -1
                    SelectedEnemy = -1
                    GoalSelected = False
                    LevelSelected = False
                    SelectedBackdrop = -1
                    SelectedPortal = -1

                    'Is the player selecting a backdrop?
                ElseIf SelectedTool = Tools.Pointer AndAlso CheckBackdropSelection(PointOffset) > -1 Then
                    'Yes, the player is selecting a backdrop.

                    SelectedBackdrop = CheckBackdropSelection(PointOffset)

                    SelectionOffset.X = PointOffset.X - Backdrops(SelectedBackdrop).Rect.X
                    SelectionOffset.Y = PointOffset.Y - Backdrops(SelectedBackdrop).Rect.Y

                    'Deselect other game objects.
                    SelectedBill = -1
                    SelectedCloud = -1
                    SelectedBush = -1
                    SelectedEnemy = -1
                    SelectedBlock = -1
                    GoalSelected = False
                    LevelSelected = False
                    SpawnSelected = False
                    SelectedPortal = -1

                Else

                    'No, the player is not selecting a game object.

                    MouseDownEditingSelectionTools(PointOffset)

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
            SpawnSelected = False
            SelectedBackdrop = -1
            SelectedPortal = -1

        End If

    End Sub

    Private Sub MouseDownEditingSelectionTools(PointOffset As Point)

        Select Case SelectedTool

            Case Tools.Portal

                'Snap portal to grid.
                Dim SnapPoint As New Point(CInt(Math.Round(PointOffset.X / GridSize) * GridSize),
                                               CInt(Math.Round(PointOffset.Y / GridSize) * GridSize))

                Dim SnapPointB As New Point(SnapPoint.X + GridSize, SnapPoint.Y)

                AddPortal(New Rectangle(SnapPoint, New Drawing.Size(GridSize, GridSize)), SnapPoint, SnapPointB)

                'Change tool to the mouse pointer.
                SelectedTool = Tools.Pointer

                'Turn tool preview off.
                ShowToolPreview = False

                'Select the newly created portal.
                SelectedPortal = Portals.Length - 1
                PortalEntranceSelected = True

                SelectionOffset.X = PointOffset.X - Portals(Portals.Length - 1).Rect.X
                SelectionOffset.Y = PointOffset.Y - Portals(Portals.Length - 1).Rect.Y

                AutoSizeLevel(New Rectangle(New Point(Portals(SelectedPortal).PatrolA.X, Portals(SelectedPortal).PatrolA.Y), New Drawing.Size(GridSize, GridSize)))
                AutoSizeLevel(New Rectangle(New Point(Portals(SelectedPortal).PatrolB.X, Portals(SelectedPortal).PatrolB.Y), New Drawing.Size(GridSize, GridSize)))

            Case Tools.Backdrop

                'Snap backdrop to grid.
                Dim SnapPoint As New Point(CInt(Math.Round(PointOffset.X / GridSize) * GridSize),
                                               CInt(Math.Round(PointOffset.Y / GridSize) * GridSize))

                AddBackdrop(New Rectangle(SnapPoint, New Drawing.Size(GridSize, GridSize)), Color.Black)

                'Change tool to the mouse pointer.
                SelectedTool = Tools.Pointer

                'Turn tool preview off.
                ShowToolPreview = False

                'Select the newly created backdrop.
                SelectedBackdrop = Backdrops.Length - 1

                SelectionOffset.X = PointOffset.X - Backdrops(Backdrops.Length - 1).Rect.X
                SelectionOffset.Y = PointOffset.Y - Backdrops(Backdrops.Length - 1).Rect.Y

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

                SelectionOffset.X = PointOffset.X - Blocks(Blocks.Length - 1).Rect.X
                SelectionOffset.Y = PointOffset.Y - Blocks(Blocks.Length - 1).Rect.Y

            Case Tools.Bill

                'Snap bill to grid.
                AddBill(New Point(CInt(Math.Round(PointOffset.X / GridSize) * GridSize),
                               CInt(Math.Round(PointOffset.Y / GridSize) * GridSize)))

                'Change tool to the mouse pointer.
                SelectedTool = Tools.Pointer

                'Turn tool preview off.
                ShowToolPreview = False

                'Select the newly created bill.
                SelectedBill = Cash.Length - 1

                SelectionOffset.X = PointOffset.X - Cash(Cash.Length - 1).Rect.X
                SelectionOffset.Y = PointOffset.Y - Cash(Cash.Length - 1).Rect.Y

            Case Tools.Cloud

                'Snap cloud to grid.
                Dim SnapPoint As New Point(CInt(Math.Round(PointOffset.X / GridSize) * GridSize),
                                               CInt(Math.Round(PointOffset.Y / GridSize) * GridSize))

                AddCloud(New Rectangle(SnapPoint, New Drawing.Size(GridSize, GridSize)))

                'Change tool to the mouse pointer.
                SelectedTool = Tools.Pointer

                'Turn tool preview off.
                ShowToolPreview = False

                'Select the newly created cloud.
                SelectedCloud = Clouds.Length - 1

                SelectionOffset.X = PointOffset.X - Clouds(Clouds.Length - 1).Rect.X
                SelectionOffset.Y = PointOffset.Y - Clouds(Clouds.Length - 1).Rect.Y

            Case Tools.Bush

                'Snap bush to grid.
                Dim SnapPoint As New Point(CInt(Math.Round(PointOffset.X / GridSize) * GridSize),
                                               CInt(Math.Round(PointOffset.Y / GridSize) * GridSize))

                AddBush(New Rectangle(SnapPoint, New Drawing.Size(GridSize, GridSize)))

                'Change tool to the mouse pointer.
                SelectedTool = Tools.Pointer

                'Turn tool preview off.
                ShowToolPreview = False

                'Select the newly created bush.
                SelectedBush = Bushes.Length - 1

                SelectionOffset.X = PointOffset.X - Bushes(Bushes.Length - 1).Rect.X
                SelectionOffset.Y = PointOffset.Y - Bushes(Bushes.Length - 1).Rect.Y

            Case Tools.Enemy

                'Snap enemy to grid.
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

                SelectionOffset.X = PointOffset.X - Enemies(Enemies.Length - 1).Rect.X
                SelectionOffset.Y = PointOffset.Y - Enemies(Enemies.Length - 1).Rect.Y

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

                SelectionOffset.X = PointOffset.X - Goal.Rect.X
                SelectionOffset.Y = PointOffset.Y - Goal.Rect.Y

            Case Tools.Pointer

                SelectionOffset.X = PointOffset.X - Level.Rect.X
                SelectionOffset.Y = PointOffset.Y - Level.Rect.Y

                LevelSelected = True

                'Deselect game objects.
                SelectedBlock = -1
                SelectedBill = -1
                SelectedCloud = -1
                SelectedBush = -1
                GoalSelected = False
                SelectedEnemy = -1
                SpawnSelected = False
                SelectedBackdrop = -1
                SelectedPortal = -1

        End Select

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

    Private Function CheckBackdropSelection(e As Point) As Integer

        If Backdrops IsNot Nothing Then

            For Each Backdrop In Backdrops

                'Has the player selected a backdrop?
                If Backdrop.Rect.Contains(e) Then
                    'Yes, the player has selected a backdrop.

                    Return Array.IndexOf(Backdrops, Backdrop)

                    Exit Function

                End If

            Next

        End If

        Return -1

    End Function

    Private Function CheckBlockSelection(e As Point) As Integer

        If Blocks IsNot Nothing Then

            For Each Block In Blocks

                'Has the player selected a block?
                If Block.Rect.Contains(e) Then
                    'Yes, the player has selected a block.

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

                'Has the player selected a bill?
                If Bill.Rect.Contains(e) Then
                    'Yes, the player has selected a bill.

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

                'Has the player selected a bush?
                If Bush.Rect.Contains(e) Then
                    'Yes, the player has selected a bush.

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

    Private Function CheckPortalSelection(e As Point) As Boolean

        If Portals IsNot Nothing Then

            For Each Portal In Portals

                Dim PortalEntrance As New Rectangle(New Point(Portal.PatrolA.X, Portal.PatrolA.Y), New Drawing.Size(GridSize, GridSize))

                If PortalEntrance.Contains(e) Then

                    Return True

                    Exit Function

                End If

                Dim PortalExit As New Rectangle(New Point(Portal.PatrolB.X, Portal.PatrolB.Y), New Drawing.Size(GridSize, GridSize))

                If PortalExit.Contains(e) Then

                    Return True

                    Exit Function

                End If

            Next

        End If

        Return False

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

        'Write Color
        Write(File_Number, Level.Color)

        'Write Text
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

    Private Sub SaveTest2LevelFile(FilePath As String)

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

        'Write Color
        Write(File_Number, Level.Color)
        'Write(File_Number, Color.SkyBlue.ToArgb)

        'Write Text
        Write(File_Number, "Level")

        'Write Spawn to File
        'Write ID
        Write(File_Number, ObjectID.Spawn)

        'Write Position
        Write(File_Number, Spawn.Rect.X)
        Write(File_Number, Spawn.Rect.Y)

        'Write Size
        Write(File_Number, Spawn.Rect.Width)
        Write(File_Number, Spawn.Rect.Height)

        'Write PatrolA
        Write(File_Number, Spawn.PatrolA.X)
        Write(File_Number, Spawn.PatrolA.Y)

        'Write PatrolB
        Write(File_Number, Spawn.PatrolB.X)
        Write(File_Number, Spawn.PatrolB.Y)

        'Write Color
        Write(File_Number, Color.OrangeRed.ToArgb)

        'Write Text
        Write(File_Number, "Spawn")

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

                'Write Color
                Write(File_Number, Color.OrangeRed.ToArgb)

                'Write Text
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

                'Write Color
                Write(File_Number, Color.OrangeRed.ToArgb)

                'Write Text
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

                'Write Color
                Write(File_Number, Color.OrangeRed.ToArgb)

                'Write Text
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

                'Write Color
                Write(File_Number, Color.OrangeRed.ToArgb)

                'Write Text
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

                'Write Color
                Write(File_Number, Color.OrangeRed.ToArgb)

                'Write Text
                Write(File_Number, "Enemy")

            Next

        End If

        'Write Backdrops to File
        If Backdrops IsNot Nothing Then

            For Each Backdrop In Backdrops

                'Write ID
                Write(File_Number, ObjectID.Backdrop)

                'Write Position
                Write(File_Number, Backdrop.Rect.X)
                Write(File_Number, Backdrop.Rect.Y)

                'Write Size
                Write(File_Number, Backdrop.Rect.Width)
                Write(File_Number, Backdrop.Rect.Height)

                'Write PatrolA
                Write(File_Number, Backdrop.PatrolA.X)
                Write(File_Number, Backdrop.PatrolA.Y)

                'Write PatrolB
                Write(File_Number, Backdrop.PatrolB.X)
                Write(File_Number, Backdrop.PatrolB.Y)

                'Write Color
                Write(File_Number, Backdrop.Color)

                'Write Text
                Write(File_Number, "Backdrop")

            Next

        End If

        'Write portals to File
        If Portals IsNot Nothing Then

            For Each Portal In Portals

                'Write ID
                Write(File_Number, ObjectID.Portal)

                'Write Position
                Write(File_Number, Portal.Rect.X)
                Write(File_Number, Portal.Rect.Y)

                'Write Size
                Write(File_Number, Portal.Rect.Width)
                Write(File_Number, Portal.Rect.Height)

                'Write PatrolA
                Write(File_Number, Portal.PatrolA.X)
                Write(File_Number, Portal.PatrolA.Y)

                'Write PatrolB
                Write(File_Number, Portal.PatrolB.X)
                Write(File_Number, Portal.PatrolB.Y)

                'Write Color
                Write(File_Number, Portal.Color)

                'Write Text
                Write(File_Number, "Portal")

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

        'Write Color
        Write(File_Number, Color.OrangeRed.ToArgb)

        'Write Text
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

            'BufferGridLines()

        End If

    End Sub

    Private Sub OpenTest2LevelFile(FilePath As String)

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

                    'Read Color
                    FileSystem.Input(File_Number, .Color)

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

            'BufferGridLines()

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

                Case ObjectID.Level

                    Level.Color = FileObject.Color

                Case ObjectID.Spawn

                    'Load Rect Position
                    Spawn.Rect.X = FileObject.Rect.X
                    Spawn.Rect.Y = FileObject.Rect.Y

                    'Load Vec2 Position
                    Spawn.Position.X = FileObject.Rect.X
                    Spawn.Position.Y = FileObject.Rect.Y

                    Hero.Rect.X = Spawn.Rect.X
                    Hero.Rect.Y = Spawn.Rect.Y

                    Hero.Position.X = Spawn.Rect.X
                    Hero.Position.Y = Spawn.Rect.Y

                Case ObjectID.Backdrop

                    AddBackdrop(FileObject.Rect, Color.FromArgb(FileObject.Color))

                Case ObjectID.Portal

                    AddPortal(FileObject.Rect,
                              New Point(FileObject.PatrolA.X, FileObject.PatrolA.Y),
                              New Point(FileObject.PatrolB.X, FileObject.PatrolB.Y))

            End Select

        Next

    End Sub

    Private Sub Form1_MouseMove(sender As Object, e As MouseEventArgs) Handles MyBase.MouseMove

        If GameState = AppState.Editing Then

            MouseMoveEditing(e)

        End If

        If GameState = AppState.Playing Then

            If EditPlayButton.Rect.Contains(e.Location) Then

                EditPlayButtonHover = True

            Else

                EditPlayButtonHover = False

            End If

        End If

        If GameState = AppState.Start Then

            If StartScreenOpenButton.Rect.Contains(e.Location) Then

                StartScreenOpenButtonHover = True

            Else

                StartScreenOpenButtonHover = False

            End If

            If StartScreenNewButton.Rect.Contains(e.Location) Then

                StartScreenNewButtonHover = True

            Else

                StartScreenNewButtonHover = False

            End If

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

            If EnemyToolButton.Rect.Contains(e.Location) Then

                If ShowMenu = False Then

                    EnemyToolButtonHover = True

                End If

            Else

                EnemyToolButtonHover = False

            End If

            If BackdropToolButton.Rect.Contains(e.Location) Then

                If ShowMenu = False Then

                    BackdropToolButtonHover = True

                End If

            Else

                BackdropToolButtonHover = False

            End If

            If GoalToolButton.Rect.Contains(e.Location) Then

                If ShowMenu = False Then

                    GoalToolButtonHover = True

                End If

            Else

                GoalToolButtonHover = False

            End If

            If PortalToolButton.Rect.Contains(e.Location) Then

                If ShowMenu = False Then

                    PortalToolButtonHover = True

                End If

            Else

                PortalToolButtonHover = False

            End If

            If BushToolButton.Rect.Contains(e.Location) Then

                If ShowMenu = False Then

                    BushToolButtonHover = True

                End If

            Else

                BushToolButtonHover = False

            End If

            If CloudToolButton.Rect.Contains(e.Location) Then

                If ShowMenu = False Then

                    CloudToolButtonHover = True

                End If

            Else

                CloudToolButtonHover = False

            End If

            If BillToolButton.Rect.Contains(e.Location) Then

                If ShowMenu = False Then

                    BillToolButtonHover = True

                End If

            Else

                BillToolButtonHover = False

            End If

            If BlockToolButton.Rect.Contains(e.Location) Then

                If ShowMenu = False Then

                    BlockToolButtonHover = True

                End If

            Else

                BlockToolButtonHover = False

            End If

            If PointerToolButton.Rect.Contains(e.Location) Then

                If ShowMenu = False Then

                    PointerToolButtonHover = True

                End If

            Else

                PointerToolButtonHover = False

            End If

            If EditPlayButton.Rect.Contains(e.Location) Then

                If ShowMenu = False Then

                    EditPlayButtonHover = True

                End If

            Else

                EditPlayButtonHover = False

            End If

            If SaveButton.Rect.Contains(e.Location) Then

                If ShowMenu = True Then

                    SaveButtonHover = True

                End If

            Else

                SaveButtonHover = False

            End If

            If OpenButton.Rect.Contains(e.Location) Then

                If ShowMenu = True Then

                    OpenButtonHover = True

                End If

            Else

                OpenButtonHover = False

            End If

            If NewButton.Rect.Contains(e.Location) Then

                If ShowMenu = True Then

                    NewButtonHover = True

                End If

            Else

                NewButtonHover = False

            End If

            If ExitButton.Rect.Contains(e.Location) Then

                If ShowMenu = True Then

                    ExitButtonHover = True

                End If

            Else

                ExitButtonHover = False

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

        If SelectedPortal > -1 Then

            If e.Button = MouseButtons.Left Then

                If PortalEntranceSelected = True Then

                    'Move entrance, snap to grid
                    Portals(SelectedPortal).PatrolA.X = CInt(Math.Round((PointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                    Portals(SelectedPortal).PatrolA.Y = CInt(Math.Round((PointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

                    AutoSizeLevel(New Rectangle(New Point(Portals(SelectedPortal).PatrolA.X, Portals(SelectedPortal).PatrolA.Y), New Drawing.Size(GridSize, GridSize)))

                Else

                    'Move exit, snap to grid
                    Portals(SelectedPortal).PatrolB.X = CInt(Math.Round((PointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                    Portals(SelectedPortal).PatrolB.Y = CInt(Math.Round((PointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

                    AutoSizeLevel(New Rectangle(New Point(Portals(SelectedPortal).PatrolB.X, Portals(SelectedPortal).PatrolB.Y), New Drawing.Size(GridSize, GridSize)))

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

        If SpawnSelected = True Then

            If e.Button = MouseButtons.Left Then

                'Move Spawn, snap to grid
                Spawn.Rect.X = CInt(Math.Round((PointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                Spawn.Rect.Y = CInt(Math.Round((PointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

                AutoSizeLevel(Spawn.Rect)

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

        If SelectedBackdrop > -1 Then

            If e.Button = MouseButtons.Left Then

                'Is the player resizing the backdrop?
                If SizingHandleSelected = True Then
                    'Yes, the player is resizing the backdrop.

                    'Snap backdrop width to grid.
                    Backdrops(SelectedBackdrop).Rect.Width = CInt(Math.Round((PointOffset.X - Backdrops(SelectedBackdrop).Rect.X) / GridSize)) * GridSize

                    'Limit smallest backdrop width to one grid width.
                    If Backdrops(SelectedBackdrop).Rect.Width < GridSize Then Backdrops(SelectedBackdrop).Rect.Width = GridSize

                    'Snap backdrop height to grid.
                    Backdrops(SelectedBackdrop).Rect.Height = CInt(Math.Round((PointOffset.Y - Backdrops(SelectedBackdrop).Rect.Y) / GridSize)) * GridSize

                    'Limit smallest backdrop height to one grid height.
                    If Backdrops(SelectedBackdrop).Rect.Height < GridSize Then Backdrops(SelectedBackdrop).Rect.Height = GridSize

                    AutoSizeLevel(Backdrops(SelectedBackdrop).Rect)

                Else

                    'Snap backdrop to grid
                    Backdrops(SelectedBackdrop).Rect.X = CInt(Math.Round((PointOffset.X - SelectionOffset.X) / GridSize)) * GridSize
                    Backdrops(SelectedBackdrop).Rect.Y = CInt(Math.Round((PointOffset.Y - SelectionOffset.Y) / GridSize)) * GridSize

                    AutoSizeLevel(Backdrops(SelectedBackdrop).Rect)

                End If

            End If

        End If

        If LevelSelected = True Then

            If e.Button = MouseButtons.Left Then

                Camera.Position.X = SelectionOffset.X - e.X

                Camera.Position.Y = SelectionOffset.Y - e.Y

                UpdateCameraOffset()

                'BufferGridLines()

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

                    'BufferGridLines()

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

                        'UpdateCameraOffset()

                        'BufferGridLines()

                    Else

                        MovePointerRight()

                    End If

                End If

                If GameState = AppState.Start Then

                    MovePointerToStartScreenOpenButton()

                End If

            'Has the player pressed the left arrow key down?
            Case Keys.Left
                'Yes, the player has pressed the left arrow key down.

                LeftArrowDown = True

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        MoveCameraLeft()

                        'UpdateCameraOffset()

                        'BufferGridLines()

                    Else

                        MovePointerLeft()

                    End If

                End If

                If GameState = AppState.Start Then

                    MovePointerToStartScreenNewButton()

                End If

            Case Keys.Up

                UpArrowDown = True

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        MoveCameraUp()

                        'UpdateCameraOffset()

                        'BufferGridLines()

                    Else

                        MovePointerUp()

                    End If

                End If

                If GameState = AppState.Start Then

                    MovePointerToStartScreenNewButton()

                End If

            Case Keys.Down

                DownArrowDown = True

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        MoveCameraDown()

                        'UpdateCameraOffset()

                        'BufferGridLines()

                    Else

                        MovePointerDown()

                    End If

                End If

                If GameState = AppState.Start Then

                    MovePointerToStartScreenOpenButton()

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

                        ShowSaveWarning = True

                        'Does the player want to save this level before opening a level?
                        If MsgBox("Changes to " & LevelName & " may be lost." & vbCrLf & "Open a level anyway?",
                                  MsgBoxStyle.Question Or MsgBoxStyle.OkCancel,
                                  "Open Level - Platformer with Level Editor") = MsgBoxResult.Ok Then
                            'No, the player doesn't want to save this level before opening a level?

                            ShowOpenLevelDialog()

                        End If

                        ShowSaveWarning = False

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

                            OpenTest2LevelFile(OpenFileDialog1.FileName)

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

                        ShowSaveWarning = True

                        'Does the player want to save this level before opening a level?
                        If MsgBox("Changes to " & LevelName & " may be lost." & vbCrLf & "Open a level anyway?",
                                  MsgBoxStyle.Question Or MsgBoxStyle.OkCancel,
                                  "Open Level - Platformer with Level Editor") = MsgBoxResult.Ok Then
                            'No, the player doesn't want to save this level before opening a level?

                            ShowOpenLevelDialog()

                        End If

                        ShowSaveWarning = False

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

                            OpenTest2LevelFile(OpenFileDialog1.FileName)

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

                    If SelectedBackdrop > -1 Then

                        RemoveBackdrop(SelectedBackdrop)

                        SelectedBackdrop = -1

                    End If

                    If GoalSelected = True Then

                        'Place goal off level.
                        Goal.Rect.X = -100
                        Goal.Rect.Y = -100

                        GoalSelected = False

                    End If

                End If

            Case Keys.M 'Mute

                If IsMKeyDown = False Then

                    IsMKeyDown = True

                    If IsPlaying("Music") = True Then

                        PauseSound("Music")

                        IsMuted = True

                    Else

                        LoopSound("Music")

                        IsMuted = False

                    End If

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

                            DeselectObjects()

                            ShowMenu = True

                            'MovePointerCenterMenu()

                            MovePointerOverSaveButton()

                        Else

                            ShowMenu = False

                        End If

                    End If

                End If

            Case Keys.PageUp

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        SelectNextToolToTheLeft()

                    End If

                End If

            Case Keys.PageDown

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        SelectNextToolToTheRight()

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

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        If Camera.Velocity.X > 0 Then

                            'Stop Camera
                            Camera.Velocity.X = 0 'Zero speed.

                            'UpdateCameraOffset()

                            'BufferGridLines()

                        End If

                    Else

                        If MousePointer.Velocity.X > 0 Then

                            MousePointer.Velocity.X = 0 'Zero speed.

                        End If

                    End If

                End If

                If GameState = AppState.Start Then

                    If MousePointer.Velocity.X > 0 Then

                        MousePointer.Velocity.X = 0 'Zero speed.

                    End If

                End If

            Case Keys.Left

                LeftArrowDown = False

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        If Camera.Velocity.X < 0 Then

                            'Stop Camera
                            Camera.Velocity.X = 0 'Zero speed.

                            'UpdateCameraOffset()

                            'BufferGridLines()

                        End If

                    Else

                        If MousePointer.Velocity.X < 0 Then

                            'Stop mouse pointer
                            MousePointer.Velocity.X = 0 'Zero speed.

                        End If

                    End If

                End If

                If GameState = AppState.Start Then

                    If MousePointer.Velocity.X < 0 Then

                        MousePointer.Velocity.X = 0 'Zero speed.

                    End If

                End If

            Case Keys.Up

                UpArrowDown = False

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        'Is the camera moving up?
                        If Camera.Velocity.Y < 0 Then
                            'Yes, the camera is moving up.

                            'Stop Camera
                            Camera.Velocity.Y = 0 'Zero speed.

                            'UpdateCameraOffset()

                            'BufferGridLines()

                        End If

                    Else

                        If MousePointer.Velocity.Y < 0 Then

                            'Stop mouse pointer
                            MousePointer.Velocity.Y = 0 'Zero speed.

                        End If

                    End If

                End If

                If GameState = AppState.Start Then

                    If MousePointer.Velocity.Y < 0 Then

                        MousePointer.Velocity.Y = 0 'Zero speed.

                    End If

                End If

            Case Keys.Down

                DownArrowDown = False

                If GameState = AppState.Editing Then

                    If ShowMenu = False Then

                        'Is the camera moving down?
                        If Camera.Velocity.Y > 0 Then
                            'Yes, the camera is moving down.

                            'Stop Camera
                            Camera.Velocity.Y = 0 'Zero speed.

                            'UpdateCameraOffset()

                            'BufferGridLines()

                        End If

                    Else

                        If MousePointer.Velocity.Y > 0 Then

                            'Stop mouse pointer
                            MousePointer.Velocity.Y = 0 'Zero speed.

                        End If

                    End If

                End If

                If GameState = AppState.Start Then

                    If MousePointer.Velocity.Y > 0 Then

                        MousePointer.Velocity.Y = 0 'Zero speed.

                    End If

                End If

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

            Case Keys.M

                IsMKeyDown = False

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

        DoButtonLogic()

    End Sub

    Private Sub DoButtonLogic()

        DoLetterButtonLogic()

        DoDPadLogic()

        DoStartBackLogic()

        DoBumperLogic()

        DoStickLogic()

    End Sub

    Private Sub DoStickLogic()

        If LeftStickButtonPressed = True Then

            If GameState = AppState.Start Or GameState = AppState.Editing Then

                If IsLeftStickDown = False Then

                    IsLeftStickDown = True

                    DoMouseLeftDown()

                End If

            End If

        Else

            If GameState = AppState.Start Or GameState = AppState.Editing Then

                If IsLeftStickDown = True Then

                    IsLeftStickDown = False

                    DoMouseLeftUp()

                End If

            End If

        End If

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

                If SelectedBackdrop > -1 Then

                    RemoveBackdrop(SelectedBackdrop)

                    SelectedBackdrop = -1

                End If

                If GoalSelected = True Then

                    'Place goal off level.
                    Goal.Rect.X = -100
                    Goal.Rect.Y = -100

                    GoalSelected = False

                End If

            End If

            If GameState = AppState.Start Then

                If ShowOpenFileDialog = False Then

                    MovePointerToStartScreenNewButton()

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

                If SelectedBackdrop > -1 Then

                    RemoveBackdrop(SelectedBackdrop)

                    SelectedBackdrop = -1

                End If

                If GoalSelected = True Then

                    'Place goal off level.
                    Goal.Rect.X = -100
                    Goal.Rect.Y = -100

                    GoalSelected = False

                End If

            End If

            If GameState = AppState.Start Then

                If ShowOpenFileDialog = False Then

                    MovePointerToStartScreenOpenButton()

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

                        'BufferGridLines()

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

                        DeselectObjects()

                        ShowMenu = True

                        'MovePointerCenterMenu()

                        MovePointerOverSaveButton()

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

            If GameState = AppState.Editing Then


                If ShowMenu = False Then

                    MovePointerLeftDPad()

                Else

                    If ShowSaveWarning = True Or ShowSaveFileDialog = True Then

                        MovePointerLeftDPad()

                    End If

                End If

            End If

            If GameState = AppState.Start Then

                If ShowOpenFileDialog = True Then

                    MovePointerLeftDPad()

                Else

                    MovePointerToStartScreenNewButton()

                End If

            End If

        Else

            ControllerLeft = False

        End If

        If DPadRightPressed = True Then

            ControllerRight = True

            If GameState = AppState.Editing Then

                If ShowMenu = False Then

                    MovePointerRightDPad()

                Else

                    If ShowSaveWarning = True Or ShowSaveFileDialog = True Then

                        MovePointerRightDPad()

                    End If

                End If

            End If

            If GameState = AppState.Start Then

                If ShowOpenFileDialog = True Then

                    MovePointerRightDPad()

                Else

                    MovePointerToStartScreenOpenButton()

                End If

            End If

        Else

            ControllerRight = False

        End If

        If DPadUpPressed = True Then

            If GameState = AppState.Editing Then

                If ShowMenu = True Then

                    If ShowSaveWarning = True Or ShowSaveFileDialog = True Then

                        MovePointerUpDPad()

                    Else

                        If IsDPadUp = False Then

                            IsDPadUp = True

                            Dim MousePointerOffset As Point = MousePointer.Rect.Location

                            'Convert mouse pointer from screen coordinates to client coordinates.
                            MousePointerOffset.X -= ScreenOffset.X
                            MousePointerOffset.Y -= ScreenOffset.Y

                            'Is the mouse pointer on the menu?
                            If Not MenuBackground.Rect.Contains(MousePointerOffset) Then
                                'No, the mouse pointer is not on the menu.

                                MovePointerOverExitButton()

                            End If

                            'Is the mouse pointer on the open button?
                            If OpenButton.Rect.Contains(MousePointerOffset) Then
                                'Yes, the mouse pointer is on the open button.

                                MovePointerOverSaveButton()

                            End If

                            'Is the mouse pointer on the new button?
                            If NewButton.Rect.Contains(MousePointerOffset) Then
                                'Yes, the mouse pointer is on the new button.

                                MovePointerOverOpenButton()

                            End If

                            'Is the mouse pointer on the exit button?
                            If ExitButton.Rect.Contains(MousePointerOffset) Then
                                'Yes, the mouse pointer is on the exit button.

                                MovePointerOverNewButton()

                            End If

                            'Is the mouse pointer on the save button?
                            If SaveButton.Rect.Contains(MousePointerOffset) Then
                                'Yes, the mouse pointer is on the save button.

                                MovePointerOverExitButton()

                            End If

                        End If

                    End If

                Else

                    MovePointerUpDPad()

                End If

            End If

            If GameState = AppState.Start Then

                If ShowOpenFileDialog = True Then

                    MovePointerUpDPad()

                Else

                    MovePointerToStartScreenNewButton()

                End If

            End If

            If GameState = AppState.Playing Then

                ControllerUp = True

            End If

        Else

            If GameState = AppState.Playing Then

                ControllerUp = False

            End If

            If GameState = AppState.Editing Then

                IsDPadUp = False

            End If

        End If

        If DPadDownPressed = True Then

            If GameState = AppState.Editing Then

                If ShowMenu = True Then

                    If ShowSaveWarning = True Or ShowSaveFileDialog = True Then

                        MovePointerDownDPad()

                    Else

                        If IsDPadDown = False Then

                            IsDPadDown = True

                            Dim MousePointerOffset As Point = MousePointer.Rect.Location

                            MousePointerOffset.X -= ScreenOffset.X
                            MousePointerOffset.Y -= ScreenOffset.Y

                            'Is the mouse pointer on the menu?
                            If Not MenuBackground.Rect.Contains(MousePointerOffset) Then
                                'No, the mouse pointer is not on the menu.

                                MovePointerOverSaveButton()

                            End If

                            'Is the mouse pointer on the open button?
                            If OpenButton.Rect.Contains(MousePointerOffset) Then
                                'Yes, the mouse pointer is on the open button.

                                MovePointerOverNewButton()

                            End If

                            'Is the mouse pointer on the save button?
                            If SaveButton.Rect.Contains(MousePointerOffset) Then
                                'Yes, the mouse pointer is on the save button.

                                MovePointerOverOpenButton()

                            End If

                            'Is the mouse pointer on the new button?
                            If NewButton.Rect.Contains(MousePointerOffset) Then
                                'Yes, the mouse pointer is on the new button.

                                MovePointerOverExitButton()

                            End If

                            'Is the mouse pointer on the exit button?
                            If ExitButton.Rect.Contains(MousePointerOffset) Then
                                'Yes, the mouse pointer is on the exit button.

                                MovePointerOverSaveButton()

                            End If

                        End If

                    End If

                Else

                    MovePointerDownDPad()

                End If

            End If


            If GameState = AppState.Start Then

                If ShowOpenFileDialog = True Then

                    MovePointerDownDPad()

                Else

                    MovePointerToStartScreenOpenButton()

                End If

            End If

        Else

            IsDPadDown = False

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

                    IsMouseDown = True

                    DoMouseLeftDown()

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

                    MovePointerOverSaveButton()

                    If IsMouseDown = False Then

                        DoMouseLeftDown()

                        IsMouseDown = True

                    End If

                End If

            End If

            If GameState = AppState.Start Then
                'B is the controller shortcut to create a new level.

                If ShowOpenFileDialog = False Then

                    'Move mouse pointer over the new level button.
                    Cursor.Position = New Point(ScreenOffset.X + StartScreenNewButton.Rect.X + StartScreenNewButton.Rect.Width \ 2,
                                                ScreenOffset.Y + StartScreenNewButton.Rect.Y + StartScreenNewButton.Rect.Height \ 2)

                    If IsMouseDown = False Then

                        DoMouseLeftDown()

                        IsMouseDown = True

                    End If

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

                    MovePointerOverOpenButton()

                    If IsMouseDown = False Then

                        DoMouseLeftDown()

                        IsMouseDown = True

                    End If

                End If

            End If

            If GameState = AppState.Start Then
                'Y is the controller shortcut to open a level.

                If ShowOpenFileDialog = False Then

                    'Move mouse pointer over the open button.
                    Cursor.Position = New Point(ScreenOffset.X + StartScreenOpenButton.Rect.X + StartScreenOpenButton.Rect.Width \ 2,
                                                ScreenOffset.Y + StartScreenOpenButton.Rect.Y + StartScreenOpenButton.Rect.Height \ 2)

                    If IsMouseDown = False Then

                        DoMouseLeftDown()

                        IsMouseDown = True

                    End If

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

            DeathRumble()

            ResetCash()

            ResurrectEnemies()

            ResetOurHero()

            FrameHero()

            UpdateCameraOffset()

            'BufferGridLines()

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

                    'BufferGridLines()

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

            ShowOpenFileDialog = True

            If OpenFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then

                If My.Computer.FileSystem.FileExists(OpenFileDialog1.FileName) = True Then

                    InitializeObjects()

                    OpenTest2LevelFile(OpenFileDialog1.FileName)

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

            ShowOpenFileDialog = False

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

            If e.Button = MouseButtons.Left Or e.Button = MouseButtons.Middle Then

                MouseDownEditingSelection(e.Location)

                MouseDownEditingButtons(e.Location)

            End If

            If e.Button = MouseButtons.Right Then

                DeselectObjects()

                ShowMenu = True

                'MovePointerCenterMenu()

                MovePointerOverSaveButton()

            End If

        Else

            If e.Button = MouseButtons.Left Or e.Button = MouseButtons.Middle Then

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

            DeselectObjects()

            ShowSaveWarning = True

            'Does the player want to save this level before opening a level?
            If MsgBox("Changes to " & LevelName & " may be lost." & vbCrLf & "Open a level anyway?",
                      MsgBoxStyle.Question Or MsgBoxStyle.OkCancel,
                      "Open Level - Platformer with Level Editor") = MsgBoxResult.Ok Then
                'No, the player doesn't want to save this level before opening a level?

                ShowOpenLevelDialog()

            End If

            ShowSaveWarning = False

        End If

        'Is the player selecting the new button?
        If NewButton.Rect.Contains(e) Then
            'Yes, the player is selecting the new button.

            ShowSaveWarning = True

            'Does the player want to save this level before creating a new level?
            If MsgBox("Changes to " & LevelName & " may be lost." & vbCrLf & "Create a new level anyway?",
                      MsgBoxStyle.Question Or MsgBoxStyle.OkCancel,
                      "New Level - Platformer with Level Editor") = MsgBoxResult.Ok Then
                'No, the player doesn't want to save this level before creating a new level?

                InitAndCreateNewLevel()

                ShowMenu = False

            End If

            ShowSaveWarning = False

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

            'BufferGridLines()

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

        Title.Rect = New Rectangle(ClientRectangle.Width \ 2 - 425,
                                   ClientRectangle.Height \ 2 - 175,
                                   850,
                                   245)

        StartScreenNewButton.Rect = New Rectangle(ClientRectangle.Width \ 2 - 230,
                                                  ClientRectangle.Height \ 2 + 70,
                                                  210,
                                                  90)

        StartScreenOpenButton.Rect = New Rectangle(ClientRectangle.Width \ 2 + 20,
                                                   ClientRectangle.Height \ 2 + 70,
                                                   210,
                                                   90)

    End Sub

    Private Sub ResizeHUD()

        CashCollectedPostion.Y = ClientRectangle.Top + 5

        EditPlayButton.Rect = New Rectangle(ClientRectangle.Left + 210,
                                            ClientRectangle.Bottom - 90,
                                            120,
                                            90)

        'Place the FPS display at the bottom of the client area.
        FPS_Postion.Y = ClientRectangle.Bottom - 75

    End Sub

    Private Sub ResizeMenu()

        MenuBackground.Rect = New Rectangle((ClientRectangle.Width \ 2) - MenuBackground.Rect.Width \ 2,
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
                                        MenuBackground.Rect.Top + 86 * 3, 290,
                                        80)

        MenuButton.Rect = New Rectangle(ClientRectangle.Right - 90,
                                        ClientRectangle.Bottom - 90,
                                        90,
                                        90)

    End Sub

    Private Sub ResizeToolBar()

        ToolBarBackground.Rect = New Rectangle(ClientRectangle.Left,
                                               ClientRectangle.Bottom - 90,
                                               ClientRectangle.Width,
                                               100)

        PointerToolButton.Rect = New Rectangle(ClientRectangle.Left + 331,
                                               ClientRectangle.Bottom - 90,
                                               90,
                                               90)

        BlockToolButton.Rect = New Rectangle(ClientRectangle.Left + 422,
                                             ClientRectangle.Bottom - 90,
                                             90,
                                             90)

        BlockToolIcon.Rect = New Rectangle(ClientRectangle.Left + 435,
                                           ClientRectangle.Bottom - 77,
                                           64,
                                           64)

        BillToolButton.Rect = New Rectangle(ClientRectangle.Left + 513,
                                            ClientRectangle.Bottom - 90,
                                            90,
                                            90)

        BillToolIcon.Rect = New Rectangle(ClientRectangle.Left + 526,
                                          ClientRectangle.Bottom - 77,
                                          64,
                                          64)

        BushToolButton.Rect = New Rectangle(ClientRectangle.Left + 604,
                                            ClientRectangle.Bottom - 90,
                                            90,
                                            90)

        BushToolIcon.Rect = New Rectangle(ClientRectangle.Left + 618,
                                          ClientRectangle.Bottom - 77,
                                          64,
                                          64)

        CloudToolButton.Rect = New Rectangle(ClientRectangle.Left + 695,
                                             ClientRectangle.Bottom - 90,
                                             90,
                                             90)

        CloundToolIcon.Rect = New Rectangle(ClientRectangle.Left + 708,
                                            ClientRectangle.Bottom - 77,
                                            64,
                                            64)

        GoalToolButton.Rect = New Rectangle(ClientRectangle.Left + 786,
                                            ClientRectangle.Bottom - 90,
                                            90,
                                            90)

        GoalToolIcon.Rect = New Rectangle(ClientRectangle.Left + 798,
                                          ClientRectangle.Bottom - 77,
                                          64,
                                          64)

        PortalToolButton.Rect = New Rectangle(ClientRectangle.Left + 1059,
                                              ClientRectangle.Bottom - 90,
                                              90,
                                              90)

        PortalToolIcon.Rect = New Rectangle(ClientRectangle.Left + 1072,
                                            ClientRectangle.Bottom - 77,
                                            64,
                                            64)

        EnemyToolButton.Rect = New Rectangle(ClientRectangle.Left + 877,
                                             ClientRectangle.Bottom - 90,
                                             90,
                                             90)

        EnemyToolIcon.Rect = New Rectangle(ClientRectangle.Left + 890,
                                           ClientRectangle.Bottom - 77,
                                           64,
                                           64)

        BackdropToolButton.Rect = New Rectangle(ClientRectangle.Left + 968,
                                                ClientRectangle.Bottom - 90,
                                                90,
                                                90)

        BackdropToolIcon.Rect = New Rectangle(ClientRectangle.Left + 981,
                                              ClientRectangle.Bottom - 77,
                                              64,
                                              64)

    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles MyBase.Closing

        GameLoopCancellationToken.Cancel(True)

        CloseSounds()

    End Sub

    Private Sub UpdateCamera()

        LookAhead()

        KeepCameraOnTheLevel()

    End Sub

    Private Sub FrameHero()

        Camera.Position.X = Hero.Rect.X - (Camera.Rect.Width / 2)

        Camera.Position.Y = Hero.Rect.Y - (Camera.Rect.Height / 2) - (Camera.Rect.Height / 1.25) - Hero.Rect.Height

        UpdateCameraOffset()

    End Sub

    Private Sub LookAhead()

        'Is our hero near the right side of the frame?
        If Hero.Position.X > Camera.Position.X + Camera.Rect.Width / 1.5 Then
            'If Hero.X > Camera.X + Camera.Width / 1.5 Then
            'Yes, our hero is near the right side of the frame.

            'Move camera to the right.
            Camera.Position.X = Hero.Rect.Left - Camera.Rect.Width / 1.5
            'Camera.X = Hero.Left - Camera.Width / 1.5

            'UpdateCameraOffset()

        End If

        'Is our hero near the left side of the frame?
        If Hero.Rect.X < Camera.Position.X + Camera.Rect.Width / 4 Then
            'If Hero.X < Camera.X + Camera.Width / 4 Then
            'Yes, our hero is near the left side of the frame.

            'Move camera to the left.
            Camera.Position.X = Hero.Rect.Left - Camera.Rect.Width / 4
            'Camera.X = Hero.Left - Camera.Width / 4

            'UpdateCameraOffset()

        End If

        'Is our hero near the bottom side of the frame?
        If Hero.Rect.Y > Camera.Position.Y + Camera.Rect.Height / 1.25 Then
            'If Hero.Y > Camera.Y + Camera.Height / 1.25 Then
            'Yes, our hero is near the bottom side of the frame.

            'Move camera down.
            Camera.Position.Y = Hero.Rect.Top - Camera.Rect.Height / 1.25
            'Camera.Y = Hero.Top - Camera.Height / 1.25

            'UpdateCameraOffset()

        End If

        'Is our hero near the top side of the frame?
        If Hero.Rect.Y < Camera.Position.Y + Camera.Rect.Height / 6 Then
            'Yes, our hero is near the top side of the frame.

            'Move camera up.
            Camera.Position.Y = Hero.Rect.Top - Camera.Rect.Height / 6

            'UpdateCameraOffset()

        End If

    End Sub

    Private Sub KeepCameraOnTheLevel()

        'Has the camera moved passed the left side of the level?
        If Camera.Position.X < Level.Rect.Left Then
            'Yes, the camera has moved pass the left side of the level.

            'Limit the camera movement to the left side of the level.
            Camera.Position.X = Level.Rect.Left

            'UpdateCameraOffset()

        End If

        'Has the camera moved passed the right side of the level?
        If Camera.Position.X + Camera.Rect.Width > Level.Rect.Right Then
            'Yes, the camera has moved pass the right side of the level.

            'Limit the camera movement to the right side of the level.
            Camera.Position.X = Level.Rect.Right - Camera.Rect.Width

            'UpdateCameraOffset()

        End If

        'Has the camera moved passed the top side of the level?
        If Camera.Position.Y < Level.Rect.Top Then
            'Yes, the camera has moved passed the top side of the level.

            'Limit camera movement to the top side of the level.
            Camera.Position.Y = Level.Rect.Top

            'UpdateCameraOffset()

        End If

        'Has the camera moved passed the bottom side of the level?
        If Camera.Position.Y + Camera.Rect.Height > Level.Rect.Bottom Then
            'Yes, the camera has moved pass the bottom side of the level.

            'Limit camera movement to the bottom of the level.
            Camera.Position.Y = Level.Rect.Bottom - Camera.Rect.Height

            'UpdateCameraOffset()

        End If

    End Sub

    Private Sub UpdateCameraOffset()

        Camera.Rect.X = Camera.Position.X

        Camera.Rect.Y = Camera.Position.Y

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

    Private Sub CreateSoundFilesFromResources()

        Dim File As String = Path.Combine(Application.StartupPath, "level.mp3")
        CreateSoundFileFromResource(File, My.Resources.level)

        File = Path.Combine(Application.StartupPath, "CashCollected.mp3")
        CreateSoundFileFromResource(File, My.Resources.CashCollected)

        File = Path.Combine(Application.StartupPath, "eliminated.mp3")
        CreateSoundFileFromResource(File, My.Resources.eliminated)

        File = Path.Combine(Application.StartupPath, "clear.mp3")
        CreateSoundFileFromResource(File, My.Resources.clear)

        File = Path.Combine(Application.StartupPath, "Portal.mp3")
        CreateSoundFileFromResource(File, My.Resources.Portal2)

    End Sub

    Private Sub CreateSoundFileFromResource(File As String, Resource As Byte())
        'Create sound file as needed.

        'Has the file been made?
        If Not IO.File.Exists(File) Then
            'No, the file hasn't been made.

            'Make the file.
            IO.File.WriteAllBytes(File, Resource)

        End If

    End Sub

    Private Shared Sub CreateDemoFileFromResource()
        'Create demo file as needed.

        Dim File As String = Path.Combine(Application.StartupPath, "Demo.txt")

        'Has the file been made?
        If Not IO.File.Exists(File) Then
            'No, the file hasn't been made.

            'Make the file.
            IO.File.WriteAllText(File, My.Resources.Demo23)

        End If

    End Sub

    Private Sub VibrateLeft(ControllerNumber As Integer, Speed As UShort)
        'The range of speed is 0 through 65,535. Unsigned 16-bit (2-byte) integer.
        'The left motor is the low-frequency rumble motor.

        'Set left motor speed.
        Vibration.wLeftMotorSpeed = Speed

        SendVibrationMotorCommand(ControllerNumber)

        LeftVibrateStart = Now

        IsLeftVibrating = True

    End Sub

    Private Sub VibrateRight(ControllerNumber As Integer, Speed As UShort)
        'The range of speed is 0 through 65,535. Unsigned 16-bit (2-byte) integer.
        'The right motor is the high-frequency rumble motor.

        'Set right motor speed.
        Vibration.wRightMotorSpeed = Speed

        SendVibrationMotorCommand(ControllerNumber)

        RightVibrateStart = Now

        IsRightVibrating = True

    End Sub

    Private Sub SendVibrationMotorCommand(ControllerNumber As Integer)
        'Sends vibration motor speed command to the specified controller.

        Try

            'Send motor speed command to the specified controller.
            If XInputSetState(ControllerNumber, Vibration) = 0 Then
                'The motor speed was set. Success.
            Else
                'The motor speed was not set. Fail.
                'Text = XInputSetState(ControllerNumber, vibration).ToString
            End If

        Catch ex As Exception

            DisplayError(ex)

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

        Cursor.Position = New Point(ScreenOffset.X + MenuBackground.Rect.X + MenuBackground.Rect.Width \ 2,
                                    ScreenOffset.Y + MenuBackground.Rect.Y + MenuBackground.Rect.Height \ 2)

    End Sub

    Private Sub MovePointerToStartScreenNewButton()
        'Move mouse pointer over the new level button on the start screen.

        Cursor.Position = New Point(ScreenOffset.X + StartScreenNewButton.Rect.X + StartScreenNewButton.Rect.Width \ 2,
                                    ScreenOffset.Y + StartScreenNewButton.Rect.Y + StartScreenNewButton.Rect.Height \ 2)

    End Sub

    Private Sub MovePointerToStartScreenOpenButton()
        'Move mouse pointer over the open level button on the start screen.

        Cursor.Position = New Point(ScreenOffset.X + StartScreenOpenButton.Rect.X + StartScreenOpenButton.Rect.Width \ 2,
                                    ScreenOffset.Y + StartScreenOpenButton.Rect.Y + StartScreenOpenButton.Rect.Height \ 2)

    End Sub

    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)

        'Intentionally left blank. Do not remove.

    End Sub

    Private Sub Form1_MouseWheel(sender As Object, e As MouseEventArgs) Handles MyBase.MouseWheel

        'Is the player rolling the mouse wheel up or down?
        If e.Delta > 0 Then
            'The player is rolling the mouse wheel up.

            If GameState = AppState.Editing Then

                MouseWheelUpEditing(e.Location)

            End If

            If GameState = AppState.Start Then

                MovePointerToStartScreenNewButton()

            End If

        Else
            'The player is rolling the mouse wheel down.

            If GameState = AppState.Editing Then

                MouseWheelDownEditing(e.Location)

            End If

            If GameState = AppState.Start Then

                MovePointerToStartScreenOpenButton()

            End If

        End If

    End Sub

    Private Sub MouseWheelDownEditing(MousePointer As Point)

        If ShowMenu = False Then

            SelectNextToolToTheRight()

        Else

            MouseWheelDownEditingShowMenuTrue(MousePointer)

        End If

    End Sub

    Private Sub MouseWheelDownEditingShowMenuTrue(MousePointer As Point)

        'Is the mouse pointer on the menu?
        If Not MenuBackground.Rect.Contains(MousePointer) Then
            'No, the mouse pointer is not on the menu.

            MovePointerOverSaveButton()

        End If

        'Is the mouse pointer on the open button?
        If OpenButton.Rect.Contains(MousePointer) Then
            'Yes, the mouse pointer is on the open button.

            MovePointerOverNewButton()

        End If

        'Is the mouse pointer on the save button?
        If SaveButton.Rect.Contains(MousePointer) Then
            'Yes, the mouse pointer is on the save button.

            MovePointerOverOpenButton()

        End If

        'Is the mouse pointer on the new button?
        If NewButton.Rect.Contains(MousePointer) Then
            'Yes, the mouse pointer is on the new button.

            MovePointerOverExitButton()

        End If

        'Is the mouse pointer on the exit button?
        If ExitButton.Rect.Contains(MousePointer) Then
            'Yes, the mouse pointer is on the exit button.

            MovePointerOverSaveButton()

        End If

    End Sub

    Private Sub MouseWheelUpEditing(MousePointer As Point)

        If ShowMenu = False Then

            SelectNextToolToTheLeft()

        Else

            MouseWheelUpEditingShowMenuTrue(MousePointer)

        End If

    End Sub

    Private Sub MouseWheelUpEditingShowMenuTrue(MousePointer As Point)

        'Is the mouse pointer on the menu?
        If Not MenuBackground.Rect.Contains(MousePointer) Then
            'No, the mouse pointer is not on the menu.

            MovePointerOverExitButton()

        End If

        'Is the mouse pointer on the open button?
        If OpenButton.Rect.Contains(MousePointer) Then
            'Yes, the mouse pointer is on the open button.

            MovePointerOverSaveButton()

        End If

        'Is the mouse pointer on the new button?
        If NewButton.Rect.Contains(MousePointer) Then
            'Yes, the mouse pointer is on the new button.

            MovePointerOverOpenButton()

        End If

        'Is the mouse pointer on the exit button?
        If ExitButton.Rect.Contains(MousePointer) Then
            'Yes, the mouse pointer is on the exit button.

            MovePointerOverNewButton()

        End If

        'Is the mouse pointer on the save button?
        If SaveButton.Rect.Contains(MousePointer) Then
            'Yes, the mouse pointer is on the save button.

            MovePointerOverExitButton()

        End If

    End Sub

    Private Sub MovePointerOverExitButton()

        'Move mouse pointer over the exit button.
        Cursor.Position = New Point(ScreenOffset.X + ExitButton.Rect.X + ExitButton.Rect.Width \ 2,
                                    ScreenOffset.Y + ExitButton.Rect.Y + ExitButton.Rect.Height \ 2)

    End Sub

    Private Sub MovePointerOverOpenButton()

        'Move mouse pointer over the open level button.
        Cursor.Position = New Point(ScreenOffset.X + OpenButton.Rect.X + OpenButton.Rect.Width \ 2,
                                    ScreenOffset.Y + OpenButton.Rect.Y + OpenButton.Rect.Height \ 2)

    End Sub

    Private Sub MovePointerOverNewButton()

        'Move mouse pointer over the new level button.
        Cursor.Position = New Point(ScreenOffset.X + NewButton.Rect.X + NewButton.Rect.Width \ 2,
                                    ScreenOffset.Y + NewButton.Rect.Y + NewButton.Rect.Height \ 2)

    End Sub

    Private Sub MovePointerOverSaveButton()

        'Move mouse pointer over the save level button.
        Cursor.Position = New Point(ScreenOffset.X + SaveButton.Rect.X + SaveButton.Rect.Width \ 2,
                                    ScreenOffset.Y + SaveButton.Rect.Y + SaveButton.Rect.Height \ 2)

    End Sub

    Private Sub SelectNextToolToTheRight()

        'Has the player reached the right end of the toolbar?
        If SelectedTool = Tools.Portal Then
            'Yes, the player has reached the right end of the toolbar.

            'Start over by selecting the first tool on the toolbar. Far left end.
            SelectedTool = Tools.Pointer

            ShowToolPreview = False

        Else

            DeselectObjects()

            ToolPreview.Width = GridSize
            ToolPreview.Height = GridSize

            'Select the next tool to the right on the toolbar.
            SelectedTool += 1

            If SelectedTool = Tools.Pointer Then

                ShowToolPreview = False

            Else

                ShowToolPreview = True

            End If

        End If

    End Sub

    Private Sub SelectNextToolToTheLeft()

        'Has the player reached the left end of the toolbar?
        If SelectedTool = Tools.Pointer Then
            'Yes, the player has reached the left end of the toolbar.

            DeselectObjects()

            ToolPreview.Width = GridSize
            ToolPreview.Height = GridSize

            'Start over by selecting the last tool on the toolbar. Far right end.
            SelectedTool = Tools.Portal

            ShowToolPreview = True

        Else

            DeselectObjects()

            ToolPreview.Width = GridSize
            ToolPreview.Height = GridSize

            'Select the next tool to the left on the toolbar.
            SelectedTool -= 1

            If SelectedTool = Tools.Pointer Then

                ShowToolPreview = False

            Else

                ShowToolPreview = True

            End If

        End If

    End Sub

    Private Sub Form1_DoubleClick(sender As Object, e As EventArgs) Handles MyBase.DoubleClick

        If GameState = AppState.Editing Then

            If ShowMenu = False Then

                'Is player double clicking on a game object that can have its color set?
                If SizingHandleSelected = False AndAlso
                SelectedBlock = -1 AndAlso
                SelectedBill = -1 AndAlso
                SelectedBush = -1 AndAlso
                SelectedCloud = -1 AndAlso
                GoalSelected = False AndAlso
                SelectedEnemy = -1 AndAlso
                SpawnSelected = False AndAlso
                SelectedPortal = -1 Then
                    'Yes, the player is double clicking a game object that can have its color set.

                    ShowColorPicker()

                End If

            End If

        End If

    End Sub

    Private Sub ShowColorPicker()

        'Is the game object a backdrop?
        If SelectedBackdrop > -1 Then
            'Yes, the game object is a backdrop.

            ColorDialog1.Color = Color.FromArgb(Backdrops(SelectedBackdrop).Color)

            ColorDialog1.FullOpen = True

            If ColorDialog1.ShowDialog() = DialogResult.OK Then

                Backdrops(SelectedBackdrop).Color = ColorDialog1.Color.ToArgb

            End If

        Else
            'No, then the game object is the level.

            ColorDialog1.Color = Color.FromArgb(Level.Color)

            ColorDialog1.FullOpen = True

            If ColorDialog1.ShowDialog() = DialogResult.OK Then

                Level.Color = ColorDialog1.Color.ToArgb

            End If

        End If

    End Sub

End Class


'Monica is our an AI assistant. She helped with the Dll imports.
'https://monica.im/


'I also make coding videos on my YouTube channel.
'https://www.youtube.com/@codewithjoe6074

