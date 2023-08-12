using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerCharacterSelection : NetworkBehaviour
{
    private const int NoCharacterSelectedValue = -1;

    private NetworkVariable<int> CharSelected = new NetworkVariable<int>(NoCharacterSelectedValue);
    private NetworkVariable<int> PlayerId = new NetworkVariable<int>(NoCharacterSelectedValue);

    private void Start()
    {
        if (IsServer)
        {
            PlayerId.Value = CharacterSelectionManager.Instance.GetPlayerId(OwnerClientId);
        }
        else if (!IsOwner && HasAcharacterSelected())
        {
            CharacterSelectionManager.Instance.SetPlayebleChar(
                PlayerId.Value,
                CharSelected.Value,
                IsOwner);
        }

        gameObject.name = $"Player{PlayerId.Value + 1}";
    }

    private bool HasAcharacterSelected()
    {
        return PlayerId.Value != NoCharacterSelectedValue;
    }

    private void OnEnable()
    {
        PlayerId.OnValueChanged += OnPlayerIdSet;
        //CharSelected.OnValueChanged += OnCharacterChanged;
        ButtonPress.OnButtonPress += OnUIButtonPress;
    }

    private void OnDisable()
    {
        PlayerId.OnValueChanged -= OnPlayerIdSet;
        //CharSelected.OnValueChanged -= OnCharacterChanged;
        ButtonPress.OnButtonPress -= OnUIButtonPress;
    }

    private void OnPlayerIdSet(int oldValue, int newValue)
    {
        CharacterSelectionManager.Instance.SetPlayebleChar(newValue, newValue, IsOwner);

        if (IsServer)
            CharSelected.Value = newValue;
    }

    private void OnUIButtonPress(ButtonActions buttonAction)
    {
        if (!IsOwner)
            return;

        switch (buttonAction)
        {
            case ButtonActions.Ready:
                CharacterSelectionManager.Instance.SetPlayerReadyUIButtons(
                    true,
                    CharSelected.Value);

                ReadyServerRpc();
                break;

            case ButtonActions.NotReady:
                CharacterSelectionManager.Instance.SetPlayerReadyUIButtons(
                    false,
                    CharSelected.Value);

                NotReadyServerRpc();
                break;
        }
    }

    [ServerRpc]
    private void ReadyServerRpc()
    {
        CharacterSelectionManager.Instance.PlayerReady(
            OwnerClientId,
            PlayerId.Value,
            CharSelected.Value);
    }

    [ServerRpc]
    private void NotReadyServerRpc()
    {
        CharacterSelectionManager.Instance.PlayerNotReady(OwnerClientId, CharSelected.Value);
    }
}
