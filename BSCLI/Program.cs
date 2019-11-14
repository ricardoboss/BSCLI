using System;

namespace BSCLI
{
    internal static class Program
    {
        internal const int Version = 3;

        private static void Main(string[] args)
        {
            try
            {
                Environment.SetupEnvironment();

                switch (args.Arg(0))
                {
                    case "show":
                        Commands.CommandShow();
                        break;
                    case "bsr":
                    case "!bsr":
                    case "save":
                        Commands.CommandSave(args.Arg(1));
                        break;
                    case "install":
                        Commands.CommandInstall(args.Arg(1));
                        break;
                    case "ver":
                    case "version":
                        Commands.CommandVersion();
                        break;
                    default:
                        Commands.CommandHelp();
                        break;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("An error occurred while processing your command:");
                Console.Error.Write(e.GetType().Name + ": ");
                Console.Error.WriteLine(e.Message);
                Console.Error.Write(e.StackTrace);
            }
            finally
            {
                Environment.CleanupEnvironment();
            }
        }
    }
}