using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Tiny.Input;
using Unity.Transforms;

public class PaddleInputUpdateSystem : JobComponentSystem
{
    EntityQuery m_BallQuery;

    // Don't need to be *exactly* between x extents
    private const float kTouchWidthSlop = 0.025f;

    // Don't adjust if pretty close to center y (amount of y extents to ignore)
    private const float kTouchCenterSlopRatio = 0.25f;

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
        var playAreaEntity = GetSingletonEntity<PlayArea>();
        var playAreaTouchPointBuffer = EntityManager.GetBuffer<TouchPoint>(playAreaEntity);

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
                var nextAutoKeyTime = 0.0f;

                var wasUserInput = TranslationFromUserInput(ref translation, playAreaTouchPointBuffer, localBounds, speed, deltaTime, Input, keyCodeUp, keyCodeDown);
                if (!wasUserInput)
                {
                    nextAutoKeyTime = autoKeyTimer.Value + deltaTime;
                    if (autoKeyTimer.Value >= autoKeyConfig.AutoKeyTimeout)
                    {
                        TranslationFromAutoInput(ref translation, ballEntities, translationFromEntity, autoKeyConfig, localBounds, speed, deltaTime);
                    }
                }

                autoKeyTimer.Value = nextAutoKeyTime;
            }).Run();

        ballEntities.Dispose();

        return lastJobHandle;
    }

    private static void TranslationFromAutoInput(ref Translation translation, in NativeArray<Entity> ballEntities,
        in ComponentDataFromEntity<Translation> translationFromEntity, in AutoKeyConfig autoKeyConfig,
        in LocalBounds localBounds, in Speed speed, float deltaTime)
    {
        if (ballEntities.Length > 0)
        {
            // Follow nearest ball
            var bestIndex = 0;
            float bestDist = 100.0f;
            for (int i = 0; i < ballEntities.Length; i++)
            {
                var be = ballEntities[i];
                var bt = translationFromEntity[be].Value.xy;
                var d = math.distance(bt, translation.Value.xy);
                if (d < bestDist)
                {
                    bestDist = d;
                    bestIndex = i;
                }
            }

            var ballEntity = ballEntities[bestIndex];
            var ballTranslation = translationFromEntity[ballEntity];
            if (math.abs(ballTranslation.Value.x - translation.Value.x) <
                autoKeyConfig.AutoReactDistance)
            {
                if (ballTranslation.Value.y > (localBounds.Center.y + (autoKeyConfig.AutoBoundsRange * localBounds.Extents.y)))
                    Up(ref translation, localBounds, speed, deltaTime);
                else if (ballTranslation.Value.y < (localBounds.Center.y - (autoKeyConfig.AutoBoundsRange * localBounds.Extents.y)))
                    Down(ref translation, localBounds, speed, deltaTime);
            }
        }
    }

    private static bool TranslationFromUserInput(ref Translation translation,
        in DynamicBuffer<TouchPoint> playAreaTouchPointBuffer, in LocalBounds localBounds,
        in Speed speed, float deltaTime, in InputSystem Input, in KeyCodeUp keyCodeUp, in KeyCodeDown keyCodeDown)
    {
        var userInput = false;
        if (playAreaTouchPointBuffer.Length > 0)
        {
            for (int i = 0; i < playAreaTouchPointBuffer.Length; i++)
            {
                var touchPoint = playAreaTouchPointBuffer[i].Value;
                var inLeft = touchPoint.x > (localBounds.Center.x - localBounds.Extents.x - kTouchWidthSlop);
                var inRight = touchPoint.x < (localBounds.Center.x + localBounds.Extents.x + kTouchWidthSlop);
                var inTouch = inLeft && inRight;
                if (inTouch)
                {
                    var upperBound = localBounds.Center.y + (localBounds.Extents.y * kTouchCenterSlopRatio);
                    var lowerBound = localBounds.Center.y - (localBounds.Extents.y * kTouchCenterSlopRatio);
                    if (touchPoint.y > upperBound)
                    {
                        userInput = true;
                        Up(ref translation, localBounds, speed, deltaTime);
                    }
                    else if (touchPoint.y < lowerBound)
                    {
                        userInput = true;
                        Down(ref translation, localBounds, speed, deltaTime);
                    }
                }
            }
        }

        else if (Input.GetKey(keyCodeUp.Value))
        {
            userInput = true;
            Up(ref translation, localBounds, speed, deltaTime);
        }
        else if (Input.GetKey(keyCodeDown.Value))
        {
            userInput = true;
            Down(ref translation, localBounds, speed, deltaTime);
        }

        return userInput;
    }


    static void Down(ref Translation translation, in LocalBounds localBounds, in Speed speed, float deltaTime)
    {
        var min = -0.5f + localBounds.Extents.y;
        var max = 0.5f - localBounds.Extents.y;
        var cur = translation.Value.y;

        translation.Value.y = math.clamp(cur - (speed.Value * deltaTime), min, max);
    }

    static void Up(ref Translation translation, in LocalBounds localBounds, in Speed speed, float deltaTime)
    {
        var min = -0.5f + localBounds.Extents.y;
        var max = 0.5f - localBounds.Extents.y;
        var cur = translation.Value.y;

        translation.Value.y = math.clamp(cur + (speed.Value * deltaTime), min, max);
    }
}