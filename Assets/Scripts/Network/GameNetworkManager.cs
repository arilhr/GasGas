using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameNetworkManager : NetworkManager
{

    [SerializeField] private int minPlayers = 2;
    [Scene] [SerializeField] private string menuScene = string.Empty;

    [Header("Player Lobby")]
    [SerializeField] private PlayerLobby playerLobbyPrefab = null;

    [Header("Player In-Game")]
    [SerializeField] private PlayerInGame playerInGamePrefab = null;
    [SerializeField] private GameObject spawnSystemPrefab = null;
    [SerializeField] private GameObject countdownSystemPrefab = null;

    [Header("Game Scene")]
    [Scene] [SerializeField] private string gameScene = string.Empty;
    
    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;
    public static event Action OnServerStopped;

    public List<PlayerLobby> PlayerLobbies { get; } = new List<PlayerLobby>();

    public List<PlayerInGame> PlayerInGames { get; } = new List<PlayerInGame>();

    public override void OnStartServer()
    {
        spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
    }

    /* ON CLIENT */
    public override void OnStartClient()
    {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach (var prefab in spawnablePrefabs)
        {
            ClientScene.RegisterPrefab(prefab);
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        OnClientDisconnected?.Invoke();
    }

    /* ON SERVER */
    public override void OnServerConnect(NetworkConnection conn)
    {
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }

        if (SceneManager.GetActiveScene().path != menuScene)
        {
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            bool isLeader = PlayerLobbies.Count == 0;

            PlayerLobby roomPlayerInstance = Instantiate(playerLobbyPrefab);

            roomPlayerInstance.IsLeader = isLeader;

            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            var player = conn.identity.GetComponent<PlayerLobby>();

            PlayerLobbies.Remove(player);

            NotifyPlayersOfReadyState();
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        OnServerStopped?.Invoke();

        PlayerLobbies.Clear();

        PlayerInGames.Clear();
    }

    public void NotifyPlayersOfReadyState()
    {
        foreach (var player in PlayerLobbies)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    private bool IsReadyToStart()
    {
        if (numPlayers < minPlayers) { return false; }

        foreach (var player in PlayerLobbies)
        {
            if (!player.isReady) { return false; }
        }

        return true;
    }

    public void StartGame()
    {
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            if (!IsReadyToStart()) { return; }

            ServerChangeScene(gameScene);
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        // From menu to game
        if (SceneManager.GetActiveScene().path == menuScene && newSceneName.Contains("Game"))
        {
            for (int i = PlayerLobbies.Count - 1; i >= 0; i--)
            {
                var conn = PlayerLobbies[i].connectionToClient;
                var gameplayerInstance = Instantiate(playerInGamePrefab);
                gameplayerInstance.SetDisplayName(PlayerLobbies[i].DisplayName);

                NetworkServer.Destroy(conn.identity.gameObject);

                NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject);
            }
        }
        else if(SceneManager.GetActiveScene().path.Contains("Game"))
        {
            for (int i = PlayerInGames.Count - 1; i >= 0; i--)
            {
                var conn = PlayerInGames[i].connectionToClient;

                NetworkServer.Destroy(conn.identity.gameObject);
            }
        }

        base.ServerChangeScene(newSceneName);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName.Contains("Game"))
        {
            GameObject playerSpawnSystemInstance = Instantiate(spawnSystemPrefab);
            NetworkServer.Spawn(playerSpawnSystemInstance);

            GameObject countdownSystemInstance = Instantiate(countdownSystemPrefab);
            NetworkServer.Spawn(countdownSystemInstance);
        }
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);

        OnServerReadied?.Invoke(conn);
    }
}
