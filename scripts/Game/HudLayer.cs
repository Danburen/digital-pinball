using Godot;
using Godot.Collections;
using System;

public partial class HudLayer : CanvasLayer
{
	private HBoxContainer _heartContainer;
	private Texture2D _heartTexture;

	public override void _Ready(){
		_heartContainer = GetNode<HBoxContainer>("TopBar/HBoxContainer");
		_heartTexture = GD.Load<Texture2D>("res://assets/img/ball.svg");
		UpdateHearts();

		Globals.OnHeartsChanged += UpdateHearts;
	}

	public void UpdateHearts(){
		 GD.Print($"UI Update: {Globals.hearts}");
        foreach (var child in _heartContainer.GetChildren())
        {
            (child as Node).QueueFree();
        }

        for (int i = 0; i < Globals.hearts; i++)
        {
            var heart = new TextureRect();
            heart.Texture = _heartTexture;
            heart.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
            _heartContainer.AddChild(heart);
        }
	}

    public void Unregister()
    {
        Globals.OnHeartsChanged -= UpdateHearts;
    }
}
