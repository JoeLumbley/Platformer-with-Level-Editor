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

'Level music by Joseph Lumbley Jr.

'Monica is our an AI assistant.
'https://monica.im/

'I'm making a video to explain the code on my YouTube channel.
'https://www.youtube.com/@codewithjoe6074
'

Imports System.ComponentModel
Imports System.IO
Imports System.Numerics
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Windows

Public Class Form1

    Private Enum AppState As Integer
        Start
        Playing
        Editing
    End Enum

    Private Enum ObjectID As Integer
        Level
        Block
        Bill
        Bush
        Cloud
    End Enum

    Private Enum Tools As Integer
        Pointer
        Block
        Bill
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

    Private Gravity As Single = 3000

    Private AirResistance As Single = 100.0F

    '500 slippery 1000 grippy
    Private Friction As Single = 1500

    Private OurHero As GameObject

    Private Platforms() As GameObject

    Private Blocks() As GameObject

    Private Clouds() As GameObject

    Private Bushes() As GameObject

    Private Cash() As GameObject

    Private FileObjects() As GameObject

    Private EditPlayButton As GameObject

    Private SaveButton As GameObject

    Private ToolBarBackground As GameObject

    Private PointerToolButton As GameObject

    Private BlockToolButton As GameObject

    Private BlockToolIcon As GameObject

    Private SelectedTool As Tools = Tools.Pointer

    Private ShowToolPreview As Boolean = False

    Private Title As GameObject

    Private StartScreenOpenButton As GameObject

    Private StartScreenNewButton As GameObject

    Private ScoreIndicators As GameObject

    Private ToolPreview As Rectangle

    Private SelectedCloud As Integer = -1

    Private SelectedBlock As Integer = -1

    Private SelectedPlatform As Integer = -1

    Private SelectedBill As Integer = -1

    Private SelectedBush As Integer = -1

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

    Private ReadOnly TitleFont As New Font(New FontFamily("Bahnschrift"), 38, FontStyle.Bold)

    Private OutinePen As New Pen(Color.Black, 4)

    Private LightSkyBluePen As New Pen(Color.LightSkyBlue, 4)

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

    Private GameLoopCancellationToken As New CancellationTokenSource()

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

    Public Shared Sub ClickMouseLeft()
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
    Public Shared Sub DoMouseLeftDown()

        ' Simulate a left mouse button down event
        Dim inputDown As New INPUTStruc()
        inputDown.type = INPUT_MOUSE
        inputDown.union.mi.dwFlags = MOUSEEVENTF_LEFTDOWN

        ' Send the input events using SendInput
        Dim inputs As INPUTStruc() = {inputDown}
        SendInput(CUInt(inputs.Length), inputs, Marshal.SizeOf(GetType(INPUTStruc)))

    End Sub

    Public Shared Sub DoMouseLeftUp()

        ' Simulate a left mouse button up event
        Dim inputUp As New INPUTStruc()
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

        InitializeGameObjects()

        InitializeToolBarButtons()

        InitializeForm()

        InitializeBuffer()

        Title.Text = "Platformer" & vbCrLf & "with Level Editor"

        OutinePen.LineJoin = Drawing2D.LineJoin.Round

        My.Computer.Audio.Play(My.Resources.level,
        AudioPlayMode.BackgroundLoop)

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

    Private Sub InitializeGameObjects()

        OurHero.Rect = New Rectangle(100, 500, 64, 64)

        OurHero.Position = New PointF(OurHero.Rect.X, OurHero.Rect.Y)

        OurHero.Velocity = New PointF(0, 0)

        OurHero.MaxVelocity = New PointF(400, 1000)

        OurHero.Acceleration = New PointF(300, 300)

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
        Clouds(Clouds.Length - 1).Rect = New Rectangle(512, 64, 192, 128)

        Array.Resize(Clouds, Clouds.Length + 1)
        Clouds(Clouds.Length - 1).Rect = New Rectangle(1728, 64, 128, 64)

        ReDim Bushes(0)
        Bushes(Bushes.Length - 1).Rect = New Rectangle(768, 768, 320, 64)

        Array.Resize(Bushes, Bushes.Length + 1)
        Bushes(Bushes.Length - 1).Rect = New Rectangle(1600, 768, 64, 64)

        ReDim Cash(0)
        Cash(Cash.Length - 1).Rect = New Rectangle(1088, 320, 64, 64)
        Cash(Cash.Length - 1).Collected = False

        Array.Resize(Cash, Cash.Length + 1)
        Cash(Cash.Length - 1).Rect = New Rectangle(1472, 64, 64, 64)
        Cash(Cash.Length - 1).Collected = False

    End Sub

    Private Sub InitializeForm()

        Me.WindowState = FormWindowState.Maximized

        Text = "Platformer with Level Editor - Code with Joe"

        SetStyle(ControlStyles.UserPaint, True)

        SetStyle(ControlStyles.OptimizedDoubleBuffer, True)

        SetStyle(ControlStyles.AllPaintingInWmPaint, True)

    End Sub

    Private Sub UpdateFrame()

        Select Case GameState

            Case AppState.Playing

                UpdateControllerData()

                UpdateDeltaTime()

                UpdateOurHero()

            Case AppState.Editing

                UpdateControllerData()

        End Select

    End Sub

    Private Sub UpdateControllerData()

        UpdateControllerPosition()

        'UpdateBatteryInfo()

    End Sub

    Private Sub UpdateControllerPosition()

        For ControllerNumber = 0 To 3 'Up to 4 controllers

            Try

                ' Check if the function call was successful
                If XInputGetState(ControllerNumber, ControllerPosition) = 0 Then
                    ' The function call was successful, so you can access the controller state now

                    UpdateButtonPosition()

                    UpdateLeftThumbstickPosition()

                    'UpdateRightThumbstickPosition()

                    'UpdateLeftTriggerPosition()

                    'UpdateRightTriggerPosition()

                    Connected(ControllerNumber) = True

                Else
                    ' The function call failed, so you cannot access the controller state

                    'Text = "Failed to get controller state. Error code: " & XInputGetState(ControllerNumber, ControllerPosition).ToString

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

            'Move mouse pointer to the left.
            Cursor.Position = New Point(Cursor.Position.X - 10, Cursor.Position.Y)


            'LabelLeftThumbX.Text = "Controller: " & ControllerNumber.ToString & " Left Thumbstick: Left"

            'Timer2.Start()





        ElseIf ControllerPosition.Gamepad.sThumbLX >= NeutralEnd Then
            'The left thumbstick is in the right position.


            'Move mouse pointer to the right.
            Cursor.Position = New Point(Cursor.Position.X + 10, Cursor.Position.Y)


            'LabelLeftThumbX.Text = "Controller: " & ControllerNumber.ToString & " Left Thumbstick: Right"

            'Timer2.Start()





        Else
            'The left thumbstick is in the neutral position.

        End If

        'What position is the left thumbstick in on the Y-axis?
        If ControllerPosition.Gamepad.sThumbLY <= NeutralStart Then
            'The left thumbstick is in the down position.



            'Move mouse pointer down.
            Cursor.Position = New Point(Cursor.Position.X, Cursor.Position.Y + 10)


            'LabelLeftThumbY.Text = "Controller: " & ControllerNumber.ToString & " Left Thumbstick: Down"

            'Timer2.Start()





        ElseIf ControllerPosition.Gamepad.sThumbLY >= NeutralEnd Then
            'The left thumbstick is in the up position.


            'Move mouse pointer down.
            Cursor.Position = New Point(Cursor.Position.X, Cursor.Position.Y - 10)


            'LabelLeftThumbY.Text = "Controller: " & ControllerNumber.ToString & " Left Thumbstick: Up"

            'Timer2.Start()







        Else
            'The left thumbstick is in the neutral position.

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

                'Skydive steering
                If RightArrowDown = True Or ControllerRight = True Then

                    OurHero.Velocity.X += 25.5F * DeltaTime.TotalSeconds

                ElseIf LeftArrowDown = True Or ControllerLeft = True Then

                    OurHero.Velocity.X += -25.5F * DeltaTime.TotalSeconds

                End If

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

        Wraparound()

        UpdateHeroMovement()

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
                        OurHero.Velocity.X = 0

                        If OurHero.Position.Y > Block.Rect.Bottom - OurHero.Rect.Height \ 2 Then
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

                                    OurHero.Velocity.Y += -1300.0F

                                    Jumped = True

                                End If

                            End If

                            If ControllerA = True Then

                                If ControllerJumped = False Then

                                    OurHero.Velocity.Y += -1300.0F

                                    ControllerJumped = True

                                End If

                            End If

                        Else
                            'No, our hero is NOT on top of the block.

                            'Stop the move
                            OurHero.Velocity.X = 0

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

        DrawStartScreenNewButton()

        DrawStartScreenOpenButton()

    End Sub

    Private Sub DrawPlaying()

        DrawBackground()

        DrawClouds()

        DrawBushes()

        DrawBlocks()

        DrawCash()

        DrawOurHero()

        DrawCollectedCash()

        DrawFPS()

        DrawEditButton()

    End Sub

    Private Sub DrawEditing()

        DrawBackground()

        DrawGridLines()

        DrawClouds()

        DrawBushes()

        DrawBlocks()

        DrawCash()

        DrawOurHero()

        DrawToolPreview()

        DrawToolBar()

        DrawPlayButton()

        DrawSaveButton()

        DrawFPS()

    End Sub

    Private Sub DrawToolBar()

        DrawToolbarBackground()

        DrawPointerToolButton()

        DrawBlockToolButton()

    End Sub

    Private Sub DrawOurHero()

        With Buffer.Graphics

            .FillRectangle(Brushes.Red, OurHero.Rect)

            .DrawString("Hero", CWJFont, Brushes.White, OurHero.Rect, AlineCenterMiddle)

            'Draw hero position
            .DrawString("X: " & OurHero.Position.X.ToString & vbCrLf & "Y: " & OurHero.Position.Y.ToString,
                        CWJFont, Brushes.White,
                        OurHero.Rect.X,
                        OurHero.Rect.Y - 50,
                        New StringFormat With {.Alignment = StringAlignment.Near})

        End With

    End Sub

    Private Sub DrawBlocks()

        With Buffer.Graphics

            If Blocks IsNot Nothing Then

                For Each Block In Blocks

                    .FillRectangle(Brushes.Chocolate, Block.Rect)

                    If SelectedBlock = Array.IndexOf(Blocks, Block) Then

                        'Draw selection rectangle.
                        .DrawRectangle(New Pen(Color.Red, 6), Block.Rect)

                        'Position sizing handle.
                        SizingHandle.X = Block.Rect.Right - SizingHandle.Width \ 2
                        SizingHandle.Y = Block.Rect.Bottom - SizingHandle.Height \ 2

                        'Draw sizing handle.
                        .FillRectangle(Brushes.Black,
                                       SizingHandle)

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

                        'Draw selection rectangle.
                        .DrawRectangle(New Pen(Color.Red, 6), Bush.Rect)

                        'Position sizing handle.
                        SizingHandle.X = Bush.Rect.Right - SizingHandle.Width \ 2
                        SizingHandle.Y = Bush.Rect.Bottom - SizingHandle.Height \ 2

                        'Draw sizing handle.
                        .FillRectangle(Brushes.Black,
                                       SizingHandle)

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

                    .DrawRectangle(OutinePen, Cloud.Rect)

                    If SelectedCloud = Array.IndexOf(Clouds, Cloud) Then

                        'Draw selection rectangle.
                        .DrawRectangle(New Pen(Color.Red, 6), Cloud.Rect)

                        'Position sizing handle.
                        SizingHandle.X = Cloud.Rect.Right - SizingHandle.Width \ 2
                        SizingHandle.Y = Cloud.Rect.Bottom - SizingHandle.Height \ 2

                        'Draw sizing handle.
                        .FillRectangle(Brushes.Black,
                                       SizingHandle)

                    End If

                Next

            End If

        End With

    End Sub

    Private Sub DrawCash()

        With Buffer.Graphics

            If Cash IsNot Nothing Then

                For Each Bill In Cash

                    Select Case GameState

                        Case AppState.Playing

                            If Bill.Collected = False Then

                                .FillRectangle(Brushes.Goldenrod, Bill.Rect)

                                .DrawString("$", FPSFont, Brushes.OrangeRed, Bill.Rect, AlineCenterMiddle)

                            End If

                        Case AppState.Editing

                            .FillRectangle(Brushes.Goldenrod, Bill.Rect)

                            .DrawString("$", FPSFont, Brushes.OrangeRed, Bill.Rect, AlineCenterMiddle)

                            If SelectedBill = Array.IndexOf(Cash, Bill) Then

                                'Draw selection rectangle.
                                .DrawRectangle(New Pen(Color.Red, 6), Bill.Rect)

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

                .FillRectangle(Brushes.Chocolate, ToolPreview)

            End If

        End With

    End Sub

    Private Sub DrawToolbarBackground()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black, ToolBarBackground.Rect)

        End With

    End Sub

    Private Sub DrawPointerToolButton()

        With Buffer.Graphics

            If SelectedTool = Tools.Pointer Then

                .FillRectangle(DarkCharcoalGreyBrush, PointerToolButton.Rect)

                .DrawString("ë", PointerToolFont, Brushes.White, PointerToolButton.Rect, AlineCenterMiddle)

            Else
                .FillRectangle(Brushes.Black, PointerToolButton.Rect)

                .DrawString("ë", PointerToolFont, Brushes.White, PointerToolButton.Rect, AlineCenterMiddle)

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


    Private Sub DrawSaveButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black, SaveButton.Rect)

            .DrawString("Save", FPSFont, Brushes.White, SaveButton.Rect, AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawFPS()

        With Buffer.Graphics

            .DrawString(FPS.ToString & " FPS", FPSFont, Brushes.White, FPS_Postion)

        End With

    End Sub

    Private Sub AddBlock(Location As Point)

        If Blocks IsNot Nothing Then

            Array.Resize(Blocks, Blocks.Length + 1)

        Else

            ReDim Blocks(0)

        End If

        'Init block
        Blocks(Blocks.Length - 1).Rect.Location = Location

        Blocks(Blocks.Length - 1).Rect.Size = New Size(GridSize, GridSize)

        Blocks(Blocks.Length - 1).Position.X = Location.X
        Blocks(Blocks.Length - 1).Position.Y = Location.Y

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

    Private Sub RemoveCloud(Index As Integer)

        'Remove the cloud from clouds.
        Clouds = Clouds.Where(Function(e, i) i <> Index).ToArray()

    End Sub

    Private Sub DrawStartScreenOpenButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black,
                           StartScreenOpenButton.Rect)

            .DrawString("Open",
                        FPSFont,
                        Brushes.White,
                        StartScreenOpenButton.Rect,
                        AlineCenterMiddle)

        End With

    End Sub

    Private Sub DrawStartScreenNewButton()

        With Buffer.Graphics

            .FillRectangle(Brushes.Black,
                           StartScreenNewButton.Rect)

            .DrawString("New",
                        FPSFont,
                        Brushes.White,
                        StartScreenNewButton.Rect,
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

    Private Sub DrawBackground()

        With Buffer.Graphics

            .Clear(Color.LightSkyBlue)

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

        CashCollectedPostion.Y = ClientRectangle.Top + 5

        EditPlayButton.Rect = New Rectangle(ClientRectangle.Left + 210, ClientRectangle.Bottom - 90, 120, 90)

        SaveButton.Rect = New Rectangle(ClientRectangle.Right - 152,
                                        ClientRectangle.Bottom - 90,
                                        150,
                                        100)

        ToolBarBackground.Rect = New Rectangle(ClientRectangle.Left, ClientRectangle.Bottom - 90, ClientRectangle.Width, 100)

        PointerToolButton.Rect = New Rectangle(ClientRectangle.Left + 331, ClientRectangle.Bottom - 90, 90, 90)

        BlockToolButton.Rect = New Rectangle(ClientRectangle.Left + 422, ClientRectangle.Bottom - 90, 90, 90)

        BlockToolIcon.Rect = New Rectangle(ClientRectangle.Left + 447, ClientRectangle.Bottom - 65, 40, 40)

        Title.Rect = New Rectangle(ClientRectangle.Left, ClientRectangle.Top, ClientRectangle.Width, ClientRectangle.Height)

        StartScreenNewButton.Rect = New Rectangle(ClientRectangle.Width \ 2 - 200, ClientRectangle.Height \ 2 + 100, 150, 90)

        StartScreenOpenButton.Rect = New Rectangle(ClientRectangle.Width \ 2 + 100, ClientRectangle.Height \ 2 + 100, 150, 90)

        BufferGridLines()

    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles MyBase.Closing

        GameLoopCancellationToken.Cancel(True)

    End Sub

    Protected Overrides Sub OnPaintBackground(ByVal e As PaintEventArgs)

        'Intentionally left blank. Do not remove.

    End Sub

    Private Sub Form1_MouseDown(sender As Object, e As MouseEventArgs) Handles MyBase.MouseDown

        Select Case GameState

            Case AppState.Start

                MouseDownStart(e)

            Case AppState.Playing

                If EditPlayButton.Rect.Contains(e.Location) Then

                    GameState = AppState.Editing

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

                    OpenTestLevelFile(OpenFileDialog1.FileName)

                    Text = Path.GetFileName(OpenFileDialog1.FileName) & " - Platformer with Level Editor - Code with Joe"

                    LastFrame = Now

                    GameState = AppState.Playing

                End If

            End If

        End If

        'New Button
        If StartScreenNewButton.Rect.Contains(e.Location) Then

            LastFrame = Now

            GameState = AppState.Playing

        End If

    End Sub

    Private Sub MouseDownEditing(e As Point)

        MouseDownEditingSelection(e)

        MouseDownEditingButtons(e)

    End Sub

    Private Sub MouseDownEditingButtons(e As Point)

        'Is the player clicking the play button?
        If EditPlayButton.Rect.Contains(e) Then
            'Yes, the player is clicking the play button.

            'Deselect game objects.
            SelectedBlock = -1
            SelectedBill = -1
            SelectedCloud = -1
            SelectedBush = -1

            'Resume Play
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

            'Deselect game objects.
            SelectedBlock = -1
            SelectedBill = -1
            SelectedCloud = -1
            SelectedBush = -1

            'Snap preview to grid.
            ToolPreview.X = CInt(Math.Round(e.X / GridSize)) * GridSize
            ToolPreview.Y = CInt(Math.Round(e.Y / GridSize)) * GridSize

            ToolPreview.Width = GridSize
            ToolPreview.Height = GridSize

            SelectedTool = Tools.Block

            ShowToolPreview = True

        End If

        'Is the player clicking the save button?
        If SaveButton.Rect.Contains(e) Then
            'Yes, the player is clicking the save button.

            SaveFileDialog1.FileName = ""
            SaveFileDialog1.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
            SaveFileDialog1.FilterIndex = 1
            SaveFileDialog1.RestoreDirectory = True

            If SaveFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then

                SaveTestLevelFile(SaveFileDialog1.FileName)

                Text = Path.GetFileName(SaveFileDialog1.FileName) & " - Platformer with Level Editor - Code with Joe"

            End If

        End If

    End Sub

    Private Sub MouseDownEditingSelection(e As Point)

        If SizingHandle.Contains(e) Then

            SizingHandleSelected = True

        Else

            SizingHandleSelected = False

            'Is the player selecting a block?
            If CheckBlockSelection(e) > -1 Then
                'Yes, the player is selecting a block.

                SelectedBlock = CheckBlockSelection(e)

                SelectionOffset.X = e.X - Blocks(SelectedBlock).Rect.X
                SelectionOffset.Y = e.Y - Blocks(SelectedBlock).Rect.Y

                'Deselect other game objects.
                SelectedBill = -1
                SelectedCloud = -1
                SelectedBush = -1

                'Is the player selecting a bill?
            ElseIf CheckBillSelection(e) > -1 Then
                'Yes, the player is selecting a bill.

                SelectedBill = CheckBillSelection(e)

                SelectionOffset.X = e.X - Cash(SelectedBill).Rect.X
                SelectionOffset.Y = e.Y - Cash(SelectedBill).Rect.Y

                'Deselect other game objects.
                SelectedBlock = -1
                SelectedCloud = -1
                SelectedBush = -1

                'Is the player selecting a cloud?
            ElseIf CheckCloudSelection(e) > -1 Then
                'Yes, the player is selecting a cloud.

                SelectedCloud = CheckCloudSelection(e)

                SelectionOffset.X = e.X - Clouds(SelectedCloud).Rect.X
                SelectionOffset.Y = e.Y - Clouds(SelectedCloud).Rect.Y

                'Deselect other game objects.
                SelectedBlock = -1
                SelectedBill = -1
                SelectedBush = -1

                'Is the player selecting a bush?
            ElseIf CheckBushSelection(e) > -1 Then
                'Yes, the player is selecting a bush.

                SelectedBush = CheckBushSelection(e)

                SelectionOffset.X = e.X - Bushes(SelectedBush).Rect.X
                SelectionOffset.Y = e.Y - Bushes(SelectedBush).Rect.Y

                'Deselect other game objects.
                SelectedBlock = -1
                SelectedBill = -1
                SelectedCloud = -1

            Else
                'No, the player is selecting nothing.

                'Is the player over the toolbar?
                If ToolBarBackground.Rect.Contains(e) = False Then
                    'No, the player is NOT over the toolbar.

                    If SelectedTool = Tools.Block Then

                        'Snap block to grid.
                        AddBlock(New Point(CInt(Math.Round(e.X / GridSize) * GridSize),
                                   CInt(Math.Round(e.Y / GridSize) * GridSize)))

                        'Change tool to the mouse pointer.
                        SelectedTool = Tools.Pointer

                        'Turn tool preview off.
                        ShowToolPreview = False

                        'Select the newly created block.
                        SelectedBlock = Blocks.Length - 1

                    Else

                        'Deselect game objects.
                        SelectedBlock = -1
                        SelectedBill = -1
                        SelectedCloud = -1
                        SelectedBush = -1

                    End If

                End If

            End If

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

    Private Sub SaveTestLevelFile(FilePath As String)

        Dim File_Number As Integer = FreeFile()

        FileOpen(File_Number, FilePath, OpenMode.Output)

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

                Write(File_Number, "Block")

            Next

        End If

        'Write Bills to File
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

                Write(File_Number, "Cloud")

            Next

        End If

        FileClose(File_Number)

    End Sub

    Private Sub OpenTestLevelFile(FilePath As String)

        Dim File_Number As Integer = FreeFile()

        Dim Index As Integer = -1

        FileObjects = Nothing

        FileOpen(File_Number, FilePath, OpenMode.Input)

        'Read Objects from File
        Do Until EOF(File_Number)

            Index += 1

            ReDim Preserve FileObjects(Index)

            With FileObjects(Index)

                'Read ID
                FileSystem.Input(File_Number, .ID)

                'Read Position
                FileSystem.Input(File_Number, .Rect.X)

                FileSystem.Input(File_Number, .Rect.Y)

                'Read Size
                FileSystem.Input(File_Number, .Rect.Width)

                FileSystem.Input(File_Number, .Rect.Height)

                FileSystem.Input(File_Number, .Text)

            End With

        Loop

        FileClose(File_Number)

        If FileObjects IsNot Nothing Then

            LoadGameObjects()

        Else

            'Clear object arrays.
            Blocks = Nothing
            Cash = Nothing
            Bushes = Nothing
            Clouds = Nothing

        End If

    End Sub

    Private Sub LoadGameObjects()

        Dim BlockIndex As Integer = -1

        Dim BillIndex As Integer = -1

        Dim BushIndex As Integer = -1

        Dim CloudIndex As Integer = -1

        'Clear object arrays.
        Blocks = Nothing
        Cash = Nothing
        Bushes = Nothing
        Clouds = Nothing

        For Each FileObject In FileObjects

            'Load Blocks
            If FileObject.ID = ObjectID.Block Then

                BlockIndex += 1 'Add a block to the blocks array.

                ReDim Preserve Blocks(BlockIndex) 'Resize the blocks array.

                Blocks(BlockIndex).ID = FileObject.ID


                Blocks(BlockIndex).Rect.X = FileObject.Rect.X

                Blocks(BlockIndex).Rect.Y = FileObject.Rect.Y


                Blocks(BlockIndex).Position.X = FileObject.Rect.X

                Blocks(BlockIndex).Position.Y = FileObject.Rect.Y


                Blocks(BlockIndex).Rect.Width = FileObject.Rect.Width

                Blocks(BlockIndex).Rect.Height = FileObject.Rect.Height


                Blocks(BlockIndex).Text = FileObject.Text

            End If

            'Load Bills
            If FileObject.ID = ObjectID.Bill Then

                BillIndex += 1 'Add a bill to the cash array.

                ReDim Preserve Cash(BillIndex) 'Resize the cash array.

                Cash(BillIndex).ID = FileObject.ID


                Cash(BillIndex).Rect.X = FileObject.Rect.X

                Cash(BillIndex).Rect.Y = FileObject.Rect.Y


                Cash(BillIndex).Position.X = FileObject.Rect.X

                Cash(BillIndex).Position.Y = FileObject.Rect.Y


                Cash(BillIndex).Rect.Width = FileObject.Rect.Width

                Cash(BillIndex).Rect.Height = FileObject.Rect.Height


                Cash(BillIndex).Text = FileObject.Text


                Cash(BillIndex).Collected = False

            End If

            'Load Bushes
            If FileObject.ID = ObjectID.Bush Then

                BushIndex += 1 'Add a bush to the bushes array.

                ReDim Preserve Bushes(BushIndex) 'Resize the bushes array.

                Bushes(BushIndex).ID = FileObject.ID


                Bushes(BushIndex).Rect.X = FileObject.Rect.X

                Bushes(BushIndex).Rect.Y = FileObject.Rect.Y


                Bushes(BushIndex).Position.X = FileObject.Rect.X

                Bushes(BushIndex).Position.Y = FileObject.Rect.Y


                Bushes(BushIndex).Rect.Width = FileObject.Rect.Width

                Bushes(BushIndex).Rect.Height = FileObject.Rect.Height


                Bushes(BushIndex).Text = FileObject.Text

            End If

            'Load Clouds
            If FileObject.ID = ObjectID.Cloud Then

                CloudIndex += 1 'Add a cloud to the clouds array.

                ReDim Preserve Clouds(CloudIndex) 'Resize the clouds array.

                Clouds(CloudIndex).ID = FileObject.ID


                Clouds(CloudIndex).Rect.X = FileObject.Rect.X

                Clouds(CloudIndex).Rect.Y = FileObject.Rect.Y


                Clouds(CloudIndex).Position.X = FileObject.Rect.X

                Clouds(CloudIndex).Position.Y = FileObject.Rect.Y


                Clouds(CloudIndex).Rect.Width = FileObject.Rect.Width

                Clouds(CloudIndex).Rect.Height = FileObject.Rect.Height


                Clouds(CloudIndex).Text = FileObject.Text

            End If

        Next

    End Sub

    Private Sub Form1_MouseMove(sender As Object, e As MouseEventArgs) Handles MyBase.MouseMove

        If GameState = AppState.Editing Then

            MouseMoveEditing(e)

        End If

    End Sub

    Private Sub MouseMoveEditing(e As MouseEventArgs)

        If e.Button = MouseButtons.None Then

            If SelectedTool = Tools.Block Then

                If ToolBarBackground.Rect.Contains(e.Location) = False Then

                    ShowToolPreview = True

                    ToolPreview.X = CInt(Math.Round(e.X / GridSize)) * GridSize
                    ToolPreview.Y = CInt(Math.Round(e.Y / GridSize)) * GridSize

                Else

                    ShowToolPreview = False

                End If

            End If

        End If

        If SelectedCloud > -1 Then

            If e.Button = MouseButtons.Left Then

                If SizingHandleSelected = True Then

                    'Snap cloud width to grid.
                    Clouds(SelectedCloud).Rect.Width = CInt(Math.Round((e.X - Clouds(SelectedCloud).Rect.X) / GridSize)) * GridSize

                    'Limit smallest cloud width to one grid width.
                    If Clouds(SelectedCloud).Rect.Width < GridSize Then Clouds(SelectedCloud).Rect.Width = GridSize

                    'Snap cloud height to grid.
                    Clouds(SelectedCloud).Rect.Height = CInt(Math.Round((e.Y - Clouds(SelectedCloud).Rect.Y) / GridSize)) * GridSize

                    'Limit smallest cloud height to one grid height.
                    If Clouds(SelectedCloud).Rect.Height < GridSize Then Clouds(SelectedCloud).Rect.Height = GridSize

                Else

                    'Snap cloud to grid
                    Clouds(SelectedCloud).Rect.X = CInt(Math.Round((e.X - SelectionOffset.X) / GridSize)) * GridSize
                    Clouds(SelectedCloud).Rect.Y = CInt(Math.Round((e.Y - SelectionOffset.Y) / GridSize)) * GridSize

                End If

            End If

        End If

        If SelectedBlock > -1 Then

            If e.Button = MouseButtons.Left Then

                'Is the player resizing the block?
                If SizingHandleSelected = True Then
                    'Yes, the player is resizing the block.

                    'Snap block width to grid.
                    Blocks(SelectedBlock).Rect.Width = CInt(Math.Round((e.X - Blocks(SelectedBlock).Rect.X) / GridSize)) * GridSize

                    'Limit smallest block width to one grid width.
                    If Blocks(SelectedBlock).Rect.Width < GridSize Then Blocks(SelectedBlock).Rect.Width = GridSize

                    'Snap block height to grid.
                    Blocks(SelectedBlock).Rect.Height = CInt(Math.Round((e.Y - Blocks(SelectedBlock).Rect.Y) / GridSize)) * GridSize

                    'Limit smallest block height to one grid height.
                    If Blocks(SelectedBlock).Rect.Height < GridSize Then Blocks(SelectedBlock).Rect.Height = GridSize

                Else

                    'Snap block to grid
                    Blocks(SelectedBlock).Rect.X = CInt(Math.Round((e.X - SelectionOffset.X) / GridSize)) * GridSize
                    Blocks(SelectedBlock).Rect.Y = CInt(Math.Round((e.Y - SelectionOffset.Y) / GridSize)) * GridSize

                End If

            End If

        End If

        If SelectedBill > -1 Then

            If e.Button = MouseButtons.Left Then

                'Move bill snap to grid.
                Cash(SelectedBill).Rect.X = CInt(Math.Round((e.X - SelectionOffset.X) / GridSize)) * GridSize
                Cash(SelectedBill).Rect.Y = CInt(Math.Round((e.Y - SelectionOffset.Y) / GridSize)) * GridSize

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
                    Bushes(SelectedBush).Rect.Width = CInt(Math.Round((e.X - Bushes(SelectedBush).Rect.X) / GridSize)) * GridSize

                    'Limit smallest bush width to one grid width.
                    If Bushes(SelectedBush).Rect.Width < GridSize Then Bushes(SelectedBush).Rect.Width = GridSize

                    'Snap bush height to grid.
                    Bushes(SelectedBush).Rect.Height = CInt(Math.Round((e.Y - Bushes(SelectedBush).Rect.Y) / GridSize)) * GridSize

                    'Limit smallest bush height to one grid height.
                    If Bushes(SelectedBush).Rect.Height < GridSize Then Bushes(SelectedBush).Rect.Height = GridSize

                Else
                    'No, the player is not resizing the bush.
                    'The player is moving the bush.

                    'Move bush snap to grid
                    Bushes(SelectedBush).Rect.X = CInt(Math.Round((e.X - SelectionOffset.X) / GridSize)) * GridSize
                    Bushes(SelectedBush).Rect.Y = CInt(Math.Round((e.Y - SelectionOffset.Y) / GridSize)) * GridSize

                End If

            End If

        End If

    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown

        Select Case e.KeyCode

            'Has the player pressed the right arrow key down?
            Case Keys.Right
                'Yes, the player has pressed the right arrow key down.

                RightArrowDown = True

                LeftArrowDown = False

            'Has the player pressed the left arrow key down?
            Case Keys.Left
                'Yes, the player has pressed the left arrow key down.

                LeftArrowDown = True

                RightArrowDown = False

            'Has the player pressed the B key down?
            Case Keys.B
                'Yes, the player has pressed the B key down.

                BDown = True

                'Has the player pressed the delete key down?
            Case Keys.Delete
                'Yes, the player has pressed the delete key down.

                If GameState = AppState.Editing Then

                    If SelectedBlock > -1 Then

                        RemoveBlock(SelectedBlock)

                    End If

                    If SelectedBill > -1 Then

                        RemoveBill(SelectedBill)

                    End If

                    If SelectedBush > -1 Then

                        RemoveBush(SelectedBush)

                    End If

                    If SelectedCloud > -1 Then

                        RemoveCloud(SelectedCloud)

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

                If GameState = AppState.Editing Then

                    If IsMouseDown = True Then

                        IsMouseDown = False

                        DoMouseLeftUp()

                    End If

                End If

            Case 1 'Up

                ControllerLeft = False

                ControllerRight = False

                If ControllerJumped = True Then ControllerJumped = False

                ControllerA = False

                ControllerB = False

                If GameState = AppState.Editing Then

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


                If GameState = AppState.Editing Then

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

                If GameState = AppState.Editing Then

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

                If GameState = AppState.Editing Then

                    If IsMouseDown = True Then

                        IsMouseDown = False

                        DoMouseLeftUp()

                    End If

                    'Move mouse pointer up.
                    Cursor.Position = New Point(Cursor.Position.X - 2, Cursor.Position.Y - 2)

                End If

            Case 6 'Down+Left

                ControllerLeft = True

                ControllerRight = False

                If ControllerJumped = True Then ControllerJumped = False

                ControllerA = False

                ControllerB = False

                If GameState = AppState.Editing Then

                    If IsMouseDown = True Then

                        IsMouseDown = False

                        DoMouseLeftUp()

                    End If

                    'Move mouse pointer to the right.
                    Cursor.Position = New Point(Cursor.Position.X - 2, Cursor.Position.Y + 2)

                End If

            Case 8 'Right

                ControllerRight = True

                ControllerLeft = False

                If ControllerJumped = True Then ControllerJumped = False

                ControllerA = False

                ControllerB = False

                If GameState = AppState.Editing Then

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

                If GameState = AppState.Editing Then

                    If IsMouseDown = True Then

                        IsMouseDown = False

                        DoMouseLeftUp()

                    End If

                    'Move mouse pointer up.
                    Cursor.Position = New Point(Cursor.Position.X + 2, Cursor.Position.Y - 2)

                End If

            Case 10 'Down+Right

                ControllerLeft = False

                ControllerRight = True

                If ControllerJumped = True Then ControllerJumped = False

                ControllerA = False

                ControllerB = False

                If GameState = AppState.Editing Then

                    If IsMouseDown = True Then

                        IsMouseDown = False

                        DoMouseLeftUp()

                    End If

                    'Move mouse pointer to the right.
                    Cursor.Position = New Point(Cursor.Position.X + 2, Cursor.Position.Y + 2)

                End If

            Case 16 'Start
            Case 32 'Back
            Case 64 'Left Stick
            Case 128 'Right Stick
            Case 256 'Left bumper
            Case 512 'Right bumper
            Case 4096 'A

                ControllerA = True

                ControllerLeft = False

                ControllerRight = False

                'If ControllerJumped = True Then ControllerJumped = False

                ControllerB = False

                If GameState = AppState.Editing Then

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

            Case 16384 'X
            Case 32768 'Y
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

    Private Function IsOnBill() As Integer

        If Cash IsNot Nothing Then

            For Each Bill In Cash

                If OurHero.Rect.IntersectsWith(Bill.Rect) = True Then

                    'return index of Plateform
                    Return Array.IndexOf(Cash, Bill)

                End If

            Next

        End If

        Return -1

    End Function

    Private Sub Wraparound()

        'When our hero exits the bottom side of the client area.
        If OurHero.Position.Y > ClientRectangle.Bottom Then

            OurHero.Velocity.Y = 0F
            OurHero.Velocity.X = 0F

            OurHero.Position.X = 1500.0F

            'Our hero reappears on the top side the client area.
            OurHero.Position.Y = ClientRectangle.Top - OurHero.Rect.Height

        End If

    End Sub

End Class


