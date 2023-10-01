using Godot;
using System;

public partial class MoveUpAndDown : Sprite2D
{
	[Export]
	private float intentsity = 9;
	[Export]
	private float length = 15;

    private float originalY;

    public override void _Ready()
	{
		originalY = Position.Y;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var newY = originalY + (float)Math.Sin(intentsity * Time.GetTicksMsec() / 1000.0f) * length;
		Position = new Vector2(Position.X, newY);
	}
}
