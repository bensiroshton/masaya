using NUnit.Framework;
using Siroshton.Masaya.Core;
using Siroshton.Masaya.Util;

namespace Siroshton.Masaya.Tests.Util
{
    public class SelectionUtilTests
    {
        [Test]
        public void SelectNextIncremental()
        {
            int selected = -1;
        
            for(int i=0;i<100;i++)
            {
                selected = SelectionUtil.SelectNext(Types.NextSelectionType.Incremental, selected, 3);
                Assert.AreEqual(selected, i % 3);
            }
        }

        [Test]
        public void SelectNextRandom()
        {
            int selected = -1;
            int count = 10;
            
            for (int i = 0; i < 100; i++)
            {
                selected = SelectionUtil.SelectNext(Types.NextSelectionType.Random, selected, count);
                Assert.GreaterOrEqual(selected, 0);
                Assert.Less(selected, count);
            }
        }

        [Test]
        public void SelectNextRandomButNotLast()
        {
            int lastSelected = -1;
            int selected = -1;
            int count = 10;

            for (int i = 0; i < 100; i++)
            {
                selected = SelectionUtil.SelectNext(Types.NextSelectionType.RandomButNotLast, lastSelected, count);
                Assert.GreaterOrEqual(selected, 0);
                Assert.Less(selected, count);
                Assert.AreNotEqual(selected, lastSelected);
                lastSelected = selected;
            }
        }

        [Test]
        public void SelectNextPingPong()
        {
            SelectionUtil.PingPong pingPong = new SelectionUtil.PingPong();
            pingPong.selected = -1;

            int selected = -1;
            int count = 10;

            for(int times = 0; times < 5; times++)
            {
                for (int i = 0; i < count; i++)
                {
                    selected = SelectionUtil.SelectNextPingPong(ref pingPong, count);
                }

                for (int i = count - 2; i > 0; i--)
                {
                    selected = SelectionUtil.SelectNextPingPong(ref pingPong, count);
                    Assert.AreEqual(selected, i);
                }
            }
        }
    }
}