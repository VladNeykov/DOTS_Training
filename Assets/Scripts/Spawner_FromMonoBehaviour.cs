using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// ReSharper disable once InconsistentNaming
public class Spawner_FromMonoBehaviour : MonoBehaviour
{
    public GameObject Prefab;
    public int CountX = 10;
    public int CountY = 10;
    public int CountZ = 10;
    public float cubeSize = 1f;

    [Range(0f, 0.5f)]
    public float hollowRange = 0.45f;

    public GameObject sphere;
    public int sphereCount;
    public float sphereSpawnRadius = 10f;

    void Start()
    {
        if (Prefab == null || sphere == null)
        {
            Debug.LogWarning("Prefab(s) not assigned!");
            return;
        }

        var entityManager = World.Active.EntityManager;

        // Create entity prefab for the cubes
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(Prefab, World.Active);

        // Create entity prefab for the spheres
        var spherePrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(sphere, World.Active);

        // Instantiate the parent
        var parent = entityManager.Instantiate(prefab);

        // Add a buffer to store the children of the parent
        entityManager.AddBuffer<Child>(parent);

        // Iterate through all cubes
        for (var x = 0; x < CountX; x++)
        {
            for (var y = 0; y < CountY; y++)
            {
                for(var z = 0; z < CountZ; z++)
                {
                    // Determine the shell (to make a hollow cube with a set thickness)
                    var halfSizeX = math.round(CountX / 2);
                    var rangeX = math.lerp(0, CountX, hollowRange);
                    rangeX = math.clamp(rangeX, 0, halfSizeX - 1);

                    var halfSizeY = math.round(CountY / 2);
                    var rangeY = math.lerp(0, CountY, hollowRange);
                    rangeY = math.clamp(rangeY, 0, halfSizeY - 1);

                    var halfSizeZ = math.round(CountZ / 2);
                    var rangeZ = math.lerp(0, CountZ, hollowRange);
                    rangeZ = math.clamp(rangeZ, 0, halfSizeZ - 1);

                    // Skip cubes which fall within the "hollow" core area
                    if ((x >= halfSizeX - rangeX && x + 1 < halfSizeX + rangeX) && (y >= halfSizeY - rangeY && y + 1 < halfSizeY + rangeY)
                        && (z >= halfSizeZ - rangeZ && z + 1 < halfSizeZ + rangeZ))
                    {
                        continue;
                    }

                    // Efficiently instantiate a bunch of entities from the already converted entity prefab
                    var instance = entityManager.Instantiate(prefab);

                    // Place the instantiated entity in a grid
                    var position = transform.TransformPoint(new float3(x, y, z)* cubeSize - new float3(CountX, CountY, CountZ) * 0.5f * cubeSize);
                    entityManager.SetComponentData(instance, new Translation { Value = position });
                    entityManager.AddComponentData<Scale>(instance, new Scale { Value = cubeSize });

                    // Random rotation in 90 intervals
                    var rotateX = RandomSeeded(x + y + z) > 0.5f ? 0f : math.PI/2f;
                    var rotateY = RandomSeeded(x + y + z + 1) > 0.5f ? 0f : math.PI / 2f;
                    var rotateZ = RandomSeeded(x + y + z + 2) > 0.5f ? 0f : math.PI / 2f;

                    entityManager.SetComponentData<Rotation>(instance, new Rotation { Value = quaternion.EulerXYZ(new float3(rotateX, rotateY, rotateZ))});

                    entityManager.AddComponentData<SourcePosition>(instance, new SourcePosition { Value = position });
                    entityManager.AddComponent(instance, typeof(LocalToParent));

                    // Add parent reference
                    entityManager.AddComponentData<Parent>(instance, new Parent { Value = parent });

                    // Add child reference in parent
                    entityManager.GetBuffer<Child>(parent).Add(new Child { Value = instance });
                }
            }
        }

        // Iterate through all spheres
        for (int i = 0; i < sphereCount; i++)
        {
            var sphereInstance = entityManager.Instantiate(spherePrefab);

            Vector3 randomPosition = new Vector3(
                math.lerp(-1f, 1f, RandomSeeded(i)), 
                math.lerp(-1f,1f, RandomSeeded(i + 1)), 
                math.lerp(-1f, 1f, RandomSeeded(i + 2))) * sphereSpawnRadius;

            entityManager.SetComponentData(sphereInstance, new Translation { Value = transform.TransformPoint(randomPosition) });
            entityManager.AddComponentData<TargetPosition>(sphereInstance, new TargetPosition { Value = randomPosition });
        }
    }

    public static float RandomSeeded(int seed)
    { 
        return math.frac(math.sin(math.dot(new float2(seed,seed), new float2(12.9898f, 78.233f)))*43758.5453f);
    }
}
