using UnityEngine;
using System.Collections.Generic;

public class ParticleGenerator : MonoBehaviour {

    //TODO
    /*
    fonction pour la position du centre en fonction du temps !!x
        avec offset random ?
    fonction pour l'angle en fonction du temps !!x
        avec offset random ?
    extraire certaines propriétés de l'héritage : scale, rotation, speed, lifetime...
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
    private List<float> timers;
    private List<float> randoms;
    private float globalTime;
    
    
    public ParticlePattern Pattern { get; set; }
    public string PrefabName { get; set; }
    public Vector3 Target { get; set; }
    public bool Active { get; set; }

    void Awake()
    {
        Pattern = defaultPattern;
        PrefabName = defaultPrefabName;
        Target = new Vector3();
    }

    // Use this for initialization
    void Start() {
        particules = new List<Particle>();
        timers = new List<float>();
        randoms = new List<float>();
        for (int i = 0; i < 1 + Pattern.Repeat; i++)
        {
            timers.Add((1f / Pattern.Rate) - Pattern.Delay - Pattern.RepeatDelay * i);
            randoms.Add(Random.value);
        }
        globalTime = -Pattern.Delay;
        Active = true;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Active)
        {
            globalTime += Time.deltaTime;
            for (int i = 0; i < 1 + Pattern.Repeat; i++)
            {
                updateGenerator(i);
            }
        }
    }

    private void updateGenerator(int index)
    {
        timers[index] += Time.deltaTime;
        var time = globalTime - Pattern.RepeatDelay * index;

        if (time > Pattern.Duration && particules.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        updateParticules(Time.deltaTime);

        var rateTime = 1f / Pattern.Rate;
        int i = Mathf.FloorToInt(timers[index] / rateTime);
        float reminder = timers[index] - (float)i * rateTime;
        //TODO s'assurer que toute les boucles se font même si time dépasse la durée
        while (timers[index] > rateTime && time < Pattern.Duration)
        {
            i--;

            if (Pattern.PatternType == ParticlePatternType.Projectile)
            {
                foreach (float angle in (Pattern as ProjectileParticlePattern).ComputeAngles(time - reminder - rateTime * i, Target, randoms[index]))
                {
                    var particuleObj = GameObject.Instantiate(Resources.Load("fxPrefab/" + PrefabName)) as GameObject;
                    particuleObj.transform.parent = gameObject.transform;

                    particuleObj.transform.localPosition = Pattern.ComputeCenter(time - reminder - rateTime * i, Target, randoms[index]);

                    particuleObj.transform.localRotation = Quaternion.Euler(0, 0, Pattern.ComputeRotation(angle));

                    var particule = new Particle(particuleObj, angle, (Pattern as ProjectileParticlePattern).ComputeSpeed(time - reminder - rateTime * i, randoms[index]), Pattern.ComputeScale(), Pattern.ComputeRotationSpeed(), Pattern.LifeTime);
                    updateParticule(particule, reminder);
                    if (i != 0)
                    {
                        updateParticule(particule, rateTime * i);
                    }

                    particules.Add(particule);
                }
            } else if (Pattern.PatternType == ParticlePatternType.Simple)
            {
                var particuleObj = GameObject.Instantiate(Resources.Load("fxPrefab/" + PrefabName)) as GameObject;
                particuleObj.transform.parent = gameObject.transform;

                particuleObj.transform.localPosition = Pattern.ComputeCenter(time - reminder - rateTime * i, Target, randoms[index]);

                particuleObj.transform.localRotation = Quaternion.Euler(0, 0, Pattern.ComputeRotation(0));

                var particule = new Particle(particuleObj, 0, 0, Pattern.ComputeScale(), Pattern.ComputeRotationSpeed(), Pattern.LifeTime);
                updateParticule(particule, reminder);
                if (i != 0)
                {
                    updateParticule(particule, rateTime * i);
                }

                particules.Add(particule);
            }

            timers[index] -= rateTime;
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

            var scale = Pattern.ComputeScale(part.Time / part.LifeTime);
            part.Obj.transform.localScale = Vector3.one * scale;

            part.Obj.transform.Rotate(0, 0, deltaTime * part.RotationSpeed);

            if (Pattern.Color != Color.clear)
            {
                part.Obj.GetComponent<SpriteRenderer>().color = Pattern.Color;
            }

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
        public float RotationSpeed { get; private set; }
        public float LifeTime { get; private set; }
        public float Time { get; set; }

        public Particle(GameObject obj, float angle, float speed, float scale, float rotationSpeed, float lifeTime)
        {
            Obj = obj;
            Angle = angle;
            MaxSpeed = speed;
            Speed = speed;
            Scale = scale;
            RotationSpeed = rotationSpeed;
            LifeTime = lifeTime;
            Time = 0;
        }
    }
}
