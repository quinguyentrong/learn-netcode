using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PadController : NetworkBehaviour
{
    [SerializeField] CharacterDataSO CharacterData;

    private Vector3 Offset = new Vector3(0f, 0f, 10f);
    private float SmoothTime = 0.15f;
    private Vector3 Velocity = Vector3.zero;

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    if (IsServer)
        //    {
        //        StartCoroutine(HostShutdown());
        //    }
        //    else
        //    {
        //        Shutdown();
        //    }
        //}
        if (!IsOwner) return;

        MoveServerRpc();
    }

    [ServerRpc]
    private void MoveServerRpc()
    {
        MoveClientRpc();
    }

    [ClientRpc]
    private void MoveClientRpc()
    {
        Vector3 targetPosition = new Vector3((Camera.main.ScreenToWorldPoint(Input.mousePosition) + Offset).x, transform.position.y);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref Velocity, SmoothTime);
    }

    //IEnumerator HostShutdown()
    //{
    //    ShutdownClientRpc();

    //    yield return new WaitForSeconds(0.5f);

    //    Shutdown();
    //}

    //void Shutdown()
    //{
    //    NetworkManager.Singleton.Shutdown();
    //    LoadingSceneManager.Instance.LoadScene(SceneName.Menu, false);
    //}

    //[ClientRpc]
    //void ShutdownClientRpc()
    //{
    //    if (IsServer)
    //        return;

    //    Shutdown();
    //}
}
