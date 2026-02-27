using Godot;
using System;

public partial class Brick : StaticBody2D
{
	private int _hitPoints = 2;
	private bool _flag = false;
	private Label _label;
	private ColorRect _colorRect;
	private RectangleShape2D _shape;
	private CpuParticles2D _explosion;
	private Vector2 _size = new(40f, 15f);
	private Tween _activeTween;
	private int _maxHitPoints;
	public override void _Ready()
	{
		_label = GetNode<Label>("Label");
		_colorRect = GetNode<ColorRect>("ColorRect");
		_shape = GetNode<CollisionShape2D>("CollisionShape2D").Shape as RectangleShape2D;
		_explosion = GetNode<CpuParticles2D>("CPUParticles2D");
		_label.Text = _hitPoints.ToString();
		UpdateColor();
		UpdateExplosion();
		UpdateSize(_size);
	}
	public void UpdateExplosion()
	{
		_explosion.Color = _colorRect.Color;
		_explosion.ColorRamp = Globals.GetLinerTransparentGrad(_colorRect.Color);
	}

	/// <summary>
	/// Set the size of child and parent node of bricks
	/// must calld after child node instantiaion.
	/// </summary>
	/// <param name="size">size</param>
	public void UpdateSize(Vector2 size)
	{
		_label.AddThemeFontSizeOverride("font_size",(int) size.Y);
		_shape.Size = size;
		_colorRect.Size = size;
		_colorRect.Position = - size / 2;
		_explosion.EmissionRectExtents = size / 2;
	}

	public void UpdateColor()
	{
		Color color = Globals.GetPointsColor(_hitPoints);
		Color fontColor = Globals.GetPointsColorLabel(_hitPoints);
		_colorRect.Color = color;
		_label.AddThemeColorOverride("font_color", fontColor);
	}

	public int OnHit(int damage)
	{
		_hitPoints -= damage;
		_label.Text = _hitPoints.ToString();
		if (_hitPoints <= 0)
		{
			Destruction();
		}
		Globals.globalScore += _hitPoints;
		return _hitPoints;
	}

	public void Destruction()
	{
		_colorRect.Hide();
		_label.Hide();
		CollisionLayer = 0;
    	CollisionMask = 0;
		_explosion.Emitting = true;
		GetTree().CreateTimer(_explosion.Lifetime).Timeout += QueueFree;
	}


	public void SetBrickSize(float width, float height)
	{
		_size = new(width, height);
	}

	public int GetHitPoints()
	{
		return _hitPoints;
	}

	public int GetMaxHitPoints()
	{
		return _maxHitPoints;
	}

	public void SetHitPoints(int value)
	{
		_hitPoints = value;
		_maxHitPoints = Mathf.Max(value, _maxHitPoints);
	}

	public void UpdateHitPoints(int value)
	{
		_hitPoints = value;
		_label.Text = _hitPoints.ToString();
		UpdateColor();
		UpdateExplosion();
	}

	public Tween DropFromAbove(Vector2 startPosition, Vector2 targetPosition, float duration = 0.3f)
	{
    	Position = startPosition;
    
    	_activeTween = CreateTween();
    	_activeTween.SetTrans(Tween.TransitionType.Quad);
    	_activeTween.SetEase(Tween.EaseType.In);
    
    	_activeTween.TweenProperty(this, "position", targetPosition, duration);
    
    	// 落地时可选：添加轻微回弹效果
    	_activeTween.Chain().TweenProperty(this, "position:y", targetPosition.Y - 5, 0.05f)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);
    	_activeTween.Chain().TweenProperty(this, "position:y", targetPosition.Y, 0.05f);
		return _activeTween;
	}
}
