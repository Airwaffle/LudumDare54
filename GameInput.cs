using System;
using System.Collections.Generic;
using Godot;

public static class GameInput
{
    private static Dictionary<string, PlayerAI> playerAIs = new Dictionary<string, PlayerAI>(); 

    internal static bool IsActionJustPressed(string prefix, string action)
    {
        if (playerAIs.ContainsKey(prefix)) 
            return playerAIs[prefix].IsActionJustPressed(action);
        return Input.IsActionJustPressed(prefix + action);
    }

    internal static bool IsActionJustReleased(string prefix, string action)
    {
         if (playerAIs.ContainsKey(prefix)) 
            return playerAIs[prefix].IsActionJustReleased(action);
        return Input.IsActionJustReleased(prefix + action);
    }

    internal static bool IsActionPressed(string prefix, string action)
    {
         if (playerAIs.ContainsKey(prefix)) 
            return playerAIs[prefix].IsActionPressed(action);
        return Input.IsActionPressed(prefix + action);
    }

    internal static void RegisterAI(PlayerAI playerAI, string playerPrefix)
    {
        playerAIs.Add(playerPrefix, playerAI);
    }

    internal static void UnregisterAI(string playerPrefix)
    {
        playerAIs.Remove(playerPrefix);
    }
}