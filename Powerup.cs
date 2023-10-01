using System;
using Godot;

public partial class Powerup : Node2D
{
	private PowerupType powerupType;

	private Sprite2D graphic;
	private PowerupSpawner spawner;

	public PowerupType PowerupType => powerupType;

	public override void _Ready()
	{
		graphic = GetNode<Sprite2D>("Graphic");
	}

	public void RemoveSelf()
	{
		Visible = false;
		QueueFree();
		spawner?.RemovePowerup(this);
	}

	internal void SetSpawner(PowerupSpawner powerupSpawner, PowerupType powerup)
	{
		spawner = powerupSpawner;
		powerupType = powerup;
		graphic.Modulate = PowerupData.GetColor(powerupType);
	}
}
