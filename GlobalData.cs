using Godot;

public partial class GlobalData : Node
{
	public int Player1Wins = 0;
	public int Player2Wins = 0;

	public int SoundEffectDB = 0;
	public int MusicDB = 0;
    
    public bool Player1IsHuman = true;
    public bool Player2IsHuman = true;

    internal int GetScore(string playerPrefix)
    {
        if (playerPrefix == "P1") 
			return Player1Wins;
		return Player2Wins;
    }

    internal bool GetIsHuman(int player)
    {
        if (player == 1) return Player1IsHuman;
        return Player2IsHuman;
    }

    internal bool GetIsHuman(string playerPrefix)
    {
        if (playerPrefix == "P1") return Player1IsHuman;
        return Player2IsHuman;
    }

    internal void SetIsHuman(bool isHuman, int player)
    {
         if (player == 1) Player1IsHuman = isHuman;
         if (player == 2) Player2IsHuman = isHuman;
    }
}
