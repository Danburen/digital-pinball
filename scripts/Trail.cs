using Godot;
using Godot.Collections;
using System;

public partial class Trail : Line2D
{
	[Export] public int BaseLength { get; set; } = 8;
    [Export] public int MaxLength { get; set; } = 15;
    [Export] public float MinPointDistance { get; set; } = 5f;
    [Export] public float TrailWidth { get; set; } = 12f;
    [Export] public float VelocityStretch { get; set; } = 0.5f;
    [Export] public float InertiaDelay { get; set; } = 0.03f;
    
    private Node2D _parent;
    private Array<Vector2> _points = new();
    private Array<Vector2> _velocities = new();
    private Vector2 _lastParentPos;
    private Vector2 _parentVelocity;
    
    public override void _Ready()
    {
        _parent = GetParent<Node2D>();
        _lastParentPos = _parent.GlobalPosition;
        
        Width = TrailWidth;
        JointMode = LineJointMode.Round;
        Antialiased = true;
        TextureMode = LineTextureMode.None;
        
        // 纯色
        Gradient = new Gradient();
        Gradient.AddPoint(0, Colors.White);
        Gradient.AddPoint(1, new Color(1, 1, 1, 0));
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        
        // 计算父节点速度
        Vector2 currentPos = _parent.GlobalPosition;
        _parentVelocity = (currentPos - _lastParentPos) / dt;
        _lastParentPos = currentPos;
        
        UpdateTrail(dt);
    }
    
    private void UpdateTrail(float dt)
    {
        float speed = _parentVelocity.Length();
        int targetLength = CalculateLength(speed);

        // 距离采样
        bool shouldAdd;
        if (_points.Count == 0)
        {
            shouldAdd = true;
        }
        else
        {
            float dist = _parent.GlobalPosition.DistanceTo(_points[0]);
            float adaptiveDist = Mathf.Max(MinPointDistance, 8f - speed * 0.02f);
            shouldAdd = dist >= adaptiveDist;
        }
        
        if (shouldAdd)
        {
            _points.Insert(0, _parent.GlobalPosition);
            _velocities.Insert(0, _parentVelocity);
            
            // 裁剪
            while (_points.Count > targetLength)
            {
                _points.RemoveAt(_points.Count - 1);
                _velocities.RemoveAt(_velocities.Count - 1);
            }
        }
        
        // 物理惯性
        ApplyInertia(dt);
        
        // 更新显示
        UpdateVisuals(speed);
    }
    
    private int CalculateLength(float speed)
    {
        float t = Mathf.Clamp(speed / 500f, 0f, 1f);
        return Mathf.RoundToInt(Mathf.Lerp(BaseLength, MaxLength, t * VelocityStretch));
    }
    
    private void ApplyInertia(float dt)
    {
        float damping = 1f - dt / InertiaDelay;
        
        for (int i = 0; i < _points.Count; i++)
        {
            _velocities[i] *= damping;
            _points[i] += _velocities[i] * dt;
        }
    }
    
    private void UpdateVisuals(float speed)
    {
        if (_points.Count < 2)
        {
            ClearPoints();
            return;
        }
        
        // 转本地坐标
        var localPoints = new Vector2[_points.Count];
        for (int i = 0; i < _points.Count; i++)
        {
            localPoints[i] = ToLocal(_points[i]);
        }
        Points = localPoints;
        
        // 宽度随速度变化
        float targetWidth = TrailWidth * (1f + speed * 0.005f);
        Width = Mathf.Lerp(Width, targetWidth, 0.05f);
        
        // 宽度曲线（头部粗尾部细）
        if (WidthCurve == null)
        {
            var curve = new Curve();
            curve.AddPoint(new Vector2(0, 1f));
            curve.AddPoint(new Vector2(0.5f, 0.6f));
            curve.AddPoint(new Vector2(1f, 0f));
            WidthCurve = curve;
        }
    }
}
