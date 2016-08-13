
using Anjril.Common.Network;
using Anjril.Common.Network.TcpImpl;
using Anjril.Common.Network.TcpImpl.Properties;
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
        MoveMessages = new ConcurrentQueue<MoveMessage>();
        MapMessages = new ConcurrentQueue<MapMessage>();
        BattleStartMessages = new ConcurrentQueue<BattleStartMessage>();
        BattleActionMessages = new ConcurrentQueue<BattleActionMessage>();
    }

    public ConcurrentQueue<MoveMessage> MoveMessages { get; private set; }

    public ConcurrentQueue<MapMessage> MapMessages { get; private set; }

    public ConcurrentQueue<BattleStartMessage> BattleStartMessages { get; private set; }

    public ConcurrentQueue<BattleActionMessage> BattleActionMessages { get; private set; }

    public ISocketClient Client { get; private set; }
    public int PlayerId { get; private set; }

    private int messageCount = 0;
    private int port = 4245;

    public void InitClient()
    {
        if (PlayerId == 0)
        {
            Settings.Default.ClientPort = port;
            Client = new TcpSocketClient();
            string rep = Client.Connect("127.0.0.1", 1337, MessageReceived, "jpiji");
            //string rep = Client.Connect("192.168.1.23", 1337, MessageReceived, "jpiji");

            Debug.Log("connect " + rep);
            PlayerId = Int32.Parse(rep.Split(':')[1]);
        }
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
        if (message.StartsWith(prefix))
        {
            var entities = message.Remove(0, prefix.Length);
            var entitiesCount = entities.Split(';').Length - 1;
            for (int i = 0; i < entitiesCount; i++)
            {
                var entityStr = message.Split(';')[i];

                MoveMessage move = new MoveMessage(entityStr);

                MoveMessages.Enqueue(move);

            }
        }

        prefix = "map:";
        if (message.StartsWith(prefix))
        {
            Debug.Log("map received");
            var map = message.Remove(0, prefix.Length);

            //Debug.Log(map);
            var origin = map.Split('+')[0];
            var segments = map.Split('+')[1];
            var originPos = new Position(Int32.Parse(origin.Split(':')[0]), Int32.Parse(origin.Split(':')[1]));

            MapMessages.Enqueue(new MapMessage(originPos, segments));
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
