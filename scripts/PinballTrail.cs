using Godot;
using Godot.Collections;
using System;

public partial class PinballTrail : Line2D
{
	[Export] public uint length = 10;

	private Vector2 _offset = Vector2.Zero;
	private Node2D _parentNode;
	public override void _Ready()
	{
		_parentNode = GetParent<Node2D>();
		_offset = Position;
		TopLevel = true;
	}

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition = Vector2.Zero;
		Vector2 point = _parentNode.GlobalPosition += _offset;
		AddPoint(point, 0);
		if (GetPointCount() > length)
		{
			RemovePoint(GetPointCount() - 1);
		}
    }
}
