using Godot;
using System.Linq;

public partial class MusicPlayer : AudioStreamPlayer2D
{
    private GlobalData globalData;

    private Player player1;
    private Player player2;
    private BallSpawner ballSpawner;

    public override void _Ready()
	{
		globalData = GetNode<GlobalData>("/root/GlobalData");
		
		player1 = GetNode<Player>("/root/MainScene/P1");
		player2 = GetNode<Player>("/root/MainScene/P2");
		ballSpawner = GetNode<BallSpawner>("/root/MainScene/Stage/BallSpawner");
	}

	public override void _Process(double delta)
	{
		VolumeDb = globalData.MusicDB;

		float pitch = 1;

		var bomb = ballSpawner.balls.FirstOrDefault(x => x.currentPowerup == PowerupType.Bomb);

		if (bomb != null) 
		{
			var bombTime = bomb.GetPowerupTime();
			pitch = 1 - bombTime * 0.2f;
		} else if (player1.IsPoweredUp() || player2.IsPoweredUp()) {
			pitch = 1.3f;
		}

		if (pitch <= 0) pitch = 0.001f;
		PitchScale = pitch;
	}
}
