using Godot;
using System;

public partial class HUDHeart : Node2D
{
	private const float popDelay = 1000 * 3 / 10.0f; 

    private Sprite2D normalGraphic;
    private AnimatedSprite2D popAnimation;
    private Sprite2D background;

    private ulong destructionTime;

    public override void _Ready()
	{
		normalGraphic = GetNode<Sprite2D>("Normal");
		popAnimation = GetNode<AnimatedSprite2D>("Pop");
		background = GetNode<Sprite2D>("Background");

		popAnimation.Visible = false;
		background.Visible = false;
	}

    public override void _Process(double delta)
    {
        if (destructionTime == 0) 
		return;

		if (Time.GetTicksMsec() > destructionTime + popDelay) {
			popAnimation.Visible = false;
			background.Visible = true;
		}
    }

    public void Pop() {
		popAnimation.Visible = true;
		normalGraphic.Visible = false;
		popAnimation.Play();
		destructionTime = Time.GetTicksMsec();
	}
}
