using Godot;
using System;
using System.ComponentModel;

public partial class StartMenu : Control
{
	public override void _Ready()
	{
		AnchorRight = 1;
        AnchorBottom = 1;
        MouseFilter = MouseFilterEnum.Ignore;
	}

    public void _on_play_button_pressed()
    {
        GD.Print("Play button pressed");
        Globals.CutScene()
            .SetLayer(100)
            .FadeIn(
                () => {
                    GetTree().ChangeSceneToFile("res://scenes/Game.tscn");
                    Globals.CutScene().FadeOut();
                }
            );
        //GetTree().ChangeSceneToFile("res://scenes/auto_play.tscn");
    }
}
