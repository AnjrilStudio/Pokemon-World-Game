using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class EffectStatus : HitEffect
{
    private Random rnd = new Random();
    public int Chance { get; private set; }
    public Status Status;

    public EffectStatus(int chance, Status status)
    {
        Chance = chance;
        Status = status;
    }

    public override void apply(BattleEntity self, BattleEntity target, Direction dir)
    {
        
        //todo
       
    }

}
