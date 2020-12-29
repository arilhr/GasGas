using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Mirror.Discovery
{
    [RequireComponent(typeof(NetworkDiscovery))]
    public class LANServer : MonoBehaviour
    {
        readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();

        public NetworkDiscovery networkDiscovery;

        [SerializeField] private GameNetworkManager networkManager;

        [SerializeField] private ConnectingInfo connectInfo;

        [SerializeField] private GameObject mainMenuCanvas;

        [SerializeField] [Range(1, 5)] private float waitTime = 1f;

        private bool isHosting = false;

        private float updateServerTime = 2f;
        private float currentUpdateServerTime;


#if UNITY_EDITOR
        void OnValidate()
        {
            if (networkDiscovery == null)
            {
                networkDiscovery = GetComponent<NetworkDiscovery>();
                UnityEditor.Events.UnityEventTools.AddPersistentListener(networkDiscovery.OnServerFound, OnDiscoveredServer);
                UnityEditor.Undo.RecordObjects(new Object[] { this, networkDiscovery }, "Set NetworkDiscovery");
            }

            networkManager = FindObjectOfType<GameNetworkManager>();

            currentUpdateServerTime = updateServerTime;
        }


#endif
        private void Update()
        {
            TimeUpdateServer();
        }

        private void TimeUpdateServer()
        {
            if (isHosting)
            {
                currentUpdateServerTime -= Time.deltaTime;
                if (currentUpdateServerTime <= 0)
                    UpdateServer();
            }
        }

        private void UpdateServer()
        {
            discoveredServers.Clear();
            networkDiscovery.AdvertiseServer();
        }


        public void StartHosting()
        {
            // Debug log
            Debug.Log($"Hosting a server.");

            // Start Hosting
            discoveredServers.Clear();
            networkManager.StartHost();
            networkDiscovery.AdvertiseServer();

            isHosting = true;

            mainMenuCanvas.SetActive(false);
        }

        public void FindRoomServer()
        {
            // Debug
            Debug.Log($"Finding Server...");
            connectInfo.SetText("Finding a server", false);

            discoveredServers.Clear();
            networkDiscovery.StartDiscovery();
            StartCoroutine(RoomServerUpdate(waitTime));
        }

        IEnumerator RoomServerUpdate(float _waitTime)
        {
            yield return new WaitForSeconds(_waitTime);

            if (discoveredServers.Count == 0)
            {
                connectInfo.SetText("Server not found", true);
            }
            else
            {
                Debug.Log(discoveredServers.Count);
                foreach (ServerResponse info in discoveredServers.Values)
                {
                    Debug.Log($"Halo2");
                    string serverAddress = info.EndPoint.Address.ToString();

                    Debug.Log($"Find server: {serverAddress}");

                    mainMenuCanvas.SetActive(false);

                    Connect(info);

                }
            }
        }

        public void Connect(ServerResponse info)
        {
            networkManager.StartClient(info.uri);
        }

        private void OnDiscoveredServer(ServerResponse info)
        {
            discoveredServers[info.serverId] = info;
        }
    }

}
