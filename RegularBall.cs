using Godot;
using System;
using System.Linq;

public partial class RegularBall : Node2D
{
	[Export]
	float bounceHeight = 25;

	Vector2 moveDir = Vector2.Zero;
	Vector2 groundPos;


	float jumpOffset = 0f;
	float jumpSpeed = 0f;

	private RandomNumberGenerator random;

	private Node2D graphic;
	private Area2D moveArea;
	private Area2D hitArea;
	private Area2D smashArea;

	private PowerupType currentPowerup = PowerupType.None;

	private float originalYScale;

	public bool IsMoving() {
		return moveDir != Vector2.Zero;
	}


	public override void _Ready()
	{
		originalYScale = Scale.Y;
		graphic = GetNode<Node2D>("Graphic");
		moveArea = GetNode<Area2D>("/root/MainScene/Stage/BallArea");
		hitArea = graphic.GetNode<Area2D>("HitArea");
		smashArea = graphic.GetNode<Area2D>("SmashArea");

		groundPos = Position;
		random = new RandomNumberGenerator();
		//SetRandomDirection();
		jumpSpeed = bounceHeight;

		bounceHeight = random.RandfRange(25, 40);

	}

	public bool TryApplyPowerup(PowerupType powerupTypes) 
	{
		if (currentPowerup != PowerupType.None)
			return false;

		currentPowerup = powerupTypes;
		graphic.Modulate = PowerupData.GetColor(powerupTypes);
		return true;
	}

	public void PerformSmash(float strength, string aimedPlayer)
	{
		var player  = GetNode<Node2D>($"/root/MainScene/{aimedPlayer}");

		

		var newDir = player.GlobalPosition - GlobalPosition;
		//newDir.Y *= 0.5f;
		moveDir = newDir.Normalized() * 2 * strength;

		// if (GlobalPosition.X > orginPosition.X)
		// {

		// }
		// else
		// {
		// 	var newDir = handCollision.GlobalPosition - GlobalPosition;
		// 	newDir.Y *= 0.5f;
		// 	moveDir = newDir.Normalized();
		// }
	}

	public override void _PhysicsProcess(double delta)
	{
		var overlapping = smashArea.GetOverlappingAreas();
		var isHome = overlapping.Contains(moveArea);

		var handCollision = overlapping.FirstOrDefault((x) =>
		{
			return x.Name == "HandArea";
		});


		if (!isHome)
		{
			moveDir *= -1;

			var collisionVector = GlobalPosition - (moveArea.GetChild(0) as Node2D).GlobalPosition;
			collisionVector = collisionVector.Normalized();
			var cross = collisionVector.Cross(moveDir);
			var dot = collisionVector.Dot(moveDir);

			// if (collisionVector.X > collisionVector.Y)
			// 	moveDir.X *= -1;
			// else 
			// 	moveDir.Y *= -1;

			// GD.Print($"Colliding with vector {collisionVector}");
			// GD.Print($"Colliding with cross {cross}");
			// GD.Print($"Colliding with dot {dot}");
		}

		var pos = groundPos;
		pos.X += moveDir.X * 10;
		pos.Y += moveDir.Y * 5;

		jumpOffset -= jumpSpeed;

		if (jumpSpeed > 0 || (jumpSpeed <= 0 && jumpOffset < 0))
		{
			jumpSpeed -= 2;
			Scale = new Vector2(Scale.X, originalYScale);
		}
		else if (jumpOffset > 0)
		{
			jumpSpeed = bounceHeight;
			Scale = new Vector2(Scale.X, originalYScale * 0.7f);
		}

		groundPos = pos;
		Position = groundPos;

		graphic.Position = new Vector2(0, jumpOffset);

		moveDir.X *= 0.99f;

	}

	private void SetRandomDirection()
	{
		moveDir.X = random.RandfRange(-1, 1);
		moveDir.Y = random.RandfRange(-0.5f, 0.5f);
		moveDir = moveDir.Normalized();
	}
}
