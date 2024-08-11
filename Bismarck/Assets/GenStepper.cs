using System;
using System.Collections;
using System.Collections.Generic;
using bismarck.world;
using UnityEngine;

public class GenStepper : MonoBehaviour
{
    private int iterations = 0;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            /* Continue the generation process */
            WorldManager.Instance.RegenWorld(iterations++);
        }
    }
}
