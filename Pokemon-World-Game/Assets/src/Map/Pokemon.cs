using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class Pokemon
{
    public int PokedexId { get; private set; }
    public int Level { get; private set; }
    public string Name { get; private set; }

    public Pokemon(int pokedexId, int level)
    {
        PokedexId = pokedexId;
        Level = level;
        switch (pokedexId)
        {
            default:
            case 1:
                Name = "Rattata";
                break;
            case 2:
                Name = "Roucool";
                break;
            case 3:
                Name = "Ptitard";
                break;
        }
    }
}
