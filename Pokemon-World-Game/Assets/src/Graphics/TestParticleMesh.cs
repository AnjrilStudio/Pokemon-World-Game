using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class TestParticleMesh : ParticleMesh
{
    public override Mesh ComputeMesh()
    {
        Vector3 p0 = new Vector3(0, 0, 0);
        Vector3 p1 = new Vector3(0, 0.66f, 0);
        Vector3 p2 = new Vector3(0.33f, 1, 0);
        Vector3 p3 = new Vector3(1, 1, 0);
        Vector3 p4 = new Vector3(0.4f, 0.6f, 0);

        Vector3 anchor = new Vector3(0.25f, 0.75f, 0);


        var vertices = new Vector3[] { p0, p1, p2, p3, p4 };
        for(int i = 0; i < vertices.Length; i++)
        {
            vertices[i].Set(vertices[i].x - anchor.x, vertices[i].y - anchor.y, vertices[i].z - anchor.z);
        }

        var mesh = new Mesh();

        mesh.Clear();
        mesh.vertices = vertices;

        mesh.triangles = new int[]{
            0,1,4,
            1,2,4,
            2,3,4
        };

        mesh.colors = new Color[] { new Color(0, 0, 0, 0), new Color(255, 255, 255, 0), new Color(0, 0, 0, 0), new Color(0, 0, 0, 0), new Color(0, 0, 0, 0) };


        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        ;

        return mesh;
    }
}
