using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class GroundEntity
{
    public int Id { get; private set; }
    public GameObject Object { get; private set; }

    public GroundEntity(int id, GameObject obj)
    {
        Id = id;
        Object = obj;
    }
}
