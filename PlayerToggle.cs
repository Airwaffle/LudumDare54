using Godot;

public partial class PlayerToggle : Node2D
{
	private Sprite2D onGraphic;
	private Sprite2D offGraphic;
	private Main main;
    private RichTextLabel text;
    [Export]
	private int player;

	private bool isHovered = false;
    private GlobalData globalData;
    private bool isHuman = true;


	public override void _Ready()
	{
		globalData = GetNode<GlobalData>("/root/GlobalData");
		isHuman = globalData.GetIsHuman(player);

		onGraphic = GetNode<Sprite2D>("GraphicOn");
		offGraphic = GetNode<Sprite2D>("GraphicOff");
		main = GetNode<Main>("/root/MainScene");
		text = GetNode<RichTextLabel>("Text");
		UpdateOnOff();
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("UIClick") && isHovered)
		{
			main.TogglePlayerAI(player);
			isHuman = !isHuman;
			globalData.SetIsHuman(isHuman, player);
			UpdateOnOff();
		}
	}

    private void UpdateOnOff()
    {
        onGraphic.Visible = isHuman;
		offGraphic.Visible = !isHuman;
		text.Text = isHuman ? "HUMAN" : "AI";
    }

    public void _on_area_2d_mouse_entered()
	{
		isHovered = true;
	}

	public void _on_area_2d_mouse_exited()
	{
		isHovered = false;
	}
}
