using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    public MetaBallField Field = new MetaBallField();
    
    private MeshFilter _filter;
    private Mesh _mesh;
    
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<int> indices = new List<int>();

    private const int Steps = 20;

    /// <summary>
    /// Executed by Unity upon object initialization. <see cref="https://docs.unity3d.com/Manual/ExecutionOrder.html"/>
    /// </summary>
    private void Awake()
    {
        // Getting a component, responsible for storing the mesh
        _filter = GetComponent<MeshFilter>();
        
        // instantiating the mesh
        _mesh = _filter.mesh = new Mesh();
        
        // Just a little optimization, telling unity that the mesh is going to be updated frequently
        _mesh.MarkDynamic();
    }

    /// <summary>
    /// Executed by Unity on every frame <see cref="https://docs.unity3d.com/Manual/ExecutionOrder.html"/>
    /// You can use it to animate something in runtime.
    /// </summary>
    private void Update()
    {
        vertices.Clear();
        indices.Clear();
        normals.Clear();
        
        Field.Update();

        Vector3 bbox = Field.maxPoint - Field.minPoint;
        float cubeSize = Math.Max(bbox.x, Math.Max(bbox.y, bbox.z)) / Steps;

        for (int i = 0; i < Steps; i++)
        {
            for (int j = 0; j < Steps; j++)
            {
                for (int k = 0; k < Steps; k++)
                {
                    Vector3 poss = new Vector3(
                        Field.minPoint.x + i * cubeSize,
                        Field.minPoint.y + j * cubeSize,
                        Field.minPoint.z + k * cubeSize
                    );
                    
                    trianglesAt(poss, cubeSize);
                }
            }   
        }

        // Here unity automatically assumes that vertices are points and hence (x, y, z) will be represented as (x, y, z, 1) in homogenous coordinates
        _mesh.Clear();
        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(indices, 0);
        // _mesh.RecalculateNormals(); // Use _mesh.SetNormals(normals) instead when you calculate them
        _mesh.SetNormals(normals);
        // Upload mesh data to the GPU
        _mesh.UploadMeshData(false);
    }

    void trianglesAt(Vector3 pos, float cubeSize)
    {
        byte mask = 0;
        for (int vi = 0; vi < MarchingCubes.Tables._cubeVertices.Length; vi++)
        {
            Vector3 dpos = pos + MarchingCubes.Tables._cubeVertices[vi] * cubeSize;
            byte flag = (Field.F(dpos) < 0) ? (byte)0 : (byte)1;
            mask |= (byte)(flag << vi);
        }
        
        for (int i = 0; i < MarchingCubes.Tables.CaseToTrianglesCount[mask]; i++)
        {
            int3 triangle = MarchingCubes.Tables.CaseToVertices[mask][i];
            int[] edges = new int[]{triangle.x, triangle.y, triangle.z};

            for (int j = 0; j < 3; j++)
            {
                Vector3 p = edgeToPos(pos, edges[j], cubeSize);
                
                Vector3 norm = new Vector3(
                    Field.F(p + 0.001f * Vector3.left) - Field.F(p + 0.001f * Vector3.right),
                    Field.F(p + 0.001f * Vector3.down) - Field.F(p + 0.001f * Vector3.up),
                    Field.F(p + 0.001f * Vector3.forward) - Field.F(p + 0.001f * Vector3.back)).normalized;
                
                indices.Add(vertices.Count);
                vertices.Add(p);
                normals.Add(norm);
            }
        }
    }

    Vector3 edgeToPos(Vector3 pos, int edgeNum, float cubeSize)
    {
        Vector3 p1 = pos + MarchingCubes.Tables._cubeVertices[MarchingCubes.Tables._cubeEdges[edgeNum][0]] * cubeSize;
        Vector3 p2 = pos + MarchingCubes.Tables._cubeVertices[MarchingCubes.Tables._cubeEdges[edgeNum][1]] * cubeSize;

        float f1 = Field.F(p1);
        float f2 = Field.F(p2);
        float t = f2 / (f2 - f1);

        return  t * p1 + (1.0f - t) * p2;
    }
}