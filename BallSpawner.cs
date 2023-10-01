using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BallSpawner : Node2D
{
	private ulong[] timeBetweenBallsMs = new[] {
		(ulong)(10 * 1000),
		(ulong)(20 * 1000),
		(ulong)(15 * 1000),
		(ulong)(15 * 1000),
		(ulong)(10 * 1000),
		(ulong)(10 * 1000),
		(ulong)(5 * 1000),
		};

	private int timeBetweenBallsIndex = 0;

	[Export]
	private Node2D[] spawnPoints;

	private PackedScene ballScene;
	private RegularBall startBall;
	public List<RegularBall> balls = new List<RegularBall>();

	private ulong lastTimeBall;

	public override void _Ready()
	{
		ballScene = (PackedScene)ResourceLoader.Load("res://regular_ball.tscn");

		startBall = GetNode<RegularBall>("/root/MainScene/RegularBall");
		startBall.SetSpawner(this);

		balls.Add(startBall);
	}

	public override void _Process(double delta)
	{
		if (startBall != null)
		{
			DoStartBallWait();
			return;
		}


		var allBallsPoweredUp = !balls.Any() || balls.All(x => x.IsPoweredUp());
		if (allBallsPoweredUp || Time.GetTicksMsec() > lastTimeBall + timeBetweenBallsMs[timeBetweenBallsIndex])
		{
			SpawnBall();
			lastTimeBall = Time.GetTicksMsec();
			timeBetweenBallsIndex++;
			timeBetweenBallsIndex = Math.Min(timeBetweenBallsMs.Length - 1, timeBetweenBallsIndex);
		}
	}

	public void RemoveBall(RegularBall ball)
	{
		balls.Remove(ball);
	}

	private void DoStartBallWait()
	{
		if (startBall?.IsMoving() == true)
		{
			startBall = null;
			lastTimeBall = Time.GetTicksMsec();
		}
	}

	public void SpawnBall()
	{
		var slot = GetBallPosition();
		if (slot == null)
			return;

		var ball = (RegularBall)ballScene.Instantiate();
		AddChild(ball);

		balls.Add(ball);
		ball.SetSpawner(this);
		ball.GlobalPosition = slot.Value;
	}

	private Vector2? GetBallPosition()
	{
		Node2D spawnPointSelected = null;
		foreach (var spawnPoint in spawnPoints)
		{
			if (balls.Any(x => (x.GlobalPosition - spawnPoint.GlobalPosition).Length() < 1))
				continue; // Slot occupied

			spawnPointSelected = spawnPoint;
			break;
		}
		return spawnPointSelected?.GlobalPosition;
	}

}
