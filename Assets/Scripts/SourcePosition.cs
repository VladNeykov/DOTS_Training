using System;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// A value to store the original position of each cube, before any potential offset by a sphere.
/// </summary>
public struct SourcePosition : IComponentData
{
    public float3 Value;
}
