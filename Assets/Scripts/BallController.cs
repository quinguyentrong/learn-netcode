using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BallController : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D Rb2D;
    private bool IsSetActive = false;
    private Vector2 Direction;

    private void Start()
    {
        Rb2D.useFullKinematicContacts = true;
    }

    private void Update()
    {
        if (IsServer == false) return;

        if (gameObject.transform.position.y >= 6 || gameObject.transform.position.y <= -6)
        {
            GameplayManager.Instance.SpawnBall();
            NetworkObjectDespawner.DespawnNetworkObject(NetworkObject);
        }

        Rb2D.MovePosition((Vector2)Rb2D.transform.position + Direction * Time.deltaTime);

        if (IsSetActive) return;

        IsSetActive = true;

        Direction = new Vector2(0, 3);
    }

    private void AddVelocityWhenCollision(Vector2 VelocityDirection)
    {
        Direction = VelocityDirection * 3;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            AddVelocityWhenCollision(((Vector2)transform.position - (Vector2)collision.transform.position).normalized);
        }
    }
}
