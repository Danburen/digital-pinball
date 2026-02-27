using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Baffle : CharacterBody2D
{
	public enum BaffleStatus{
		IDLE,
		MOVE
	}
	[Export] public float width = 100;
	[Export] public float BaffleOffsetY = 50f;
	[Export] public float limitLeft = 0f;
	[Export] public float limitRight = 200f;
	[Export] public float smoothSpeed = 15.0f;
	[Export] public float stopThreshold = 1f;
	[ExportCategory("Oscillator")]
	[Export] public float spring = 100.0f;
	[Export] public float damp = 25.0f;
	[Export] public float velocityMultiplier = 0.5f;

	private Node _parentNode;
	private Vector2 _surfaceCenterPos;
	private float _targetX = 0f;
	private Panel _bafflePanel;
	private CapsuleShape2D _collisionShape;
	private float _displacement = 0.0f;
	private float _oscillatorVelocity = 0.0f;
	private float _halfPanelWidth;
	private BaffleStatus _status;
	private IPlayeable _parent;
	public override void _Ready()
	{
		_bafflePanel = GetNode<Panel>("Panel");
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D")
			.Shape as CapsuleShape2D;

		_bafflePanel.SetSize(new Vector2(width, 17f));
		_collisionShape.Height = width;
		_halfPanelWidth =  width / 2;
		limitLeft = 0f;
		limitRight = GetViewportRect().Size.X - width;
		Globals.bafflePos.Y = GetViewportRect().Size.Y - _bafflePanel.Size.Y - BaffleOffsetY;
		Position = new Vector2(Globals.bafflePos.X, Globals.bafflePos.Y);
		_surfaceCenterPos = new Vector2(_halfPanelWidth, Position.Y);
		GD.Print("Baffle ready. Limit left: ", limitLeft, ", Limit right: ", limitRight);
		_parent = GetParent<IPlayeable>();
	}

    public override void _Input(InputEvent @event)
    {
		if(_parent.IsAutoPlay()) return;
        float inputX = 0f;
		if (@event is InputEventMouseMotion mouseMotion)
		{
			inputX = GetGlobalMousePosition().X;
		}
		else if (@event is InputEventScreenTouch screenTouch && screenTouch.IsPressed())
		{
			inputX = screenTouch.Position.X;
		}
		else if(@event is InputEventScreenDrag screenDrag)
		{
			inputX = screenDrag.Position.X;
		}

		if (inputX != 0f)
		{
			_targetX = Mathf.Clamp(inputX - _halfPanelWidth, limitLeft, limitRight);
		}
	}

	public override void _Process(double delta)
	{
		float currentX = Globals.bafflePos.X;
		float distance = Mathf.Abs(_targetX - currentX);
		if (distance < stopThreshold)
		{
			Globals.bafflePos.X = _targetX;
			_status = BaffleStatus.IDLE;
		}else
		{
			if(smoothSpeed > 0)
			{
				Globals.bafflePos.X = Mathf.Lerp(currentX, _targetX, (float)delta * smoothSpeed);
				_status = BaffleStatus.MOVE;
			}else
			{
				Globals.bafflePos.X = _targetX;
				_status = BaffleStatus.IDLE;
			}
		}
		
		float dir = Mathf.Clamp((_targetX - Globals.bafflePos.X) / _halfPanelWidth, -1, 1);
		Position = new Vector2(Globals.bafflePos.X, Globals.bafflePos.Y);
		_surfaceCenterPos = new Vector2(Position.X + _bafflePanel.Size.X / 2, Position.Y);
		
		_oscillatorVelocity += dir * velocityMultiplier;
        float force = -spring * _displacement + damp * _oscillatorVelocity;
        _oscillatorVelocity -= (float)(force * delta);
		_displacement -= (float)(_oscillatorVelocity * delta);
		_bafflePanel.Rotation = - _displacement;
    }

	public Vector2 GetSurfacePos()
	{
		return _surfaceCenterPos;
	}

	public void SetTargetX(float x)
	{
		_targetX = x;
	}
	
	public BaffleStatus GetStatus()
	{
		return _status;
	}
}
