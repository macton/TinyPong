using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
[AddComponentMenu("TinyPong/Score")]
[ConverterVersion("macton", 5)]
public class ScoreAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Score());
        dstManager.AddComponentData(entity, new PrevScore {Value = -1});
        dstManager.AddBuffer<LinkedEntityGroup>(entity);
    }
}