using UnityEngine;
using static Siroshton.Masaya.Core.Types;

namespace Siroshton.Masaya.Util
{
    static public class SelectionUtil
    {

        public struct PingPong
        {
            public int selected;
            public Direction1D direction;
        }

        static public int SelectNext(NextSelectionType option, int lastSelected, int count)
        {
            if( count == 0 ) return -1;
            else if( count == 1 ) return 0;
            else if( option == NextSelectionType.Incremental )
            {
                lastSelected += 1;
                if( lastSelected >= count ) return 0;
                else return lastSelected;
            }
            else if( option == NextSelectionType.Random )
            {
                return Random.Range(0, count);
            }
            else if( option == NextSelectionType.RandomButNotLast )
            {
                int next;
                do
                {
                    next = Random.Range(0, count);
                    if( next != lastSelected ) return next;
                }
                while(true);
            }
            else
            {
                Debug.LogError($"{option} is not a supported selection type.");
                return -1;
            }
        }

        static public int SelectNextPingPong(ref PingPong pingPong, int count)
        {
            if( count < 0 ) return -1;
            else if( count == 1 ) return 0;
            else if(pingPong.direction == Direction1D.Forward)
            {
                pingPong.selected++;

                if( pingPong.selected < 0 )
                {
                    pingPong.selected = 0;
                }
                else if( pingPong.selected >= count )
                {
                    pingPong.selected = count - 2;
                    pingPong.direction = Direction1D.Backward;
                }
            }
            else
            {
                pingPong.selected--;
                if( pingPong.selected > count - 1 )
                {
                    pingPong.selected = count - 1;
                }
                else if( pingPong.selected < 0 )
                {
                    pingPong.selected = 1;
                    pingPong.direction = Direction1D.Forward;
                }
            }

            return pingPong.selected;
        }
    }
}