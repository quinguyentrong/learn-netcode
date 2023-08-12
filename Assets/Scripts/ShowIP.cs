using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ShowIP : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI IPText;

    private void Start()
    {
        IPText.text = "IP: " + GameObject.Find("NetworkManager").GetComponent<UnityTransport>().ConnectionData.Address;
    }
}
