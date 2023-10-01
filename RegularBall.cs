using Godot;
using System;
using System.Linq;

public partial class RegularBall : Node2D
{
	const int bombMaxNumber = 5;
	const int firesToPlace = 6;
	const ulong homingTimeMs = 7 * 1000;
	const ulong homingSoundTimeMs = 440;

	[Export]
	float bounceHeight = 25;
	private PackedScene fireScene;
	private PackedScene explosionScene;
	private PackedScene crosshairScene;
	private AudioStreamPlayer2D audioPlayer;
	Vector2 moveDir = Vector2.Zero;

	float jumpOffset = 0f;
	float jumpSpeed = 0f;

	private RandomNumberGenerator random;

	private Node2D ballRender;
	private Node2D graphic;
	private Node2D main;
	private Area2D ballXWall;
	private Area2D ballYWall;
	private Area2D hitArea;
	private Area2D smashArea;

	public PowerupType currentPowerup = PowerupType.None;
    private GlobalData globalData;
    private float originalYScale;
	private BallSpawner spawner;

	[Export]
	private Texture2D[] numbers;

	private Sprite2D bombNumberDisplay;
	private Sprite2D bombGraphic;
	private AnimatedSprite2D fireAnimation;

	private int firePlacedCount = 0;
	private ulong powerupStartTime;
	private ulong homingSoundTime;
	private bool bounced = false;
	private string ownerPlayer;
	private Player homingPlayer;
	private Node2D crosshair;
	private AudioStreamWav homingSound;
	private AudioStreamWav placeFireSound;

	public bool IsMoving()
	{
		return moveDir != Vector2.Zero;
	}

	public bool IsPoweredUp()
	{
		return currentPowerup != PowerupType.None;
	}

	public override void _Ready()
	{
		globalData =  GetNode<GlobalData>("/root/GlobalData");
		
		originalYScale = Scale.Y;
		ballRender = GetNode<Node2D>("BallRender");
		graphic = GetNode<Node2D>("BallRender/Graphic");
		main = GetNode<Node2D>("/root/MainScene");
		ballXWall = main.GetNode<Area2D>("Stage/BallXArea");
		ballYWall = main.GetNode<Area2D>("Stage/BallYArea");
		hitArea = GetNode<Area2D>("BallRender/HitArea");
		smashArea = GetNode<Area2D>("BallRender/SmashArea");
		fireAnimation = GetNode<AnimatedSprite2D>("BallRender/FireAnimation");

		bombNumberDisplay = GetNode<Sprite2D>("BallRender/BombNumber");
		bombGraphic = GetNode<Sprite2D>("BallRender/BombGraphic");

		bombNumberDisplay.Visible = false;
		bombGraphic.Visible = false;
		fireAnimation.Visible = false;

		random = new RandomNumberGenerator();
		jumpSpeed = bounceHeight;

		bounceHeight = random.RandfRange(25, 40);

		fireScene = (PackedScene)ResourceLoader.Load("res://fire.tscn");
		explosionScene = (PackedScene)ResourceLoader.Load("res://explosion.tscn");
		crosshairScene = (PackedScene)ResourceLoader.Load("res://crosshair.tscn");

		audioPlayer = new AudioStreamPlayer2D();
		AddChild(audioPlayer);

		homingSound = (AudioStreamWav)ResourceLoader.Load("res://sounds/homing.wav");
		placeFireSound = (AudioStreamWav)ResourceLoader.Load("res://sounds/placeFire.wav");
	}

	public bool TryApplyPowerup(PowerupType powerupType, string ownerPlayerPrefix)
	{
		if (currentPowerup != PowerupType.None)
			return false;

		ownerPlayer = ownerPlayerPrefix;
		applyPowerup(powerupType);
		return true;
	}

	private void updatePowerup()
	{
		if (currentPowerup == PowerupType.None)
			return;

		if (currentPowerup == PowerupType.Fire)
		{
			var properGround = hitArea.GetOverlappingAreas().Any(x => x.Name == "P1Area" || x.Name == "P2Area");
			if (bounced && properGround)
			{
				var fire = (Fire)fireScene.Instantiate();
				main.AddChild(fire);
				fire.GlobalPosition = ballRender.GlobalPosition;
				firePlacedCount++;

				audioPlayer.Stream = placeFireSound;
				audioPlayer.Play();

				if (firePlacedCount >= firesToPlace)
				{
					applyPowerup(PowerupType.None);
				}
			}
			fireAnimation.LookAt(fireAnimation.GlobalPosition + moveDir.Normalized() * -1);
		}

		if (currentPowerup == PowerupType.Bomb)
		{
			var bombTime = GetPowerupTime();
			var number = bombMaxNumber - (int)Math.Floor(bombTime);

			if (number > 0)
			{
				bombNumberDisplay.Texture = numbers[number - 1];
			}
			else
			{
				var explosion = (Node2D)explosionScene.Instantiate();
				main.AddChild(explosion);
				explosion.GlobalPosition = ballRender.GlobalPosition;
				RemoveSelf();
				spawner?.SpawnBall();
			}
		}

		if (currentPowerup == PowerupType.Homing)
		{
			var homingDir = (homingPlayer.Position - ballRender.GlobalPosition).Normalized();
			moveDir = homingDir * 0.5f;
			crosshair.GlobalPosition = homingPlayer.GlobalPosition;

			if (Time.GetTicksMsec() - homingSoundTime > homingSoundTimeMs)
			{
				homingSoundTime = Time.GetTicksMsec();
				audioPlayer.Stream = homingSound;
				audioPlayer.Play();
			}

			if (Time.GetTicksMsec() - powerupStartTime > homingTimeMs)
			{
				moveDir *= 2;
				applyPowerup(PowerupType.None);
			}
		}
	}

	public float GetPowerupTime()
	{
		return (Time.GetTicksMsec() - powerupStartTime) / 1000.0f;
	}

	private void RemoveSelf()
	{
		spawner?.RemoveBall(this);
		QueueFree();
	}

	private void applyPowerup(PowerupType powerupType)
	{
		currentPowerup = powerupType;
		graphic.Modulate = PowerupData.GetColor(powerupType);

		if (powerupType == PowerupType.None)
		{
			powerupStartTime = 0;
			if (crosshair != null)
			{
				crosshair.QueueFree();
				crosshair = null;
			}
		}
		else
			powerupStartTime = Time.GetTicksMsec();

		fireAnimation.Visible = powerupType == PowerupType.Fire;
		if (powerupType == PowerupType.Fire)
		{
			firePlacedCount = 0;
			fireAnimation.Play();
			fireAnimation.Modulate = PowerupData.GetColor(powerupType);
		}

		graphic.Visible = powerupType != PowerupType.Bomb;
		bombGraphic.Visible = powerupType == PowerupType.Bomb;
		bombNumberDisplay.Visible = powerupType == PowerupType.Bomb;
		if (powerupType == PowerupType.Bomb)
		{
			bombGraphic.Modulate = PowerupData.GetColor(powerupType);
		}

		if (powerupType == PowerupType.Homing)
		{
			homingPlayer = main.GetNode<Player>(ownerPlayer == "P1" ? "P2" : "P1");
			crosshair = (Node2D)crosshairScene.Instantiate();
			homingSoundTime = Time.GetTicksMsec();
			main.AddChild(crosshair);
		}
	}

	public void PerformSmash(float strength, string aimedPlayer)
	{
		var player = GetNode<Node2D>($"/root/MainScene/{aimedPlayer}");
		var newDir = player.GlobalPosition - GlobalPosition;
		moveDir = newDir.Normalized() * 2 * strength;
	}

	public override void _PhysicsProcess(double delta)
	{
		audioPlayer.VolumeDb = globalData.SoundEffectDB;

		var overlapping = smashArea.GetOverlappingAreas();
		var isXWall = overlapping.Contains(ballXWall);
		var isYWall = overlapping.Contains(ballYWall);

		var handCollision = overlapping.FirstOrDefault((x) =>
		{
			return x.Name == "HandArea";
		});

		if (isXWall && currentPowerup != PowerupType.Homing)
			moveDir.X *= -1;
		if (isYWall && currentPowerup != PowerupType.Homing)
			moveDir.Y *= -1;

		var pos = Position;
		pos.X += moveDir.X * 10;
		pos.Y += moveDir.Y * 5;

		jumpOffset -= jumpSpeed;

		if (jumpSpeed > 0 || (jumpSpeed <= 0 && jumpOffset < 0))
		{
			jumpSpeed -= 2;
			Scale = new Vector2(Scale.X, originalYScale);
			bounced = false;
		}
		else if (jumpOffset > 0)
		{
			jumpSpeed = bounceHeight;
			Scale = new Vector2(Scale.X, originalYScale * 0.7f);
			bounced = true;
		}

		Position = pos;

		ballRender.Position = new Vector2(0, jumpOffset);
		moveDir *= 0.99f;

		updatePowerup();
	}

	internal void SetSpawner(BallSpawner ballSpawner)
	{
		spawner = ballSpawner;
	}

}
