using Godot;
using System;

public partial class Camera2d : Camera2D
{
	[Export] public float DefaultAmplitude { get; set; } = 10f;
    [Export] public float DefaultFrequency { get; set; } = 15f;
    [Export] public float DefaultDuration { get; set; } = 0.3f;
    [Export] public bool DynamicEnable = true;
    [Export] public float DynamicFactor = 0.5f;

    private Vector2 _default_zoom;
    private Vector2 _default_position;
    private float _default_smoothSpeed;
    private float _maxDistance;

    public Node2D target = null;
    public Node2D[] objs {get; set; } = [];
	private float _amplitude = 0f;
    private float _frequency = 0f;
    private float _remainingTime = 0f;
    private float _totalDuration = 0f; 
    
    private float _time = 0f;
    private FastNoiseLite _noise;
    private Vector2 _currentOffset = Vector2.Zero;
	 public override void _Ready()
    {
        _noise = new FastNoiseLite();
        _noise.Seed = new Random().Next();
        _noise.Frequency = 1f;

        _default_position = Position;
        _default_zoom = Zoom;
        _default_smoothSpeed = PositionSmoothingSpeed;
        _maxDistance = (GetViewportRect().Size / 2).DistanceTo(
            GetViewportRect().Size
        );
    }

	public override void _Process(double delta)
    {
        float dt = (float)delta;
        ProcesShake(dt);
        ProcessZoom(dt);
	}
    private void ProcesShake(float dt)
    {        
        if (_remainingTime > 0)
        {
            _time += dt;
            _remainingTime -= dt;
            
            ApplyShake(dt);
        }
        else
        {
            if (_currentOffset.LengthSquared() > 0.01f)
            {
                _currentOffset = _currentOffset.Lerp(Vector2.Zero, dt * 10f);
                Offset = _currentOffset;
            }
            else
            {
                _currentOffset = Vector2.Zero;
                Offset = Vector2.Zero;
                _time = 0f;
            }
        }
    }
    private void ProcessZoom(float dt)
    {
        if(target != null)
        {
            FollowTarget(dt);
        }
        else if(DynamicEnable && objs.Length != 0)
        {
            bool isObjsInsideTree = true;
            foreach (Node2D obj in objs)
            {
                if(!IsInstanceValid(obj) || !obj.IsInsideTree())
                {
                    isObjsInsideTree = false;
                    break;
                }
            }

            if(isObjsInsideTree)
            {
                Vector2 center = GetViewportRect().Size / 2;
                Vector2 averagePos = Vector2.Zero;
                foreach (Node2D obj in objs)
                {
                    averagePos += obj.Position;
                }
                averagePos /= objs.Length;

                Vector2 disFromCenter = (center - averagePos) / center;
                DragHorizontalOffset = disFromCenter.X * DynamicFactor;
                DragVerticalOffset = disFromCenter.Y * DynamicFactor;

                float distance = averagePos.DistanceTo(new Vector2(
                    center.X, GetViewportRect().Size.Y
                ));

                float factor = (float)Mathf.Lerp(0.975, 1.025, 1 - distance / _maxDistance);
                Zoom = new Vector2(
                    factor, factor
                );
            }
        }
        
    }

	private void ApplyShake(float dt)
	{
		float t = 1f - (_remainingTime / _totalDuration);
		float currentAmp = _amplitude * (1f - t * t); // fade out
		float noiseTime = _time * _frequency;
		float x = _noise.GetNoise1D(noiseTime) * currentAmp;
		float y = _noise.GetNoise1D(noiseTime + 1000) * currentAmp * 0.8f;  // weak in y axis
			
		_currentOffset = new Vector2(x, y);
		Offset = _currentOffset;
	}

	private float GetOriginalDuration()
	{
		if (_remainingTime <= 0) return 0.001f;
		return _remainingTime / (1f - Mathf.Sqrt(1f - (_amplitude > 0 ? 0 : 1)));
	}
    public void Shake(float amplitude, float frequency, float duration)
    {
         if (amplitude >= _amplitude || _remainingTime <= 0)
        {
            _amplitude = amplitude;
            _frequency = frequency;
            _totalDuration = duration;
            _remainingTime = duration;
            _time = 0f;
            _noise.Seed = new Random().Next();
        }
    }

    public void FollowTarget(float delta)
    {
        GlobalPosition = new Vector2(
            Mathf.Lerp(GlobalPosition.X, target.GlobalPosition.X, 12f * delta),
            Mathf.Lerp(GlobalPosition.Y, target.GlobalPosition.Y, 12f * delta)
        );
    }
}
