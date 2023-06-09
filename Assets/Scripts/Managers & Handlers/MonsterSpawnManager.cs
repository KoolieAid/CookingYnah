using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MonsterSpawnManager : MonoBehaviour
{

    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private SerializedDictionary<Transform, Transform> spawnBaseDictionary = new SerializedDictionary<Transform, Transform>();
    private List<Transform> spawnPointList = new List<Transform>();
    private List<Transform> basePointList = new List<Transform>();
    [SerializeField] private MonsterGroup[] monsterGroups;

    [SerializeField] private GameObject spawnHandlerPrefab;

    private int monsterID = 1000;

    private void Start()
    {
        foreach (KeyValuePair<Transform, Transform> pair in spawnBaseDictionary)
        {
            spawnPointList.Add(pair.Key);
            basePointList.Add(pair.Value);
        }
        SpawnAllMonsters();
    }

    void SpawnAllMonsters()
    {
        for (int i = 0; i < spawnPointList.Count; i++)
        {
            SpawnInitialMonsters(i);
        }
    }

    void SpawnInitialMonsters(int index)
    {
        GameObject clone = Instantiate(spawnHandlerPrefab, transform);
        SpawningHandler sH = clone.GetComponent<SpawningHandler>();
        sH.SetData(monsterPrefab, spawnPointList[index], basePointList[index]);

        Dictionary<EnemyUnitData, int> mGDictionary = monsterGroups[index].GroupComposition;

        foreach (KeyValuePair<EnemyUnitData, int> pair in mGDictionary)
        {
            for (int j = 0; j < pair.Value; j++)
            {
                sH.AddEnemyToSpawn(monsterID,pair.Key);
                monsterID++;
            }

        }
        sH.StartSpawning();
    }
}
