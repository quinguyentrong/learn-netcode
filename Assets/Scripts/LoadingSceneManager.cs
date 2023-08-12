using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneName
{
    Bootstrap = 0,
    Menu = 1,
    Room = 2,
    Gameplay = 3,
    EndGame = 4,
}
public class LoadingSceneManager : MonoBehaviour
{
    public static LoadingSceneManager Instance;
    public SceneName SceneActive;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        Application.targetFrameRate = 60;
    }

    public void Init()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
    }

    public void LoadScene(SceneName sceneToLoad, bool isNetworkSessionActive = true)
    {
        StartCoroutine(Loading(sceneToLoad, isNetworkSessionActive));
    }

    private IEnumerator Loading(SceneName sceneToLoad, bool isNetworkSessionActive)
    {
        if (isNetworkSessionActive)
        {
            if (NetworkManager.Singleton.IsServer)
                LoadSceneNetwork(sceneToLoad);
        }
        else
        {
            LoadSceneLocal(sceneToLoad);
        }

        yield return new WaitForSeconds(1f);
    }

    private void LoadSceneLocal(SceneName sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad.ToString());
    }

    public void LoadSceneNetwork(SceneName sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName.ToString(),
            LoadSceneMode.Single);
    }

    public void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        Enum.TryParse(sceneName, out SceneActive);

        if (!ClientConnection.Instance.CanClientConnect(clientId)) return;

        switch (SceneActive)
        {
            case SceneName.Room:
                CharacterSelectionManager.Instance.ServerSceneInit(clientId);
                break;

            case SceneName.Gameplay:
                GameplayManager.Instance.ServerSceneInit(clientId);
                break;

                //case SceneName.Victory:
                //case SceneName.Defeat:
                //    EndGameManager.Instance.ServerSceneInit(clientId);
                //    break;
        }
    }
}
