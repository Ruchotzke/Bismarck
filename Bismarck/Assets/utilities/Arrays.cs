using System.Collections.Generic;
using UnityEngine;

namespace utilities
{
    /// <summary>
    /// A class used for array algorithms.
    /// </summary>
    public static class Arrays
    {
        /// <summary>
        /// Shuffle the provided array in place.
        /// </summary>
        /// <param name="arr"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void Shuffle<T>(T[] arr)
        {
            for (int i = arr.Length - 1; i > 0; i--)
            {
                /* Roll a swap position */
                int j = Random.Range(0, i + 1);
                
                /* Swap if needed */
                if (i != j)
                {
                    (arr[i], arr[j]) = (arr[j], arr[i]);
                }
            }
        }
        
        /// <summary>
        /// Shuffle the provided list in place.
        /// </summary>
        /// <param name="arr"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void Shuffle<T>(List<T> arr)
        {
            for (int i = arr.Count - 1; i > 0; i--)
            {
                /* Roll a swap position */
                int j = Random.Range(0, i + 1);
                
                /* Swap if needed */
                if (i != j)
                {
                    (arr[i], arr[j]) = (arr[j], arr[i]);
                }
            }
        }
    }
}