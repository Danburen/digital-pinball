using Godot;
using System;

public partial class Transition : Control
{
	private AnimationPlayer animationPlayer;
	private ColorRect colorRect;
	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Ignore;
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		colorRect = GetNode<ColorRect>("ColorRect");
		colorRect.MouseFilter = MouseFilterEnum.Ignore;
	}

	public void FadeIn()
	{
		animationPlayer.Play("FadeIn");
	}

	public void FadeOut()
	{
		animationPlayer.PlayBackwards("FadeIn");
	}
}
