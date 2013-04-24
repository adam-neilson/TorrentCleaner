using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentCleaner.Tests
{
    [TestClass]
    public class FileInfoFullNameComparerTest
    {
        [TestMethod]
        public void EqualityTest()
        {
            FileInfo file1 = new FileInfo(@"c:\these\files\are\equal.txt");
            FileInfo file2 = new FileInfo(@"c:\these\files\are\equal.txt");

            var result = new[] { file1 }.SequenceEqual(new[] { file2 }, FileSystemInfoFullNameComparer.Instance);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void EqualityDifferentCaseTest()
        {
            FileInfo file1 = new FileInfo(@"c:\these\FILES\are\EQUAL.txt");
            FileInfo file2 = new FileInfo(@"c:\these\files\are\equal.txt");

            var result = new[] { file1 }.SequenceEqual(new[] { file2 }, FileSystemInfoFullNameComparer.Instance);

            Assert.IsTrue(result);            
        }

        [TestMethod]
        public void EqualityFailureTest()
        {
            FileInfo file1 = new FileInfo(@"c:\these\files\are\not\equal.txt");
            FileInfo file2 = new FileInfo(@"c:\these\files\are\not\at\all\equal.txt");

            var result = new[] { file1 }.SequenceEqual(new[] { file2 }, FileSystemInfoFullNameComparer.Instance);
            
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DictionaryTest()
        {
            FileInfo file1 = new FileInfo(@"c:\these\FILES\are\EQUAL.txt");
            FileInfo file2 = new FileInfo(@"c:\these\files\are\equal.txt");

            Dictionary<FileInfo, string> dictionary = new Dictionary<FileInfo, string>(FileSystemInfoFullNameComparer.Instance);

            dictionary[file1] = "first";
            dictionary[file2] = "second";

            Assert.AreEqual("second", dictionary[file1]);
        }
    }
}
