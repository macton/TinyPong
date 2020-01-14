using System.Collections.Generic;
using Unity.Entities;
using Unity.Tiny.Input;
using Unity.Mathematics;
using UnityEngine;

[RequiresEntityConversion]
[AddComponentMenu("TinyPong/Score")]
[ConverterVersion("macton", 5)]
public class ScoreAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs 
{
    public GameObject[] Numbers;
    
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.AddRange(Numbers);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Score());
        dstManager.AddComponentData(entity, new PrevScore {Value = -1});
        var numbersBuffer = dstManager.AddBuffer<UIElement>(entity);
        for (int i = 0; i < Numbers.Length; i++)
            numbersBuffer.Add(new UIElement {Value = conversionSystem.GetPrimaryEntity(Numbers[i])});
        var instanceBuffer = dstManager.AddBuffer<LinkedEntityGroup>(entity);
    }
}