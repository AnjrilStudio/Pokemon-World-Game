using Anjril.PokemonWorld.Common;
using Anjril.PokemonWorld.Common.Message;
using Anjril.PokemonWorld.Common.State;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BattleContext
{
    public List<BattleEntityClient> Turns { get; set;}
    public Dictionary<int, BattleEntityClient> Entities { get; set; }
    public Dictionary<int, GroundEffectClient> GroundEffects { get; set; }

    public List<BattleStateEntity> PokemonToGoList { get; set; }
    public List<int> PokemonToComeBackList { get; set; }
    public List<BattleEntityClient> PokemonToKOList { get; set; }
    public bool EndBattle { get; set; }

    public float Tilesize { get; set; }
    public BattleArenaClient Arena { get; set; }
    public int CurrentTurn { get; set; }
    public int CurrentActionNumber { get; set; }
    public BattleActionMessage CurrentBattleAction { get; set; }

    public GameObject ScaleNode { get; set; }
    public GameObject BattleObject { get; set; }

    public List<string> BattleLog { get; set; }

    public Queue<DialogMessage> DialogBoxQueue { get; set; }
    public int TextIndex { get; set; }
    public float TextTimer { get; set; }

    public BattleContext(GameObject battleObject)
    {
        BattleObject = battleObject;

        CurrentBattleAction = null;
        EndBattle = false;

        Tilesize = 0.32f;
        CurrentTurn = 0;
        CurrentActionNumber = -1;

        TextIndex = 0;
        TextTimer = 0;


        Arena = new BattleArenaClient(20, 0.32f);
        Turns = new List<BattleEntityClient>();
        PokemonToGoList = new List<BattleStateEntity>();
        PokemonToComeBackList = new List<int>();
        PokemonToKOList = new List<BattleEntityClient>();
        Entities = new Dictionary<int, BattleEntityClient>();
        GroundEffects = new Dictionary<int, GroundEffectClient>();
        

        ScaleNode = GameObject.FindGameObjectWithTag("BattleScale");

        BattleLog = new List<string>();
        BattleLog.Add("Debut du combat");

        DialogBoxQueue = new Queue<DialogMessage>();

        //displayGUI();
        //initCamera();
    }
}
