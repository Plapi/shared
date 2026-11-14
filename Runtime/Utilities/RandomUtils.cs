using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace com.Plapamaru.Shared.Utilities
{
    public static class RandomUtils
    {
        public static void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public static bool CoinFlip()
        {
            return Random.value < 0.5f;
        }

        public static T SelectWeighted<T>(IReadOnlyList<T> items, IReadOnlyList<float> weights, out int index)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("No items to select from.", nameof(items));
            if (weights == null || weights.Count != items.Count)
                throw new ArgumentException("Weights must match items in length.", nameof(weights));

            if (items.Count == 1)
            {
                index = 0;
                return items[0];
            }

            float totalWeight = 0f;
            for (int i = 0; i < weights.Count; i++)
                totalWeight += weights[i];

            if (totalWeight <= 0f)
                throw new InvalidOperationException("All weights are zero, no item can be selected.");

            float randomValue = Random.value * totalWeight;
            float cumulative = 0f;
            for (int i = 0; i < items.Count; i++)
            {
                cumulative += weights[i];
                if (randomValue <= cumulative)
                {
                    index = i;
                    return items[i];
                }
            }

            index = items.Count - 1;
            return items[index];
        }

        public static T SelectWeighted<T>(IReadOnlyList<T> items, IReadOnlyList<float> weights)
        {
            return SelectWeighted(items, weights, out _);
        }
    }
}
