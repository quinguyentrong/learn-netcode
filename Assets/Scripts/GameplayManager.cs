using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameplayManager : NetworkBehaviour
{
    public static GameplayManager Instance;

    public Action SpawnBall;

    [SerializeField] private CharacterDataSO[] CharactersData;

    [SerializeField] private Transform[] PadStartingPositions;

    private int NumberOfPlayerConnected;

    private List<ulong> ConnectedClients = new List<ulong>();

    private List<PadController> PlayerPad = new List<PadController>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //private void Update()
    //{
    //    Debug.Log(NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.Singleton.LocalClientId));
    //}

    //private void OnEnable()
    //{
    //    if (!IsServer)
    //        return;

    //    NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    //}

    //private void OnClientDisconnect(ulong clientId)
    //{
    //    foreach (var player in PlayerPad)
    //    {
    //        if (player != null)
    //        {
    //            if (player.characterData.clientId == clientId)
    //            {

    //            }
    //        }
    //    }
    //}

    public void ServerSceneInit(ulong clientId)
    {
        ConnectedClients.Add(clientId);

        if (ConnectedClients.Count < NetworkManager.Singleton.ConnectedClients.Count)
            return;

        foreach (var client in ConnectedClients)
        {
            int index = 0;

            foreach (CharacterDataSO data in CharactersData)
            {
                if (data.isSelected && data.clientId == client)
                {
                    GameObject playerSpaceship =
                        NetworkObjectSpawner.SpawnNewNetworkObjectChangeOwnershipToClient(
                            data.PadPrefab,
                            PadStartingPositions[NumberOfPlayerConnected].position,
                            data.clientId,
                            true);

                    PadController playerShipController =
                        playerSpaceship.GetComponent<PadController>();

                    PlayerPad.Add(playerShipController);

                    NumberOfPlayerConnected++;
                }

                index++;
            }
        }
    }
}
