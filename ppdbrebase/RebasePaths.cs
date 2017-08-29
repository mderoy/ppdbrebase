using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using Mono.Cecil.Rocks;
using Mono.Cecil.Mdb;
using NiceIO;
using ppdbrebase.Utilities;


namespace ppdbrebase
{
    public class RebasePaths
    {
        private const string WindowsStylePathHeader = "C:\\";
        private const string WindowsSubsystemForLinuxStylePathHeader = "/mnt/c/";
        public enum PathStyle
        {
            Windows = '\\',
            Unix = '/',
        }

        public static string Rebase(string inputPath)
        {
            //The base is not what was passed in,so don't rebase this input
            if (inputPath.Substring(0, Options.OldBase.Length) != Options.OldBase)
                return inputPath;

            Console.WriteLine("Original Path: " + inputPath);
            var outputPath = Options.NewBase +
                             inputPath.Substring(Options.OldBase.Length, inputPath.Length - Options.OldBase.Length)
                                 .Replace('/', (char)Options.NewPathStyle)
                                 .Replace('\\', (char)Options.NewPathStyle);
            Console.WriteLine("Changed To: " + outputPath);
            Console.WriteLine();
            return outputPath;
        }
        public static IEnumerable<MethodDebugInformation> GetDebugInfo(ModuleDefinition module)
        {
            var methodDebugInfos = from t in module.GetTypes()
                                    from m in t.Methods
                                    where m.DebugInformation != null
                                    select m.DebugInformation;
            foreach(var debugInfo in methodDebugInfos)
                yield return debugInfo;
        }

        public static bool RebaseSymbolFile(NPath symbolFileNPath)
        {
            NPath assemblyFileNPath = symbolFileNPath.ChangeExtension("dll");
            //Verify we have both a symbol file and a dll
            if (!symbolFileNPath.FileExists())
            {
                Console.WriteLine("Symbol File " + symbolFileNPath + " Does Not Exist");
                return false;
            }

            if (!assemblyFileNPath.FileExists())
            {
                Console.WriteLine("No DLL for Input Symbol File " + symbolFileNPath);
                return false;
            }

            //Create a backup of the original file, do not overwrite existing backups
            var backupSymbolFileNPath = (symbolFileNPath + ".orig").ToNPath();
            if (!backupSymbolFileNPath.FileExists())
                symbolFileNPath.Copy(backupSymbolFileNPath);

            var readerParameters = new ReaderParameters { SymbolReaderProvider = new DefaultSymbolReaderProvider(), ReadWrite = true };

            //Rewrite The Symbols
            using (var module = ModuleDefinition.ReadModule(assemblyFileNPath, readerParameters))
            {
                using (var pdbreader = module.SymbolReader)
                {
                    foreach (var debugInfo in GetDebugInfo(module))
                    {
                        if (debugInfo.HasSequencePoints)
                        {
                            foreach (var seqpoint in debugInfo.SequencePoints)
                            {
                                seqpoint.Document.Url = Rebase(seqpoint.Document.Url);
                            }
                        }
                    }
                }
                var writerParameters = new WriterParameters() { WriteSymbols = true };
                module.Write(assemblyFileNPath, writerParameters);
            }
            return true;
        }
    }
}
