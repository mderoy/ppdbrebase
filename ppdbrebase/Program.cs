using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceIO;
using ppdbrebase.Utilities;

namespace ppdbrebase
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!Options.InitAndSetup(args))
            {
                Console.WriteLine("Failed To Parse Arguments");
                return;
            }

            if (Options.InputFile != null)
            {
                var inputFile = Options.InputFile.ToNPath();
                Directory.SetCurrentDirectory(inputFile.Parent);
                Console.WriteLine("Rebasing Symbols In " + inputFile);
                if (!RebasePaths.RebaseSymbolFile(inputFile))
                {
                    Console.WriteLine("Failed To Rebase Symbols In " + inputFile);
                }
            }
            else if (Options.InputDir != null)
            {
                Directory.SetCurrentDirectory(Options.InputDir);
                foreach (var inputFile in Options.InputDir.ToNPath().Files("*.pdb", Options.Recursive))
                {
                    Console.WriteLine("Rebasing Symbols In " + inputFile);
                    if (!RebasePaths.RebaseSymbolFile(inputFile))
                    {
                        Console.WriteLine("Failed To Rebase Symbols In " + inputFile);
                    }
                }
            }
        }
    }
}
