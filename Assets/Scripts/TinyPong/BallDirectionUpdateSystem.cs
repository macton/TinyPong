using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

public class directionUpdateSystem : JobComponentSystem
{
    EntityQuery m_ObstacleQuery;
    
    protected override void OnCreate()
    {
        m_ObstacleQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new [] { ComponentType.ReadOnly<Obstacle>(), ComponentType.ReadOnly<LocalBounds>(), ComponentType.ReadOnly<ObstacleBendAngle>() },
        });
    }
    
    protected override JobHandle OnUpdate(JobHandle lastJobHandle)
    {
        var deltaTime = math.min(1.0f / 60.0f, Time.DeltaTime);
        var obstacleBounds = m_ObstacleQuery.ToComponentDataArray<LocalBounds>(Allocator.TempJob);
        var obstacleBendAngles = m_ObstacleQuery.ToComponentDataArray<ObstacleBendAngle>(Allocator.TempJob);

        Entities
            .WithAll<Ball>()
            .ForEach((ref Direction direction, in LocalBounds localBounds, in Translation translation) =>
            {
                var radius = localBounds.Extents.x;
                var localPosition = translation.Value.xy;
                
                var playAreaMin = -0.5f + radius;
                var playAreaMax = 0.5f - radius;
        
                if (localPosition.y <= playAreaMin)
                    direction.Value = new float2(direction.Value.x, -direction.Value.y);
                if (localPosition.y >= playAreaMax)
                    direction.Value = new float2(direction.Value.x, -direction.Value.y);
                if (localPosition.x <= playAreaMin)
                    direction.Value = new float2(-direction.Value.x, direction.Value.y);
                if (localPosition.x >= playAreaMax)
                    direction.Value = new float2(-direction.Value.x, direction.Value.y);

                for (int i = 0; i < obstacleBounds.Length; i++)
                {
                    var obstacle = obstacleBounds[i];
                    var obstacleBendAngle = obstacleBendAngles[i];
                    if (obstacle.Contains(localPosition, radius))
                    {
                        var minY = obstacle.Center.y - obstacle.Extents.y;
                        var maxY = obstacle.Center.y + obstacle.Extents.y;
                        var isRight = localPosition.x > obstacleBounds[i].Center.x;
                        
                        // Bend Obstacle normal, reflect purely based on normal at intersection.
                        var bendEdge = (math.unlerp(minY, maxY, localPosition.y) - 0.5f) * 2.0f;
                        var bendAngle = bendEdge * obstacleBendAngle.Value;
                        var obstacleNormalAngle = isRight ? bendAngle : (math.PI - bendAngle);
                        var obstacleNormal = new float2(math.cos(obstacleNormalAngle), math.sin(obstacleNormalAngle));
                        
                        direction.Value = obstacleNormal;
                        break;
                    }
                }
                    
            }).Run();
        
        obstacleBounds.Dispose();
        obstacleBendAngles.Dispose();

        return lastJobHandle;
    }
}