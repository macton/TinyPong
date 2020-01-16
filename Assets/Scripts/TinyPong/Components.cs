using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Input;

public struct Ball : IComponentData
{
}

public struct Paddle : IComponentData
{
}

public struct Obstacle : IComponentData
{
}

public struct ObstacleBendAngle : IComponentData
{
    public float Value;
}


public struct Direction : IComponentData
{
    public float2 Value;
}

public struct Speed : IComponentData
{
    public float Value;
}

public struct LocalBounds : IComponentData
{
    public float2 Center;
    public float2 Extents;
    
    public bool Contains(float2 pt, float ptRadius)
    {
        if (pt.x + ptRadius < Center.x - Extents.x)
            return false;
        if (pt.x - ptRadius > Center.x + Extents.x)
            return false;

        if (pt.y + ptRadius < Center.y - Extents.y)
            return false;
        if (pt.y - ptRadius > Center.y + Extents.y)
            return false;

        return true;
    }
}

public struct KeyCodeUp : IComponentData
{
    public KeyCode Value;
}

public struct KeyCodeDown : IComponentData
{
    public KeyCode Value;
}

public struct AutoKeyTimer : IComponentData
{
    public float Value;
}

public struct Score : IComponentData
{
    public int Value;
}

public struct PrevScore : IComponentData
{
    public int Value;
}

public struct UIElement : IBufferElementData
{
    public Entity Value;
}

public struct ScoreManager : IComponentData
{
    public Entity LeftScoreEntity;
    public Entity RightScoreEntity;
    public Entity LeftPaddleEntity;
    public Entity RightPaddleEntity;
}

public struct AutoKeyConfig : IComponentData
{
    public float AutoKeyTimeout;
    public float AutoReactDistance;
    public float AutoBoundsRange;
}

public struct PowerUp : IComponentData
{
}

public struct PowerUpMoreBalls : IBufferElementData
{
    public Entity Value;
}
