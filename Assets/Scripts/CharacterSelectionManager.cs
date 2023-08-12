using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public enum ConnectionState
{
    connected = 0,
    disconnected = 1,
    ready = 2,
}

[Serializable]
public struct PlayerConnectionState
{
    public ConnectionState playerState;
    public PlayerCharacterSelection playerObject;
    public string playerName;
    public ulong clientId;
}

[Serializable]
public struct CharacterContainer
{
    //public Image imageContainer;                    // The image of the character container
    //public TextMeshProUGUI nameContainer;           // Character name container
    public GameObject NameTagNormal;                       // The border of the character container when not ready
    //public GameObject NameTagReady;                  // The border of the character container when ready
    public GameObject NameTagClient;                 // Client border of the character container
    //public Image playerIcon;                        // The background icon of the player (p1, p2)
    public GameObject WaitingText;                  // The waiting text on the container were no client connected
    public GameObject BackgroundClientPadNotReady;               // The background of the ship when not ready
    public Image PadImage;               // The image of the ship when not ready
    //public GameObject backgroundShipReady;          // The background of the ship when ready
    //public Image PadReadyImage;          // The image of the ship when ready
    public GameObject BackgroundClientPadReady;    // Client background of the ship when ready
    public Image ClientPadReadyImage;    // Client image of the ship when ready
}

public class CharacterSelectionManager : NetworkBehaviour
{
    public static CharacterSelectionManager Instance;

    public CharacterDataSO[] CharactersData;

    [SerializeField] PlayerConnectionState[] PlayerStates;

    [SerializeField] CharacterContainer[] CharactersContainers;

    [SerializeField] GameObject PlayerPrefab;

    [SerializeField] GameObject ReadyButton;

    [SerializeField] GameObject CancelButton;

    [SerializeField] SceneName NextScene;

    bool IsTimerOn;
    float Timer = 1;

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

    void Update()
    {
        if (!IsServer)
            return;

        if (!IsTimerOn)
            return;

        Timer -= Time.deltaTime;
        if (Timer <= 0f)
        {
            IsTimerOn = false;
            StartGame();
        }
    }

    void StartGame()
    {
        LoadingSceneManager.Instance.LoadScene(NextScene);
    }

    public int GetPlayerId(ulong clientId)
    {
        for (int i = 0; i < PlayerStates.Length; i++)
        {
            if (PlayerStates[i].clientId == clientId)
                return i;
        }

        Debug.LogError("This should never happen");
        return -1;
    }

    public void SetPlayebleChar(int playerId, int characterSelected, bool isClientOwner)
    {
        SetCharacterUI(playerId, characterSelected);
        if (isClientOwner)
        {
            CharactersContainers[playerId].NameTagClient.SetActive(true);
            CharactersContainers[playerId].NameTagNormal.SetActive(false);
        }
        else
        {
            CharactersContainers[playerId].NameTagNormal.SetActive(true);
            CharactersContainers[playerId].NameTagClient.SetActive(false);
        }

        CharactersContainers[playerId].BackgroundClientPadNotReady.SetActive(true);
        CharactersContainers[playerId].WaitingText.SetActive(false);
    }

    public void SetCharacterUI(int playerId, int characterSelected)
    {
        CharactersContainers[playerId].PadImage.sprite =
            CharactersData[characterSelected].CharacterPadSprite;

        CharactersContainers[playerId].ClientPadReadyImage.sprite =
            CharactersData[characterSelected].CharacterPadSprite;
    }

    public void ServerSceneInit(ulong clientId)
    {
        GameObject go =
            NetworkObjectSpawner.SpawnNewNetworkObjectChangeOwnershipToClient(
                PlayerPrefab,
                transform.position,
                clientId,
                true);

        for (int i = 0; i < PlayerStates.Length; i++)
        {
            if (PlayerStates[i].playerState == ConnectionState.disconnected)
            {
                PlayerStates[i].playerState = ConnectionState.connected;
                PlayerStates[i].playerObject = go.GetComponent<PlayerCharacterSelection>();
                PlayerStates[i].playerName = go.name;
                PlayerStates[i].clientId = clientId;
                break;
            }
        }

        for (int i = 0; i < PlayerStates.Length; i++)
        {
            if (PlayerStates[i].playerObject != null)
                PlayerConnectsClientRpc(
                    PlayerStates[i].clientId,
                    i,
                    PlayerStates[i].playerState,
                    PlayerStates[i].playerObject.GetComponent<NetworkObject>());
        }

    }

    [ClientRpc]
    public void PlayerConnectsClientRpc(
        ulong clientId,
        int stateIndex,
        ConnectionState state,
        NetworkObjectReference player)
    {
        if (IsServer)
            return;

        if (state != ConnectionState.disconnected)
        {
            PlayerStates[stateIndex].playerState = state;
            PlayerStates[stateIndex].clientId = clientId;

            if (player.TryGet(out NetworkObject playerObject))
                PlayerStates[stateIndex].playerObject =
                    playerObject.GetComponent<PlayerCharacterSelection>();
        }
    }

    public void SetPlayerReadyUIButtons(bool isReady, int characterSelected)
    {
        if (isReady && !CharactersData[characterSelected].isSelected)
        {
            ReadyButton.SetActive(false);
            CancelButton.SetActive(true);
        }
        else if (!isReady && CharactersData[characterSelected].isSelected)
        {
            ReadyButton.SetActive(true);
            CancelButton.SetActive(false);
        }
    }

    public void PlayerReady(ulong clientId, int playerId, int characterSelected)
    {
        if (!CharactersData[characterSelected].isSelected)
        {
            PlayerReadyClientRpc(clientId, playerId, characterSelected);

            StartGameTimer();
        }
    }

    public void PlayerNotReady(ulong clientId, int characterSelected = 0, bool isDisconected = false)
    {
        int playerId = GetPlayerId(clientId);
        IsTimerOn = false;
        Timer = 1;

        RemoveReadyStates(clientId, isDisconected);

        if (isDisconected)
        {
            PlayerDisconnectedClientRpc(playerId);
        }
        else
        {
            PlayerNotReadyClientRpc(clientId, playerId, characterSelected);
        }
    }

    [ClientRpc]
    void PlayerReadyClientRpc(ulong clientId, int playerId, int characterSelected)
    {
        CharactersData[characterSelected].isSelected = true;
        CharactersData[characterSelected].clientId = clientId;
        CharactersData[characterSelected].playerId = playerId;
        PlayerStates[playerId].playerState = ConnectionState.ready;

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            CharactersContainers[playerId].BackgroundClientPadReady.SetActive(true);
            CharactersContainers[playerId].BackgroundClientPadNotReady.SetActive(false);
        }
        else
        {
            CharactersContainers[playerId].BackgroundClientPadNotReady.SetActive(false);
            CharactersContainers[playerId].BackgroundClientPadReady.SetActive(true);
        }
    }

    [ClientRpc]
    void PlayerNotReadyClientRpc(ulong clientId, int playerId, int characterSelected)
    {
        CharactersData[characterSelected].isSelected = false;
        CharactersData[characterSelected].clientId = 0UL;
        CharactersData[characterSelected].playerId = -1;

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            CharactersContainers[playerId].NameTagClient.SetActive(true);
            CharactersContainers[playerId].BackgroundClientPadReady.SetActive(false);
            CharactersContainers[playerId].BackgroundClientPadNotReady.SetActive(true);
        }
        else
        {
            CharactersContainers[playerId].NameTagNormal.SetActive(true);
            CharactersContainers[playerId].BackgroundClientPadNotReady.SetActive(true);
            CharactersContainers[playerId].BackgroundClientPadReady.SetActive(false);
        }
    }

    void StartGameTimer()
    {
        foreach (PlayerConnectionState state in PlayerStates)
        {
            if (state.playerState == ConnectionState.connected) return;
        }

        Timer = 1;
        IsTimerOn = true;
    }

    void RemoveReadyStates(ulong clientId, bool disconected)
    {
        for (int i = 0; i < PlayerStates.Length; i++)
        {
            if (PlayerStates[i].playerState == ConnectionState.ready &&
                PlayerStates[i].clientId == clientId)
            {

                if (disconected)
                {
                    PlayerStates[i].playerState = ConnectionState.disconnected;
                    UpdatePlayerStateClientRpc(clientId, i, ConnectionState.disconnected);
                }
                else
                {
                    PlayerStates[i].playerState = ConnectionState.connected;
                    UpdatePlayerStateClientRpc(clientId, i, ConnectionState.connected);
                }
            }
        }
    }

    [ClientRpc]
    public void PlayerDisconnectedClientRpc(int playerId)
    {
        SetNonPlayableChar(playerId);

        // All character data unselected
        RemoveSelectedStates();

        PlayerStates[playerId].playerState = ConnectionState.disconnected;
    }

    void SetNonPlayableChar(int playerId)
    {
        CharactersContainers[playerId].NameTagNormal.SetActive(true);
        CharactersContainers[playerId].NameTagClient.SetActive(false);
        CharactersContainers[playerId].BackgroundClientPadNotReady.SetActive(false);
        CharactersContainers[playerId].BackgroundClientPadReady.SetActive(false);
        CharactersContainers[playerId].WaitingText.SetActive(true);
    }

    void RemoveSelectedStates()
    {
        for (int i = 0; i < CharactersData.Length; i++)
        {
            CharactersData[i].isSelected = false;
        }
    }

    [ClientRpc]
    void UpdatePlayerStateClientRpc(ulong clientId, int stateIndex, ConnectionState state)
    {
        if (IsServer)
            return;

        PlayerStates[stateIndex].playerState = state;
        PlayerStates[stateIndex].clientId = clientId;
    }
}
