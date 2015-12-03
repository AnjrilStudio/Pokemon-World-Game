using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class Pokemon
{
    public int Id { get; private set; }
    public int Level { get; private set; }

    public Pokemon(int id, int level)
    {
        Id = id;
        Level = level;
    }
}
