using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[RequiresEntityConversion]
[AddComponentMenu("TinyPong/Ball")]
[ConverterVersion("macton", 1)]
public class BallAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Range(0.0f, 2.0f)]
    public float Speed;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Ball());
        dstManager.AddComponentData(entity, new Direction {Value = new float2(1.0f, 0.0f)}); 
        dstManager.AddComponentData(entity, new Speed {Value = Speed});
        dstManager.AddComponentData(entity, new LocalBounds());
    }
}
