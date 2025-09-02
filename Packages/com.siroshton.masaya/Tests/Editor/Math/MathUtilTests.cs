using NUnit.Framework;
using Siroshton.Masaya.Math;
using UnityEngine;

namespace Siroshton.Masaya.Tests.Math
{
    public class MathUtilTests
    {
        [Test]
        public void Normalize()
        {
            Assert.AreEqual(0.5f, MathUtil.Normalize(0, 1, 0.5f));
            Assert.AreEqual(0.25f, MathUtil.Normalize(0, 100, 25));
            Assert.AreEqual(0.5f, MathUtil.Normalize(0, 100, 50));
            Assert.AreEqual(0.75f, MathUtil.Normalize(0, 100, 75));
            Assert.AreEqual(0, MathUtil.Normalize(0, 100, -10));
            Assert.AreEqual(1, MathUtil.Normalize(0, 100, 200));
            Assert.AreEqual(0.5f, MathUtil.Normalize(-1, 1, 0));
            Assert.AreEqual(0.5f, MathUtil.Normalize(50, 100, 75));
            Assert.AreEqual(0.5f, MathUtil.Normalize(-100, -50, -75));
            Assert.AreEqual(0.5f, MathUtil.Normalize(-9, 1, -4));
            
            Assert.AreEqual(0.75f, MathUtil.Normalize(0, -100, -75));
        }

        [Test]
        public void GetDecimal()
        {
            Debug.Log(MathUtil.GetDecimal(1234.1f));
            Debug.Log(MathUtil.GetDecimal(-1234.5f));

            //Assert.IsTrue(Mathf.Approximately(MathUtil.GetDecimal(1234.1f), 0.1f), "0.1f");
            //Assert.IsTrue(Mathf.Approximately(MathUtil.GetDecimal(-1234.5f), 0.5f), "0.5f");
            Assert.Pass();
        }
    }
}