using Godot;

public enum PowerupType
{
    None,
    Bomb,
    Homing,
    Fire,
}

public static class PowerupData
{
    public static Color GetColor(PowerupType type) {
        switch (type) {
            case PowerupType.Bomb:
                return Colors.RoyalBlue;
            case PowerupType.Homing:
                return Colors.GreenYellow;
            case PowerupType.Fire:
                return Colors.Orange;
        }
        return Colors.White;
        
    }
}