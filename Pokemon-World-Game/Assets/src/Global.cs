
using Anjril.Common.Network;
using Anjril.Common.Network.TcpImpl;
using Anjril.Common.Network.TcpImpl.Properties;
using Anjril.PokemonWorld.Common.Message;
using Anjril.PokemonWorld.Common.Parameter;
using Anjril.PokemonWorld.Common.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Global
{
    public static Global Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = new Global();
            return _Instance;
        }
    }

    private static Global _Instance;

    private Global()
    {
        MoveMessages = new ConcurrentQueue<PositionEntity>();
        MapMessages = new ConcurrentQueue<MapMessage>();
        BattleStartMessages = new ConcurrentQueue<BattleStartMessage>();
        BattleActionMessages = new ConcurrentQueue<BattleActionMessage>();
    }

    public ConcurrentQueue<PositionEntity> MoveMessages { get; private set; }

    public ConcurrentQueue<MapMessage> MapMessages { get; private set; }

    public ConcurrentQueue<BattleStartMessage> BattleStartMessages { get; private set; }

    public ConcurrentQueue<BattleActionMessage> BattleActionMessages { get; private set; }

    public ISocketClient Client { get; private set; }
    public int PlayerId { get; private set; }
    public List<Pokemon> Team { get; private set; }

    public string CurrentScene { get; set; }

    private int messageCount = 0;
    private int port = 4245;
    private string serverIp = "127.0.0.1";
    private string userName = "jpiji";

    public void InitClient()
    {
        if (PlayerId == 0)
        {
            Settings.Default.ClientPort = port;
            Client = new TcpSocketClient();
            string rep = Client.Connect(serverIp, 1337, MessageReceived, userName);
            //string rep = Client.Connect("192.168.1.23", 1337, MessageReceived, "jpiji");
            //string rep = Client.Connect("192.168.1.31", 1337, MessageReceived, "jpiji");

            Debug.Log("connect " + rep);
            PlayerId = Int32.Parse(rep.Split(':')[1]);
            Team = new List<Pokemon>();
        }
    }

    public void Login(string serverip, string username)
    {
        serverIp = serverip;
        userName = username;
    }

    public void SendCommand(BaseParam param)
    {
        Client.Send(param.ToString());
    }

    private void MessageReceived(IRemoteConnection sender, string message)
    {
        messageCount++;
        if (messageCount < 10)
        {
            Debug.Log("received " + message);
        }

        var prefix = "entities:";
        if (message.StartsWith(prefix) && CurrentScene == "scene_map")
        {
            var notif = new PositionMessage();
            notif.DeserializeArguments(message.Remove(0, prefix.Length));

            foreach (var entity in notif.Entities)
            {
                MoveMessages.Enqueue(entity);
            }
        }

        prefix = "team:";
        if (message.StartsWith(prefix))
        {
            var teamStr = message.Remove(0, prefix.Length);
            var pokemonCount = teamStr.Split(',').Count();
            Team = new List<Pokemon>();
            for (int i = 0; i < pokemonCount; i++)
            {
                var pokemonStr = teamStr.Split(',')[i];
                var id = Int32.Parse(pokemonStr.Split('.')[0]);
                var level = Int32.Parse(pokemonStr.Split('.')[1]);
                Team.Add(new Pokemon(id, level));
            }
        }

        prefix = "map:";
        if (message.StartsWith(prefix))
        {
            Debug.Log("map received");

            var map = new MapMessage();
            map.DeserializeArguments(message.Remove(0, prefix.Length));

            //Debug.Log(map);

            MapMessages.Enqueue(map);
        }

        prefix = "battlestart:";
        if (message.StartsWith(prefix))
        {
            Debug.Log("battle start received");
            var battleStart = message.Remove(0, prefix.Length);

            BattleStartMessage battle = new BattleStartMessage(battleStart);
            BattleStartMessages.Enqueue(battle);
        }

        prefix = "battleaction:";
        if (message.StartsWith(prefix))
        {
            Debug.Log("battle action received");
            var battleActionStr = message.Remove(0, prefix.Length);
            Debug.Log(battleActionStr);

            BattleActionMessage battleAction = new BattleActionMessage(battleActionStr);
            BattleActionMessages.Enqueue(battleAction);
        }
    }

}
