using Godot;
using System;
using System.Linq;
using System.Runtime.Intrinsics.X86;

public partial class Game : Control, IPlayeable
{
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
	private PackedScene _fullMaskLayer;
	private Pinball _currentBall;
	private Panel _gameTopBarPanel;
	private LevelGenerator _levelGen;
	private int _currentRowIndex = 0;
	public override void _Ready()
	{
		_brickScene = GD.Load<PackedScene>("res://scenes/brick.tscn");
		_pinballScene = GD.Load<PackedScene>("res://scenes/pinball.tscn");
		_fullMaskLayer = GD.Load<PackedScene>("res://scenes/ui/full_mask_layer.tscn");

		_bricksContainer = GetNode<Node>("Bricks");
		_pinBallsContainer = GetNode<Node>("Balls");
		_baffle = GetNode<Baffle>("Baffle");
		_gameTopBarPanel = GetNode<Panel>("HUDLayer/TopBar/Panel");
		Globals.camera = GetNode<Camera2d>("Camera2D");
		Globals.HUDLayer = GetNode<CanvasLayer>("HUDLayer");
		ProcessMode = ProcessModeEnum.Disabled;

		_levelGen = new LevelGenerator(columns);
		UpdateUserInterface();
		CalculatePlaceSpace();
		GenerateInitialBricks();
		SpawnInitialBall(); 
		Globals.gameStatus = 1;
		Globals.hearts = 3;
		Globals.camera.objs =
				[.. _pinBallsContainer.GetChildren().OfType<Node2D>(), _baffle];
		GD.Print($"View Rect {GetViewportRect().Size.Y}, bafflePos:{Globals.bafflePos}");

		_brickDropTimer = new Timer();
		_brickDropTimer.WaitTime = dropInterval;
		_brickDropTimer.OneShot = false;
		_brickDropTimer.Connect("timeout", new Callable(this, nameof(OnBrickDropTimerTimeout)));
		AddChild(_brickDropTimer);
		_brickDropTimer.Start();
		Globals.CutScene().FadeOut(
			() => {	
				ProcessMode = ProcessModeEnum.Always;
				GameOver(Globals.GameOverType.BricksReachedBottom);
			}	
		);
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
		GD.Print(values);
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
	private void SpawnNewRowAtTop() {  SpawnRowPosYAt(Globals.topBarHeight + margin); }

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
		GD.Print("Bricks dropping, current row index: " + _currentRowIndex);
	}

	public void GameOver(Globals.GameOverType type)
	{
		ProcessMode = ProcessModeEnum.Disabled;
		GD.Print("Game Over!");
		Globals.gameStatus = 2;
		_brickDropTimer.Stop();
		HudLayer hud = GetNode<HudLayer>("HUDLayer");
		hud.Unregister();
		Globals.CutScene(100).FadeIn(
			() => {
				GetTree().ChangeSceneToFile("res://scenes/auto_play.tscn");
				Globals.CutScene(100).FadeOut();
			}
		);
	}
	private void UpdateUserInterface()
	{
		_gameTopBarPanel.SetSize(new(_gameTopBarPanel.Size.X, Globals.topBarHeight));
	}
	private void SpawnInitialBall()
    {
        _currentBall = _pinballScene.Instantiate<Pinball>();
		_currentBall.Initialize(_baffle);
        _pinBallsContainer.AddChild(_currentBall);
        _currentBall.AppearEffect();
        GD.Print("Ball spawned and attached to baffle");
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
		float startY = Globals.topBarHeight + margin;
		for(int i = 0; i < InitialRows; i++)
		{
			SpawnRowPosYAt(startY + i * (_brickHeight + vSpace));
		}
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
	public bool IsAutoPlay()
	{
		return false;
	}
}
