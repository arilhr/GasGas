using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class PlayerLobby : NetworkBehaviour
{

    [Header("UI")]
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[2];
    [SerializeField] private Image[] playerReadyImages = new Image[2];
    [SerializeField] private Button startGameButton = null;

    [SyncVar(hook = nameof(HandlePlayerNameChanged))]
    public string DisplayName = "Player Name";

    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool isReady = false;

    private bool isLeader;

    public bool IsLeader
    {
        set
        {
            isLeader = value;
            startGameButton.gameObject.SetActive(value);
        }
    }

    private GameNetworkManager lobby;
    private GameNetworkManager Lobby
    {
        get
        {
            if (lobby != null) { return lobby; }
            return lobby = NetworkManager.singleton as GameNetworkManager;
        }
    }

    public override void OnStartAuthority()
    {
        CmdSetDisplayName(PlayerName.DisplayName);

        lobbyUI.SetActive(true);
    }

    public override void OnStartClient()
    {
        Lobby.PlayerLobbies.Add(this);

        UpdateDisplay();
    }

    public override void OnStopClient()
    {
        Lobby.PlayerLobbies.Remove(this);

        UpdateDisplay();
    }

    public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();
    public void HandlePlayerNameChanged(string oldValue, string newValue) => UpdateDisplay();

    private void UpdateDisplay()
    {
        if (!hasAuthority)
        {
            foreach (var player in Lobby.PlayerLobbies)
            {
                if (player.hasAuthority)
                {
                    player.UpdateDisplay();
                    break;
                }
            }

            return;
        }

        for (int i = 0; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting For Player";
        }

        for (int i = 0; i < Lobby.PlayerLobbies.Count; i++)
        {
            playerNameTexts[i].text = Lobby.PlayerLobbies[i].DisplayName;
            playerReadyImages[i].color = Lobby.PlayerLobbies[i].isReady ?
                Color.green : Color.white;
        }
    }

    public void HandleReadyToStart(bool readyToStart)
    {
        if (!isLeader) { return; }

        startGameButton.interactable = readyToStart;
    }

    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }

    [Command]
    public void CmdReadyUp()
    {
        isReady = !isReady;

        Lobby.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdStartGame()
    {
        if (Lobby.PlayerLobbies[0].connectionToClient != connectionToClient) { return; }

        Lobby.StartGame();
    }
}
