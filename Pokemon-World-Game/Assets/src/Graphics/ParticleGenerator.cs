using UnityEngine;
using System.Collections.Generic;

public class ParticleGenerator : MonoBehaviour {

    //TODO
    /*
    fonction pour la position du centre en fonction du temps !!x
        avec offset random ?
    fonction pour l'angle en fonction du temps !!x
        avec offset random ?
    fonction pour la trajectoire en fonction du temps
    fonction pour le scale en fonction du temps
    fonction pour le speed en fonction du temps
    fonction pour le rate en fonction du temps
    
    utilisation de prefab x
    particules qui émettent des particules
        on create
        pendant
        on death
    */
    //TODO
    
    private ParticlePattern defaultPattern = new RandomConeParticlePattern(15);
    private string defaultPrefabName = "flame";

    private List<Particle> particules;
    private float timer;
    private float time;
    
    public ParticlePattern Pattern { get; set; }
    public string PrefabName { get; set; }
    public Vector3 Target { get; set; }

    void Awake()
    {
        //pattern = new CircleSpiralParticlePattern(1000, 1, 2f);
        Pattern = defaultPattern;
        PrefabName = defaultPrefabName;
        Target = new Vector3();
    }

    // Use this for initialization
    void Start() {
        particules = new List<Particle>();
        timer = -Pattern.Delay;
        time = -Pattern.Delay;
    }
	
	// Update is called once per frame
	void Update () {
        
        if (time > Pattern.Duration && particules.Count == 0)
        {
            Destroy(gameObject);
            return;
        }
        timer += Time.deltaTime;
        time += Time.deltaTime;
        updateParticules(Time.deltaTime);

        var rateTime = 1f / Pattern.Rate;
        int i = Mathf.FloorToInt(timer / rateTime);
        float reminder = timer - (float)i * rateTime;
        while (timer > rateTime && time < Pattern.Duration)
        {
            i--;
            foreach(float angle in Pattern.ComputeAngles(time - reminder - rateTime * i, Target))
            {
                /*var particuleObj = new GameObject();
                var meshFilter = particuleObj.AddComponent<MeshFilter>();
                meshFilter.mesh = mesh;

                var meshRenderer = particuleObj.AddComponent<MeshRenderer>();
                var mats = meshRenderer.materials;
                mats[0] = new Material(material);
                meshRenderer.materials = mats;
                //mats[0].color = new Color(1, Random.value * 0.92f, Random.value * 0.16f);*/

                var particuleObj = GameObject.Instantiate(Resources.Load(PrefabName)) as GameObject;
                particuleObj.transform.parent = gameObject.transform;

                particuleObj.transform.localPosition = Pattern.ComputeCenter(time - reminder - rateTime * i, Target);
            
                particuleObj.transform.localRotation = Quaternion.Euler(0, 0, Random.value * 360);

                var rotation = 400f;
                if (Random.value < 0.5)
                {
                    rotation = -rotation;
                }

                var particule = new Particle(particuleObj, angle, Pattern.Speed, Pattern.Scale, rotation, Pattern.LifeTime);
                updateParticule(particule, reminder);
                if (i != 0)
                {
                    updateParticule(particule, rateTime * i);
                }

                particules.Add(particule);
            }

            timer -= rateTime;
        }
    }

    private void updateParticules(float deltaTime)
    {
        foreach(Particle part in particules)
        {
            updateParticule(part, deltaTime);
        }

        particules.RemoveAll(p => p.Time > p.LifeTime);
    }

    private void updateParticule(Particle part, float deltaTime)
    {
        if (part.Obj != null)
        {
            //part.Speed = Mathf.Lerp(part.MaxSpeed, part.MaxSpeed / 2, part.Time / part.LifeTime);
            var oldPos = part.Obj.transform.localPosition;
            part.Obj.transform.localPosition = new Vector3(oldPos.x + Mathf.Cos(part.Angle * Mathf.PI / 180) * part.Speed * deltaTime, oldPos.y + Mathf.Sin(part.Angle * Mathf.PI / 180) * part.Speed * deltaTime, oldPos.z);
            part.Obj.transform.localScale = new Vector3(part.Scale, part.Scale, part.Scale);
            part.Obj.transform.Rotate(0, 0, deltaTime * part.Rotation);
            /*if (part.Rotation < 0)
            {
                part.Obj.GetComponent<MeshRenderer>().materials[0].color = new Color(1, Mathf.Lerp(0.92f, 0, part.Time / part.LifeTime), Mathf.Lerp(0.16f, 0, part.Time / part.LifeTime));
            }*/
            
            part.Time += deltaTime;
            if (part.Time > part.LifeTime)
            {
                GameObject.Destroy(part.Obj);
            }
        }
    }


    private class Particle
    {
        public GameObject Obj { get; private set; }

        public float Angle { get; private set; }
        public float MaxSpeed { get; private set; }
        public float Speed { get; set; }
        public float Scale { get; private set; }
        public float Rotation { get; private set; }
        public float LifeTime { get; private set; }
        public float Time { get; set; }

        public Particle(GameObject obj, float angle, float speed, float scale, float rotation, float lifeTime)
        {
            Obj = obj;
            Angle = angle;
            MaxSpeed = speed;
            Speed = speed;
            Scale = scale;
            Rotation = rotation;
            LifeTime = lifeTime;
            Time = 0;
        }
    }
}
