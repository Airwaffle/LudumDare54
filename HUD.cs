using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class HUD : Node2D
{
	[Export]
	private string playerPrefix;
	private Player player;

    private List<HUDHeart> hearts = new List<HUDHeart>();

	private int lastTimeHearts;

	public override void _Ready()
	{
		player = GetNode<Player>($"/root/MainScene/{playerPrefix}");

		var heartScene = (PackedScene)ResourceLoader.Load("res://hud_heart.tscn");
		for (int i = 0; i < player.maxLives; i++)
		{
			var heart = (HUDHeart)heartScene.Instantiate();
			AddChild(heart);
			heart.Position = new Vector2(i * 200, 0);
			hearts.Add(heart);
		}

		lastTimeHearts = player.maxLives;
	}

	public override void _Process(double delta)
	{
		if (!hearts.Any())
			return;

		if (player.lives < lastTimeHearts) {
			var heart = hearts.Last();
			heart.Pop();
			hearts.Remove(heart);
			lastTimeHearts = player.lives;
		}
	}
}
