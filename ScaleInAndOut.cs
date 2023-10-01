using Godot;
using System;

public partial class ScaleInAndOut : Sprite2D
{
	[Export]
	private float intentsity = 9;
	[Export]
	private float length = 1;

    private float originalScale;

    public override void _Ready()
	{
		originalScale = Scale.Y;
	}

	public override void _Process(double delta)
	{
		var newScale = originalScale + (float)Math.Sin(intentsity * Time.GetTicksMsec() / 1000.0f) * length;
		Scale = new Vector2(newScale, newScale);
	}
}
