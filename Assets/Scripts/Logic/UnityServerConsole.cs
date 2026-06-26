using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Sanicball.UI;
using SanicballCore.MatchMessages;
using SanicballCore.Server;
using UnityEngine;

namespace Sanicball.Logic
{
    public class UnityServerConsole : MonoBehaviour
    {
        private Server server;
        private CommandQueue commandQueue;
        private Thread serverThread;
        private bool isRunning = false;
        private ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();

        // Static reference so we can stop it from anywhere
        public static UnityServerConsole Instance { get; private set; }
        public bool IsRunning => isRunning;
        public bool isHost = false;

        public void StartServer(string serverName)
        {
            if (isRunning)
                return;
            Instance = this; // Set the instance

            commandQueue = new CommandQueue();
            string dataPath = Application.persistentDataPath;
            server = new Server(commandQueue, true, serverName, "Local LAN Server");

            server.OnLog += (sender, e) =>
            {
                string msg = $"[Server] {e.Entry.Message}";
                switch (e.Entry.Type)
                {
                    case SanicballCore.Server.LogType.Normal:
                        Debug.Log(msg);
                        break;
                    case SanicballCore.Server.LogType.Warning:
                        Debug.LogWarning(msg);
                        break;
                    case SanicballCore.Server.LogType.Error:
                        Debug.LogError(msg);
                        break;
                }
                logQueue.Enqueue(e.Entry.Message);
                Debug.Log($"Enqueued: {e.Entry.Message}");
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
            try
            {
                server.Start();
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Server Thread Exception] {ex.Message}");
            }
            finally
            {
                isRunning = false;
                isHost = false;
            }
        }

        public void Update()
        {
            if (Chat.Instance == null)
            {
                Debug.LogWarning("Chat.Instance is null!");
            }

            while (logQueue.TryDequeue(out var log))
            {
                Chat.Instance?.ShowMessage(ChatMessageType.System, "Server", log);
            }
        }

        public IEnumerator Shutdown()
        {
            Debug.Log("[Server] Shutting down local server...");
            commandQueue.Add(new Command("stop"));
            yield return new WaitForSeconds(2f);
            if (serverThread != null && serverThread.IsAlive)
            {
                serverThread.Abort();
            }
            isRunning = false;
            Instance = null;
            Debug.Log("[Server] Should be shut down");
        }

        // Static helper to stop the server from any other script
        public IEnumerator Stop()
        {
            Debug.Log("UnityServerConsole.Stop() called");
            if (Instance == null)
                Debug.LogWarning("Got Stop even though there is no server");
            if (Instance != null)
                yield return Instance.StartCoroutine(Instance.Shutdown());
        }
    }
}
