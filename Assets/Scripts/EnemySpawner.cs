using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public int Amount;
        public GameObject Enemy;
        public float SpawnTime;
        public float RestTime;
    }

    public List<Wave> Waves;
    private int waveIndex = 0;
    private Wave currentWave;
    private float spawnTime = 2.0f;

    private float RestTime = 0.1f;
    private Boolean rest = false;
    private float mobCount = 0;

    private Random rand = new Random();

    // Use this for initialization
	void OnEnable ()
    {
        currentWave = Waves[0];
	}
	
	// Update is called once per frame
	/*void Update ()
    {
        if (waveIndex >= Waves.Count) return;

        if (currentWave.RestTime < 0)
        {
            waveIndex += 1;
            if (waveIndex >= Waves.Count) return;

            currentWave = Waves[waveIndex];
            return;
        }

        if(currentWave.Amount <= 0)
        {
            currentWave.RestTime -= Time.deltaTime;
            return;
        }

        if(spawnTime < 0)
        {
            Spawn(currentWave.Enemy);

            spawnTime = currentWave.SpawnTime;
            currentWave.Amount--;
            return;
        }

        spawnTime -= Time.deltaTime;        
	}*/

    private void Update()
    {
        if (!rest)
        {
            if (mobCount > 0)
            {
                if (spawnTime < 0)
                {
                    Spawn("soldier");

                    if (waveIndex > 5)
                    {
                        if (rand.Next(0, 100) >= 60 - waveIndex)
                        {
                            if (rand.Next(0, 1) == 1)
                            {
                                Spawn("tank");
                            }
                            else
                            {
                                Spawn("plane");
                            }
                        }
                    }
                    
                    spawnTime = Math.Max(0.1f, 1.5f - waveIndex * 0.1f);
                    mobCount--;
                }
                
                spawnTime -= Time.deltaTime;
                return;
            }
            else
            {
                waveIndex++;
                rest = true;
                return;
            }
        }

        if (rest && RestTime > 0)
        {
            RestTime -= Time.deltaTime;
            return;
        }

        if (RestTime <= 0)
        {
            mobCount = waveIndex * 2;
            RestTime = 5;
            rest = false;
        }
    }

    private void Spawn(String tag)
    {
        var spawnedEnemy = Pool.Instance.ActivateObject(tag);
        spawnedEnemy.SetActive(true);

        EnemyManagerScript.Instance.RegisterEnemy(spawnedEnemy);
    }
}

