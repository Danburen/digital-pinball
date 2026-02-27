using System;
using System.Linq;
using Godot;
using Godot.Collections;

public partial class Pinball : CharacterBody2D
{
    public enum BallState { Attached, Launching, Free ,Exploding}
	[Export] public float Speed = 400.0f;
    [Export] public float MaxSpeed = 800.0f;
    [Export] public float FuelSpeedBouns = 30f;
    [Export] public float LaunchSpeed = 500.0f;
    [Export] public float LaunchAngleRange = 60f;
    [Export] public float AttachOffsetY = 5f;
	private Vector2 _dir = Vector2.Up.Normalized(); // default up
	[Export] public float radius = 16.0f;
	[Export] public Color _ballColor = new("#ffffff");
    public int MaxCombo = 17;
    private int MaxDamage = 131072;
    private int damage = 2;
    private BallState _state = BallState.Attached;
    private Baffle _baffle;
    private bool _launchInputReceived = false;
    private Vector2 _lastTrailPos;
    private Label _label;
    private CollisionShape2D _colliShape;
    private CircleShape2D _colliCircleShape;
    private CpuParticles2D _effect_explosionBig;
    private CpuParticles2D _effect_explosionSmall;
    private CpuParticles2D _effect_appear;
    private PackedScene _effect_bump_scene = GD.Load<PackedScene>("res://scenes/effects/bump_partical.tscn");
    private PackedScene _ballScene = GD.Load<PackedScene>("res://scenes/pinball.tscn");
    private Sprite2D _sprite;
    private Line2D _trailNode;
    private IPlayeable _parentNode;
    private bool _collisionFlag = false;
    private int _comboCount = 0;
    private int _fuel = 0;
	public override void _Ready()
	{
        Hide();
		_colliShape = GetNode<CollisionShape2D>("CollisionShape2D");
        _colliCircleShape = _colliShape.Shape as CircleShape2D;
        _label = GetNode<Label>("Label");
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _lastTrailPos = GlobalPosition;
        _effect_explosionBig = GetNode<CpuParticles2D>("ExplosionBig");
        _effect_explosionSmall = GetNode<CpuParticles2D>("ExplosionSmall");
        _effect_appear = GetNode<CpuParticles2D>("AppearEffect");
        _trailNode = GetNode<Line2D>("Trail");
        _trailNode.Hide();
        _parentNode = GetParent().GetParent<IPlayeable>();
        UpdatePinballProperties();
        SetPhysicsProcess(false);
	}
    public override void _Input(InputEvent @event)
    {
        if(_parentNode.IsAutoPlay()) return;
        if (_state == BallState.Attached)
        {
            if (@event is InputEventMouseButton mouseBtn && mouseBtn.IsPressed())
            {
                Launch();
            }
            else if (@event is InputEventScreenTouch screenTouch && screenTouch.IsPressed())
            {
                Launch();
            }
        }
    }
    

    public override void _Process(double delta)
    {
        if(_state == BallState.Attached)
        {
            UpdateAttachPosition();
        }
    }


    public override void _PhysicsProcess(double delta)
    {
        Velocity = _dir * Speed;
		var collision = MoveAndCollide(Velocity * (float)delta);
        if(collision != null)
        {
            HandleCollision(collision);
        }
        else
        {
            _collisionFlag = false;
        }
        CheckScreenBounce();
        QueueRedraw();
    }

    public void Initialize(Baffle baffle)
    {
        _baffle = baffle;
        _state = BallState.Attached;
        SetPhysicsProcess(false);
        UpdateAttachPosition();
    }

    public void UpdateExplosion()
	{
        var grad = Globals.GetLinerTransparentGrad(_ballColor);
		_effect_explosionBig.Color = _ballColor;
		_effect_explosionBig.ColorRamp = grad;
        _effect_explosionSmall.Color = _ballColor;
		_effect_explosionSmall.ColorRamp = grad;
        _effect_explosionBig.ScaleAmountMax = radius / 64f;
        _effect_explosionBig.ScaleAmountMin = radius / 128f;
	}
    public void ExplodeWithHPDecrease()
    {
        UpdateExplosion();
		_state = BallState.Exploding;
        _label.Hide();
        _trailNode.Hide();
        _sprite.Hide();
        _colliShape.Disabled = true;
		_effect_explosionBig.Emitting = true;
        _effect_explosionSmall.Emitting = true;
        SetPhysicsProcess(false);
		GetTree()
            .CreateTimer(Mathf.Max(_effect_explosionSmall.Lifetime, _effect_explosionSmall.Lifetime))
            .Timeout += () => {
                QueueFree(); 
                BallDropped();
            };
    }
    public void BallDropped()
    {
        int cnt = GetParent().GetChildCount();
        if(cnt == 1)
        {
            if(Globals.hearts == 0 && Globals.gameStatus == 1)
            {
                GetParent<Node>().GetParent<Game>().GameOver(Globals.GameOverType.HeartsDepleted);
            }
            Globals.hearts--;
            Pinball newBall = _ballScene.Instantiate<Pinball>();
            newBall.Initialize(_baffle);
            GetParent().AddChild(newBall);
            newBall.AppearEffect();
            Globals.combo = 0;

            Globals.camera.objs =
			[.. GetParent<Node>().GetChildren().OfType<Node2D>(), _baffle];
        }
    }
    public void AppearEffect()
    {
        Show();
        Modulate = new Color(1, 1, 1, 0);
        _effect_appear.Emitting = true;
        var tween = CreateTween();
        
        tween.TweenProperty(this, "modulate", Colors.White, 1f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
    }
    private void SpawnBumpEffect(Vector2 pos, Vector2 normal)
    {
        var bumpEffect = _effect_bump_scene.Instantiate<CpuParticles2D>();
        bumpEffect.GlobalPosition = pos;
        bumpEffect.Direction = normal;
        GetTree().CurrentScene.AddChild(bumpEffect);
    }
    public void Launch()
    {
       if(_state == BallState.Attached)
       {
         _state = BallState.Free;
        
        float randomAngle = GD.Randf() * LaunchAngleRange - LaunchAngleRange / 2;
        float rad = Mathf.DegToRad(-90 + randomAngle);
        
        _dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).Normalized();
        Speed = LaunchSpeed;
        SetPhysicsProcess(true);
        _trailNode.ClearPoints();
        _trailNode.Show();
       }
    }
    private void UpdateAttachPosition()
    {
        if (_baffle == null) return;

        Vector2 targetPos = new(
            _baffle.GetSurfacePos().X, 
            _baffle.GetSurfacePos().Y - AttachOffsetY - radius
        );
        
        GlobalPosition = targetPos;
        Velocity = Vector2.Zero;
        _lastTrailPos = GlobalPosition;
    }

    public void ResetToBaffle()
    {
        _state = BallState.Attached;
        _dir = Vector2.Up;
        Speed = 0;
        UpdateAttachPosition();
        AppearEffect();
    }


    private void HandleCollision(KinematicCollision2D coll)
    {
        var normal = coll.GetNormal();
        if(! _collisionFlag)
        {
            var collider = coll.GetCollider();
            if(collider is Brick brick)
            {
                Globals.camera.Shake(5f, 10f, 0.5f);
                _comboCount = Math.Min(_comboCount + 1, MaxCombo);  
                if(_fuel > 0)
                {
                    if(damage / brick.GetHitPoints() >= 2)
                    {
                        Globals.camera.Shake(10f, 20f, 0.5f);
                        _fuel-= 1;
                        brick.Destruction();
                    }
                    else
                    {
                        int remainingHp = brick.OnHit(damage * (int)MathF.Pow(2, _fuel));
                        if(remainingHp < 0)
                        {
                            _fuel = (int)MathF.Log2((- remainingHp) + 1) - 1;
                        }
                        else
                        {
                            _fuel = 0;
                        }
                    }
                    UpdatePinballProperties();
                    return;
                }
                brick.OnHit(damage);
                if(damage <= 65536)
                {
                    if(brick.GetHitPoints() + damage == damage)
                    {
                        damage *= 2;
                    }
                    else
                    {
                        damage = _comboCount > 1 ? damage * 2 : damage;
                    }
                }
                _dir = _dir.Bounce(normal).Normalized();
            }
            else if(collider is Baffle baffle)
            {
                Globals.camera.Shake(5f, 10f, 0.2f);
                var hitOffset = CalculateBaffleHitOffset(baffle);
                _dir = CalculateBaffleBounce(hitOffset);
                SpawnBumpEffect(coll.GetPosition(), coll.GetNormal());
                float acellerate = Mathf.Min(_fuel* FuelSpeedBouns + Speed, MaxSpeed);
                Velocity = _dir * acellerate;
                _comboCount = 0;
                _fuel = (int)MathF.Log2(damage) - 1 + _fuel;
                damage = 2;
            }
            UpdatePinballProperties();
            _collisionFlag = true;
            return;
        }
        _dir = _dir.Bounce(normal).Normalized();
    }
    private float CalculateBaffleHitOffset(Baffle baffle)
    {
        CollisionShape2D collShape = baffle.GetNode<CollisionShape2D>("CollisionShape2D");
        float halfWidth;
        if(collShape.Shape is CapsuleShape2D capsule)
        {
            halfWidth = capsule.Height / 2F;
        }
        else
        {
            halfWidth = 50f;
        }
        var center = baffle.GetSurfacePos();
        return Mathf.Clamp(( GlobalPosition.X - center.X) / halfWidth, -1f, 1f);
    }

    private static Vector2 CalculateBaffleBounce(float offset)
    {
        // -1(左) -> 60度, 0(中) -> 90度, 1(右) -> 120度
        var angleDeg = 45 + (offset + 1) * 45;
        var angleRad = Mathf.DegToRad(angleDeg);
        
        return new Vector2(Mathf.Cos(angleRad), -Mathf.Sin(angleRad));  // -sin 因为Y向下
    }

	private void CheckScreenBounce()
    {
        var screenSize = GetViewportRect().Size;
        var pos = Position;
        
        if (pos.Y > screenSize.Y - radius)
        {
            Globals.camera.Shake(10f, 50f, 1.0f);
            // Position = new Vector2(pos.X, screenSize.Y - radius);
            // dir.Y = -Mathf.Abs(dir.Y);
            // GLOBAL_VALUE.gameStatus = 2;
            ExplodeWithHPDecrease();
            return;
        }

        bool bounce = false;
        if (pos.X < radius)
        {
            Position = new Vector2(radius, pos.Y);
            _dir.X = Mathf.Abs(_dir.X);
            bounce = true;
        }
        else if (pos.X > screenSize.X - radius)
        {
            Position = new Vector2(screenSize.X - radius, pos.Y);
            _dir.X = -Mathf.Abs(_dir.X);
            bounce = true;
        }

        if (pos.Y < radius + (_parentNode.IsAutoPlay() ? 0 : Globals.topBarHeight))
        {
            Position = new Vector2(pos.X, radius + (_parentNode.IsAutoPlay() ? 0 : Globals.topBarHeight));
            _dir.Y = Mathf.Abs(_dir.Y);
            bounce = true;
        }
        if(bounce)
        {
            Globals.camera.Shake(4f, 10f, 0.1f);
        }
        _dir = _dir.Normalized();
    }

    private void UpdatePinballProperties()
    {
        _label.Text = $"{damage}";
        int multipy = (int) MathF.Log2(damage) + _fuel;
        int colorWeight = damage * (int)MathF.Pow(2, _fuel);
        Color color = Globals.GetPointsColor(colorWeight);
        Color fontColor = Globals.GetPointsColorLabel(colorWeight);

        _label.AddThemeFontSizeOverride("font_size", 
            (int)(radius + 16) 
        );
        _label.AddThemeColorOverride("font_color", fontColor);
        _ballColor = color;
        _sprite.Modulate = color;

        radius = 16 + multipy * 1;
        _colliCircleShape.Radius = radius;
        _sprite.Scale = Vector2.One * (radius / 64f);
        _trailNode.Width = radius * 2;
        _trailNode.DefaultColor = _ballColor;
        _trailNode.Gradient = Globals.GetLinerTransparentGrad(_ballColor);
    }

    public BallState GetBallState()
    {
        return _state;
    }
}
