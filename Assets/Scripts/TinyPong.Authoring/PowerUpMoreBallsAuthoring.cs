using System.Collections.Generic;
using Unity.Entities;
using Unity.Tiny.Input;
using Unity.Mathematics;
using UnityEngine;

[RequiresEntityConversion]
[AddComponentMenu("TinyPong/PowerUps/More Balls")]
[ConverterVersion("macton", 1)]
public class PowerUpMoreBallsAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs 
{
    public GameObject[] Balls;
    
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.AddRange(Balls);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new PowerUp());
        dstManager.AddComponentData(entity, new LocalBounds());
        var ballsBuffer = dstManager.AddBuffer<PowerUpMoreBalls>(entity);
        for (int i = 0; i < Balls.Length; i++)
            ballsBuffer.Add(new PowerUpMoreBalls {Value = conversionSystem.GetPrimaryEntity(Balls[i])});
    }
}