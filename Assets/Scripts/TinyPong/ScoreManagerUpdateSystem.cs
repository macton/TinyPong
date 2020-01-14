using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class ScoreManagerUpdateSystem : JobComponentSystem
{
    EntityQuery m_BallQuery;
    
    protected override void OnCreate()
    {
        m_BallQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new [] { ComponentType.ReadOnly<Ball>(), ComponentType.ReadOnly<LocalBounds>(), ComponentType.ReadWrite<Translation>() },
        });
    }
    
    protected override JobHandle OnUpdate(JobHandle lastJobHandle)
    {
        var scoreFromEntity = GetComponentDataFromEntity<Score>(false);
        var translationFromEntity = GetComponentDataFromEntity<Translation>(false);
        var localBoundsFromEntity = GetComponentDataFromEntity<LocalBounds>(true);
        var ballEntities = m_BallQuery.ToEntityArray(Allocator.TempJob);
        
        Entities.ForEach((in ScoreManager scoreManager) =>
        {
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
                    var scoreValue = (scoreFromEntity[scoreManager.RightScoreEntity].Value + 1)%1000;
                    scoreFromEntity[scoreManager.RightScoreEntity] = new Score { Value = scoreValue};
                    translationFromEntity[entity] = new Translation();
                }

                if (localPosition.x >= playAreaMax)
                {
                    var scoreValue = (scoreFromEntity[scoreManager.LeftScoreEntity].Value + 1)%1000;
                    scoreFromEntity[scoreManager.LeftScoreEntity] = new Score { Value = scoreValue};
                    translationFromEntity[entity] = new Translation();
                }
            }

        }).Run();

        ballEntities.Dispose();

        return lastJobHandle;
    }
}