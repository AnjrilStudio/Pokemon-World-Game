using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class BattleStateMessage
{
    public int CurrentTurn { get; private set; }
    public List<BattleStateEntity> Entities { get; private set; }

    public BattleStateMessage(string battleStateStr)
    {
        CurrentTurn = Int32.Parse(battleStateStr.Split('@')[0]);

        var entitiesStr = battleStateStr.Split('@')[1];
        Entities = new List<BattleStateEntity>();

        var entitiesCount = entitiesStr.Split(';').Length - 1;
        for (int i = 0; i < entitiesCount; i++)
        {
            var entityStr = entitiesStr.Split(';')[i];
            BattleStateEntity entity = new BattleStateEntity(entityStr);

            Entities.Add(entity);
        }
    }
}
