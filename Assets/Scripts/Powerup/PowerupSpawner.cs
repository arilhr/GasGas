using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PowerupSpawner : NetworkBehaviour
{

    [SerializeField]
    private List<GameObject> powerupPrefabs;

    private PowerSpawnPoint[] spawnPoints = null;

    private void Start()
    {
        CountdownSystem.OnStartGame += StartSpawn;
    }

    [Server]
    public void StartSpawn()
    {
        spawnPoints = gameObject.GetComponentsInChildren<PowerSpawnPoint>();

        foreach (var spawnPoint in spawnPoints)
            SpawnPowerup(spawnPoint.gameObject.transform.position);
    }

    private void SpawnPowerup(Vector3 _pos)
    {
        int powerupType = RandomingPower();

        GameObject powerupGameObject = Instantiate(powerupPrefabs[powerupType], _pos, Quaternion.identity);

        NetworkServer.Spawn(powerupGameObject);
    }

    private int RandomingPower()
    {
        int power = Random.Range(0, powerupPrefabs.Count);

        return power;
    }
}
