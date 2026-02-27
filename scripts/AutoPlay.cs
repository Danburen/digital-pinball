using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class AutoPlay : Node2D, IPlayeable
{
	public enum AutoPlayStatus{
		Move,
		IDLE,
		TryLaunch,
		TryCatch
	}
	[Export] public int columns = 5;
	[Export] public int InitialRows = 6;
	[Export] public float margin = 50;
	[Export] public float vSpace = 20f;
	[Export] public float hSpaceFactor = 0.2f;
	[Export] public float LWRatio = 0.375f;

	[ExportGroup("Timer")]
	[Export] public float dropInterval = 5f;
	[Export] public float rowDropDelay = 0.05f;
	[Export] public float dropDuration = 0.3f;
	private Timer _brickDropTimer;
	private float _brickWidth;
	private float _brickHeight;
	private float _horizontalGap;
	private Node _bricksContainer;
	private Baffle _baffle;
	private Node _pinBallsContainer;
	private PackedScene _brickScene;
	private PackedScene _pinballScene;
	private Pinball _currentBall;
	private LevelGenerator _levelGen;
	private int _currentRowIndex = 0;
	private AutoPlayStatus _status;
	private PackedScene _gameoverScene;
	public override void _Ready()
	{
		_brickScene = GD.Load<PackedScene>("res://scenes/brick.tscn");
		_pinballScene = GD.Load<PackedScene>("res://scenes/pinball.tscn");
		_gameoverScene = GD.Load<PackedScene>("res://scenes/ui/gameover.tscn");

		if(Globals.gameStatus == 2){
			CanvasLayer gameoverLayer = new();
			gameoverLayer.Layer = 1;
			AddChild(gameoverLayer);
			Gameover scene =  _gameoverScene.Instantiate<Gameover>();
			gameoverLayer.AddChild(scene);
		}
		
		_bricksContainer = GetNode<Node>("Bricks");
		_pinBallsContainer = GetNode<Node>("Balls");
		_baffle = GetNode<Baffle>("Baffle");
		Globals.camera = GetNode<Camera2d>("Camera2D");
		Globals.HUDLayer = null;

		_levelGen = new LevelGenerator(columns);
		CalculatePlaceSpace();
		GenerateInitialBricks();
		SpawnInitialBall(); 
		Globals.camera.objs =
			[.. _pinBallsContainer.GetChildren().OfType<Node2D>(), _baffle];
		GD.Print("Fade out start");
		Globals.hearts = 9999;
		Globals.CutScene().FadeOut(() => {
            GD.Print("Fade out complete");
			_brickDropTimer = new Timer
			{
				WaitTime = dropInterval,
				OneShot = false
			};
			_brickDropTimer.Connect("timeout", new Callable(this, nameof(OnBrickDropTimerTimeout)));
			AddChild(_brickDropTimer);
			_brickDropTimer.Start();

			_status = AutoPlayStatus.IDLE;
		});
	}

    public override void _Process(double delta)
    {
        ProcessAutoPlay((float) delta);
    }

	private void OnBrickDropTimerTimeout()
	{
		ShiftAllBricksDown();
		SpawnNewRowAtTop();
	}

	private void SpawnRowPosYAt(float posY)
	{
		float startX = margin;
		int[] values = _levelGen.GenerageRow(_currentRowIndex ++);
		for(int i = 0; i < columns; i++)
		{
			if(values[i] != 0)
			{
				SpawnBrick(
					new Vector2(startX + _brickWidth / 2 + i * (_brickWidth + _horizontalGap), posY),
					values[i]
				);
			}
		}
	}
	private void SpawnNewRowAtTop() {  SpawnRowPosYAt(margin); }

	private void ShiftAllBricksDown()
	{
		float moveDistance = _brickHeight + vSpace;
		foreach (Node child in _bricksContainer.GetChildren())
		{
			if (child is Brick brick)
			{
				Vector2 targetPos = brick.Position + new Vector2(0, _brickHeight + vSpace);
				Tween tween = brick.DropFromAbove(brick.Position, targetPos, dropDuration);

				if (targetPos.Y > Globals.bafflePos.Y - _brickHeight)
                {
                    tween.Finished += () => GameOver(Globals.GameOverType.BricksReachedBottom);
                }
			}
		}
	}

	public void GameOver(Globals.GameOverType type)
	{
		_brickDropTimer.Stop();
		Globals.CutScene().FadeIn(() => {
			Reset();
			Globals.CutScene().FadeOut();
		});
	}
	private void SpawnInitialBall()
    {
        _currentBall = _pinballScene.Instantiate<Pinball>();
		_currentBall.Initialize(_baffle);
        _pinBallsContainer.AddChild(_currentBall);
        _currentBall.AppearEffect();
    }
	private void CalculatePlaceSpace()
	{
		Rect2 viewRect = GetViewportRect();
		float areaWidth = viewRect.Size.X - 2 * margin;
		_brickWidth = areaWidth / (hSpaceFactor * (columns - 1) + columns);
		_brickHeight = _brickWidth * LWRatio;
		_horizontalGap = _brickWidth * hSpaceFactor;; 
	}

	private void GenerateInitialBricks()
	{
		float startY = margin;
		for(int i = 0; i < InitialRows; i++)
		{
			SpawnRowPosYAt(startY + i * (_brickHeight + vSpace));
		}
	}

	private void Reset()
	{
		_brickDropTimer.Stop();
		foreach(Node c in _bricksContainer.GetChildren()) c.QueueFree();
		foreach(Node c in _pinBallsContainer.GetChildren()) c.QueueFree();
		Globals.camera.objs = null;
		_levelGen = new LevelGenerator(columns);
		CalculatePlaceSpace();
		GenerateInitialBricks();
		SpawnInitialBall(); 
		Globals.camera.objs =
			[.. _pinBallsContainer.GetChildren().OfType<Node2D>(), _baffle];

		_brickDropTimer.Start();
	}
	private void SpawnBrick(Vector2 position, int hitPoints)
	{
		Brick brick = _brickScene.Instantiate<Brick>();
		brick.SetHitPoints(hitPoints);
		brick.SetBrickSize(_brickWidth, _brickHeight);
		_bricksContainer.AddChild(brick);
		brick.Position = position;
		brick.DropFromAbove(new Vector2(position.X, -50), position, dropDuration);
	}

	public void ProcessAutoPlay(float delta)
	{
		Array<Pinball> balls = new();
		foreach(Node c in _pinBallsContainer.GetChildren())
		{
			if(c is Pinball pinball) balls.Add(pinball);
		}
		if(balls.Count == 0) return;

		// Array<Brick> bricks = new();
		// foreach(Node c in _bricksContainer.GetChildren()) 
		// {
		// 	if(c is Brick brick) bricks.Add(brick);
		// }
		
		List<Pinball> ballsList = balls.ToList();
		ballsList.Sort((a, b) => a.Position.Y.CompareTo(b.Position.Y));
		Pinball focusBall = ballsList[0];
		if(focusBall == null) return;
		// status reset
		if(_status == AutoPlayStatus.TryCatch && _baffle.GetStatus() == Baffle.BaffleStatus.IDLE)
		{
			_status = AutoPlayStatus.IDLE;
		}
		if(_status == AutoPlayStatus.TryLaunch && focusBall.GetBallState() == Pinball.BallState.Free)
		{
			_status = AutoPlayStatus.IDLE;
		}
		// action
		if(focusBall.GetBallState() == Pinball.BallState.Attached && _baffle.GetStatus() == Baffle.BaffleStatus.IDLE)
		{
			if(_status == AutoPlayStatus.IDLE)
			{
				_baffle.SetTargetX((float)GD.RandRange(_baffle.limitLeft, _baffle.limitRight));
				var timer = GetTree().CreateTimer(0.5f);
				timer.Connect(Timer.SignalName.Timeout, Callable.From(() => {
					focusBall.Launch();
				}), (uint)ConnectFlags.OneShot);
				_status = AutoPlayStatus.TryLaunch;
			}
				
		}
		else if(focusBall.GetBallState() == Pinball.BallState.Free && focusBall.Velocity.Y > 0)
		{
			if(_baffle.Position.Y - focusBall.Position.Y < 100)
			{
				if(_status != AutoPlayStatus.TryCatch){
					var t = (_baffle.Position.Y - focusBall.Position.Y) / focusBall.Velocity.Y;
					float forecastPos = focusBall.Velocity.X * t + focusBall.Position.X;
					_baffle.SetTargetX(forecastPos - _baffle.width / 2  + (float)GD.RandRange(0, _baffle.width / 2));
					_status = AutoPlayStatus.TryCatch;
				}
			}
			else
			{
				_baffle.SetTargetX(focusBall.Position.X);
			}
		}
	}

	public bool IsAutoPlay()
	{
		return true;
	}
}
