# ⚙️ C# System & Console Helpers

A lightweight library of reusable utilities for C# console applications—providing system/SMART-drive information, flicker-free double-buffered output, and a built-in console progress bar.

---

## 🚀 Key Features

- 🖥️ System Info  
  Fetch CPU, RAM, GPU and other hardware metrics at runtime.  
- 💽 SMART Drive Data  
  Query S.M.A.R.T. attributes (temperature, health, usage) for local drives.  
- 🎛️ Double-Buffered Console  
  Eliminate flicker by buffering all writes and flushing in one pass.  
- 📊 Console Progress Bar  
  Render a customizable progress bar using the double-buffer backend.  

---

## 🛠️ Prerequisites

- .NET Framework 4.6+ or .NET Core 3.1+  
- Windows 7 or later (for SMART access)  

---

## 📥 Installation

1. Clone the repo  
   ```bash
   git clone https://github.com/GameMill/cSharp_Helper_Function.git
   cd cSharp_Helper_Function
   ```
2. Open `Functions.sln` in Visual Studio or your favorite IDE.  
3. Build the **Functions** project and add a project reference to your console app.

---

## 💡 Usage Examples

### 1. Retrieve Basic System Info  
```csharp
using Helpers.System;

var sys = SystemInfo.GetOverview();
Console.WriteLine($"CPU: {sys.CpuName} | RAM: {sys.TotalMemory}MB");
```

### 2. Read SMART Data from a Drive  
```csharp
using Helpers.Drives;

var smart = DriveInfo.GetSmartData("C:\\");
Console.WriteLine($"Drive Temp: {smart.Temperature}°C | Health: {smart.Health}");
```

### 3. Initialize & Use Double-Buffered Console  
```csharp
using Helpers.Console;

ConsoleBuffer.Initialize();
ConsoleBuffer.WriteAt(0, 0, "Loading...");
// ... more buffered writes
ConsoleBuffer.Flush();
```

### 4. Display a Progress Bar  
```csharp
using Helpers.Console;

ConsoleBuffer.Initialize();
for (int i = 0; i <= 100; i++)
{
    ConsoleProgressBar.Draw(i, 100, "Processing");
    Thread.Sleep(50);
}
ConsoleBuffer.Flush();
```

---

## 📄 License

Distributed under the [MIT License](LICENSE).  
```
