using System;
using System.Collections;
using System.Collections.Generic;
using bismarck.world;
using UnityEngine;

public class GenStepper : MonoBehaviour
{
    private bool paused = true;

    private void Start()
    {
        StartCoroutine(WorldCoroutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            paused = !paused;
        }
    }

    private IEnumerator WorldCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !paused);
            
            WorldManager.Instance.RegenWorld();
        }
    }
}
