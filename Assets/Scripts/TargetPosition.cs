using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// A value to store the target position for each sphere.
/// </summary>
public struct TargetPosition : IComponentData
{
    public float3 Value;
}
