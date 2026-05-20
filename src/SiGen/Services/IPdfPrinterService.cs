using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace SiGen.Services;

public interface IPdfPrinterService
{
    public bool CanPrintPdf { get; }

    public async Task PrintPdfAsync(string filePath)
    {
        throw new NotImplementedException();
    }
}

public class PdfPrinterService : IPdfPrinterService
{
    public bool CanPrintPdf { get; private set; }

    private string? _acrobatReaderPath;

    public PdfPrinterService()
    {
        ValidateCanPrintPdf();
    }

    private void ValidateCanPrintPdf()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _acrobatReaderPath = FindAcrobatReader();
            CanPrintPdf = !string.IsNullOrEmpty(_acrobatReaderPath);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                 RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            CanPrintPdf = true;
        }
        else
        {
            CanPrintPdf = false;
        }
    }

    private static string? FindAcrobatReader()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        // Check common installation paths for newer versions (Acrobat.exe)
        var acrobatPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Adobe", "Acrobat DC", "Acrobat", "Acrobat.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Adobe", "Acrobat DC", "Acrobat", "Acrobat.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Adobe", "Acrobat Reader DC", "Reader", "Acrobat.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Adobe", "Acrobat Reader DC", "Reader", "Acrobat.exe"),
        };

        foreach (var path in acrobatPaths)
        {
            if (File.Exists(path))
                return path;
        }

        // Check common installation paths for older versions (AcroRd32.exe)
        var legacyPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Adobe", "Acrobat Reader DC", "Reader", "AcroRd32.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Adobe", "Acrobat Reader DC", "Reader", "AcroRd32.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Adobe", "Reader 11.0", "Reader", "AcroRd32.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Adobe", "Reader 11.0", "Reader", "AcroRd32.exe"),
        };

        foreach (var path in legacyPaths)
        {
            if (File.Exists(path))
                return path;
        }

        // Try to find Acrobat.exe from registry
        var registryPaths = new[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\Acrobat.exe",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\Acrobat.exe",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\AcroRd32.exe",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\AcroRd32.exe"
        };

        foreach (var regPath in registryPaths)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(regPath);
                if (key != null)
                {
                    var path = key.GetValue(null) as string;
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        return path;
                }
            }
            catch { }
        }

        return null;
    }

    public async Task PrintPdfAsync(string filePath)
    {
        if (!CanPrintPdf) return;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await PrintWindows(filePath);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                 RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            await PrintUnix(filePath);
        }
    }

    private async Task PrintWindows(string pdfPath)
    {
        if (string.IsNullOrEmpty(_acrobatReaderPath))
            return;

        // Use /t for silent printing to default printer
        // /t <filename> <printername> <drivername> <portname>
        // Empty strings for printer/driver/port use system defaults
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = _acrobatReaderPath,
            Arguments = $"/p \"{pdfPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });

        if (process != null)
        {
            await process.WaitForExitAsync();
        }
    }

    private static async Task PrintUnix(string pdfPath)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "lp",
            Arguments = $"\"{pdfPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        });

        if (process != null)
        {
            await process.WaitForExitAsync();
        }
    }
}
