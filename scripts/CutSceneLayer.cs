using Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public partial class CutSceneLayer : CanvasLayer
{
    private AnimationPlayer _animationPlayer;
    private Action _onCompleteCallback;
    private bool _initialized = false;
    
    // we shall use cache to store the animations ready to play since the globals
	// class initialize this class using CallDefered() with not automatically 
	// calling _ready method.
    private string _pendingAnimation;
    private Action _pendingCallback;

    public override void _Ready()
    {
        if (_initialized) return;
        
        Visible = false;
        _animationPlayer = GetNode<AnimationPlayer>("Control/AnimationPlayer");
        
        var anim = _animationPlayer.GetAnimation("fade_out");

        _animationPlayer.AnimationFinished += OnAnimationFinished;
        _initialized = true;
        
        // execute cached animation play
        if (_pendingAnimation != null)
        {
            PlayAnimation(_pendingAnimation, _pendingCallback);
            _pendingAnimation = null;
            _pendingCallback = null;
        }
    }

	public new CutSceneLayer SetLayer(int layer)
	{
		this.Layer = layer;
		return this;
	}

    private void ShowAndProcess()
    {
        Visible = true;
        ProcessMode = ProcessModeEnum.Always;
    }

    public void FadeOut(Action onComplete = null)
    {
        if (!_initialized)
        {
            _pendingAnimation = "fade_out";
            _pendingCallback = onComplete;
            return;
        }
        PlayAnimation("fade_out", onComplete);
    }

    public void FadeIn(Action onComplete = null)
    {
        if (!_initialized)
        {
            _pendingAnimation = "fade_in";
            _pendingCallback = onComplete;
            return;
        }
        PlayAnimation("fade_in", onComplete);
    }

    private void PlayAnimation(string animName, Action onComplete)
    {
        ShowAndProcess();
        _onCompleteCallback = onComplete;
        _animationPlayer.Play(animName);
    }

    public void OnAnimationFinished(StringName name)
    {
        Visible = false;
        ProcessMode = ProcessModeEnum.Disabled;
        _onCompleteCallback?.Invoke();
        _onCompleteCallback = null;
    }
}