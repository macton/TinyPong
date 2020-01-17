using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// Spawn one prefab entity at a time at spawner location on grid at the given frequency.
/// </summary>
public class SoloSpawnerSystem : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    protected override void OnCreate()
    {
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Clamp delta time so you can't overshoot.
        var deltaTime = math.min(Time.DeltaTime, 0.05f);
        var commandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer();
        
        Entities.ForEach((ref SoloSpawner soloSpawner, in LocalBounds localBounds, in LocalToParent localToParent, in Parent parent, in Translation translation) =>
        {
            var secondsUntilGenerate = soloSpawner.SecondsUntilGenerate;
            secondsUntilGenerate -= deltaTime;
            if (secondsUntilGenerate <= 0.0f)
            {
                if (soloSpawner.GeneratedCount < soloSpawner.GenerateMaxCount)
                {
                    var entity = commandBuffer.Instantiate(soloSpawner.Prefab);
                    var rx = (2.0f * soloSpawner.Random.NextFloat()) - 1.0f;
                    var ry = (2.0f * soloSpawner.Random.NextFloat()) - 1.0f;
                    var dx = rx * localBounds.Extents.x;
                    var dy = ry * localBounds.Extents.y;
                    commandBuffer.SetComponent(entity, new Translation
                    {
                        Value = new float3
                        {
                            x = translation.Value.x + dx,
                            y = translation.Value.y + dy,
                            z = translation.Value.z
                        }
                    });
                    commandBuffer.AddComponent(entity, parent);    
                    commandBuffer.AddComponent(entity, localToParent);
                    soloSpawner.GeneratedCount++;
                }
                secondsUntilGenerate = soloSpawner.CoolDownSeconds;
            }

            soloSpawner.SecondsUntilGenerate = secondsUntilGenerate;
        }).Run();

        return inputDeps;
    }
}

