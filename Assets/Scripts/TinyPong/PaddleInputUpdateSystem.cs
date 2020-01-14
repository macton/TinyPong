using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Tiny.Input;
using Unity.Transforms;

public class PaddleInputUpdateSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle lastJobHandle)
    {
        var deltaTime = math.min(1.0f / 60.0f, Time.DeltaTime);
        var Input = World.GetExistingSystem<InputSystem>();
        
        Entities
            .WithStructuralChanges()
            .ForEach((ref Translation translation, in KeyCodeUp keyCodeUp, in Speed speed, in LocalBounds localBounds) =>
        {
            if (Input.GetKey(keyCodeUp.Value))
            {
                var min = -0.5f + localBounds.Extents.y;
                var max = 0.5f - localBounds.Extents.y;
                var cur = translation.Value.y;
                
                translation.Value.y = math.clamp( cur + (speed.Value * deltaTime), min, max);
            }
        }).Run();
        
        Entities
            .WithStructuralChanges()
            .ForEach((ref Translation translation, in KeyCodeDown keyCodeDown, in Speed speed, in LocalBounds localBounds) =>
            {
                if (Input.GetKey(keyCodeDown.Value))
                {
                    var min = -0.5f + localBounds.Extents.y;
                    var max = 0.5f - localBounds.Extents.y;
                    var cur = translation.Value.y;
                
                    translation.Value.y = math.clamp( cur - (speed.Value * deltaTime), min, max);
                }
            }).Run();

        return lastJobHandle;
    }
}