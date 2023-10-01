using Godot;
using System;

public partial class OverallScore : Node2D
{
	[Export]
	private string playerPrefix;
	private GlobalData globalVariables;
	private RichTextLabel overallScoreLabel;

	public override void _Ready()
	{
		globalVariables = GetNode<GlobalData>("/root/GlobalData");
		overallScoreLabel = GetNode<RichTextLabel>("Text");

		var score = globalVariables.GetScore(playerPrefix);
		
		if (score < 10)
			overallScoreLabel.Text = "0" + score.ToString();
		else
			overallScoreLabel.Text = score.ToString();

		//if (score == 0)	overallScoreLabel.Visible = false;

	}

}
