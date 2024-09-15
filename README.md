# **Make Your Own Games**



**"Platformer with Level Editor"** is a powerful tool for aspiring game developers. By delving into the source code and project structure of a platformer game with a level editor, you can gain hands-on experience and valuable insights to create your own games.

![163](https://github.com/user-attachments/assets/0d73e214-4b16-49c9-a7ec-be1896427bca)



The key things you can expect to explore with this learning tool are:

### **Game Mechanics**
Understanding the inner workings of a game is crucial. With this tool, you’ll explore physics (how objects move), collision detection (when objects interact), character movement (controls), and game loops (the heartbeat of your game).

### **Programming Patterns**
Game development relies on common programming patterns. Structured programming helps organize code into manageable pieces, and event handling ensures your game responds to player actions (like jumping or shooting).

![167](https://github.com/user-attachments/assets/5788aacc-5b04-4862-bb88-281d99df3e58)



### **Level Design**
The level editor is your canvas. Learn how to create captivating levels, store them efficiently, and load them seamlessly during gameplay. Level design impacts player experience, so it’s a critical skill.



![166](https://github.com/user-attachments/assets/7a263e93-d8a8-49f9-a6d6-49108dea7c9f)



### **Sound Integration**
Sound effects and music enhance immersion. Discover how to integrate audio assets into your game.
Whether it’s a cheerful jump sound or an epic boss battle theme, audio matters!

### **Xbox Controller Support**
Many players use controllers. Mastering Xbox controller integration allows you to create games that feel natural and enjoyable for console players.


Remember, “Platformer with Level Editor” isn’t just about theory—it’s hands-on learning. Dive in, experiment, and let your creativity flow! 🎮✨






![152](https://github.com/JoeLumbley/Platformer-with-Level-Editor/assets/77564255/a2c5b26f-9b93-4a48-a898-5c7df788ed37)


# Code Walkthrough
**"Platformer with Level Editor"** is a game application that allows users to create and customize their own platformer levels. The application includes various tools and features to help players design, edit, and play through their custom levels.

### **Imports and Class Declaration**

```vb
Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Numerics
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading

Public Class Form1
```

- **Imports**: These lines bring in useful libraries that provide extra functionality. For example, `System.IO` helps with file operations, and `System.Drawing` is used for graphics.

- **Public Class Form1**: This declares a class named `Form1`. In VB.NET, a class is like a blueprint for creating objects. Here, `Form1` represents the main window of the application.

### **DLL Imports for Audio and Controller**

```vb
<DllImport("winmm.dll", EntryPoint:="mciSendStringW")>
Private Shared Function mciSendStringW(<MarshalAs(UnmanagedType.LPTStr)> ByVal lpszCommand As String, <MarshalAs(UnmanagedType.LPWStr)> ByVal lpszReturnString As StringBuilder, ByVal cchReturn As UInteger, ByVal hwndCallback As IntPtr) As Integer
End Function
```

- **DllImport**: This is used to call functions from external libraries (DLLs). Here, it allows the program to play audio files.

- **Function mciSendStringW**: This function sends commands to the multimedia control interface (MCI) for audio playback.

### **Game State and Controller Setup**

```vb
Private Enum AppState As Integer
    Start
    Playing
    Editing
    Clear
End Enum
```

- **Enum**: An enumeration (enum) is a way to define a group of related constants. Here, `AppState` defines different states the game can be in: starting, playing, editing, and clear.

### **Variables for Game Objects**

```vb
Private Hero As GameObject
Private Cash() As GameObject
Private Blocks() As GameObject
```

- **Variables**: These are used to store data. `Hero` is a single game object representing the player. `Cash` and `Blocks` are arrays that can hold multiple game objects (like bills or blocks).

### **Game Initialization**

```vb
Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    InitializeApp()
End Sub
```

- **Form1_Load**: This is an event that runs when the form (window) loads. It calls `InitializeApp()` to set up the game.

### **Initializing the Game**

```vb
Private Sub InitializeApp()
    CreateSoundFilesFromResources()
    InitializeObjects()
    InitializeForm()
End Sub
```

- **InitializeApp**: This method sets up the game by creating sound files, initializing game objects, and setting up the form's appearance.

### **Creating Sounds**

```vb
Private Sub CreateSoundFilesFromResources()
    Dim File As String = Path.Combine(Application.StartupPath, "level.mp3")
    CreateSoundFileFromResource(File, My.Resources.level)
End Sub
```

- **CreateSoundFilesFromResources**: This method creates sound files from resources included in the project. It uses the `CreateSoundFileFromResource` method to save the sound file.

### **Game Loop**

```vb
Private Sub GameTimer_Tick(sender As Object, e As EventArgs) Handles GameTimer.Tick
    UpdateFrame()
    Refresh()
End Sub
```

- **GameTimer_Tick**: This event runs repeatedly at set intervals (like a clock tick ⏰). It updates the game state and redraws the window.

### **Updating the Frame**

```vb
Private Sub UpdateFrame()
    Select Case GameState
        Case AppState.Start
            ' Logic for starting state
        Case AppState.Playing
            ' Logic for playing state
        Case AppState.Editing
            ' Logic for editing state
    End Select
End Sub
```

- **UpdateFrame**: This method checks the current game state and runs the appropriate logic based on whether the game is starting, playing, or in editing mode.

### **User Input Handling**

```vb
Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
    Select Case e.KeyCode
        Case Keys.E
            DoEKeyDownLogic()
        Case Keys.P
            DoPKeyDownLogic()
    End Select
End Sub
```

- **Form1_KeyDown**: This event handles keyboard input. When a key is pressed, it checks which key and calls the corresponding method to handle that input.

### **Game Object Interaction**

```vb
Private Sub DrawOurHero()
    With Buffer.Graphics
        If Hero.Rect.IntersectsWith(Camera.Rect) Then
            ' Draw hero logic here
        End If
    End With
End Sub
```

- **DrawOurHero**: This method draws the hero on the screen. It checks if the hero is within the camera's view before drawing it.

### **Ending the Game**

```vb
Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles MyBase.Closing
    CloseSounds()
End Sub
```

- **Form1_Closing**: This event runs when the user closes the application. It calls `CloseSounds()` to stop any playing sounds.



This code is a basic structure of a platformer game with a level editor. Each part of the code works together to create a playable game where users can design their own levels. 











# Controller Code Walkthrough

Let's break down the steps involved in using controllers in the game code. This includes detecting the controller, reading its state, and responding to input.

### **Importing Necessary Libraries**

Before you can use controllers, you need to import the necessary libraries:

```vb
Imports System.Runtime.InteropServices
```

- This allows you to use functions from external libraries (like the XInput library) that handle controller inputs.

### **Declaring DLL Imports**

You need to declare functions from the XInput library that will help you interact with the controller:

```vb
<DllImport("XInput1_4.dll")>
Private Shared Function XInputGetState(dwUserIndex As Integer, ByRef pState As XINPUT_STATE) As Integer
End Function
```

- **XInputGetState**: This function checks the state of the controller (e.g., whether it's connected and the status of buttons and sticks).

### **Defining Structures for Controller State**

You need to define structures that represent the state of the controller:

```vb
<StructLayout(LayoutKind.Explicit)>
Public Structure XINPUT_STATE
    <FieldOffset(0)> Public dwPacketNumber As UInteger
    <FieldOffset(4)> Public Gamepad As XINPUT_GAMEPAD
End Structure

<StructLayout(LayoutKind.Sequential)>
Public Structure XINPUT_GAMEPAD
    Public wButtons As UShort
    Public bLeftTrigger As Byte
    Public bRightTrigger As Byte
    Public sThumbLX As Short
    Public sThumbLY As Short
    Public sThumbRX As Short
    Public sThumbRY As Short
End Structure
```

- **XINPUT_STATE**: Contains the packet number and the gamepad state.
- **XINPUT_GAMEPAD**: Contains button states and thumbstick positions.

### **Checking if the Controller is Connected**

You need a method to check if the controller is connected:

```vb
Private Function IsControllerConnected(controllerNumber As Integer) As Boolean
    Return XInputGetState(controllerNumber, ControllerPosition) = 0
End Function
```

- This function returns `True` if the controller is connected (indicated by a return value of 0).

### **Updating Controller State**

You need to update the controller state regularly, typically in the game loop:

```vb
Private Sub UpdateControllerData()
    For ControllerNumber As Integer = 0 To 3
        Try
            If IsControllerConnected(ControllerNumber) Then
                UpdateControllerState(ControllerNumber)
            End If
        Catch ex As Exception
            DisplayError(ex)
        End Try
    Next
End Sub
```

- This loop checks each controller (up to 4) and updates its state if connected.

### **Reading Button States**

Inside the `UpdateControllerState` method, you will read the button states:

```vb
Private Sub UpdateControllerState(controllerNumber As Integer)
    If Connected(0) AndAlso controllerNumber = 0 Then
        ' Use controller zero
        UpdateButtonPosition()
        ' Other updates for thumbsticks, triggers, etc.
    End If
End Sub
```

- You can check specific buttons by using bitwise operations on `wButtons`:

```vb
If (Gamepad.wButtons And AButton) <> 0 Then
    ' A button is pressed
End If
```

### **Handling Input Actions**

You will typically have methods to handle specific input actions based on button presses:

```vb
Private Sub DoAKeyDownLogic()
    ' Logic for when the A button is pressed
End Sub
```

### **Vibrating the Controller**

You can also make the controller vibrate based on game events:

```vb
<DllImport("XInput1_4.dll")>
Private Shared Function XInputSetState(playerIndex As Integer, ByRef vibration As XINPUT_VIBRATION) As Integer
End Function

Public Structure XINPUT_VIBRATION
    Public wLeftMotorSpeed As UShort
    Public wRightMotorSpeed As UShort
End Structure
```

- You can set the motor speeds to create a vibration effect.

### **Integrating with Game Logic**

Finally, you integrate the controller input into the game logic, allowing players to control the hero, navigate menus, and interact with the game world using the controller.

### Summary

Using controllers in your game involves:
1. Importing necessary libraries.
2. Declaring external functions to communicate with the controller.
3. Defining structures to hold controller state information.
4. Checking if the controller is connected.
5. Regularly updating the controller state in the game loop.
6. Reading button states and handling input actions.
7. Optionally, implementing vibration feedback.










# **Why GDI+**

### **Easy to Learn**

GDI+ offers a user-friendly interface that makes it perfect for beginners. Its straightforward methods for drawing shapes, text, and images are intuitive, allowing you to quickly grasp the basics of programming.

### **Minimal Setup**

Forget complicated configurations! GDI+ requires minimal setup, enabling you to dive straight into coding and focus on learning essential programming concepts without distractions.

### **Integration with .NET**

Since GDI+ is part of the .NET framework, you're likely already familiar with it from your coursework. This integration means you can build on your existing knowledge and use the tools you already know.

### **Rich Language Support**

Whether you prefer C# or VB.NET, GDI+ supports multiple programming languages, making it versatile for different learning styles.

### **Immediate Visual Feedback**

One of the best features of GDI+ is the real-time visual results. You can see the impact of your code instantly, helping you understand and retain concepts better. Drawing graphics allows you to visualize important programming ideas like loops, conditionals, and events.

### **Interactive Learning**

With GDI+, you can create interactive applications, such as simple games or simulations. This fosters creativity and keeps you engaged in your learning process.

### **Focus on Core Concepts**

GDI+ helps you build foundational programming skills, including object-oriented programming, event-driven design, and algorithmic thinking, all without the complexity of advanced graphics programming.

### **Real-World Problem Solving**

Using GDI+, you can tackle practical challenges like game design or data visualization, reinforcing your problem-solving abilities.

### **Cross-Platform Potential**

While GDI+ is primarily used in Windows environments, it serves as a great stepping stone to more advanced graphics libraries like OpenGL or DirectX as you progress.

### **Web Integration**

You can also explore how GDI+ integrates with web applications through ASP.NET, broadening your skill set and understanding of web development.

### **Community and Resources**

There’s a wealth of tutorials, documentation, and community support available for GDI+. This makes it easy for you to find the resources you need to succeed.

### **Project Examples**

Teachers can easily create engaging projects using GDI+, allowing you to apply what you learn in real-world scenarios.

GDI+ strikes a perfect balance between functionality and accessibility. Its simplicity lets you focus on learning programming fundamentals while providing enough depth for creative exploration, laying a solid foundation for your future studies in computer science and software development.





















# Getting Started

Here are some steps you can take to learn game development using the "Platformer with Level Editor" project:


### **Set up the Development Environment**


Visual Studio is an Integrated Development Environment (IDE) created by Microsoft. 

It is a powerful software development tool that provides a comprehensive set of features and tools for building a wide range of applications.

This is the IDE I use to make "Platformer with Level Editor" and I recommend that you use.

Visual Studio Community is a free version of the IDE for individual developers, small teams, and open-source projects.

This is the version I recommend you download.

Install Visual Studio from here:  https://visualstudio.microsoft.com/downloads/ and include the .NET Desktop Development workload.

![153](https://github.com/JoeLumbley/Platformer-with-Level-Editor/assets/77564255/22a61c77-908f-4e04-9266-93f3d34ec376)


### **Clone the Repository** 

1. Click the "Code" button.
2. Copy the repository's URL.
3. Open Visual Studio.
4. Click "Clone a repository".
5. Paste the repository URL into the location field.
6. Click the "Clone" button.

Once the cloning process is complete, you will have your own local copy of the game that you can run and modify on your computer.


Watch a short about cloning the repository here: https://www.youtube.com/shorts/n8bCEIdI44U

![154](https://github.com/JoeLumbley/Platformer-with-Level-Editor/assets/77564255/a937ec81-c192-4dff-b4b0-badd87c07f87)


### **Explore the Project Structure**

Familiarize yourself with the project's structure and organization. Understand the different folders and files, and how they work together to create the platformer game and level editor.


### **Modify and Extend the Project**

Once familiar with the codebase, try adding features, improving mechanics, or creating unique game elements.

### **Refer to Documentation and Resources**

Use project documentation and tutorials to deepen your understanding of concepts and techniques.

![072](https://github.com/JoeLumbley/Platformer-with-Level-Editor/assets/77564255/c4ae4c4c-7641-4a9f-96d5-c19805fdcc01)


### **Seek Community Feedback**

Engage with the project’s community for feedback and learning opportunities.

By following these steps, you’ll gain hands-on experience, learn programming patterns, and build skills in creating interactive games! 🎮🚀

![063](https://github.com/JoeLumbley/Platformer-with-Level-Editor/assets/77564255/c55ed39f-9a4e-43d6-84a0-f5c364f224d9)





![178](https://github.com/user-attachments/assets/28913edb-fb04-4f1e-9832-421f6d60457b)





![176](https://github.com/user-attachments/assets/00c53141-a430-47a7-b096-33c249621094)


![177](https://github.com/user-attachments/assets/e3199597-c7ef-4809-9515-2575cf701c95)






![169](https://github.com/user-attachments/assets/e423b338-cb9c-430c-8d31-9c67eb08f12c)































































