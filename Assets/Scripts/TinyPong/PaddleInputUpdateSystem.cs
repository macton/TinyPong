using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Tiny;
using Unity.Tiny.Input;
using Unity.Transforms;

public class PaddleInputUpdateSystem : JobComponentSystem
{
    EntityQuery m_BallQuery;
    bool m_First = true;

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

        if (m_First)
        {
            if (Input.IsTouchSupported())
                Debug.Log("TOUCH SUPPORTED");
            else
                Debug.Log("MOUSE SUPPORTED");

            m_First = false;
        }

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
                
                if (Input.IsTouchSupported() && Input.TouchCount() > 0)
                {
                    for (var i = 0; i < Input.TouchCount(); i++)
                    {
                        var itouch = Input.GetTouch(i);
                        PressAtPosition(new float2(itouch.x, itouch.y));
                    }
                }
                else
                {
                    if (Input.GetMouseButton(0))
                    {
                        var pos = PressAtPosition(Input.GetInputPosition());
                        Debug.Log( $"localBounds {localBounds.Center.x}, {localBounds.Center.y} - {localBounds.Extents.x}, {localBounds.Extents.y}");
                        if (localBounds.Contains(pos, localBounds.Extents.x))
                        {
                            if (pos.y > localBounds.Center.y)
                            {
                                autoKeyTimer.Value = 0.0f;
                                Up(ref translation, localBounds, speed, deltaTime);
                            }
                            else
                            {
                                autoKeyTimer.Value = 0.0f;
                                Down(ref translation, localBounds, speed, deltaTime);
                            }
                        }
                    }
                }
                
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
                        
                        if (ballEntities.Length > 0)
                        {
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
                            if (math.abs(ballTranslation.Value.x - translation.Value.x) < autoKeyConfig.AutoReactDistance)
                            {
                                if (ballTranslation.Value.y > (localBounds.Center.y + (autoKeyConfig.AutoBoundsRange * localBounds.Extents.y)))
                                    Up(ref translation, localBounds, speed, deltaTime);
                                else if (ballTranslation.Value.y < (localBounds.Center.y - (autoKeyConfig.AutoBoundsRange * localBounds.Extents.y)))
                                    Down(ref translation, localBounds, speed, deltaTime);
                            }
                        }
                    }
                }
            }).Run();

        ballEntities.Dispose();

        return lastJobHandle;
    }

    private float2 PressAtPosition(float2 inputScreenPosition)
    {
        var di = GetSingleton<DisplayInfo>();
        var playAreaEntity = GetSingletonEntity<PlayArea>();
        var playAreaBounds = EntityManager.GetComponentData<LocalBounds>(playAreaEntity);

        var height = di.height;
        int width = di.width;
        float targetRatio = 1920.0f / 1080.0f;
        float actualRatio = (float) width / (float) height;
        
        if (actualRatio > targetRatio)
        {
            width = (int) (di.height * targetRatio);
            inputScreenPosition.x -= (di.width - width) / 2.0f;
        }
        
        var x0 = inputScreenPosition.x / width;
        var y0 = inputScreenPosition.y / height;

        var screenYRatio = (float)height / (float)width;
        Debug.Log($"screen {width} x {height} = {screenYRatio}");
        
        var playAreaYRatio = playAreaBounds.Extents.y / playAreaBounds.Extents.x;
        Debug.Log($"playArea {playAreaBounds.Extents.x} x {playAreaBounds.Extents.y} = {playAreaYRatio}");
        
        var finalYRatio = screenYRatio / playAreaYRatio;

        var x1 = x0 - 0.5f;
        var y1 = y0 - 0.5f;
        
        var x = x1;
        var y = y1 * finalYRatio;

        Debug.Log($"Touch {x}, {y}");
        return new float2(x,y);
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