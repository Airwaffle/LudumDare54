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
		instructions = GetNode<Node2D>("Instructions"); 
		var animation = instructions.GetNode<AnimatedSprite2D>("SmashInstructions");
		firstBall = GetNode<RegularBall>("RegularBall");

		playerAIs.Add(null);
		playerAIs.Add(null);

		animation.Play();
	}

	public override void _Process(double delta)
	{
		if (firstBall.IsMoving()) {
			instructions.Visible = false;
		}
	}

	public void TogglePlayerAI(int player) 
	{
		var index = player - 1;
		if (playerAIs[index] == null) {
			playerAIs[index] = new PlayerAI();
			playerAIs[index].PlayerPrefix = $"P{player}";
			GameInput.RegisterAI(playerAIs[index], $"P{player}");
			AddChild(playerAIs[index]);
		} else if (playerAIs[index] != null) {
			GameInput.UnregisterAI($"P{player}");
			playerAIs[index].QueueFree();
			playerAIs[index] = null;
		}
	}
}
