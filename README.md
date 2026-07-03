# File Monitoring Service

A Windows Service built with C# and .NET Framework that monitors a source folder for newly added files. When a new file is detected, the service automatically renames it using a GUID and moves it to a destination folder while logging all operations.

## Features

- Monitor a source folder using `FileSystemWatcher`.
- Detect newly created files automatically.
- Rename files using a unique GUID while preserving the original extension.
- Move files to a destination folder.
- Log service events and file operations with timestamps.
- Read configuration values from `App.config`.
- Automatically create required directories if they do not exist.
- Retry file movement when files are temporarily locked.
- Support Console Debug Mode for easier development and testing.
- Windows Service Installer for deployment.

## Technologies

- C#
- .NET Framework
- Windows Service
- FileSystemWatcher
- App.config
- Event Viewer
- Windows Service Installer

## Configuration

Configure the application using `App.config`.

```xml
<appSettings>
    <add key="Source" value="C:\FileMonitoring\Source"/>
    <add key="Destination" value="C:\FileMonitoring\Destination"/>
    <add key="Log" value="C:\FileMonitoring\Logs"/>
</appSettings>
```

## How It Works

1. The service monitors the source folder.
2. When a new file is detected:
   - A GUID filename is generated.
   - The original file extension is preserved.
   - The file is moved to the destination folder.
3. Every operation is logged with a timestamp.

## Logging

The service logs:

- Service start
- Service stop
- File detection
- File movement
- Errors during processing

## Debug Mode

The project supports Console Mode for debugging.

When running interactively, the service:

- Starts as a console application.
- Displays runtime information.
- Allows stopping the service with a key press.

## Installation

The service can be installed using either:

- InstallUtil
- sc.exe

Example:

```cmd
sc create FileMonitoringService binPath= "C:\Path\File Monitoring.exe" start= auto
```

Start the service:

```cmd
sc start FileMonitoringService
```

Remove the service:

```cmd
sc delete FileMonitoringService
```

## Project Structure

```
File Monitoring
│
├── FileMonitoringService.cs
├── Installer1.cs
├── Program.cs
├── App.config
└── Properties
```

## License

This project is for learning purposes.
