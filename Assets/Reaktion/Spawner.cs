//
// Reaktion - An audio reactive animation toolkit for Unity.
//
// Copyright (C) 2013, 2014 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using UnityEngine;
using System.Collections;

namespace Reaktion {

public class Spawner : MonoBehaviour
{
    // Prefabs to be spawned.
    public GameObject[] prefabs;

    // Spawn rate settings.
    [SerializeField]
    float _spawnRate;
    public float spawnRate {
        get { return _spawnRate; }
        set { 
            _spawnRate = value;
        }
    }

    [SerializeField]
    float _spawnRateRandomness;
    public float spawnRateRandomness {
        get { return _spawnRateRandomness; }
        set { 
            _spawnRateRandomness = value;
        }
    }

    // Distribution settings.
    public enum Distribution { InSphere, InBox, AtPoints }
    public Distribution distribution;

    public float sphereRadius;
    public Vector3 boxSize;
    public Transform[] spawnPoints;

    // Rotation settings.
    public bool randomRotation;
    public bool invertSortOrder;

    // Private variables.
    float randomValue;
    float timer;
    int spawnPointIndex;

    // Spawn an instance.
    public void Spawn()
    {
        var prefab = prefabs[Random.Range(0, prefabs.Length)];

        Quaternion rotation = randomRotation ? Random.rotation : prefab.transform.localRotation;

        if (distribution == Distribution.AtPoints)
        {
            // Choose a spawn point in random order.
            spawnPointIndex += Random.Range(1, spawnPoints.Length);
            spawnPointIndex %= spawnPoints.Length;
            var pt = spawnPoints[spawnPointIndex];

            var instance = Instantiate(prefab, Vector2.zero, rotation) as GameObject;
            instance.transform.SetParent(pt, false);
            if (invertSortOrder) instance.transform.SetSiblingIndex(0);
        }
        else
        {
            Vector2 position;
                
            if (distribution == Distribution.InSphere)
            {
                position = Random.insideUnitSphere * sphereRadius;
            }
            else // Distribution.InBox
            {
                var rv = new Vector3(Random.value, Random.value, Random.value);
                position = Vector3.Scale(rv - Vector3.one * 0.5f, boxSize);
            }

            var instance = Instantiate(prefab, position, rotation) as GameObject;
            instance.transform.SetParent(transform, false);
            if (invertSortOrder) instance.transform.SetSiblingIndex(0);
        }
    }

    // Make some instances.
    public void Spawn(int count)
    {
        while (count-- > 0) Spawn();
    }

    void Update()
    {
        if (spawnRate > 0.0f)
        {
            timer += Time.deltaTime;

            while (timer > (1.0f - randomValue) / spawnRate)
            {
                Spawn();
                timer -= (1.0f - randomValue) / spawnRate;
                randomValue = Random.value * spawnRateRandomness;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.5f);

        if (distribution == Distribution.AtPoints)
        {
            foreach (var pt in spawnPoints)
                if (pt != null)
                    Gizmos.DrawWireCube(pt.position, Vector3.one * 0.1f);
        }
        else if (distribution == Distribution.InSphere)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, sphereRadius);
        }
        else // Distribution.InBox
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
        }
    }
}

} // namespace Reaktion
