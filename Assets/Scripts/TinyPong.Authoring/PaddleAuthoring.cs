using Unity.Entities;
using Unity.Tiny.Input;
using Unity.Mathematics;
using UnityEngine;

[RequiresEntityConversion]
[AddComponentMenu("TinyPong/Paddle")]
[ConverterVersion("macton", 1)]
public class PaddleAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Range(0.0f, 2.0f)]
    public float Speed;
    [Range(0.0f, 75.0f)]
    public float MaxBendAngle;
    
    public Unity.Tiny.Input.KeyCode KeyCodeUp;
    public Unity.Tiny.Input.KeyCode KeyCodeDown;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Paddle());
        dstManager.AddComponentData(entity, new Obstacle());
        dstManager.AddComponentData(entity, new ObstacleBendAngle {Value = math.radians(MaxBendAngle)});
        dstManager.AddComponentData(entity, new Speed {Value = Speed});
        dstManager.AddComponentData(entity, new LocalBounds());
        dstManager.AddComponentData(entity, new KeyCodeUp {Value = KeyCodeUp});
        dstManager.AddComponentData(entity, new KeyCodeDown {Value = KeyCodeDown});
    }
}