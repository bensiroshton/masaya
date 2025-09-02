using NUnit.Framework;
using Siroshton.Masaya.Player;
using UnityEngine;

namespace Siroshton.Masaya.Tests.Player
{
    public class LevelModifierTests
    {
        [Test]
        public void DumpDifficultyRampTable()
        {
            int baseSize = 4;
            float difficultyRamp = 0.01f;
            int last = baseSize;
            float percentGainPerLevel = 0.02f;
            int totalCost = 0;

            Debug.Log($"base block size: {baseSize}, difficulty ramp: {difficultyRamp}");

            for (int level = 1; level <= 100; level++)
            {
                int block = PlayerUtil.GetBlockSizeForLevel(baseSize, level, difficultyRamp);
                totalCost += block;
                Debug.Log($"[{level}] {block}, change: {block - last}, percent gain: {percentGainPerLevel * (float)level * 100.0f}%, total cost: {totalCost}");
                last = block;
            }

        }
    }
}