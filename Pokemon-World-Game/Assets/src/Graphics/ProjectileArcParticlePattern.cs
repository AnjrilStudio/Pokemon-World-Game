using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class ProjectileArcParticlePattern : ProjectileParticlePattern
{
    private float arcDist;

    public ProjectileArcParticlePattern(float arc):base()
    {
        arcDist = arc;
    }

    //TODO angles

    public override Vector3 ComputeCenter(float time, Vector3 target)
    {
        Vector3 arcCenter = target * 0.5F;
        float angle = Mathf.Atan2(target.y, target.x);
        angle += Mathf.PI / 2;
        arcCenter -= new Vector3(Mathf.Cos(angle) * arcDist, Mathf.Sin(angle) * arcDist, 0);
        Vector3 riseRelCenter = Vector3.zero - arcCenter;
        Vector3 setRelCenter = target - arcCenter;
        float fracComplete = time / Duration;
        Vector3 center = Vector3.Slerp(riseRelCenter, setRelCenter, fracComplete);
        center += arcCenter;

        return center;
    }
}
