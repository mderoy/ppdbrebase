using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ppdbrebase.Utilities;

namespace ppdbrebase.Tests
{
    [TestFixture]
    public class OptionsTests
    {
        [Test]
        public void InitFailsWithUnknownArgument()
        {
           Assert.IsFalse(Options.InitAndSetup(new string[] {"--input-file=testfile", "--blah-fakearge=lol"}));
        }

        [Test]
        public void InitSucceedsWithWslRebase()
        {
            Assert.IsTrue(Options.InitAndSetup(new string[] { "--input-file=testfile", "--wsl-rebase" }));
        }

        [Test]
        public void InitFailsWithWslRebaseAndOldBase()
        {
            Assert.IsFalse(Options.InitAndSetup(new string[] { "--input-file=testfile", "--wsl-rebase", "--old-base=/mnt/c/" }));
        }

        [Test]
        public void InitFailsWithWslRebaseAndNewBase()
        {
            Assert.IsFalse(Options.InitAndSetup(new string[] { "--input-file=testfile", "--wsl-rebase", "--new-base=/mnt/c/" }));
        }

        [Test]
        public void InitFailsWithWslRebaseAndUnixStyle()
        {
            Assert.IsFalse(Options.InitAndSetup(new string[] { "--input-file=testfile", "--wsl-rebase", "--new-path-style=Unix" }));
        }

        [Test]
        public void InitSucceedsWithInputFile()
        {
            Assert.IsTrue(Options.InitAndSetup(new string[] { "--input-file=testfile" }));
        }

        [Test]
        public void InitSucceedsWithInputDir()
        {
            Assert.IsTrue(Options.InitAndSetup(new string[] { "--input-dir=testdir" }));
        }

        [Test]
        public void InitFailsWithInputDirAndInputFile()
        {
            Assert.IsFalse(Options.InitAndSetup(new string[] { "--input-file=testfile", "--input-dir=testdir" }));
        }

        [Test]
        public void InitFailsWithInputFileAndRecursive()
        {
            Assert.IsFalse(Options.InitAndSetup(new string[] { "--input-file=testfile", "--recursive" }));
        }

        [Test]
        public void InitSucceedsWithInputDirAndRecursive()
        {
            Assert.IsTrue(Options.InitAndSetup(new string[] { "--input-dir=testdir", "--recursive" }));
        }

        [Test]
        public void InitFailsWithNoInputDirOrInputFile()
        {
            Assert.IsFalse(Options.InitAndSetup(new string[] { "--recursive" }));
        }
    }
}
