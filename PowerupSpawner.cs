using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PowerupSpawner : Node2D
{
	const int smashesRequired = 4;

	[Export]
	private string playerPrefix;
	[Export]
	private Node2D[] spawnPoints;

	private int lastTimeSmashes = 0;
	private Player player;
	private PackedScene powerupScene;
	private List<Powerup> existingPowerups = new List<Powerup>();
	private RandomNumberGenerator random;
	private PowerupType lastSpawned = PowerupType.None;

	public override void _Ready()
	{
		random = new RandomNumberGenerator();
		player = GetNode<Player>($"/root/MainScene/{playerPrefix}");
		powerupScene = (PackedScene)ResourceLoader.Load("res://power_up.tscn");
	}

	public override void _Process(double delta)
	{
		if (Math.Floor(lastTimeSmashes / (float)smashesRequired) !=
		Math.Floor(player.smashes / (float)smashesRequired))
		{
			SpawnPowerup();
			lastTimeSmashes = player.smashes;
		}
	}

	public void RemovePowerup(Powerup powerup)
	{
		existingPowerups.Remove(powerup);
	}

	private void SpawnPowerup()
	{
		var slot = GetPowerupPosition();
		if (slot == null)
			return;

		var powerup = (Powerup)powerupScene.Instantiate();
		AddChild(powerup);


		var powerupType = PowerupType.None;
		while (powerupType == PowerupType.None || powerupType == lastSpawned) 
			powerupType = (PowerupType)random.RandiRange(1, Enum.GetValues(typeof(PowerupType)).Length - 1);
		
		existingPowerups.Add(powerup);
		powerup.SetSpawner(this, powerupType);
		powerup.GlobalPosition = slot.Value;

		lastSpawned = powerupType;
	}

	private Vector2? GetPowerupPosition()
	{
		float longestDistance = 0;
		Node2D spawnPointSelected = null;
		foreach (var spawnPoint in spawnPoints)
		{
			if (existingPowerups.Any(x => (x.GlobalPosition - spawnPoint.GlobalPosition).Length() < 1))
				continue; // Slot occupied

			var distance = (player.GlobalPosition - spawnPoint.GlobalPosition).Length();
			if (distance > longestDistance)
			{
				longestDistance = distance;
				spawnPointSelected = spawnPoint;
			}
		}
		return spawnPointSelected?.GlobalPosition;
	}
}
