using Godot;

public enum PowerupType
{
    None,
    Bomb,
    Multiplier,
    Homing,
}

public static class PowerupData
{
    public static Color GetColor(PowerupType type) {
        switch (type) {
            case PowerupType.Bomb:
                return Colors.AliceBlue;
            case PowerupType.Homing:
                return Colors.GreenYellow;
            case PowerupType.Multiplier:
                return Colors.Purple;
        }
        return Colors.White;
        
    }
}