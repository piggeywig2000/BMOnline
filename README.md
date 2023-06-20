# Banana Mania Online
Online multiplayer mod for Super Monkey Ball Banana Mania, MMO style. Everyone connects to a central server, and when you are in a stage you will see everyone else who is in that stage.

## Features
- Player name is displayed above the other players
- Your chosen character, character skin, ball, and clothing are shown to the other players
  - If you choose a DLC character, players that don't own the DLC may see AiAi or an untextured version of your character instead
- The number of players in each stage is shown on the stage selection screen
  - The course and mode selection screens show the total number of players in every stage in that course or mode, with the exception of challenge mode, which shows the total number of players playing that challenge mode course
- In-game text chat

### Coming soon (hopefully)
- A race mode, where all players are put into a random stage at the same time together and compete to reach the goal first
- A time attack mode, where all players are put into a random stage at the same time together and have a few minutes to set the best time

## Installation
Firstly, install the [Banana Mod Manager](https://github.com/MorsGames/BananaModManager) into the game's installation folder.

Download the latest version of the mod from the [Releases](https://github.com/piggeywig2000/BMOnline/releases) page, and extract the zip into the "mods" folder inside the game's installation folder. Open Banana Mod Manager, make sure that the checkbox next to the mod is ticked, and run the game.

### Mod incompatibilities
This mod not compatible with the [Dynamic Sounds](https://gamebanana.com/mods/327614) or [Guest Character Voice Pack](https://gamebanana.com/mods/331507) mods. A fix for these is available [here](https://github.com/iswimfly/iswimflyBananaManiaMods/releases/download/MSF1.0/Multiplayer.Sound.Fixes.zip).

Credit to snowman for the original mod and iswimfly for fixing them to work with this mod.

## Controls
This mod has some extra keyboard controls. Currently they can't be changed.

- `T`: Open the chat to type and view older messages
  - `Enter`: Send chat message and close the chat
  - `Esc`: Close the chat without sending
- `F1`: Show the keyboard controls
- `F2`: Toggle the Show Name Tags setting
- `F2` and `+` or `-`: Change the Name Tags Size setting
- `F3`: Toggle the Show Player Counts setting
- `F4`: Toggle the Enable Chat setting
- `F5`: Change the Player Visibility setting
- `F5` and `+` or `-`: Change the Personal Space setting

## Settings
The mod settings can be changed in the Banana Mod Manager by clicking on the mod. If you'd like to just play the game online you don't need to worry about changing the mod settings, it should just work.

- Custom Server: Use a different server from the default one. Change these settings if you're hosting your own server and want to use that one instead.
  - Server IP: The IP address of the custom server. Can be an IPv4 address (like 172.16.154.1), an IPv6 address (like 2001:0db8:85a3:0000:0000:8a2e:0370:7334), or a domain name (like example.com). Leave it empty to use the default server IP address.
  - Server Port: The port of the custom server. Should be a number from 1 to 65535. Leave it empty to use the default port (10998).
  - Server Password: The password for the custom server. Leave it empty if the server doesn't have a password.
- In-Game Settings: In-game settings related to the mod
  - Show Name Tags: Whether to show the player names above the other players. By default this is enabled.
  - Name Tag Size: The size of the name tags. By default this is 48.
  - Show Player Counts: Whether to show the number of players playing each stage on the stage, course, and mode selection screens. By default this is enabled.
  - Enable Chat: Whether to show the chat. If disabled, you will not be able to send or receive chat messages. By default this is enabled.
  - Player Visibility: The visibility of the other players. There are 3 valid values:
    - ShowAll: Players are always visible. This is the default value.
    - HideNear: Players that are close to you or to the camera are hidden.
    - HideAll: Players are always hidden.
  - Personal Space: When using HideNear visibility, this defines how far away a player needs to be before they are visible. Measured as the distance from the line between the camera and your player's ball.

## Running the server
If you want to play online on a private server so that only people that you know can join, you'll need to host your own server. The server can run on Windows, MacOS, and Linux.

Firstly, you'll need to install the [.NET 7.0 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/7.0/runtime) if it isn't already installed. If you're on Windows you'll have a choice between the .NET Runtime, the .NET Desktop Runtime, and the ASP.NET Core Runtime. You can just install the .NET Runtime, you don't need to install the .NET Desktop Runtime or the ASP.NET Core Runtime (although it will still work if you choose one of these).

Download and extract the latest version of the server from the [Releases](https://github.com/piggeywig2000/BMOnline/releases) page.

To run the server on Windows, simply run the `BMOnline.Server.exe` file. To run the server on MacOS or Linux, open the terminal in the folder containing the server and run `dotnet BMOnline.Server.dll`.

### Connecting to the server
To connect to the server, you'll need to specify its IP address in the Server IP setting in Banana Mod Manager.
- If the server is running on the same computer, enter `127.0.0.1`
- If the server is running on a different computer on the same local network, enter its private IP address
- If the server is running on a different computer on a different network, enter its public IP address. You'll need to set up port forwarding for this to work

If you can't connect, this could be because:
- You've entered the incorrect server IP or port in Banana Mod Manager
- You're trying to connect from outside of the server's local network, but the server has not been port forwarded correctly
- You've set a password on the server, but you haven't entered the correct password into Banana Mod Manager

### Port forwarding
For someone to be able to connect to the server from outside of your local network, you'll need to port forward UDP port 10998 on your router (unless you specified a different port for the server to run on - see the server configuration section below). How exactly to port forward depends on your router, so if you don't know how to do this you'll need to search for a tutorial for your specific router.

### Server configuration
Server settings are provided in the form of command line arguments when you start the server. None of these options are required to be provided.
```
--port <port>               The port to run the server on. Should be a number from 1 to 65535. [default: 10998]
--password <password>       The server password. Omit this option to allow clients to connect to the server
                            without a password.
--max-chat-length <length>  The maximum number of characters allowed in a single chat message. Set this to 0 to
                            disable chat. [default: 280]
```
For example, `BMOnline.Server.exe --port 5678 --password "monkey"` would run the server on port 5678 with the password monkey.

## Opening the project
If you'd like to open this project yourself to view or modify the source code, you'll need to define the location of a couple of directories.

In the BMOnline.Mod directory (which also contains the `BMOnline.Mod.csproj` file), create a file called `BMOnline.Mod.csproj.user` with the following contents:
```xml
<Project>
  <PropertyGroup>
    <OutputPath>C:\Program Files (x86)\Steam\steamapps\common\smbbm\mods\piggeywig2000.online</OutputPath>
    <ReferencePath>C:\Program Files (x86)\Steam\steamapps\common\smbbm\managed</ReferencePath>
  </PropertyGroup>
</Project>
```
`OutputPath` refers to the directory to build the project into. `ReferencePath` refers to the directory that contains the game's DLLs. You should modify these paths if they're different on your computer.