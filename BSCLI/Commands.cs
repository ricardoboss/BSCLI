using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;

namespace BSCLI
{
    public static class Commands
    {
        internal static void CommandVersion()
        {
            Console.WriteLine(Program.Version);
        }

        internal static void CommandShow()
        {
            Process.Start("explorer.exe", Environment.BeatSaberLevelsPath);
        }

        internal static void CommandInstall(string file, string key = null)
        {
            if (!Path.IsPathRooted(file))
                file = Path.GetFullPath(file);
            
            if (!File.Exists(file))
            {
                Console.Error.WriteLine($"The file \"{file}\" could not be found!");

                return;
            }
            
            Console.WriteLine("Extracting archive...");

            var songExtractionDir = Environment.SetupExtractionEnvironment();

            // extract song from archive
            ZipFile.ExtractToDirectory(file, songExtractionDir);

            var infoFile = Path.Combine(songExtractionDir, "info.dat");
            if (!File.Exists(infoFile))
            {
                Console.Error.WriteLine("Info file not found!");

                return;
            }

            Console.WriteLine("Reading song info...");
            
            // create directory from song info
            var songInfo = Utils.ReadSongInfo(infoFile);
            var songDirName = Utils.GetSongNameFromInfo(songInfo);

            if (key != null)
            {
                // create key file in extracted directory if it didn't exist in the archive
                var keyFile = Path.Combine(songExtractionDir, "key");
                if (!File.Exists(keyFile))
                    File.WriteAllText(keyFile, key);
            }

            // create song directory in beat saber custom levels
            var songDirPath = Path.Combine(Environment.BeatSaberLevelsPath, songDirName);
            Directory.CreateDirectory(songDirPath);

            var songDirKeyFile = Path.Combine(songDirPath, "key");
            while (File.Exists(songDirKeyFile))
            {
                var songKey = File.ReadAllText(songDirKeyFile);
                if (songKey.Equals(key))
                {
                    Console.Error.WriteLine($"A song with the key {key} already exists.");

                    return;
                }

                songDirPath += "-";
                songDirKeyFile = Path.Combine(songDirPath, "key");
            }
            
            // move extracted dir to actual custom songs
            Utils.CopyFilesRecursively(new DirectoryInfo(songExtractionDir), new DirectoryInfo(songDirPath));
            
            Console.WriteLine("Song installed successfully!");
        }

        public static void CommandSave(string key)
        {
            var archiveFileName = Environment.SetupDownloadEnvironment();
            var downloadFinishedFlag = new ManualResetEventSlim(false);
            
            // create web client
            using (var client = new WebClient())
            {
                Console.Write("Downloading from beatsaver.com...");
                
                // report progress
                client.DownloadProgressChanged += (o, e) => Console.Write($"\rDownloading from beatsaver.com ({e.ProgressPercentage}%)");
                
                // set download finished flag on completion
                client.DownloadFileCompleted += (o, e) => downloadFinishedFlag.Set();
                
                // start async download
                client.DownloadFileAsync(new Uri("https://beatsaver.com/api/download/key/" + key), archiveFileName);
            }

            // wait for the download to finish
            downloadFinishedFlag.Wait();
            
            Console.WriteLine();
            
            // execute install command after downloading
            CommandInstall(archiveFileName, key);
        }

        internal static void CommandHelp()
        {
            foreach (var l in new[]
            {
                $"=== BSCLI v{Program.Version} ===",
                "Beat Saber Custom Level Installer - OR - Beat Saber Command Line Interface",
                "- by Ricardo Boss",
                "",
                "Usage: bscli <command> [args]",
                "",
                "Commands:",
                "\t[!]bsr|save <code>\tUse BeatSaver's \"!bsr\" key to download and install a song",
                "\tinstall <archive>\tInstalls a downloaded song",
                "\tshow\t\t\tOpen the directory where custom songs are installed to",
                "\tversion\t\t\tOutputs the version code",
                "\thelp\t\t\tPrints this help message",
                ""
            })
                Console.WriteLine(l);
        }
    }
}