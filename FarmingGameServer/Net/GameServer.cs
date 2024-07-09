using MessagePack;
using ModLoader;
using Simulation;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FarmingGameServer.Net
{
    public class GameServer
    {
        public static readonly MessagePackSerializerOptions msgPackOptions = new MessagePackSerializerOptions(MessagePack.Resolvers.ContractlessStandardResolver.Instance);

        public GameWorld world;
        public string URL;

        public Action<object> OnError;

        public List<RemotePlayer> players = new List<RemotePlayer>();

        HashSet<string> passwords;

        HttpListener httpListener;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken;

        Simulation.Timer fullWorldSyncTimer = new Simulation.Timer() { interval = 5 };

        public void Start()
        {
            if (httpListener != null)
            {
                throw new System.Exception("server have been started");
            }
            cancellationToken = cancellationTokenSource.Token;
            httpListener = new HttpListener();
            httpListener.Prefixes.Add(URL);
            httpListener.Start();

            //Task.Factory.StartNew(CreateSockets, TaskCreationOptions.LongRunning);
            Task.Run(Listen);
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            httpListener.Stop();
        }

        public void ClientInteracts()
        {
            lock (players)
            {
                foreach (var player in players)
                {
                    while (player.syncRequests.TryDequeue(out var syncRequest))
                    {
                        world.SyncInteract(syncRequest);
                    }
                }
            }
        }

        public void SyncWorld(float frequency)
        {
            SyncWorld(fullWorldSyncTimer.AddTime(frequency), frequency);
        }
        public void SyncWorld(bool fullWorld, float frequency)
        {
            lock (players)
            {
                byte[] sendData = null;
                if (players.Count > 0)
                {
                    SyncFrame syncFrame = SyncFrame.SyncDelta(world, fullWorld);
                    syncFrame.frequency = frequency;

                    sendData = SerializeAndCompress(syncFrame);
                }
                world.needSyncEntities.Clear();
                world.needSyncEvents.Clear();

                foreach (var player in players)
                {
                    if (player.initialized)
                    {
                        player.Send(sendData);
                    }
                    else
                    {
                        world.PlayerJoin(player.name);
                        SyncFrame syncFrame;

                        syncFrame = SyncFrame.SyncInfo(world);
                        syncFrame.frequency = frequency;
                        player.Send(SerializeAndCompress(syncFrame));

                        syncFrame = SyncFrame.SyncSave(world);
                        syncFrame.frequency = frequency;
                        player.Send(SerializeAndCompress(syncFrame));

                        player.initialized = true;

                    }
                }
                players.RemoveAll(e =>
                {
                    if (e.closed || e.webSocket.State != System.Net.WebSockets.WebSocketState.Open || e.sends.Count > 50)
                    {
                        world.PlayerLeave(e.name);
                        return true;
                    }
                    return false;
                });
            }
        }

        async Task Listen()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await AddPlayer();
                }
                catch (Exception e)
                {
                    OnError?.Invoke(e);
                }
            }
        }

        async Task AddPlayer()
        {
            var httpListenerContext = await httpListener.GetContextAsync();
            var request = httpListenerContext.Request;
            var player = request.Headers["player"];
            if (request.Headers["game"] != "farming-game")
            {
                httpListenerContext.Response.StatusCode = 404;
                httpListenerContext.Response.Close();
                OnError?.Invoke("player cannot join: incorrent game name.");
                return;
            }
            if (string.IsNullOrEmpty(player))
            {
                httpListenerContext.Response.StatusCode = 404;
                httpListenerContext.Response.Close();
                OnError?.Invoke("player cannot join: empty player name.");
                return;
            }
            if (passwords != null && !passwords.Contains(request.Headers["password"]))
            {
                httpListenerContext.Response.StatusCode = 404;
                httpListenerContext.Response.Close();
                OnError?.Invoke("player cannot join: password incorrect.");
                return;
            }
            try
            {
                lock (players)
                {
                    if (players.Any(e => e.name == player))
                    {
                        httpListenerContext.Response.StatusCode = 400;
                        httpListenerContext.Response.Close();
                        OnError?.Invoke("player cannot join: A player with the same name in the game.");
                        return;
                    }
                }
                var webSocketContext = await httpListenerContext.AcceptWebSocketAsync(null);

                var socket = webSocketContext.WebSocket;
                lock (players)
                {
                    var remotePlayer = new RemotePlayer()
                    {
                        name = player,
                        webSocket = socket,
                    };
                    remotePlayer.ReceiveTask(cancellationToken);
                    players.Add(remotePlayer);
                }

            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
            }
        }

        public static byte[] SerializeAndCompress<T>(T data)
        {
            using var outputStream = new MemoryStream();
            using var deflate = new DeflateStream(outputStream, CompressionLevel.Optimal);
            MessagePackSerializer.Serialize(deflate, data, msgPackOptions);
            deflate.Dispose();
            return outputStream.ToArray();
        }

        public static T DecompressAndDeserialize<T>(Stream compressed)
        {
            using var stream = new DeflateStream(compressed, CompressionMode.Decompress);
            return MessagePackSerializer.Deserialize<T>(stream, GameServer.msgPackOptions);
        }

        public void SetConfig(ServerConfig config)
        {
            if (config.password != null && config.password.Count > 0)
            {
                passwords = new HashSet<string>();
                foreach (var p in config.password)
                {
                    passwords.Add(PasswordHash(p));
                }
            }
        }

        static string PasswordHash(string input)
        {
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
        }
    }
}