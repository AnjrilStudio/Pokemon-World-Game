using UnityEngine;
using System.Collections;
using Anjril.PokemonWorld.Common;

public class fxTest : MonoBehaviour
{

    float timer;
    private Move move = Move.Tackle;
    private float repeat = 5f;

    // Use this for initialization
    void Start()
    {
        timer = -1f;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 0)
        {
            foreach (FxDescriptor fx in MoveFx.Get(move))
            {
                if (fx.Pattern != null && fx.PrefabName != null)
                {
                    GameObject fxObj = new GameObject();
                    fxObj.transform.position = Vector3.zero;
                    var partgen = fxObj.AddComponent<ParticleGenerator>();
                    partgen.Pattern = fx.Pattern;
                    partgen.PrefabName = fx.PrefabName;
                    if (fx.Type == FxType.FromTarget)
                    {
                        fxObj.transform.position = Vector3.right;
                    }
                    else if (fx.Type == FxType.ToTarget)
                    {
                        partgen.Target = Vector3.right;
                    }
                }
            }

            timer -= repeat;
        }
    }
}
