using UnityEngine;
using System;
using System.Threading;
using SanicballCore.Server;

namespace Sanicball.Logic
{
    public class UnityServerConsole
    {
        private Server server;
        private CommandQueue commandQueue;
        private Thread serverThread;
        private bool isRunning = false;

        // Static reference so we can stop it from anywhere
        public static UnityServerConsole Instance { get; private set; }
        public bool IsRunning => isRunning;

        public void StartServer(string serverName)
        {
            if (isRunning) return;
            Instance = this; // Set the instance

            commandQueue = new CommandQueue();
            string dataPath = Application.persistentDataPath;
            server = new Server(commandQueue, true, serverName, dataPath);

            server.OnLog += (sender, e) => {
                string msg = $"[Server] {e.Entry.Message}";
                switch (e.Entry.Type) {
                    case SanicballCore.Server.LogType.Normal: Debug.Log(msg); break;
                    case SanicballCore.Server.LogType.Warning: Debug.LogWarning(msg); break;
                    case SanicballCore.Server.LogType.Error: Debug.LogError(msg); break;
                }
            };

            isRunning = true;
            serverThread = new Thread(RunServerInternal);
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        private void RunServerInternal()
        {
            try {
                server.Start();
            } catch (ThreadAbortException) {
                Thread.ResetAbort();
            } catch (Exception ex) {
                Debug.LogError($"[Server Thread Exception] {ex.Message}");
            } finally {
                isRunning = false;
            }
        }

        public void Shutdown()
        {
            Debug.Log("[Server] Shutting down local server...");
            if (server != null) {
                server.Dispose();
            }
            if (serverThread != null && serverThread.IsAlive) {
                serverThread.Abort();
            }
            isRunning = false;
            Instance = null;
        }

        // Static helper to stop the server from any other script
        public static void Stop() {
            if (Instance != null) Instance.Shutdown();
        }
    }
}