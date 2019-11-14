using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BSCLI
{
    public static class Utils
    {
        internal static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (var dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            
            foreach (var file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }

        internal static JObject ReadSongInfo(string infoFilePath)
        {
            var infoJson = File.ReadAllText(infoFilePath);
            
            return (JObject) JsonConvert.DeserializeObject(infoJson);
        }
        
        internal static string GetSongNameFromInfo(JObject info)
        {
            switch (info.Val("_version"))
            {
                case "2.0.0":
                    var songName = info.Val("_songName");
                    var songSubName = info.Val("_songSubName");
                    var songAuthorName = info.Val("_songAuthorName");
                    var levelAuthorName = info.Val("_levelAuthorName");

                    var sb = new StringBuilder();
                    sb.Append(songAuthorName ?? "unknown author");
                    sb.Append(" - ");
                    sb.Append(songName ?? Path.GetRandomFileName());

                    if (songSubName != null)
                    {
                        sb.Append(" (");
                        sb.Append(songSubName);
                        sb.Append(")");
                    }

                    if (levelAuthorName == null)
                        return sb.ToString();
                    
                    sb.Append(" (made by ");
                    sb.Append(levelAuthorName);
                    sb.Append(")");

                    return sb.ToString();
                default:
                    Console.WriteLine($"Unknown Beat Saber level version: {info.Val("_version")}");
                    
                    return Path.GetRandomFileName();
            }
        }
    }
}