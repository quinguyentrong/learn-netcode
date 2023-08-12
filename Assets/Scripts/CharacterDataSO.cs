using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Pad", menuName = "Character Data")]
public class CharacterDataSO : ScriptableObject
{
    public GameObject PadPrefab;
    public Sprite CharacterPadSprite;

    public ulong clientId;
    public int playerId;
    public bool isSelected;

    private void OnEnable()
    {
        EmptyData();
    }

    public void EmptyData()
    {
        isSelected = false;
        clientId = 0;
        playerId = -1;
    }
}
