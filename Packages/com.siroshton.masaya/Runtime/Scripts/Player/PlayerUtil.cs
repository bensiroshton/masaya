using UnityEngine;

namespace Siroshton.Masaya.Player
{

    static public class PlayerUtil
    {
        static public int GetBlockSizeForLevel(int baseBlockSize, int level, float difficultyRamp)
        {
            float p = level * level;
            return baseBlockSize + Mathf.CeilToInt(p * (float)baseBlockSize * difficultyRamp);
        }

    }

}