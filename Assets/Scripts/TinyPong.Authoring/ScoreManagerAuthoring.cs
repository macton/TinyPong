using System.Collections.Generic;
using Unity.Entities;
using Unity.Tiny.Input;
using Unity.Mathematics;
using UnityEngine;

[RequiresEntityConversion]
[AddComponentMenu("TinyPong/Score Manager")]
[ConverterVersion("macton", 2)]
public class ScoreManagerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public GameObject LeftScore;
    public GameObject RightScore;
    public GameObject LeftPaddle;
    public GameObject RightPaddle;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new ScoreManager
        {
            LeftScoreEntity = conversionSystem.GetPrimaryEntity(LeftScore),
            RightScoreEntity = conversionSystem.GetPrimaryEntity(RightScore),
            LeftPaddleEntity = conversionSystem.GetPrimaryEntity(LeftPaddle),
            RightPaddleEntity = conversionSystem.GetPrimaryEntity(RightPaddle)
        });
    }
}
