using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Tiny;
using Unity.Transforms;

public class ScoreUISystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle lastJobHandle)
    {
        Entities
            .WithStructuralChanges()
            .ForEach((ref PrevScore prevScore, in Score score, in Translation translation, in DynamicBuffer<UIElement> uiElementBuffer) =>
            {
                var uiElements = uiElementBuffer.Reinterpret<Entity>().ToNativeArray(Allocator.Temp);
                
                if (score.Value != prevScore.Value)
                {
                    var hundreds = score.Value / 100;
                    var tens = (score.Value - (hundreds * 100)) / 10;
                    var ones = (score.Value - (hundreds * 100) - (tens * 10));

                    var prefab0 = uiElements[hundreds];
                    var entity0 = EntityManager.Instantiate(prefab0);
                    EntityManager.SetComponentData(entity0, new Translation
                    {
                        Value = new float3
                        {
                            x = translation.Value.x,
                            y = translation.Value.y,
                            z = translation.Value.z,
                        }
                    });
                    
                    var prefab1 = uiElements[tens];
                    var entity1 = EntityManager.Instantiate(prefab1);
                    EntityManager.SetComponentData(entity1, new Translation
                    {
                        Value = new float3
                        {
                            x = translation.Value.x + 1.0f,
                            y = translation.Value.y,
                            z = translation.Value.z,
                        }
                    });
                    
                    var prefab2 = uiElements[ones];
                    var entity2 = EntityManager.Instantiate(prefab2);
                    EntityManager.SetComponentData(entity2, new Translation
                    {
                        Value = new float3
                        {
                            x = translation.Value.x + 2.0f,
                            y = translation.Value.y,
                            z = translation.Value.z,
                        }
                    });
                    
                    prevScore.Value = score.Value;
                }
                
                uiElements.Dispose();
            }).Run();

        
        return lastJobHandle;
    }
}