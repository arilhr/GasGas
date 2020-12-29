using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PowerupObject : NetworkBehaviour
{

    [SerializeField] private float duration;

    [SerializeField] private int powerType;

    [Server]
    private void OnTriggerEnter(Collider _other)
    {
        if (_other.GetComponentInParent<PlayerCar>() != null)
        {
            PlayerCar player = _other.gameObject.GetComponentInParent<PlayerCar>();
            NetworkConnection playerConn = player.gameObject.GetComponent<NetworkIdentity>().connectionToClient;

            Debug.Log(playerConn);

            player.ActivatePower(playerConn, player.gameObject, powerType, duration);

            DeactiveObject();
        }
    }

    

    [Client]
    private void DeactiveObject()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    public void SetPowerType(int _powerType) => powerType = _powerType;
}
