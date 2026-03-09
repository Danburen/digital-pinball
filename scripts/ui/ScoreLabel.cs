using Godot;
using System;

public partial class ScoreLabel : Label
{
	[Export] public double Duration = 0.5;
    [Export] public Tween.TransitionType Transition = Tween.TransitionType.Quart;
    [Export] public Tween.EaseType Ease = Tween.EaseType.Out;

    private double _currentValue = 0;
    private Tween _tween;

	public void SetValue(int targetValue)
	{
		 _tween?.Kill();
		 
        _tween = CreateTween();
        _tween.SetTrans(Transition).SetEase(Ease);
        _tween.TweenProperty(this, "_currentValue", targetValue, Duration)
              .From(_currentValue);
        _tween.Finished += () => _tween = null;
	}

	public override void _Process(double delta)
    {
        Text = $"Score: {_currentValue:0}";
    }
}
