using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;

public class CountdownSystem : NetworkBehaviour
{
    [SerializeField] private Animator animator = null;

    public static event Action OnStartGame;

    private GameNetworkManager room;

    private GameNetworkManager Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as GameNetworkManager;
        }
    }

    public void CountdownEnded()
    {
        gameObject.SetActive(false);
    }

    #region Server

    public override void OnStartServer()
    {
        GameNetworkManager.OnServerReadied += CheckToStartGame;
        GameNetworkManager.OnServerStopped += CleanUpServer;
        OnStartGame += RpcStartGame;
    }

    [ServerCallback]
    private void OnDestroy() => CleanUpServer();

    [Server]
    private void CleanUpServer()
    {
        GameNetworkManager.OnServerReadied -= CheckToStartGame;
        GameNetworkManager.OnServerStopped -= CleanUpServer;
    }

    [ServerCallback]
    public void StartGame()
    {
        OnStartGame.Invoke();
    }

    [Server]
    private void CheckToStartGame(NetworkConnection conn)
    {
        if (Room.PlayerInGames.Count(x => x.connectionToClient.isReady) != Room.PlayerInGames.Count) { return; }

        animator.enabled = true;

        RpcStartCountdown();
    }
    #endregion

    #region Client

    [ClientRpc]
    private void RpcStartCountdown()
    {
        animator.enabled = true;
    }

    [ClientRpc]
    private void RpcStartGame()
    {
        ControllerUI.EnabledController();
    }

    #endregion
}
