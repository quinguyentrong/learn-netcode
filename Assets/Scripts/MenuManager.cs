using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button StartHostButton;
    [SerializeField] private Button StartClientButton;
    [SerializeField] private TMP_InputField IPTextInput;
    [SerializeField] private UnityTransport Transport;
    private string IPAddress;
    [SerializeField] private TextMeshProUGUI ConstantText;

    [SerializeField] private CharacterDataSO[] CharacterDatas;
    [SerializeField] private SceneName NextScene = SceneName.Room;

    private IEnumerator Start()
    {
        StartHostButton.onClick.AddListener(StartHost);
        StartClientButton.onClick.AddListener(StartClient);
        IPAddress = "0.0.0.0";
        SetIpAddress();
        ClearAllCharacterData();

        yield return new WaitUntil(() => NetworkManager.Singleton.SceneManager != null);
        LoadingSceneManager.Instance.Init();
    }

    public void StartHost()
    {
        GetLocalIPAddress();
        SetIpAddress();
        NetworkManager.Singleton.StartHost();
        LoadingSceneManager.Instance.LoadScene(NextScene);
    }
    public void StartClient()
    {
        IPAddress = ConstantText.text + IPTextInput.text;
        SetIpAddress();
        NetworkManager.Singleton.StartClient();
    }
    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                if (ip.ToString().Substring(0, 8) != "192.168.")
                {
                    continue;
                }
                IPAddress = ip.ToString();
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
    public void SetIpAddress()
    {
        Transport = GameObject.Find("NetworkManager").GetComponent<UnityTransport>();
        Transport.ConnectionData.Address = IPAddress;
    }

    private void ClearAllCharacterData()
    {
        foreach (CharacterDataSO data in CharacterDatas)
        {
            data.EmptyData();
        }
    }
}
