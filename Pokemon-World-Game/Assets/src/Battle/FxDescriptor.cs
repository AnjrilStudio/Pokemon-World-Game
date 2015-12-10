using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class FxDescriptor
{
    public ParticlePattern Pattern { get; set; }
    public FxType Type { get; set; }
    public string PrefabName { get; set; }

    public FxDescriptor(string prefabName)
    {
        PrefabName = prefabName;
        Type = FxType.None;
    }
}
