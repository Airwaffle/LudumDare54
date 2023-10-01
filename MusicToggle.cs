using Godot;

public partial class MusicToggle : Node2D
{
    private Sprite2D onGraphic;
    private Sprite2D offGraphic;
    private RichTextLabel text;
    [Export]
    private string label;

    private bool isHovered = false;
    private GlobalData globalData;

    public override void _Ready()
    {
        globalData = GetNode<GlobalData>("/root/GlobalData");

        onGraphic = GetNode<Sprite2D>("GraphicOn");
        offGraphic = GetNode<Sprite2D>("GraphicOff");
        text = GetNode<RichTextLabel>("Text");

        text.Text = label;
        UpdateOnOff();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("UIClick") && isHovered)
        {
            if (label.ToLower().Contains("music"))
            {
                if (globalData.MusicDB == 0)
                    globalData.MusicDB = -80;
                else
                    globalData.MusicDB = 0;
            }
            else
            {
                if (globalData.SoundEffectDB == 0)
                    globalData.SoundEffectDB = -80;
                else
                    globalData.SoundEffectDB = 0;
            }
            UpdateOnOff();
        }
    }

    private void UpdateOnOff()
    {
        if (label.ToLower().Contains("music"))
        {
            onGraphic.Visible = globalData.MusicDB == 0;
            offGraphic.Visible = globalData.MusicDB != 0;
        }
        else
        {
            onGraphic.Visible = globalData.SoundEffectDB == 0;
            offGraphic.Visible = globalData.SoundEffectDB != 0;
        }
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
