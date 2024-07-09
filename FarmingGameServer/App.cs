using FarmingGameServer.Net;
using FarmingMod;
using ModLoader;
using Simulation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Stopwatch = System.Diagnostics.Stopwatch;
using Timer = Simulation.Timer;

namespace FarmingGameServer
{
    public class App
    {
        public string URL;

        public GameServer gameServer = new GameServer();
        Timer updateTimer = new Timer() { interval = 1 / 30.0f };
        Timer syncTimer = new Timer() { interval = 1 / 10.0f };
        long timeStamp;

        GameWorld world;

        public double deltaTime { get => world.fixedDeltaTime; }

        public void Init()
        {
            NewWorld();
            world.OnError += System.Console.WriteLine;
            gameServer.OnError += System.Console.WriteLine;
            var serverConfig = new ServerConfig();

            timeStamp = Stopwatch.GetTimestamp();
            gameServer.world = world;
            gameServer.URL = URL;
            gameServer.SetConfig(new ServerConfig());
            gameServer.Start();
            syncTimer.interval = serverConfig.syncInterval;
        }

        public void Tick()
        {
            long timeStamp1 = Stopwatch.GetTimestamp();
            double deltaTime = (timeStamp1 - timeStamp) / (double)Stopwatch.Frequency;
            timeStamp = timeStamp1;

            updateTimer.interval = world.fixedDeltaTime;
            int simulationCount = Math.Min(updateTimer.NumEvent(deltaTime), 3);
            for (int i = 0; i < simulationCount; i++)
            {
                gameServer.ClientInteracts();
                world.HostUpdate();
            }
            if (syncTimer.AddTime(deltaTime))
            {
                gameServer.SyncWorld((float)syncTimer.interval);
                gameServer.world.worldEvents.Clear();
            }
        }

        void NewWorld()
        {
            var files = Directory.EnumerateFiles("Rules", "*.json");
            //var rule = GameRule.MergeRules(files.Select(x => JsonConvert.DeserializeObject<GameRule>(File.ReadAllText(x))));
            var ruleFile = File.ReadAllText(files.First());

            List<IGameMod> mods = new List<IGameMod>();
            mods.Add(new FarmingModApply());

            world = GameWorldUtil.ServerCreateWorld(mods, ruleFile);
        }
    }
}
