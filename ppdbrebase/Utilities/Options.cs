using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceIO;
using Unity.Options;

namespace ppdbrebase.Utilities
{
    [ProgramOptions]
    public static class Options
    {

        [HelpDetails("An Single Input File To Rebase")]
        public static string InputFile;

        [HelpDetails("A Directory Of Input Files To Rebase")]
        public static string InputDir;

        [HelpDetails("Rebase Files In The InputDir Recursively")]
        public static bool Recursive;

        [HelpDetails("Equivalent to --old-base=/mnt/c/ --new-base=C:\\")]
        public static bool WslRebase;

        [HelpDetails("The Old Path Base We Want To Rebase")]
        public static string OldBase;

        [HelpDetails("The New Path Base To Replace OldBase")]
        public static string NewBase;

        [HelpDetails("Specifies The Delimiter to rebase (Default Windows)")]
        public static RebasePaths.PathStyle NewPathStyle;

        public static void SetToDefaults()
        {
            InputFile = null;
            InputDir = null;
            Recursive = false;
            WslRebase = false;
            OldBase = null;
            NewBase = null;
            NewPathStyle = RebasePaths.PathStyle.Windows;
        }

        public static string NameFor(string fieldName)
        {
            return OptionsParser.OptionNameFor(typeof(Options), fieldName);
        }

        public static bool InitAndSetup(string[] args)
        {
            SetToDefaults();

            if (OptionsParser.HelpRequested(args))
            {
                OptionsParser.DisplayHelp(typeof(Program).Assembly, false);
                return false;
            }

            var unknownArgs = OptionsParser.Prepare(args, typeof(Program).Assembly, false).ToList();

            if (unknownArgs.Count > 0)
            {
                Console.WriteLine("Unknown arguments : ");
                foreach (var remain in unknownArgs)
                {
                    Console.WriteLine("\t {0}", remain);
                }

                return false;
            }

            if (WslRebase)
            {
                const string WindowsStylePathHeader = "C:\\";
                const string WindowsSubsystemForLinuxStylePathHeader = "/mnt/c/";

                if (OldBase != null)
                {
                    Console.WriteLine("--wsl-rebase cannot be used with --old-base");
                    return false;
                }

                if (NewBase != null)
                {
                    Console.WriteLine("--wsl-rebase cannot be used with --new-base");
                    return false;
                }

                if (NewPathStyle != RebasePaths.PathStyle.Windows)
                {
                    Console.WriteLine("--wsl-rebase cannot be used with --new-path-style=Unix");
                    return false;
                }

                OldBase = WindowsSubsystemForLinuxStylePathHeader;
                NewBase = WindowsStylePathHeader;
                NewPathStyle = RebasePaths.PathStyle.Windows;
            }

            if (InputFile != null && InputDir != null)
            {
                Console.WriteLine("--input-file and --input-dir cannot both be specified");
                return false;
            }

            if (InputFile != null && Recursive == true)
            {
                Console.WriteLine("--input-file and --recursive cannot both be specified");
                return false;
            }

            if (InputFile == null && InputDir == null)
            {
                Console.WriteLine("--input-file or --input-dir must be specified");
                return false;
            }

            return true;
        }
    }
}
