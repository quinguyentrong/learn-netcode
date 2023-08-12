using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnBall : NetworkBehaviour
{
    public GameObject BallPrefabs;

    private void Start()
    {
        GameplayManager.Instance.SpawnBall += SpawningBall;
        SpawningBall();
    }

    public override void OnDestroy()
    {
        GameplayManager.Instance.SpawnBall -= SpawningBall;
        base.OnDestroy();
    }

    private void SpawningBall()
    {
        if (IsServer == false) return;

        NetworkObjectSpawner.SpawnNewNetworkObject(BallPrefabs, Vector3.zero);
    }
}
