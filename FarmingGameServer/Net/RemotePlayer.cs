using Simulation;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using System.Net.WebSockets;

namespace FarmingGameServer.Net
{
    public class RemotePlayer
    {
        public WebSocket webSocket;
        public string name;
        public int entityId;
        public bool initialized = false;
        public bool closed = false;
        public ConcurrentQueue<byte[]> sends = new ConcurrentQueue<byte[]>();
        public ConcurrentQueue<SyncRequest> syncRequests = new ConcurrentQueue<SyncRequest>();
        Task sendTask = Task.CompletedTask;

        CancellationToken cancellationToken;
        byte[] buffer;

        public void ReceiveTask(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            buffer = new byte[1024 * 1024];
            Task.Factory.StartNew(ReceiveTask);
        }

        async Task ReceiveTask()
        {
            int offset = 0;
            try
            {
                while (!closed)
                {
                    var result = await webSocket.ReceiveAsync(new System.ArraySegment<byte>(buffer, offset, buffer.Length - offset), cancellationToken);
                    offset += result.Count;
                    if (result.EndOfMessage)
                    {
                        var request = GameServer.DecompressAndDeserialize<SyncRequest>(new MemoryStream(buffer, 0, offset));
                        request.playerName = name;
                        syncRequests.Enqueue(request);

                        offset = 0;
                    }
                }

            }
            catch
            {
                closed = true;
            }
        }

        public void Send(byte[] data)
        {
            sends.Enqueue(data);
            lock (this)
            {
                sendTask = sendTask.ContinueWith(SendTask);
            }
        }

        void SendTask(Task _)
        {
            try
            {
                lock (this)
                {
                    if (!closed && sends.TryDequeue(out var data))
                    {
                        sendTask = webSocket.SendAsync(data, WebSocketMessageType.Binary, true, cancellationToken);
                    }
                }
            }
            catch
            {
                closed = true;
            }
        }
    }
}