using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;

/// <summary>
/// Behavior of the parent cube of which all other cubes are children. Used to spin the whole system.
/// </summary>
public class ParentBehavior : JobComponentSystem
{
    EntityQuery parent;

    protected override void OnCreate()
    {
        parent = GetEntityQuery(typeof(Child), typeof(Rotation));
    }

  [BurstCompile]
    struct RotateParent : IJobChunk
    {
        public ArchetypeChunkComponentType<Rotation> Rotation;
        public float deltaTime;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var rotation = chunk.GetNativeArray(Rotation);


            for(int i = 0; i < chunk.Count; i++)
            {
                rotation[i] = new Rotation
                {
                    Value = math.mul(math.normalize(rotation[i].Value),
                        quaternion.AxisAngle(math.up(), deltaTime * 0.5f))
                };
            }
        }
       

    };

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var RotationType = GetArchetypeChunkComponentType<Rotation>();

        var job = new RotateParent
        {
            Rotation = RotationType,
            deltaTime = Time.deltaTime
        };

        return job.Schedule(parent, inputDeps);
         
    }

}
