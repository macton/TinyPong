using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[RequiresEntityConversion]
[AddComponentMenu("TinyPong/Solo Spawner")]
[ConverterVersion("macton", 3)]
public class SoloSpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject Prefab;
    public float CoolDownSeconds;
    [Range(0,64*1024)]
    public int GenerateMaxCount;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new LocalBounds());
        dstManager.AddComponentData(entity, new SoloSpawner
        {
            Prefab = conversionSystem.GetPrimaryEntity(Prefab),
            CoolDownSeconds = CoolDownSeconds,
            SecondsUntilGenerate = CoolDownSeconds,
            GenerateMaxCount = GenerateMaxCount,
            GeneratedCount = 0,
            Random = new Unity.Mathematics.Random((uint)(entity.Index * 0x153FD33F) )
        });
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Prefab);
    }
}