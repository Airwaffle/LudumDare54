using Godot;
using System;

public partial class WinScreen : CanvasLayer
{
    [Export]
    private ulong countdownTimeMs = 2000;
    private Main main;
    private Player player1;
    private Player player2;
    private RichTextLabel text;
    private GlobalData globalVariables;
    private ulong countdownTime;

    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        Visible = false;
        main = GetNode<Main>($"/root/MainScene");
        player1 = main.GetNode<Player>($"P1");
        player2 = main.GetNode<Player>($"P2");
        text = GetNode<RichTextLabel>($"Text");
        globalVariables = GetNode<GlobalData>("/root/GlobalData");

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (!GameIsOver())
            return;

        if (Visible == false)
        {
            var playerWon = player2.lives <= 0 ? "1" : "2";
            var winText = $"[shake rate=20.0 level=50 connected=1]Player {playerWon} won![/shake]";
            text.Text = winText;
            countdownTime = Time.GetTicksMsec();

            if (player2.lives <= 0)
                globalVariables.Player1Wins++;
            else
                globalVariables.Player2Wins++;

        }
        Visible = true;


        if (countdownTime + countdownTimeMs <= Time.GetTicksMsec())
        {
            main.ReloadScene();
        }
    }

    private bool GameIsOver()
    {
        return player2.lives <= 0 || player1.lives <= 0;
    }


    public override void _Input(InputEvent @event)
    {
        //if (!GameIsOver())
        //    return;

        //if (@event.ac)

        //GetTree().ReloadCurrentScene();
    }
}
