using UnityEngine;
using System;
using System.Threading;
using SanicballCore.Server;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sanicball.Logic
{
    public class UnityServerConsole : MonoBehaviour
    {
        private Server server;
        private CommandQueue commandQueue;
        private Thread serverThread;
        private bool isRunning = false;

        // Static reference so we can stop it from anywhere
        public static UnityServerConsole Instance { get; private set; }
        public bool IsRunning => isRunning;
        public bool isHost = false;

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
            Instance.isHost = true;
            serverThread = new Thread(RunServerInternal);
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        public void Add(Command cmd)
        {
            commandQueue.Add(cmd);
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
                isHost = false;
            }
        }

        public void Shutdown()
        {
            Debug.Log("[Server] Shutting down local server...");
            serverThread.Abort();
            isRunning = false;
            Instance = null;
            Debug.Log("[Server] Should be shut down");
        }

        // Static helper to stop the server from any other script
        public static void Stop() {
            Debug.Log("UnityServerConsole.Stop() called");
            if (Instance == null) Debug.LogWarning("Got Stop even though there is no server");
            if (Instance != null) Instance.Shutdown();
        }
    }
}