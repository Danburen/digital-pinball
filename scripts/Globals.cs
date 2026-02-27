using System;
using Godot;

public partial class Globals : Node
{
	private CutSceneLayer _cutSceneLayer;
	public static Globals Instance { get; private set; }
	public override void _Ready()
    {
        Instance = this;
		GD.Print("Globals.Ready");
    }
	public static event Action OnHeartsChanged;
	public static int globalScore = 0;
	public static int gameStatus = 0; // 0: not started, 1: playing, 2: game over
	public static GameOverType gameoverType = GameOverType.None;
	public enum GameOverType{
		None,
		HeartsDepleted,
		BricksReachedBottom
	}
	public static Vector2 bafflePos = new (0f, 0f);

	public static float topBarHeight = 40f;
	private static int _maxHearts = 3;
	private static int _hearts = _maxHearts;

	public static CanvasLayer HUDLayer;
	public static int combo = 0;
	public static int hearts
    {
        get => _hearts;
        set
        {
            if (_hearts != value)
            {
                _hearts = value;
                OnHeartsChanged?.Invoke();
            }
        }
    }

	// we use CallDeferred() method to postpone adding CutSceneLayer to the node tree
	// because the GodotEngine would busy creating and setting up sub nodes of current node
	// it will  block the node subtree and prevent adding CutSceneLayer to tree.
	// CallDeferred will calling after the current call stack completes.
	/// <summary>
	/// Get the global cut scene node.
	/// </summary>
	/// <param name="layeroutIdx">CanvasLayer.Layout</param>
	/// <returns></returns>
	public static CutSceneLayer CutScene(int layeroutIdx = 1)
	{
		if(Instance == null) return null;

        if (Instance._cutSceneLayer == null)
		{
			Instance._cutSceneLayer = GD.Load<PackedScene>("res://scenes/cut_scene_layer.tscn")
				.Instantiate<CutSceneLayer>();
			Instance._cutSceneLayer.Layer = layeroutIdx;

			Instance.CallDeferred(nameof(DeferredAddCutScene), Instance._cutSceneLayer);
		}
		return Instance._cutSceneLayer.SetLayer(layeroutIdx);
	}
	private static void DeferredAddCutScene(CutSceneLayer layer)
	{
    	Instance.GetTree().Root.AddChild(layer);
	}
	public static int maxHearts
	{
		get => _maxHearts;
		set => _maxHearts = value;
	}

	/// <summary>
	/// only runtime
	/// </summary>
	public static Camera2d camera;

	public static Gradient GetLinerTransparentGrad(Color c)
	{
        var grad = new Gradient();
        var baseColor = c;
		grad.Colors = [
			baseColor,
			new Color(c.R, c.G, c.B, 0.0f),
		];
		grad.Offsets = [0.0f, 1.0f];
        return grad;
	}

	public static Color GetPointsColor(int value)
	{
        return value switch
        {
            2 => new("#F8F9FA"),// 纯白
            4 => new("#E9ECEF"),// 灰白
            8 => new("#DEE2E6"),// 浅灰
            16 => new("#FFE066"),// 奶油黄
            32 => new("#FFC078"),// 暖橙
            64 => new("#FF922B"),// 橙
            128 => new("#E03131"),// 红
            256 => new("#C2255C"),// 玫红
            512 => new("#9C36B5"),// 紫
            1024 => new("#6741D9"),// 深紫
            2048 => new("#4C1D95"),// 深蓝紫
			4096 => new("#00F0FF"),// 青色
			8192 => new("#FF00A0"),// 粉色
			16384 => new("#39FF14"),// 亮绿
			32768 => new("#FF006E"),// 深粉
			65536 => new("#FFD700"),// 金色
			131072 => new("#FFFFFF"),// 白色
            _ => new("#000000"),// 默认黑色
        };
    }

	public static Color GetPointsColorLabel(int value)
	{
		return value switch
		{
			2 => new("#212529"),// 深灰
			4 => new("#212529"),// 深灰
			8 => new("#212529"),// 深灰
			16 => new("#212529"),// 深灰
			32 => new("#212529"),// 深灰
			64 => new("#FFFFFF"),// 白色
			128 => new("#FFFFFF"),// 白色
			256 => new("#FFFFFF"),// 白色
			512 => new("#FFFFFF"),// 白色
			1024 => new("#FFFFFF"),// 白色
			2048 => new("#FFFFFF"),// 白色
			_ => new("#FFFFFF"),// 白色
		};
	}
}
