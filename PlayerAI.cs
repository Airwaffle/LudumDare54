using Godot;
using System;
using System.Linq;

public partial class PlayerAI : Node
{
	private const ulong holdButtonMs = 70;

	[Export]
	public string PlayerPrefix;

	private Player player;
	private BallSpawner ballSpawner;
    private PowerupSpawner powerupSpawner;
    private string currentDirection = "Down";
	private Vector2 wishedDirection = Vector2.Down;

	private bool jumped = false;
	private bool jumpPressRegistered = false;


	private bool smashed = false;
	private bool smashPressRegistered = false;

	private ulong smashTime;
	private ulong jumpTime;

	private double awaitStartTimeMS = 3000;
	private double nextJumpMs = 400;
	private RandomNumberGenerator random;



	public override void _Ready()
	{
		player = GetNode<Player>($"/root/MainScene/{PlayerPrefix}");
		ballSpawner = GetNode<BallSpawner>($"/root/MainScene/Stage/BallSpawner");
		powerupSpawner = GetNode<PowerupSpawner>($"/root/MainScene/Stage/PowerupSpawner{PlayerPrefix}");
		random = new RandomNumberGenerator();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!ballSpawner.balls.First().IsMoving())
		{
			DoStartMovement(delta);
		}
		else if (ballSpawner.balls.Any(x => x.currentPowerup == PowerupType.Homing))
		{
			DoHomingMovement(ballSpawner.balls.First(x => x.currentPowerup == PowerupType.Homing));
			DoRandomJumps(delta);
		}
		else if (powerupSpawner.Powerups.Any()) 
		{
			DoPowerupMovement(powerupSpawner.Powerups.First());
		}
		else
		{
			DoNormalMovement();
			DoRandomJumps(delta);
		}

		SetWishedDirection();
	}


	private void DoRandomJumps(double delta) 
	{
		if (player.IsPoweredUp()) return;
		if (nextJumpMs > 0) {
			nextJumpMs -= delta * 1000;
			return;
		}

		if (!jumped && !player.IsJumping)
		{
			jumped = true;
			jumpTime = Time.GetTicksMsec();
			nextJumpMs = random.RandiRange(700, 1600);
		}
	}

    private void DoPowerupMovement(Powerup powerup)
    {
		// Walk to powerup!!
        var dirToBall = powerup.GlobalPosition - player.GlobalPosition;
		wishedDirection = dirToBall.Normalized();
    }

    private void DoHomingMovement(RegularBall homingBall)
	{
		// Walk away!!
		var dirToBall = player.GlobalPosition - homingBall.GlobalPosition;
		wishedDirection = dirToBall.Normalized();
	}

	private void DoStartMovement(double delta)
	{
		awaitStartTimeMS -= delta * 1000;
		if (awaitStartTimeMS > 0)
		{
			if (!jumped && !player.IsJumping)
			{
				wishedDirection = Vector2.Zero;
				jumped = true;
				jumpTime = Time.GetTicksMsec();
			}

			return;
		}

		var dirToBall = player.GlobalPosition - ballSpawner.balls.First().GlobalPosition;
		wishedDirection = (dirToBall * -1).Normalized();

		if (dirToBall.Length() < 70 && !smashed)
		{
			wishedDirection = Vector2.Zero;
			smashed = true;
			smashTime = Time.GetTicksMsec();
		}
	}


	private void DoNormalMovement()
	{
		var dirToBall = player.GlobalPosition - ballSpawner.balls.First().GlobalPosition;

		if (dirToBall.Length() > 90)
		{
			wishedDirection = (dirToBall * -1).Normalized();
		}
		else
		{
			wishedDirection = dirToBall.Normalized();
		}

		if (dirToBall.Length() < 90 && !smashed)
		{
			wishedDirection = Vector2.Zero;
			smashed = true;
			smashTime = Time.GetTicksMsec();
		}
	}

	private void SetWishedDirection()
	{
		if (wishedDirection == Vector2.Zero)
		{
			currentDirection = "None";
			return;
		}

		if (Math.Abs(wishedDirection.Y) > Math.Abs(wishedDirection.X))
		{
			if (wishedDirection.Y > 0)
				currentDirection = "Down";
			else
				currentDirection = "Up";
		}
		else
		{
			if (wishedDirection.X > 0)
				currentDirection = "Right";
			else
				currentDirection = "Left";
		}
	}

	internal bool IsActionJustPressed(string action)
	{
		if (action == "Hit" && smashed && !smashPressRegistered)
		{
			smashPressRegistered = true;
			return true;
		}
		if (action == "Jump" && jumped && !jumpPressRegistered)
		{
			jumpPressRegistered = true;
			return true;
		}
		return false;
	}

	internal bool IsActionJustReleased(string action)
	{
		if (action == "Hit" && smashed && Time.GetTicksMsec() > smashTime + holdButtonMs)
		{
			smashPressRegistered = false;
			smashed = false;
			return true;
		}
		if (action == "Jump" && jumped && Time.GetTicksMsec() > jumpTime + holdButtonMs)
		{
			jumpPressRegistered = false;
			jumped = false;
			return true;
		}
		return false;
	}

	internal bool IsActionPressed(string action)
	{
		if (action == currentDirection)
		{
			return true;
		}

		return false;
	}

}
