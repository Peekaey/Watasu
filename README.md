
# Simple web-to-desktop file ingestion web server.

Simplifies the process of transferring files from one device to another via simple web interface

Tailored & minimal tool that aims to solve some very specific problems within a LAN environment:
  - Copying of files directly from a mobile device into CorelDraw desktop application
  - Transferring of files from a mobile device into desktop file system
  - Provide the functionality in the most simpliest way possible

Otherwise, there are better dedicated file transfer tools available.

## Features
- Webpage for uploading a file to the device running the application which opens CorelDraw if not currently open, focuses the application and then pastes the image as a bitmap
- Webpage for uploading multiple files to the device running the application saving to either "My Documents" or "My Pictures" default folder
- Passcode protected uploads & repeated failure lockouts
- File type validation
- Supports HTTP/HTTPS through Kestrel server

Utilises Blazor Server with kestrel as the web server and for access to Windows API

## How to use
1. Ensure that appropriate password has been set within the ConfigureServices() ApplicationConfigurationSettings initialisation in Program.cs
2. Compile with ```dotnet publish -c Release -r win-x64 --self-contained true```
3. Execute .exe file to launch server

![CorelDrawPage](https://github.com/Peekaey/Watasu/blob/master/Images/Corel.png?raw=true)
![UploadPage](https://github.com/Peekaey/Watasu/blob/master/Images/Upload.png?raw=true)
![PreviewExample](https://github.com/Peekaey/Watasu/blob/master/Images/Preview-Example.gif)

## Compatability
- Windows only (10/11)
