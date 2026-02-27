using Godot;
using System;
using System.Collections.Generic;
using static Globals;

public partial class Gameover : Control
{
	private AnimationPlayer _bgAnimationPlayer;
	private AnimationPlayer _uiAnimationPlayer;
	private AnimationPlayer _btnAnimationPlayer;
	private Label _endGameTipLabel;
	private Label _bricksCntLabel;
	private Label _maxComboCntLabel;
	private Label _superComboCntLabel;
	private Label _scoreLabel;
	private Label _finalScoreLabel;
	public static readonly Dictionary<GameOverType, string[]> gameOverTip = new(){
		{	GameOverType.HeartsDepleted, 
				[$"{maxHearts}个备用球全被你用光了?", 
				"注意!你还剩-1个球了!",
				"Oops, 好像有人用完了所有的备用球。",
				"游戏结束。但是下一次肯定能得更高分呢！",
				"那是最后一个备用球的, 我是不是应该再准备几个?", 
				"球用完了，但勇气可嘉！",
            	"这波是球先动的手。",
            	"建议下次多捡点生命道具...如果有的话。",
            	"你的球选择了自由。",
            	"0球 survivors, 恭喜达成'弹尽粮绝'成就。"]
		},
		{
			GameOverType.BricksReachedBottom,
				["有人好像不小心被砖块砸到脚趾了。",
				"这些砖块不应该出现在底部的。",
				"提醒一下, 别让砖块掉到底部。",
				"GG", 
				"砖块：我免费啦！你：我免费啦...",
            	"底 部 防 线 崩 溃",
            	"砖块已经占领高地（低地？）。",
            	"这不是俄罗斯方块，别让它们堆起来啊！",
            	"你的防御塔被推平了。",
            	"建议抬头看看...哦已经到底了。",
				"我好像设计游戏的时候不应该让砖块掉的这么快的。",
				"什么?别说什么小球击不中最底部的砖块。"]
		},
		{
			GameOverType.None,
			[
				"?你是怎么做到让游戏结束的?",
				"检测到未知错误：玩家太菜（不是）。",
				"程序猿表示这不应该发生...",
				"达成隐藏结局：量子态失败。",
				"你发现了连开发者都不知道的死法！"
			]
		}
	};
	public override void _Ready()
	{
		BindInstant();
		ProcessMode = ProcessModeEnum.Always;
		Visible = true;
		string[] endGameTips = gameOverTip[Globals.gameoverType];
		_endGameTipLabel.Text = endGameTips[GD.RandRange(0, endGameTips.Length -1)];

		_uiAnimationPlayer.Play("RESET");
		_btnAnimationPlayer.Play("RESET");
		_bgAnimationPlayer.Play("bg_fade_in");
		_uiAnimationPlayer.Play("label_zoom_rotate");
		_btnAnimationPlayer.Play("btn_spawn");
	}

	private void BindInstant(){
		_endGameTipLabel = GetNode<Label>("VBoxContainer/EndGameTip");
		_bricksCntLabel = GetNode<Label>("VBoxContainer/HBoxContainer/PlaceholderContainer/BricksCnt");
		_maxComboCntLabel = GetNode<Label>("VBoxContainer/HBoxContainer/PlaceholderContainer/MaxComboCnt");
		_superComboCntLabel = GetNode<Label>("VBoxContainer/HBoxContainer/PlaceholderContainer/SuperComboCnt");
		_scoreLabel = GetNode<Label>("VBoxContainer/HBoxContainer/PlaceholderContainer/Score");
		_finalScoreLabel = GetNode<Label>("VBoxContainer/HBoxContainer2/FinalScore");
		_bgAnimationPlayer = GetNode<AnimationPlayer>("BGAnimationPlayer");
		_uiAnimationPlayer = GetNode<AnimationPlayer>("UiAnimationPlayer");
		_btnAnimationPlayer = GetNode<AnimationPlayer>("BtnAnimationPlayer");
	}
}
