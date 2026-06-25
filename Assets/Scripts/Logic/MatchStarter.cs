using Lidgren.Network;
using SanicballCore;
using UnityEngine;
using SanicballCore.Server;
using System.Collections;

namespace Sanicball.Logic
{
    public class MatchStarter : MonoBehaviour
    {
        public const string APP_ID = "Sanicball";

        [SerializeField] private MatchManager matchManagerPrefab = null;
        [SerializeField] private UI.Popup connectingPopupPrefab = null;
        [SerializeField] private UI.PopupHandler popupHandler = null;

        private UI.PopupConnecting activeConnectingPopup;
        private NetClient joiningClient;
        
        // Made static so it survives scene changes even if this script's object is destroyed
        private static UnityServerConsole localServerConsole;

        private void Update()
        {
            if (joiningClient != null)
            {
                NetIncomingMessage msg;
                while (joiningClient != null && (msg = joiningClient.ReadMessage()) != null)
                {
                    HandleNetworkMessage(msg);
                }

                if (joiningClient != null && Input.GetKeyDown(KeyCode.Escape))
                {
                    CancelJoining();
                }
            }
        }

        private void HandleNetworkMessage(NetIncomingMessage msg)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.VerboseDebugMessage:
                    Debug.Log(msg.ReadString());
                    break;
                case NetIncomingMessageType.WarningMessage:
                    Debug.LogWarning(msg.ReadString());
                    break;
                case NetIncomingMessageType.ErrorMessage:
                    Debug.LogError(msg.ReadString());
                    break;
                case NetIncomingMessageType.StatusChanged:
                    NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                    string statusMsg = msg.ReadString();
                    HandleStatusChange(status, statusMsg);
                    break;
                case NetIncomingMessageType.Data:
                    if (msg.ReadByte() == MessageType.InitMessage)
                    {
                        try
                        {
                            MatchState matchInfo = MatchState.ReadFromMessage(msg);
                            BeginOnlineGame(matchInfo);
                        }
                        catch (System.Exception ex)
                        {
                            activeConnectingPopup?.ShowMessage("Failed to read match state!");
                            Debug.LogError("Match state read error: " + ex.Message);
                        }
                    }
                    break;
            }
        }

        private void HandleStatusChange(NetConnectionStatus status, string message)
        {
            if (status == NetConnectionStatus.Connected)
                activeConnectingPopup?.ShowMessage("Receiving match state...");
            else if (status == NetConnectionStatus.Disconnected)
                activeConnectingPopup?.ShowMessage(message);
        }

        public void StartLocalNetworkGame(string name)
        {
            StartCoroutine(StartLocalNetworkGameRoutine(name));
        }

        public void BeginLocalGame()
        {
            MatchManager manager = Instantiate(matchManagerPrefab);
            manager.InitLocalMatch();
        }

        private IEnumerator StartLocalNetworkGameRoutine(string name)
        {
            // Shutdown existing server if one is somehow still running
            if (localServerConsole != null) localServerConsole.Shutdown();

            localServerConsole = new UnityServerConsole();
            localServerConsole.StartServer(name);

            yield return new WaitForSeconds(2f);

            JoinOnlineGame("127.0.0.1", 25000);
        }

        public void JoinOnlineGame(string ip = "127.0.0.1", int port = 25000)
        {
            JoinOnlineGame(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), port));
        }

        public void JoinOnlineGame(System.Net.IPEndPoint endpoint)
        {
            NetPeerConfiguration conf = new NetPeerConfiguration(APP_ID);
            joiningClient = new NetClient(conf);
            joiningClient.Start();

            NetOutgoingMessage approval = joiningClient.CreateMessage();
            ClientInfo info = new ClientInfo(GameVersion.AS_FLOAT, GameVersion.IS_TESTING);
            approval.Write(Newtonsoft.Json.JsonConvert.SerializeObject(info));

            joiningClient.Connect(endpoint, approval);

            popupHandler.OpenPopup(connectingPopupPrefab);
            activeConnectingPopup = FindObjectOfType<UI.PopupConnecting>();
        }

        private void BeginOnlineGame(MatchState matchState)
        {
            Debug.Log("Transitioning to MatchManager...");
            MatchManager manager = Instantiate(matchManagerPrefab);
            manager.InitOnlineMatch(joiningClient, matchState);
            joiningClient = null; 
        }

        private void CancelJoining()
        {
            popupHandler.CloseActivePopup();
            joiningClient?.Disconnect("Cancelled");
            joiningClient = null;
            
            // If we cancel joining, we should probably stop the local server too
            if (localServerConsole != null) {
                localServerConsole.Shutdown();
                localServerConsole = null;
            }
        }

        // OnApplicationQuit is much safer for local servers than OnDestroy
        private void OnApplicationQuit()
        {
            if (localServerConsole != null)
            {
                Debug.Log("Closing local server due to application quit.");
                localServerConsole.Shutdown();
                localServerConsole = null;
            }
        }
    }
}