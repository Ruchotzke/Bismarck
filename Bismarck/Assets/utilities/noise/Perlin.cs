﻿using UnityEngine;

namespace utilities.noise
{
    /// <summary>
    /// A perlin noise generator.
    /// </summary>
    public class Perlin
    {
        /// <summary>
        /// The generator's permutation table.
        /// </summary>
        private static readonly int[] permutation = { 151,160,137,91,90,15,                 // Hash lookup table as defined by Ken Perlin.  This is a randomly
            131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,    // arranged array of all numbers from 0-255 inclusive.
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };

        /// <summary>
        /// The actual used table.
        /// </summary>
        private static readonly int[] p;

        /// <summary>
        /// Whether the texture should be repeated?
        /// </summary>
        private int repeat = 0;
        
        /// <summary>
        /// Create a new perlin noise generator.
        /// </summary>
        /// <param name="tableSize"></param>
        static Perlin()
        {
            /* Permute the original array */
            Arrays.Shuffle(permutation);
            
            /* Generate the permutation table */
            p = new int[512];
            for (int i = 0; i < 512; i++)
            {
                p[i] = permutation[i%256];
            }
        }

        /// <summary>
        /// Sample the perlin noise at a given position.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public float Sample(Vector3 point)
        {
            /* If repeat is on, make sure the point is valid */
            if (repeat > 0)
            {
                point = new Vector3(point.x % repeat, point.y % repeat, point.z % repeat);
            }
            
            /* First break each position into a fractional and integer component to sample the grid */
            Vector3Int pi = new Vector3Int((int)point.x & 255, (int)point.y & 255, (int)point.z & 255); //& wraps
            Vector3 pf = new Vector3(point.x - (int)point.x, point.y - (int)point.y, point.z - (int)point.z);
            
            /* Fade the positions for easier gradients */
            Vector3 uvw = Fade(pf);

            /* Hash the inputs using Ken Perlin's hashing algorithm */
            int aaa, aba, aab, abb, baa, bba, bab, bbb;
            aaa = p[p[p[    pi.x ]+    pi.y ]+    pi.z ];
            aba = p[p[p[    pi.x ]+inc(pi.y)]+    pi.z ];
            aab = p[p[p[    pi.x ]+    pi.y ]+inc(pi.z)];
            abb = p[p[p[    pi.x ]+inc(pi.y)]+inc(pi.z)];
            baa = p[p[p[inc(pi.x)]+    pi.y ]+    pi.z ];
            bba = p[p[p[inc(pi.x)]+inc(pi.y)]+    pi.z ];
            bab = p[p[p[inc(pi.x)]+    pi.y ]+inc(pi.z)];
            bbb = p[p[p[inc(pi.x)]+inc(pi.y)]+inc(pi.z)];
            
            float x1, x2, y1, y2;
            x1 = Mathf.Lerp(    grad (aaa, pf.x  , pf.y  , pf.z),                 // The gradient function calculates the dot product between a pseudorandom
                                grad (baa, pf.x-1, pf.y  , pf.z),               // gradient vector and the vector from the input coordinate to the 8
                                uvw.x);                                               // surrounding points in its unit cube.
            x2 = Mathf.Lerp(    grad (aba, pf.x  , pf.y-1, pf.z),               // This is all then lerped together as a sort of weighted average based on the faded (u,v,w)
                                grad (bba, pf.x-1, pf.y-1, pf.z),             // values we made earlier.
                                uvw.x);
            y1 = Mathf.Lerp(x1, x2, uvw.y);

            x1 = Mathf.Lerp(    grad (aab, pf.x  , pf.y  , pf.z-1),
                                grad (bab, pf.x-1, pf.y  , pf.z-1),
                                uvw.x);
            x2 = Mathf.Lerp(    grad (abb, pf.x  , pf.y-1, pf.z-1),
                                grad (bbb, pf.x-1, pf.y-1, pf.z-1),
                                uvw.x);
            y2 = Mathf.Lerp (x1, x2, uvw.y);
    
            return (Mathf.Lerp (y1, y2, uvw.z)+1)/2; 
        }

        /// <summary>
        /// The fade function implemented originally by ken perlin.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static float Fade(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        /// <summary>
        /// A fade function for vectors.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static Vector3 Fade(Vector3 t)
        {
            return new Vector3(Fade(t.x), Fade(t.y), Fade(t.z));
        }

        /// <summary>
        /// A helper function used in the perlin hash algorithm.
        /// "Increment"
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private int inc(int num)
        {
            num++;
            if (repeat > 0) num %= repeat;
    
            return num;
        }
        
        // Source: http://riven8192.blogspot.com/2010/08/calculate-perlinnoise-twice-as-fast.html
        private static float grad(int hash, float x, float y, float z)
        {
            switch(hash & 0xF)
            {
                case 0x0: return  x + y;
                case 0x1: return -x + y;
                case 0x2: return  x - y;
                case 0x3: return -x - y;
                case 0x4: return  x + z;
                case 0x5: return -x + z;
                case 0x6: return  x - z;
                case 0x7: return -x - z;
                case 0x8: return  y + z;
                case 0x9: return -y + z;
                case 0xA: return  y - z;
                case 0xB: return -y - z;
                case 0xC: return  y + x;
                case 0xD: return -y + z;
                case 0xE: return  y - x;
                case 0xF: return -y - z;
                default: return 0; // never happens
            }
        }
    }
}