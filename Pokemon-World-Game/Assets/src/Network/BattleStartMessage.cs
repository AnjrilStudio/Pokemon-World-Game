using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class BattleStartMessage
{
    public List<int> entitiesId { get; private set; }

    public BattleStartMessage(string entitiesStr)
    {
        entitiesId = new List<int>();

        var entitiesCount = entitiesStr.Split(';').Length - 1;
        for (int i = 0; i < entitiesCount; i++)
        {
            var idstr = entitiesStr.Split(';')[i];

            int id = Int32.Parse(idstr);

            entitiesId.Add(id);
        }
        
    }
}
