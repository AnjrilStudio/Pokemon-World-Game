
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }

    public ConcurrentQueue<MoveMessage> MoveMessages { get; private set; }

    public ConcurrentQueue<MapMessage> MapMessages { get; private set; }

}
