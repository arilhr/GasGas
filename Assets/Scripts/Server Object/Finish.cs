using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Finish : NetworkBehaviour
{
    private GameNetworkManager networkManager;

    private NetworkConnection winnerPlayer = null;

    public override void OnStartServer() => networkManager = FindObjectOfType<GameNetworkManager>();

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponentInParent<PlayerCar>() == null) { return; }

        PlayerCar playerFinish = other.gameObject.GetComponentInParent<PlayerCar>();
        winnerPlayer = playerFinish.gameObject.GetComponent<NetworkIdentity>().connectionToClient;

        foreach (PlayerInGame player in networkManager.PlayerInGames)
        {
            Debug.Log(player.connectionToClient);
            if (player.connectionToClient == winnerPlayer)
            {
                player.TargetGameWin(player.connectionToClient);
            }
            else
            {
                player.TargetGameLose(player.connectionToClient);
            }
        }
    }
}
