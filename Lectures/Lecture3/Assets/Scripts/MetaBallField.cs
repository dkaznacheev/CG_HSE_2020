using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class MetaBallField
{
    public Transform[] Balls = new Transform[0];
    public float BallRadius = 1;

    private Vector3[] _ballPositions;

    public Vector3 minPoint;
    public Vector3 maxPoint;
    
    /// <summary>
    /// Call Field.Update to react to ball position and parameters in run-time.
    /// </summary>
    public void Update()
    {
        _ballPositions = Balls.Select(x => x.position).ToArray();
        
        // Uncomment for animation!
        // _ballPositions[0] += Mathf.Sin(Time.time) * Vector3.forward;
        // _ballPositions[1] += 2f * Mathf.Sin(Time.time) * Vector3.down;
        // _ballPositions[2] += Mathf.Sin(Time.time) * Vector3.back;
        
        Vector3 radius = Vector3.one * (BallRadius * 1.25f);
        minPoint = new Vector3(
            _ballPositions.Select(p => p.x).Min(),
            _ballPositions.Select(p => p.y).Min(),
            _ballPositions.Select(p => p.z).Min()) - radius;
        maxPoint = new Vector3(
            _ballPositions.Select(p => p.x).Max(),
            _ballPositions.Select(p => p.y).Max(),
            _ballPositions.Select(p => p.z).Max()) + radius;
    }
    
    /// <summary>
    /// Calculate scalar field value at point
    /// </summary>
    public float F(Vector3 position)
    {
        float f = 0;
        // Naive implementation, just runs for all balls regardless the distance.
        // A better option would be to construct a sparse grid specifically around 
        foreach (var center in _ballPositions)
        {
            f += 1 / Vector3.SqrMagnitude(center - position);
        }

        f *= BallRadius * BallRadius;

        return f - 1;
    }
}