/* StaticUtils.WeightedRandom.cs - C's Utils
 * 
 * Generated a random number given specific weights
 * 
 * 
 * Creation Date: 16/04/2024
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [16/04/2024] - Initial implementation (C137)
 *      
 */
using System;

namespace CsUtils
{
    [Serializable]
    public struct WeightedNumber
    {
        /// <summary>
        /// The number that will be returned
        /// </summary>
        public int number;

        /// <summary>
        /// The probability of that number to be returned
        /// </summary>
        public float probability;
    }

    public static partial class StaticUtils
    {
        /// <summary>
        /// Returns a number based on a probability
        /// </summary>
        /// <param name="weightedNumbers">The numbers and probabilities</param>
        /// <returns></returns>
        public static int WeightedRandom(params WeightedNumber[] weightedNumbers)
        {
            //Get total probability
            float totalProbability = 0;
            foreach (var number in weightedNumbers)
            {
                totalProbability += number.probability;
            }

            //Normalize probabilities
            for (int i = 0; i < weightedNumbers.Length; i++)
            {
                weightedNumbers[i].probability /= totalProbability;
            }

            float randomPoint = UnityEngine.Random.value;
            for (int i = 0; i < weightedNumbers.Length; i++)
            {
                if (randomPoint < weightedNumbers[i].probability)
                {
                    return weightedNumbers[i].number;
                }
                else
                {
                    randomPoint -= weightedNumbers[i].probability;
                }
            }
            return weightedNumbers[^1].number;
        }
    }
}