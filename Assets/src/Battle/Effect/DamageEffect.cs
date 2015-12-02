using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class DamageEffect : HitEffect
{
    private Random rnd = new Random();
    public int Value { get; private set; }

    public DamageEffect(int value)
    {
        Value = value;
    }

    public override void apply(BattleEntity self, BattleEntity target, Direction dir)
    {
        var lvl = self.Level;
        var atk = self.Atk; //gérer spe
        var def = target.Def; //gérer spe

        double damage = 2;
        damage *= (2d * lvl + 10d) / 250d;
        damage *= atk / def;
        damage *= Value;
        damage *= 0.85 + rnd.NextDouble() * 15 / 100; //random
        //gérer stab, crit, type, etc

        int damageInt = (int) Math.Floor(damage);

        target.HP -= damageInt;
        if (target.HP < 0)
        {
            target.HP = 0;
        }

       
    }

}
