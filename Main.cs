using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
	private Node2D instructions;
	private RegularBall firstBall;

	private List<PlayerAI> playerAIs = new List<PlayerAI>();

	public override void _Ready()
	{
		var globalData = GetNode<GlobalData>("/root/GlobalData");
		instructions = GetNode<Node2D>("Instructions");
		var animation = instructions.GetNode<AnimatedSprite2D>("SmashInstructions");
		firstBall = GetNode<RegularBall>("RegularBall");

		playerAIs.Add(null);
		playerAIs.Add(null);

		animation.Play();

		if (!globalData.GetIsHuman(1))
		{
			TogglePlayerAI(1);
		}
		if (!globalData.GetIsHuman(2))
		{
			TogglePlayerAI(2);
		}
	}

	public override void _Process(double delta)
	{
		if (firstBall.IsMoving())
		{
			instructions.Visible = false;
		}
	}

	public void TogglePlayerAI(int player)
	{
		var index = player - 1;
		if (playerAIs[index] == null)
		{
			playerAIs[index] = new PlayerAI();
			playerAIs[index].PlayerPrefix = $"P{player}";
			GameInput.RegisterAI(playerAIs[index], $"P{player}");
			AddChild(playerAIs[index]);
		}
		else if (playerAIs[index] != null)
		{
			GameInput.UnregisterAI($"P{player}");
			playerAIs[index].QueueFree();
			playerAIs[index] = null;
		}
	}

	public void ReloadScene()
	{
		if (playerAIs[0] != null)
		{
			GameInput.UnregisterAI($"P1");
			playerAIs[0].QueueFree();
			playerAIs[0] = null;
		}
		if (playerAIs[1] != null)
		{
			GameInput.UnregisterAI($"P2");
			playerAIs[1].QueueFree();
			playerAIs[1] = null;
		}

		GetTree().ReloadCurrentScene();

	}
}
