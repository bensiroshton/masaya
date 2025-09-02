using NUnit.Framework;
using static Siroshton.Masaya.Core.Types;

namespace Siroshton.Masaya.Tests.Core
{
    public class TypesTests
    {
        [Test]
        public void TimeClip()
        {
            TimeClip clip;
            clip.start = 50;
            clip.end = 100;
            clip.duration = 200;

            Assert.AreEqual(clip.end - clip.start, clip.clipDuration);
            Assert.AreEqual(clip.start / clip.duration, clip.normalizedStartTime);
            Assert.AreEqual(clip.end / clip.duration, clip.normalizedEndTime);
            Assert.AreEqual(0.5f, clip.GetClipPositionFromTime(75));
            Assert.AreEqual(0.5f, clip.GetClipPositionFromNormalizedTime(75f / 200f));
        }
    }
}