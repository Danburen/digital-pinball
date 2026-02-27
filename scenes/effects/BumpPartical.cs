using Godot;
using System;

public partial class BumpPartical : CpuParticles2D
{
	public override void _Ready()
	{
		Finished += () => QueueFree();
		Emitting = true;
	}
}
