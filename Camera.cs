using Godot;
using System;

public partial class Camera : Camera2D
{
	[Export]
	public ulong animationInMs = 500;

	private ulong startTime;

	public override void _Ready()
	{
		startTime = Time.GetTicksMsec();
	}

	public override void _PhysicsProcess(double delta)
	{
		var time = Time.GetTicksMsec() - startTime;
		var value = 1 - ((animationInMs - time) / (float)animationInMs); 

		if (time < animationInMs)
		{
			var newZoom = 0.5f * EasingFunctions.InSine(value) + 0.5f;
			Zoom = new Vector2((float)newZoom, (float)newZoom);
		}
		else
		{
			Zoom = new Vector2(1, 1);
		}
	}
}
