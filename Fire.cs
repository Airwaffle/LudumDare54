using Godot;
using System;

public partial class Fire : Node2D
{
    private AnimatedSprite2D animation;

    public override void _Ready()
	{
		var color = PowerupData.GetColor(PowerupType.Fire);
		animation = GetNode<AnimatedSprite2D>("Animation");
		animation.Play();
		animation.Modulate = color;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
