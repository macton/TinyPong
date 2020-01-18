using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
[AddComponentMenu("TinyPong/Numbers Prefab Group")]
[ConverterVersion("macton", 1)]
public class NumbersPrefabGroupAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs 
{
    public GameObject[] Numbers;
    
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.AddRange(Numbers);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new NumbersPrefabGroup());
        var numbersBuffer = dstManager.AddBuffer<LinkedEntityGroup>(entity);
        for (int i = 0; i < Numbers.Length; i++)
            numbersBuffer.Add(new LinkedEntityGroup {Value = conversionSystem.GetPrimaryEntity(Numbers[i])});
    }
}