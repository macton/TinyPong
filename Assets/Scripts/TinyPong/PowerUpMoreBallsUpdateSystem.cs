using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

[UpdateAfter(typeof(LocalBoundsUpdateSystem))]
public class PowerUpMoreBallsUpdateSystem : JobComponentSystem
{
    private EntityQuery m_PowerUpQuery;
    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    protected override void OnCreate()
    {
        m_PowerUpQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<PowerUp>(), ComponentType.ReadOnly<LocalBounds>(),
                ComponentType.ReadOnly<PowerUpMoreBalls>()
            },
        });
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }


    protected override JobHandle OnUpdate(JobHandle lastJobHandle)
    {
        var powerUpBounds = m_PowerUpQuery.ToComponentDataArray<LocalBounds>(Allocator.TempJob);
        var powerUpEntities = m_PowerUpQuery.ToEntityArray(Allocator.TempJob);
        var powerUpMoreBallsFromEntity = GetBufferFromEntity<PowerUpMoreBalls>(true);
        var commandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer();

        Entities
            .WithAll<Ball>()
            .WithoutBurst()
            .ForEach((in LocalBounds localBounds, in Parent parent, in Translation translation) =>
            {
                var radius = localBounds.Extents.x;
                var localPosition = translation.Value.xy;

                for (int i = 0; i < powerUpBounds.Length; i++)
                {
                    var powerUp = powerUpBounds[i];
                    if (powerUp.Contains(localPosition, radius))
                    {
                        var powerUpEntity = powerUpEntities[i];
                        var powerUpBalls = powerUpMoreBallsFromEntity[powerUpEntity].Reinterpret<Entity>().AsNativeArray();
                        var powerUpBallCount = powerUpBalls.Length;
                        var distance = 2.0f * math.PI / powerUpBallCount;
                        for (int j = 0; j < powerUpBallCount; j++)
                        {
                            var ballEntity = commandBuffer.Instantiate(powerUpBalls[j]);
                            commandBuffer.AddComponent(ballEntity, parent);
                            commandBuffer.AddComponent(ballEntity, new LocalToParent());
                            commandBuffer.SetComponent(ballEntity, new Translation
                            {
                                Value = new float3
                                {
                                    x = powerUp.Center.x + math.cos(distance * j) * 0.1f,
                                    y = powerUp.Center.y + math.sin(distance * j) * 0.1f,
                                    z = translation.Value.z
                                }
                            });
                            commandBuffer.SetComponent(ballEntity, new Direction
                            {
                                Value = new float2
                                {
                                    x = math.cos(distance * j),
                                    y = math.sin(distance * j)
                                }
                            });
                        }
                        commandBuffer.DestroyEntity(powerUpEntity);
                    }
                }
            }).Run();

        powerUpBounds.Dispose();
        powerUpEntities.Dispose();

        return lastJobHandle;
    }
}