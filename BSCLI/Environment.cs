using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace BSCLI
{
    public static class Environment
    {
        private static readonly string BscliRoot = Path.Combine(Path.GetTempPath(), "bscli");
        internal static readonly string BeatSaberLevelsPath = GetBeatSaberLevelsPath();

        internal static void CleanupEnvironment()
        {
            if (Directory.Exists(BscliRoot))
                Directory.Delete(BscliRoot, true);
        }

        internal static void SetupEnvironment()
        {
            // cleanup any files/folders created
            CleanupEnvironment();
            
            // create temporary environment
            Directory.CreateDirectory(BscliRoot);
        }

        internal static string SetupDownloadEnvironment()
        {
            SetupEnvironment();
            
            // create temporary archive file
            var archiveFileName = Path.Combine(BscliRoot, "archive.zip");
            File.Create(archiveFileName).Close();
            
            return archiveFileName;
        }

        internal static string SetupExtractionEnvironment()
        {
            // create temporary directory to extract the song into
            var songTempDirPath = Path.Combine(BscliRoot, "extraction");
            Directory.CreateDirectory(songTempDirPath);

            return songTempDirPath;
        }

        private static string GetBeatSaberLevelsPath()
        {
            var beatSaberRoot = GetBeatSaberPath();
            
            return beatSaberRoot is null ? null : Path.Combine(beatSaberRoot, "Beat Saber_Data", "CustomLevels");
        }

        private static string GetSteamInstallPath()
        {
            // read steam install path 
            return (string) Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null) ??
                   (string) Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null);
        }

        private static IEnumerable<string> GetSteamLibraryPaths()
        {
            var steamInstallPath = GetSteamInstallPath();
            var libraryPaths = new List<string> {steamInstallPath};

            var librariesPath = Path.Combine(steamInstallPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(librariesPath)) return libraryPaths;
            
            IEnumerable<string> librariesVdfLines = File.ReadAllLines(librariesPath).Where(l => l.Contains("\\\\"));
            int li, lin;
            libraryPaths.AddRange(
                librariesVdfLines
                    .Select(line => (li = line.LastIndexOf("\"", StringComparison.Ordinal)) > 0 ? line.Substring(0, li) : null)
                    .Where(line => line != null)
                    .Select(line => (lin = line.LastIndexOf("\"", StringComparison.Ordinal) + 1) > 0 ? line.Substring(lin) : null)
                    .Where(line => line != null)
                    .Select(line => line.Replace("\\\\", "\\"))
            );

            return libraryPaths;
        }

        private static string GetBeatSaberPath()
        {
            return (
                from libraryPath in GetSteamLibraryPaths()
                select Path.Combine(libraryPath, "steamapps")
                into appsPath
                where Directory.Exists(appsPath)
                let bSFiles = Directory.GetFiles(appsPath, "appmanifest_620980.acf", SearchOption.TopDirectoryOnly)
                where bSFiles.Length == 1
                select Path.Combine(appsPath, "common", "Beat Saber")
            ).FirstOrDefault();
        }
    }
}