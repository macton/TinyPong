using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
[AddComponentMenu("TinyPong/Play Area")]
[ConverterVersion("macton", 3)]
public class PlayAreaAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new PlayArea());
        dstManager.AddComponentData(entity, new LocalBounds());
        dstManager.AddBuffer<TouchPoint>(entity);
    }
}