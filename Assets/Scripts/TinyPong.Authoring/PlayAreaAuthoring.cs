using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[RequiresEntityConversion]
[AddComponentMenu("TinyPong/Play Area")]
[ConverterVersion("macton", 1)]
public class PlayAreaAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Range(0.0f, 2.0f)]
    public float Speed;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new PlayArea());
        dstManager.AddComponentData(entity, new LocalBounds());
    }
}