using Anjril.PokemonWorld.Common.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class BattleStateEntity
{
    public int Id { get; private set; }
    public int PokemonId { get; private set; }
    public Position CurrentPos { get; private set; }
    public int HP { get; private set; }
    public int MaxHP { get; private set; }
    public int AP { get; private set; }
    public int MaxAP { get; private set; }
    public int MP { get; private set; }
    public int MaxMP { get; private set; }

    public BattleStateEntity(string entityStr)
    {
        Id = Int32.Parse(entityStr.Split(',')[0]);
        PokemonId = Int32.Parse(entityStr.Split(',')[1]);
        CurrentPos = new Position(entityStr.Split(',')[2]);
        HP = Int32.Parse(entityStr.Split(',')[3]);
        MaxHP = Int32.Parse(entityStr.Split(',')[4]);
        AP = Int32.Parse(entityStr.Split(',')[5]);
        MaxAP = Int32.Parse(entityStr.Split(',')[6]);
        MP = Int32.Parse(entityStr.Split(',')[7]);
        MaxMP = Int32.Parse(entityStr.Split(',')[8]);
    }
}
