using Godot;
using System;

public partial class Powerup : Node2D
{
	private PowerupType powerupType;
	private RandomNumberGenerator random;

	private Sprite2D graphic;

	public PowerupType PowerupType => powerupType;

	public override void _Ready()
	{
		random = new RandomNumberGenerator();
		powerupType = (PowerupType)random.RandiRange(1, 3);
		graphic = GetNode<Sprite2D>("Graphic");

		graphic.Modulate = PowerupData.GetColor(powerupType);

	}

	
	public override void _Process(double delta)
	{
	}
}
