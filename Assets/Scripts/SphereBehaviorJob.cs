using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

/// <summary>
/// System controlling the sphere movement
/// </summary>

public class SphereBehaviorJob : JobComponentSystem
{
    EntityQuery allSpheres;

    protected override void OnCreate()
    {
        allSpheres = GetEntityQuery(typeof(Translation), typeof(TargetPosition), ComponentType.Exclude<PreviousParent>());
    }

   [BurstCompile]
    struct FlyAround : IJobChunk
    {
        public ArchetypeChunkComponentType<Translation> Translation;
        public ArchetypeChunkComponentType<TargetPosition> TargetPosition;
        public float deltaTime;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var position = chunk.GetNativeArray(Translation);
            var targetPosition = chunk.GetNativeArray(TargetPosition);

            // Iterate through all relevant chunks
            for(var i = 0; i < chunk.Count; i++)
            {
                // Pick a new random position if the sphere is near the target position
                var distance = math.distance(position[i].Value, targetPosition[i].Value);

                var newPosition = new float3(0f, 0f, 0f);
                var seed = targetPosition[i].Value.x + targetPosition[i].Value.y + targetPosition[i].Value.z;
                
                if (distance <= 1f)
                {
                    newPosition.x = math.lerp(-1f, 1f, Spawner_FromMonoBehaviour.RandomSeeded((int)seed));
                    newPosition.y = math.lerp(-1f, 1f, Spawner_FromMonoBehaviour.RandomSeeded((int)seed + 1));
                    newPosition.z = math.lerp(-1f, 1f, Spawner_FromMonoBehaviour.RandomSeeded((int)seed + 2));

                    targetPosition[i] = new TargetPosition { Value = newPosition * 20f };           
                }
                else
                {
                    newPosition = targetPosition[i].Value;
                }

                float3 direction = math.normalize(newPosition - position[i].Value);
                position[i] = new Translation { Value = position[i].Value + direction * deltaTime * 10f }; 

            }
        }     
    };

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var TranslationType = GetArchetypeChunkComponentType<Translation>();
        var TargetPositionType = GetArchetypeChunkComponentType<TargetPosition>();

        var job = new FlyAround
        {
            Translation = TranslationType,
            TargetPosition = TargetPositionType,
            deltaTime = Time.deltaTime
        };

        return job.Schedule(allSpheres, inputDeps);
         
    }

}
