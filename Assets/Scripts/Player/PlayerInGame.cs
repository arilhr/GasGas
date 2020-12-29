using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class PlayerInGame : NetworkBehaviour
{
    [Header("Menu Scene")]
    [Scene] [SerializeField] private string menuScene = string.Empty;

    [SyncVar]
    private string displayName = "Loading...";

    [SerializeField] private GameObject gameEndPanel = null;

    [SerializeField] private GameObject gameWinPanel = null;

    [SerializeField] private GameObject gameLosePanel = null;

    [SerializeField] private GameObject gameStartPanel = null;

    public bool isReady = false;

    private GameNetworkManager lobby;
    private GameNetworkManager Lobby
    {
        get
        {
            if (lobby != null) { return lobby; }
            return lobby = NetworkManager.singleton as GameNetworkManager;
        }
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        Lobby.PlayerInGames.Add(this);
    }

    public override void OnStopClient()
    {
        Lobby.PlayerInGames.Remove(this);
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    [TargetRpc]
    public void TargetGameWin(NetworkConnection conn)
    {
        Debug.Log($"You win the game");

        gameEndPanel.SetActive(true);
        gameWinPanel.SetActive(true);

        ControllerUI.controller.SetActive(false);
    }

    [TargetRpc]
    public void TargetGameLose(NetworkConnection conn)
    {
        Debug.Log($"You lose the game");
        
        gameEndPanel.SetActive(true);
        gameLosePanel.SetActive(true);

        ControllerUI.controller.SetActive(false);
    }

    public void BackToMenu()
    {
        if (isServer)
            Lobby.StopServer();
        else
            Lobby.StopClient();
    }

    [Command]
    public void CmdReady()
    {
        isReady = !isReady;

        gameStartPanel.SetActive(false);
    }
}
