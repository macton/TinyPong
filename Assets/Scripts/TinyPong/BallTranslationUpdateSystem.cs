using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class BallTranslationUpdateSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle lastJobHandle)
    {
        var deltaTime = math.min(1.0f / 60.0f, Time.DeltaTime);

        Entities
            .WithAll<Ball>()
            .ForEach((ref Translation translation, in Direction direction, in Speed speed) =>
            {
                translation.Value.x += direction.Value.x * deltaTime * speed.Value;
                translation.Value.y += direction.Value.y * deltaTime * speed.Value;
            }).Run();

        return lastJobHandle;
    }
}