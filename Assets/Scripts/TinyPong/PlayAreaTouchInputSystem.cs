using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Tiny;
using Unity.Tiny.Input;

public class PlayAreaTouchInputSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle lastJobHandle)
    {
        if (!HasSingleton<DisplayInfo>())
            return lastJobHandle;
        if (!HasSingleton<PlayArea>())
            return lastJobHandle;
        
        var Input = World.GetExistingSystem<InputSystem>();
        var displayInfo = GetSingleton<DisplayInfo>();
        var playAreaEntity = GetSingletonEntity<PlayArea>();
        var playAreaBounds = EntityManager.GetComponentData<LocalBounds>(playAreaEntity);
        var playAreaTouchPointBuffer = EntityManager.GetBuffer<TouchPoint>(playAreaEntity);

        playAreaTouchPointBuffer.Clear();
        if (Input.IsTouchSupported() && Input.TouchCount() > 0)
        {
            for (var i = 0; i < Input.TouchCount(); i++)
            {
                var screenPoint = Input.GetTouch(i);
                var playAreaPoint = PressAtPosition(displayInfo, playAreaBounds, new float2(screenPoint.x, screenPoint.y));
                playAreaTouchPointBuffer.Add(new TouchPoint {Value = playAreaPoint});
            }
        }
        else if (Input.GetMouseButton(0))
        {
            var playAreaPoint = PressAtPosition(displayInfo, playAreaBounds, Input.GetInputPosition());
            playAreaTouchPointBuffer.Add(new TouchPoint {Value = playAreaPoint});
        }

        return lastJobHandle;
    }

    private float2 PressAtPosition(in DisplayInfo displayInfo, in LocalBounds playAreaBounds, float2 inputScreenPosition)
    {
        // Position [0,1] relative to screen. 
        // - Assumes targetRatio
        
        var height = displayInfo.height;
        int width = displayInfo.width;
        float targetRatio = 1920.0f / 1080.0f;
        float actualRatio = (float) width / (float) height;

        if (actualRatio > targetRatio)
        {
            width = (int) (displayInfo.height * targetRatio);
            inputScreenPosition.x -= (displayInfo.width - width) / 2.0f;
        }
        var x0 = inputScreenPosition.x / width;
        var y0 = inputScreenPosition.y / height;
        
        // Position [-0.5,0.5] relative to playArea
        // - Assumes playArea is maximum width of screen area.

        var screenYRatio = (float) height / (float) width;
        var playAreaYRatio = playAreaBounds.Extents.y / playAreaBounds.Extents.x;
        var finalYRatio = screenYRatio / playAreaYRatio;
        var x1 = x0 - 0.5f;
        var y1 = y0 - 0.5f;
        var x = x1;
        var y = y1 * finalYRatio;

        return new float2(x, y);
    }
}