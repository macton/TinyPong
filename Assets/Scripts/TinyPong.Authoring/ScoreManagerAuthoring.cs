using System.Collections.Generic;
using Unity.Entities;
using Unity.Tiny.Input;
using Unity.Mathematics;
using UnityEngine;

[RequiresEntityConversion]
[AddComponentMenu("TinyPong/Score Manager")]
[ConverterVersion("macton", 1)]
public class ScoreManagerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public GameObject LeftScore;
    public GameObject RightScore;
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new ScoreManager
        {
            LeftScoreEntity = conversionSystem.GetPrimaryEntity(LeftScore),
            RightScoreEntity = conversionSystem.GetPrimaryEntity(RightScore)
        });
    }
}