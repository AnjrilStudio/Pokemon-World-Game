using Anjril.PokemonWorld.Common;
using Anjril.PokemonWorld.Common.Message;
using Anjril.PokemonWorld.Common.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class GroundEffectClient
{
    public int InstanceId { get; private set; }
    public GroundEffectOverTimeId EffectId { get; private set; }
    public Position Position { get; private set; }
    public float Timer;

    public GroundEffectClient(BattleStateGroundEffect groundEffect)
    {
        InstanceId = groundEffect.InstanceId;
        EffectId = (GroundEffectOverTimeId)groundEffect.EffectId;
        Position = groundEffect.Position;
        Timer = -1;
    }
}
