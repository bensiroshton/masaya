using NUnit.Framework;
using Siroshton.Masaya.Util;

namespace Siroshton.Masaya.Tests.Util
{
    public class PathUtilTests
    {
        [Test]
        public void Split()
        {
            string path = "test/this/";
            string testPath = "test/this";
            string file = "file.ext";

            string[] parts = PathUtil.Split(path + file);

            Assert.AreEqual(testPath, parts[0]);
            Assert.AreEqual(file, parts[1]);

            parts = PathUtil.Split(null);

            Assert.AreEqual("", parts[0]);
            Assert.AreEqual("", parts[1]);

            parts = PathUtil.Split(file);

            Assert.AreEqual("", parts[0]);
            Assert.AreEqual(file, parts[1]);

            parts = PathUtil.Split(path);

            Assert.AreEqual(testPath, parts[0]);
            Assert.AreEqual("", parts[1]);
        }

        [Test]
        public void GetUniquePathsFromFiles()
        {
            string[] files = new string[] {
                "/a/blah.txt",
                "/a/b/c/two.txt",
                "/a/b/c/three.txt",
                "/x/y/z/123.jpeg",
            };

            string[] paths = PathUtil.GetUniquePathsFromFiles(files);
            Assert.AreEqual(3, paths.Length);
            Assert.AreEqual("/a", paths[0]);
            Assert.AreEqual("/a/b/c", paths[1]);
            Assert.AreEqual("/x/y/z", paths[2]);
        }
    }
}