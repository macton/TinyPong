using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class ScoreManagerUpdateSystem : JobComponentSystem
{
    EntityQuery m_BallQuery;
    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        m_BallQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new [] { ComponentType.ReadOnly<Ball>(), ComponentType.ReadOnly<LocalBounds>(), ComponentType.ReadWrite<Translation>() },
        });
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    
    protected override JobHandle OnUpdate(JobHandle lastJobHandle)
    {
        if (m_BallQuery.CalculateEntityCount() > 0)
            return UpdateScore(lastJobHandle);
        
        return lastJobHandle;
    }

    private JobHandle UpdateScore(JobHandle lastJobHandle)
    {
        var scoreFromEntity = GetComponentDataFromEntity<Score>(false);
        var translationFromEntity = GetComponentDataFromEntity<Translation>(false);
        var localBoundsFromEntity = GetComponentDataFromEntity<LocalBounds>(true);
        var ballEntities = m_BallQuery.ToEntityArray(Allocator.TempJob);
        var commandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer();

        Entities.ForEach((ScoreManager scoreManager) =>
        {
            var ballsRemaining = ballEntities.Length;
            for (int i = 0; i < ballEntities.Length; i++)
            {
                var entity = ballEntities[i];
                var localBounds = localBoundsFromEntity[entity];
                var translation = translationFromEntity[entity];
                var localPosition = translation.Value.xy;
                var radius = localBounds.Extents.x;

                var playAreaMin = -0.5f + radius;
                var playAreaMax = 0.5f - radius;

                if (localPosition.x <= playAreaMin)
                {
                    var scoreValue = (scoreFromEntity[scoreManager.RightScoreEntity].Value + 1) % 1000;
                    scoreFromEntity[scoreManager.RightScoreEntity] = new Score {Value = scoreValue};
                    var paddleTranslation = translationFromEntity[scoreManager.RightPaddleEntity];
                    translationFromEntity[entity] = new Translation
                    {
                        Value = new float3
                        {
                            x = paddleTranslation.Value.x - 0.01f,
                            y = paddleTranslation.Value.y,
                            z = paddleTranslation.Value.z,
                        }
                    };
                    if (ballsRemaining > 1)
                        commandBuffer.DestroyEntity(entity);
                    ballsRemaining--;
                }

                if (localPosition.x >= playAreaMax)
                {
                    var scoreValue = (scoreFromEntity[scoreManager.LeftScoreEntity].Value + 1) % 1000;
                    scoreFromEntity[scoreManager.LeftScoreEntity] = new Score {Value = scoreValue};
                    var paddleTranslation = translationFromEntity[scoreManager.LeftPaddleEntity];
                    translationFromEntity[entity] = new Translation
                    {
                        Value = new float3
                        {
                            x = paddleTranslation.Value.x + 0.01f,
                            y = paddleTranslation.Value.y,
                            z = paddleTranslation.Value.z,
                        }
                    };
                    if (ballsRemaining > 1)
                        commandBuffer.DestroyEntity(entity);
                    ballsRemaining--;
                }
            }
        }).Run();

        ballEntities.Dispose();
        m_EntityCommandBufferSystem.AddJobHandleForProducer(lastJobHandle); 

        return lastJobHandle;
    }
}