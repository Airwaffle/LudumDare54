using System;
using System.Data.Common;
using System.Linq;
using Godot;

enum State
{
	Normal,
	Jumping,
}

public partial class Player : Node2D
{
	[Export]
	int maxHoldMs = 80;
	[Export]
	int invincibleMs = 1000;
	[Export]
	ulong handAnimMs = 500;
	[Export]
	public int maxLives = 5;

	[Export]
	Texture2D FaceNormal;
	[Export]
	Texture2D FaceConcentrated;
	[Export]
	Texture2D FaceHurt;
	[Export]
	Texture2D FacePower;
	[Export]
	Texture2D FaceMiss;

	[Export]
	Texture2D BodyHuman;
	[Export]
	Texture2D BodyAI;


	[Export]
	string playerPrefix = "P1";
	Vector2 moveDir = Vector2.Zero;

	Vector2 lastGroundPos;
	Vector2 groundPos;
	float originalXScale;

	float jumpOffset = 0f;
	float jumpSpeed = 0f;

	bool isJumping = false;

	Node2D graphic;
	Area2D moveArea;
	Area2D feetArea;
	Area2D hurtArea;
	Area2D handArea;

	Sprite2D faceGraphic;
	Node2D handNode;
	Node2D handGraphic;

	Sprite2D body;
	private Sprite2D aiLamp;
	AnimatedSprite2D powerupIndicator;


	double hitPressTime = 0;
	double jumpPressTime = 0;

	double gotHurtTime = 0;
	double trySmashTime = 0;
	double smashTime = 0;
	private GlobalData globalData;
	public int lives = 0;
	public int smashes = 0;

	PowerupType currentPowerup;
	private AudioStreamPlayer2D audioPlayer;
	private AudioStreamWav gotHitSound;
	private AudioStreamWav pickupSound;
	private AudioStreamWav smashSound;
    private AudioStreamWav bounceSound;

	public bool IsJumping => isJumping;

    public bool IsPoweredUp()
	{
		return currentPowerup != PowerupType.None;
	}

	public override void _Ready()
	{
		globalData = GetNode<GlobalData>("/root/GlobalData");
		lives = maxLives;
		groundPos = Position;
		lastGroundPos = Position;
		originalXScale = Math.Abs(Scale.X);
		graphic = GetNode<Node2D>("PlayerGraphic");
		moveArea = GetNode<Area2D>($"/root/MainScene/Stage/{playerPrefix}Area");
		feetArea = GetNode<Area2D>($"FeetArea");
		body = graphic.GetNode<Sprite2D>($"Body");
		aiLamp = graphic.GetNode<Sprite2D>($"AILamp");
		hurtArea = graphic.GetNode<Area2D>($"Body/HurtArea");
		faceGraphic = graphic.GetNode<Sprite2D>("Face/FaceSprite");
		handNode = graphic.GetNode<Node2D>("Hands");
		handGraphic = handNode.GetNode<Node2D>("Hands");
		handArea = handNode.GetNode<Area2D>("HandArea");
		powerupIndicator = GetNode<AnimatedSprite2D>("PowerupIndicator");
		powerupIndicator.Play();

		audioPlayer = new AudioStreamPlayer2D();
		AddChild(audioPlayer);

		gotHitSound = (AudioStreamWav)ResourceLoader.Load("res://sounds/gotHit.wav");
		pickupSound = (AudioStreamWav)ResourceLoader.Load("res://sounds/pickup.wav");
		smashSound = (AudioStreamWav)ResourceLoader.Load("res://sounds/smash.wav");
		bounceSound = (AudioStreamWav)ResourceLoader.Load("res://sounds/bounce.wav");
	}

	private void ContinuesMovement()
	{
		if (GameInput.IsActionPressed(playerPrefix, "Left"))
			moveDir.X -= 1;
		if (GameInput.IsActionPressed(playerPrefix, "Right"))
			moveDir.X += 1;
		if (GameInput.IsActionPressed(playerPrefix, "Up"))
			moveDir.Y -= 1;
		if (GameInput.IsActionPressed(playerPrefix, "Down"))
			moveDir.Y += 1;
	}

	public override void _PhysicsProcess(double delta)
	{
		audioPlayer.VolumeDb = globalData.SoundEffectDB;

		var hitByBall = hurtArea.GetOverlappingAreas().Any(x => x.Name == "HitArea") && jumpOffset > -400;
		var hitByExplosion = hurtArea.GetOverlappingAreas().Any(x => x.Name == "BombArea") && jumpOffset > -130;

		if (hitByBall || hitByExplosion)
			GetHurt();

		var hitByFire = hurtArea.GetOverlappingAreas().Any(x => x.Name == "FireArea") && jumpOffset > -250;
		if (hitByFire)
		{
			GetHurt();
			var fire = hurtArea.GetOverlappingAreas().First(x => x.Name == "FireArea").GetParent();
			fire.QueueFree();
		}

		var powerupCollision = hurtArea.GetOverlappingAreas().FirstOrDefault(x => x.Name == "PowerupArea");
		if (powerupCollision != null && jumpOffset > -250)
		{
			var powerup = powerupCollision.GetParent() as Powerup;
			if (powerup != null)
			{
				currentPowerup = powerup.PowerupType;
				powerup.RemoveSelf();

				audioPlayer.Stream = pickupSound;
				audioPlayer.Play();
			}
		}

		UpdateFace();
		UpdateBody();
		UpdateHands();
		SpecialInput();

		if (moveDir.X > 0 && Scale.X > -originalXScale)
		{
			var scale = Scale;
			scale.X -= moveDir.X * (originalXScale / 2.5f);
			scale.X = Math.Max(scale.X, -originalXScale);
			Scale = scale;
		}
		if (moveDir.X < 0 && Scale.X < originalXScale)
		{
			var scale = Scale;
			scale.X -= moveDir.X * (originalXScale / 2.5f);
			scale.X = Math.Min(scale.X, originalXScale);
			Scale = scale;
		}

		var isHome = feetArea.GetOverlappingAreas().Contains(moveArea);
		if (!isHome)
		{
			groundPos = lastGroundPos;
			moveDir *= -0.5f;
		}
		else
		{
			lastGroundPos = groundPos;
		}

		var pos = groundPos;
		pos.X += moveDir.X * 10;
		pos.Y += moveDir.Y * 5;
		groundPos = pos;
		Position = groundPos;

		moveDir.X *= 0.93f;
		moveDir.Y *= 0.93f;

		if (moveDir.Length() < 0.2f)
		{
			ContinuesMovement();
		}

		jumpOffset -= jumpSpeed;

		if (jumpSpeed > 0 || (jumpSpeed <= 0 && jumpOffset < 0))
			jumpSpeed -= 5.0f;
		else if (jumpOffset > 0)
		{
			Land();
		}

		graphic.Position = new Vector2(0, jumpOffset);
	}

	private void UpdateFace()
	{
		if (IsInvincible())
			faceGraphic.Texture = FaceHurt;
		else if ((Time.GetTicksMsec() - handAnimMs) < smashTime)
			faceGraphic.Texture = FaceConcentrated;
		else if ((Time.GetTicksMsec() - handAnimMs) < trySmashTime)
			faceGraphic.Texture = FaceMiss;
		else if (currentPowerup != PowerupType.None)
			faceGraphic.Texture = FacePower;
		else
			faceGraphic.Texture = FaceNormal;
	}

	private void UpdateHands()
	{
		var isTryingSmash = (Time.GetTicksMsec() - handAnimMs) < trySmashTime;
		handNode.Visible = isTryingSmash;

		if (isTryingSmash)
		{
			var value = Math.Min((Time.GetTicksMsec() - trySmashTime) / 300, 1);
			var curveValue = EasingFunctions.InBounce(value);
			var pos = handGraphic.Position;
			pos.X = (float)(-150 * curveValue);
			handGraphic.Position = pos;
		}
	}

	private void UpdateBody()
	{
		aiLamp.Visible = !globalData.GetIsHuman(playerPrefix);

		if (globalData.GetIsHuman(playerPrefix))
		{
			body.Texture = BodyHuman;
		}
		else
		{
			body.Texture = BodyAI;
			var value = (Math.Sin((Time.GetTicksMsec() * 50) / 1000.0f) + 1) * 0.5f;
			aiLamp.Modulate = Colors.Black.Lerp(Colors.Red, (float)value);
		}

		powerupIndicator.Visible = currentPowerup != PowerupType.None;

		if (IsInvincible())
		{
			var value = (Math.Sin((Time.GetTicksMsec() - gotHurtTime) * 50 / 1000.0f) + 1) * 0.5f;
			var color = Colors.Red;
			color.R = (float)value;
			body.Modulate = color;
		}
		else if (currentPowerup != PowerupType.None)
		{
			var powerupColor = PowerupData.GetColor(currentPowerup);
			var value = (Math.Sin((Time.GetTicksMsec() - gotHurtTime) * 20 / 1000.0f) + 1) * 0.5f;

			body.Modulate = Colors.White.Lerp(powerupColor, (float)value);
			powerupIndicator.Modulate = powerupColor;

			handGraphic.Modulate = powerupColor;
		}
		else
		{
			body.Modulate = Colors.White;
			handGraphic.Modulate = Colors.White;
		}
	}

	private void GetHurt()
	{
		if (IsInvincible()) return;

		audioPlayer.Stream = gotHitSound;
		audioPlayer.Play();

		gotHurtTime = Time.GetTicksMsec();
		lives--;
	}

	private bool IsSmashing()
	{
		if (smashTime == 0) return false;
		return (Time.GetTicksMsec() - handAnimMs) < smashTime;
	}

	private bool IsInvincible()
	{
		if (gotHurtTime == 0) return false;
		return (Time.GetTicksMsec() - gotHurtTime) < invincibleMs;
	}

	private void Land()
	{
		isJumping = false;
		jumpOffset = 0;
		jumpSpeed = 0;
	}

	public void SpecialInput()
	{
		float boastSpeed = 1;
		if (isJumping)
			boastSpeed += 1;

		if (GameInput.IsActionJustPressed(playerPrefix, "Left"))
		{
			moveDir.X -= boastSpeed;
		}

		if (GameInput.IsActionJustPressed(playerPrefix, "Right"))
		{
			moveDir.X += boastSpeed;
		}

		if (GameInput.IsActionJustPressed(playerPrefix, "Up"))
		{
			moveDir.Y -= boastSpeed;
		}

		if (GameInput.IsActionJustPressed(playerPrefix, "Down"))
		{
			moveDir.Y += boastSpeed;
		}

		if (GameInput.IsActionJustPressed(playerPrefix, "Hit"))
			hitPressTime = Time.GetTicksMsec();

		if (GameInput.IsActionJustPressed(playerPrefix, "Jump") && !isJumping)
			jumpPressTime = Time.GetTicksMsec();

		if (hitPressTime != 0)
		{
			var timeDown = Time.GetTicksMsec() - hitPressTime;
			if (GameInput.IsActionJustReleased(playerPrefix, "Hit"))
			{
				DoSmash((float)(timeDown / maxHoldMs));
				hitPressTime = 0;
			}
		}

		if (jumpPressTime != 0)
		{
			var timeDown = Math.Min(Time.GetTicksMsec() - jumpPressTime, maxHoldMs);
			if (GameInput.IsActionJustReleased(playerPrefix, "Jump"))
			{
				isJumping = true;
				jumpSpeed = 90 * (float)(timeDown / maxHoldMs);
				moveDir.X *= 2;
				jumpPressTime = 0;

				audioPlayer.Stream = bounceSound;
				audioPlayer.Play();
			}
		}
	}

	private void DoSmash(float strength)
	{
		var overlapping = handArea.GetOverlappingAreas();
		var ballCollision = overlapping.FirstOrDefault((x) =>
		{
			return x.Name == "SmashArea";
		});


		trySmashTime = Time.GetTicksMsec();

		if (ballCollision == null)
			return;

		var ball = ballCollision.GetParent().GetParent() as RegularBall;
		ball.PerformSmash(strength, playerPrefix == "P1" ? "P2" : "P1");

		if (ball.TryApplyPowerup(currentPowerup, playerPrefix))
			currentPowerup = PowerupType.None;
		smashTime = Time.GetTicksMsec();
		smashes++;

		audioPlayer.Stream = smashSound;
		audioPlayer.Play();
	}

	public static float TicksSeconds => Time.GetTicksMsec() * 1000;
}
