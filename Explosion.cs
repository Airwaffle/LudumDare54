using Godot;

public partial class Explosion : Node2D
{
    private AudioStreamPlayer2D audioPlayer;
    private GlobalData globalData;


    public override void _Ready()
	{
		globalData =  GetNode<GlobalData>("/root/GlobalData");

		var animation = GetNode<AnimatedSprite2D>("Animation");
		animation.Play();

		audioPlayer = new AudioStreamPlayer2D();
		AddChild(audioPlayer);

		var explosionSound = (AudioStreamWav)ResourceLoader.Load("res://sounds/explosion.wav");
		audioPlayer.Stream = explosionSound;
		audioPlayer.Play();
	}


    public override void _Process(double delta)
    {
    		audioPlayer.VolumeDb = globalData.SoundEffectDB;
    }

    private void _on_animation_animation_finished()
	{
		GD.Print("_on_animation_animation_finished");
		QueueFree();
	}
}
