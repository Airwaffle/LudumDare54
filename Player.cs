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
	int maxLives = 5;

	[Export]
	Texture2D FaceNormal;
	[Export]
	Texture2D FaceConcentrated;
	[Export]
	Texture2D FaceHurt;
	[Export]
	Texture2D FacePower;


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
	AnimatedSprite2D powerupIndicator;


	double hitPressTime = 0;
	double jumpPressTime = 0;

	double gotHurtTime = 0;
	double trySmashTime = 0;
	double smashTime = 0;
	int lives = 0;

	PowerupType currentPowerup;

	public override void _Ready()
	{
		lives = maxLives;
		groundPos = Position;
		lastGroundPos = Position;
		originalXScale = Math.Abs(Scale.X);
		graphic = GetNode<Node2D>("PlayerGraphic");
		moveArea = GetNode<Area2D>($"/root/MainScene/Stage/{playerPrefix}Area");
		feetArea = GetNode<Area2D>($"FeetArea");
		body = graphic.GetNode<Sprite2D>($"Body");
		hurtArea = graphic.GetNode<Area2D>($"Body/HurtArea");
		faceGraphic = graphic.GetNode<Sprite2D>("Face/FaceSprite");
		handNode = graphic.GetNode<Node2D>("Hands");
		handGraphic = handNode.GetNode<Node2D>("Hands");
		handArea = handNode.GetNode<Area2D>("HandArea");
		powerupIndicator = GetNode<AnimatedSprite2D>("PowerupIndicator");
		powerupIndicator.Play();
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
		var isHurting = hurtArea.GetOverlappingAreas().Any(x => x.Name == "HitArea");
		if (isHurting)
		{
			GetHurt();
		}

		var powerupCollision = hurtArea.GetOverlappingAreas().FirstOrDefault(x => x.Name == "PowerupArea");
		if (powerupCollision != null)
		{
			var powerup = powerupCollision.FindParent("Powerup") as Powerup;
			currentPowerup = powerup.PowerupType;
			powerup.QueueFree();
		}

		UpdateFace();
		UpdateBodyColor();
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

	private void UpdateBodyColor()
	{
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
		}
		else
		{
			body.Modulate = Colors.White;
		}
	}

	private void GetHurt()
	{
		if (IsInvincible()) return;

		gotHurtTime = Time.GetTicksMsec();
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

		if (GameInput.IsActionJustPressed(playerPrefix,  "Left"))
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

		if (GameInput.IsActionJustPressed(playerPrefix,"Hit"))
			hitPressTime = Time.GetTicksMsec();

		if (GameInput.IsActionJustPressed(playerPrefix, "Jump") && !isJumping)
			jumpPressTime = Time.GetTicksMsec();

		if (hitPressTime != 0)
		{
			var timeDown = Time.GetTicksMsec() - hitPressTime;
			if (GameInput.IsActionJustReleased(playerPrefix, "Hit"))
			{
				GD.Print($"hit time down {timeDown}");
				DoSmash((float)(timeDown / maxHoldMs));
				hitPressTime = 0;
			}
		}

		if (jumpPressTime != 0)
		{
			var timeDown = Math.Min(Time.GetTicksMsec() - jumpPressTime, maxHoldMs);
			if (GameInput.IsActionJustReleased(playerPrefix, "Jump"))
			{
				GD.Print($"jump time down {timeDown}");
				isJumping = true;
				jumpSpeed = 90 * (float)(timeDown / maxHoldMs);
				moveDir.X *= 2;
				jumpPressTime = 0;
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

		var ball = ballCollision.FindParent("RegularBall") as RegularBall;
		ball.PerformSmash(strength, playerPrefix == "P1" ? "P2" : "P1");

		if (ball.TryApplyPowerup(currentPowerup))
			currentPowerup = PowerupType.None;
		smashTime = Time.GetTicksMsec();
	}

	public static float TicksSeconds => Time.GetTicksMsec() * 1000;
}
