using System;
using Godot;

public partial class Powerup : Node2D
{
	private PowerupType powerupType;
	private RandomNumberGenerator random;

	private Sprite2D graphic;
	private PowerupSpawner spawner;

	public PowerupType PowerupType => powerupType;

	public override void _Ready()
	{
		random = new RandomNumberGenerator();
		powerupType = (PowerupType)random.RandiRange(1, Enum.GetValues(typeof(PowerupType)).Length - 1);
		graphic = GetNode<Sprite2D>("Graphic");
		graphic.Modulate = PowerupData.GetColor(powerupType);
	}


	public void RemoveSelf()
	{
		Visible = false;
		QueueFree();
		spawner?.RemovePowerup(this);
	}

	internal void SetSpawner(PowerupSpawner powerupSpawner)
	{
		spawner = powerupSpawner;
	}

}
