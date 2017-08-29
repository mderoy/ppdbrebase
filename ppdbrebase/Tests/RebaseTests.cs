using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NiceIO;
using NUnit.Framework;
using NUnit.Framework.Internal;
using ppdbrebase.Utilities;

namespace ppdbrebase.Tests
{
    [TestFixture]
    public class RebaseTests
    {
        private NPath testDllsPath;
        private NPath tempPath;

        [SetUp]
        public void Setup()
        {
            testDllsPath = TestContext.CurrentContext.TestDirectory.ToNPath().Parent.Parent.Combine("Tests").Combine("TestFiles");
            tempPath = Path.GetTempPath().ToNPath().Combine("ppdbRebaseTests");

            tempPath.DeleteContents();
            tempPath.EnsureDirectoryExists();
            testDllsPath.CopyFiles(tempPath, true);
        }

        [Test]
        public void RebaseWindowsSubsystemForLinuxPaths()
        {
            Assert.IsTrue(Options.InitAndSetup(new string[] { "--input-file=ignored", "--wsl-rebase" }));
            string pathWithoutSpacesRebased = RebasePaths.Rebase("/mnt/c/workspaces/mkderoy/testfile.cs");
            Assert.AreEqual("C:\\workspaces\\mkderoy\\testfile.cs", pathWithoutSpacesRebased);
        }

        [Test]
        public void RebaseUnixToUnix()
        {
            Assert.IsTrue(Options.InitAndSetup(new string[] { "--input-file=ignored", "--old-base=/home/mkderoy/", "--new-base=/home/", "--new-path-style=Unix" }));
            string pathWithoutSpacesRebased = RebasePaths.Rebase("/home/mkderoy/workspaces/mkderoy/testfile.cs");
            Assert.AreEqual("/home/workspaces/mkderoy/testfile.cs", pathWithoutSpacesRebased);
        }

        [Test]
        public void RebaseWindowsToWindows()
        {
            Assert.IsTrue(Options.InitAndSetup(new string[] { "--input-file=ignored", "--old-base=C:\\workspaces\\mkderoy\\", "--new-base=D:\\test\\", "--new-path-style=Windows" }));
            string pathWithoutSpacesRebased = RebasePaths.Rebase("C:\\workspaces\\mkderoy\\testfile.cs");
            Assert.AreEqual("D:\\test\\testfile.cs", pathWithoutSpacesRebased);
        }

        [Test]
        public void RebaseUnixToWindows()
        {
            Assert.IsTrue(Options.InitAndSetup(new string[] { "--input-file=ignored", "--old-base=/home/mkderoy/", "--new-base=C:\\", "--new-path-style=Windows" }));
            string pathWithoutSpacesRebased = RebasePaths.Rebase("/home/mkderoy/workspaces/mkderoy/testfile.cs");
            Assert.AreEqual("C:\\workspaces\\mkderoy\\testfile.cs", pathWithoutSpacesRebased);
        }

        [Test]
        public void RebaseWindowsToUnix()
        {
            Assert.IsTrue(Options.InitAndSetup(new string[] { "--input-file=ignored", "--old-base=C:\\", "--new-base=/home/", "--new-path-style=Unix" }));
            string pathWithoutSpacesRebased = RebasePaths.Rebase("C:\\mkderoy\\testfile.cs");
            Assert.AreEqual("/home/mkderoy/testfile.cs", pathWithoutSpacesRebased);
        }

        [Test]
        public void RebaseSymbolFileCreatesBackup()
        {
            foreach (var file in tempPath.Files("*.pdb", false))
            {
                Assert.IsTrue(Options.InitAndSetup(new string[] { "--input-file=" + file, "--wsl-rebase" }));
                RebasePaths.RebaseSymbolFile(file);
                Assert.IsTrue((file + ".orig").ToNPath().FileExists());
            }
        }

        public static void TestAssemblyFileHasBase(NPath file, string testBase)
        {
            //Ensure The Paths Have Been Rebased
            var readerParameters = new ReaderParameters
            {
                SymbolReaderProvider = new DefaultSymbolReaderProvider(),
                ReadWrite = true
            };

            using (var module = ModuleDefinition.ReadModule(file.ChangeExtension("dll"), readerParameters))
            {
                using (var pdbreader = module.SymbolReader)
                {
                    foreach (var debugInfo in RebasePaths.GetDebugInfo(module))
                    {
                        if (debugInfo.HasSequencePoints)
                        {
                            foreach (var seqpoint in debugInfo.SequencePoints)
                            {
                                Assert.IsTrue(seqpoint.Document.Url.Substring(0, testBase.Length) == testBase);
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public void RebaseSymbolFileSavesSymbols()
        {
            foreach (var file in tempPath.Files("*.pdb", false))
            {
                Assert.IsTrue(Options.InitAndSetup(new string[] {"--input-file=" + file, "--wsl-rebase"}));

                //Ensure The Input Assembly Has Unchanged Base
                TestAssemblyFileHasBase(file, "/mnt/c/");

                RebasePaths.RebaseSymbolFile(file);

                //Ensure That the rebase succeeded
                TestAssemblyFileHasBase(file, "C:\\");
            }
        }
    }
}
