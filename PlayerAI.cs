using Godot;
using System;
using System.Linq;

public partial class PlayerAI : Node
{
	[Export]
	public string PlayerPrefix;

	private Player player;
	private BallSpawner ballSpawner;

	private string currentDirection = "Down";
	private Vector2 wishedDirection = Vector2.Down;

	private bool jumped = false;

	private bool smashed = false;

	public override void _Ready()
	{
		player = GetNode<Player>($"/root/MainScene/{PlayerPrefix}");
		ballSpawner = GetNode<BallSpawner>($"/root/MainScene/Stage/BallSpawner");
	}

	public override void _Process(double delta)
	{
		var dirToBall = player.GlobalPosition - ballSpawner.balls.First().GlobalPosition;

		if (dirToBall.Length() > 300) {
		wishedDirection = (dirToBall * -1).Normalized();

		} 
		else
		{
		wishedDirection = dirToBall.Normalized();

		}

		SetWishedDirection();
	}

	private void SetWishedDirection()
	{
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
		if (action == "Hit" && smashed)
		{
			return true;
		}
		if (action == "Jump" && jumped)
		{
			return true;
		}
		return false;
	}

	internal bool IsActionJustReleased(string action)
	{
		if (action == "Hit" && smashed)
		{
			smashed = false;
			return true;
		}
		if (action == "Jump" && jumped)
		{
			smashed = false;
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
