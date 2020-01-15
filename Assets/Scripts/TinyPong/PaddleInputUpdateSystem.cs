using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Tiny.Input;
using Unity.Transforms;

public class PaddleInputUpdateSystem : JobComponentSystem
{
    EntityQuery m_BallQuery;

    protected override void OnCreate()
    {
        m_BallQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<Ball>(), ComponentType.ReadOnly<LocalBounds>(),
                ComponentType.ReadWrite<Translation>()
            },
        });
    }

    protected override JobHandle OnUpdate(JobHandle lastJobHandle)
    {
        var deltaTime = math.min(1.0f / 60.0f, Time.DeltaTime);
        var Input = World.GetExistingSystem<InputSystem>();

        var translationFromEntity = GetComponentDataFromEntity<Translation>(false);
        var ballEntities = m_BallQuery.ToEntityArray(Allocator.TempJob);

        Entities
            .WithoutBurst()
            .ForEach((
                ref Translation translation, 
                ref AutoKeyTimer autoKeyTimer, 
                in AutoKeyConfig autoKeyConfig,
                in KeyCodeDown keyCodeDown,
                in KeyCodeUp keyCodeUp,
                in Speed speed, 
                in LocalBounds localBounds) =>
            {
                if (Input.GetKey(keyCodeUp.Value))
                {
                    Up(ref translation, localBounds, speed, deltaTime);
                    autoKeyTimer.Value = 0.0f;
                }
                else if (Input.GetKey(keyCodeDown.Value))
                {
                    Down(ref translation, localBounds, speed, deltaTime);
                    autoKeyTimer.Value = 0.0f;
                }
                else // Simple AI: Move toward ball.
                {
                    autoKeyTimer.Value += deltaTime;
                    if (autoKeyTimer.Value >= autoKeyConfig.AutoKeyTimeout)
                    {
                        // follow first ball
                        var ballEntity = ballEntities[0];
                        var ballTranslation = translationFromEntity[ballEntity];
                        if (math.abs(ballTranslation.Value.x - translation.Value.x) < autoKeyConfig.AutoReactDistance)
                        {
                            if (ballTranslation.Value.y > (localBounds.Center.y + (autoKeyConfig.AutoBoundsRange * localBounds.Extents.y)))
                                Up(ref translation, localBounds, speed, deltaTime);
                            else if (ballTranslation.Value.y < (localBounds.Center.y - (autoKeyConfig.AutoBoundsRange * localBounds.Extents.y)))
                                Down(ref translation, localBounds, speed, deltaTime);
                        }
                    }
                }
            }).Run();

        ballEntities.Dispose();

        return lastJobHandle;
    }

    private static void Down(ref Translation translation, in LocalBounds localBounds, in Speed speed, float deltaTime)
    {
        var min = -0.5f + localBounds.Extents.y;
        var max = 0.5f - localBounds.Extents.y;
        var cur = translation.Value.y;

        translation.Value.y = math.clamp(cur - (speed.Value * deltaTime), min, max);
    }

    private static void Up(ref Translation translation, in LocalBounds localBounds, in Speed speed, float deltaTime)
    {
        var min = -0.5f + localBounds.Extents.y;
        var max = 0.5f - localBounds.Extents.y;
        var cur = translation.Value.y;

        translation.Value.y = math.clamp(cur + (speed.Value * deltaTime), min, max);
    }
}