using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class LocalBoundsUpdateSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle lastJobHandle)
    {
        Entities.ForEach((ref LocalBounds localBounds, in Translation translation, in LocalToParent localToParent) =>
        {
            var height = math.abs(math.mul(localToParent.Value, new float4(0.0f, 1.0f, 0.0f, 0.0f)).y);
            var width = math.abs(math.mul(localToParent.Value, new float4(1.0f, 0.0f, 0.0f, 0.0f)).x);
            localBounds.Center = translation.Value.xy;
            localBounds.Extents = new float2 {x = width * 0.5f, y = height * 0.5f};
        }).Run();

        return lastJobHandle;
    }
}