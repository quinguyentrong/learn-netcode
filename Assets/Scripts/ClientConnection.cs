using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientConnection : NetworkBehaviour
{
    public static ClientConnection Instance;
    [SerializeField] private int MaxConnections;
    [SerializeField] private CharacterDataSO[] CharacterDatas;

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

    public bool IsExtraClient(ulong clientId)
    {
        return CanConnect(clientId);
    }

    public bool CanClientConnect(ulong clientId)
    {
        if (!IsServer)
            return false;

        bool canConnect = CanConnect(clientId);
        if (!canConnect)
        {
            RemoveClient(clientId);
        }

        return canConnect;
    }

    private bool CanConnect(ulong clientId)
    {
        if (LoadingSceneManager.Instance.SceneActive == SceneName.Room)
        {
            int playersConnected = NetworkManager.Singleton.ConnectedClientsList.Count;

            if (playersConnected > MaxConnections)
            {
                print($"Sorry we are full {clientId}");
                return false;
            }

            print($"You are allowed to enter {clientId}");
            return true;
        }
        else
        {
            if (ItHasACharacterSelected(clientId))
            {
                print($"You are allowed to enter {clientId}");
                return true;
            }
            else
            {
                print($"Sorry we are full {clientId}");
                return false;
            }
        }
    }

    private void RemoveClient(ulong clientId)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        ShutdownClientRpc(clientRpcParams);
    }

    private bool ItHasACharacterSelected(ulong clientId)
    {
        foreach (var data in CharacterDatas)
        {
            if (data.clientId == clientId)
            {
                return true;
            }
        }

        return false;
    }

    [ClientRpc]
    private void ShutdownClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Shutdown();
    }

    private void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
        LoadingSceneManager.Instance.LoadScene(SceneName.Menu, false);
    }
}
