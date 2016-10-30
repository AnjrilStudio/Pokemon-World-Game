using UnityEngine;
using System.Collections;
using Anjril.PokemonWorld.Common;

public class fxTest : MonoBehaviour
{

    float timer = 0;
    private Move move = Move.Gust;
    private float repeat = 5f;

    // Use this for initialization
    void Start()
    {
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
                }
            }

            timer -= repeat;
        }
    }
}
