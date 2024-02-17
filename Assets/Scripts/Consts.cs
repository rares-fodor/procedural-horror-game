using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Consts
{
    public static string EMPTY_STR = "";
    public static string MAIN_MENU_SCENE = "MainMenu";
    public static string MAIN_GAME_SCENE = "SampleScene";

    public static KeyCode INTERACT_KEY = KeyCode.E;
    public static string INTERACT_MESSAGE = "Hold " + INTERACT_KEY + " to collect";
    public static string DIALOGUE_MESSAGE = "Press " + INTERACT_KEY + " to talk";

    public static string NPC_NAME = "Old Tree";

    public static KeyCode SKIP_DIALOGUE_LINE_KEY = KeyCode.Space;

    public static KeyCode TELEPORT_KEY = KeyCode.T;
    public static KeyCode HINT_KEY = KeyCode.F;

    public static int PILLAR_COUNT = 7;
    public static int MAX_PLAYER_COUNT = 4;

    public static int PLAYER_MAX_HP = 2;
}
