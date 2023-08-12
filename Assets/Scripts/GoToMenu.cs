using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToMenu : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitUntil(() => LoadingSceneManager.Instance != null);

        LoadingSceneManager.Instance.LoadScene(SceneName.Menu, false);
    }
}
