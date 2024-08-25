using UnityEngine;

namespace utilities
{
    /// <summary>
    /// Easing functions from https://easings.net/
    /// </summary>
    public static class Easing
    {
        public static float EaseOutExpo(float t)
        {
            return t == 0 ? 0 : Mathf.Pow(2, 10 * t - 10);
        }
    }
}