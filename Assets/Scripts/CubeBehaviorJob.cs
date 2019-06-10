using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

/// <summary>
/// System controlling the cube behavior (shrinking/moving away from the spheres)
/// </summary>
public class CubeBehaviorJob : JobComponentSystem
{

    EntityQuery cubeQuery;
    EntityQuery sphereQuery;

    protected override void OnCreate()
    {
        cubeQuery = GetEntityQuery(typeof(Translation), typeof(Scale), typeof(SourcePosition));
        sphereQuery = GetEntityQuery(typeof(Translation), ComponentType.Exclude<SourcePosition>());
    }

    [BurstCompile]
    struct ReactToMouse : IJobChunk
    {
        [DeallocateOnJobCompletion]
        public NativeArray<Translation> SpherePositions;
        public ArchetypeChunkComponentType<Translation> Translation;
        public ArchetypeChunkComponentType<Scale> Scale;
        public ArchetypeChunkComponentType<SourcePosition> SourcePosition;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkScale = chunk.GetNativeArray(Scale);
            var chunkPosition = chunk.GetNativeArray(Translation);
            var chunkSourcePosition = chunk.GetNativeArray(SourcePosition);

            for(var i = 0; i < chunk.Count; i++)
            {
                var distance = 0f;
                var closestSphere = -1;
                var closestDistance = 99999f;

                // Find closest sphere
                for(var j = 0; j < SpherePositions.Length; j++)
                {
                    var newDistance = math.distance(chunkSourcePosition[i].Value, SpherePositions[j].Value);

                    if(newDistance < closestDistance)
                    {
                        closestDistance = newDistance;
                        closestSphere = j;
                    }           
                }

                distance = closestDistance;

                // Adjust the distance
                distance *= 0.2f;
                var directionalVector = math.normalize(SpherePositions[closestSphere].Value - chunkSourcePosition[i].Value);

                // Resize the spheres
                chunkScale[i] = new Scale { Value = math.lerp(0f, 1f, math.saturate(distance)) };

                // Move cubes away from the spheres
                chunkPosition[i] = new Translation
                {
                    Value = chunkSourcePosition[i].Value - math.lerp(directionalVector * 2, new float3(0f,0f,0f), math.saturate(distance))
                };

            }
        }
    }



    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var translationType = GetArchetypeChunkComponentType<Translation>();
        var scaleType = GetArchetypeChunkComponentType<Scale>();
        var sourcePositionType = GetArchetypeChunkComponentType<SourcePosition>();
        var pos = sphereQuery.ToComponentDataArray<Translation>(Allocator.TempJob);


        var job = new ReactToMouse()
        {
            Translation = translationType,
            Scale = scaleType,
            SourcePosition = sourcePositionType,
            SpherePositions = pos
        };

        return job.Schedule(cubeQuery, inputDeps);
    }
}