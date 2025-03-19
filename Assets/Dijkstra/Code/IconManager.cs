using UnityEditor;
using UnityEngine;


public class IconManager
{
    public enum LabelIcon
    {
        Gray = 0,
        Blue,
        Teal,
        Green,
        Yellow,
        Orange,
        Red,
        Magenta
    }


    public enum Icon
    {
        CircleGray = 0,
        CircleBlue,
        CircleTeal,
        CircleGreen,
        CircleYellow,
        CircleOrange,
        CircleRed,
        CircleMagenta,
        DiamondGray,
        DiamondBlue,
        DiamondTeal,
        DiamondGreen,
        DiamondYellow,
        DiamondOrange,
        DiamondRed,
        DiamondMagenta
    }


    public static void SetIcon(GameObject gObj, LabelIcon icon)
    {
        Texture2D iconAsTexture = EditorGUIUtility.IconContent($"sv_label_{(int)icon}").image as Texture2D;
        SetIcon(gObj, iconAsTexture);
    }


    public static void SetIcon(GameObject gObj, Icon icon)
    {
        Texture2D iconAsTexture = EditorGUIUtility.IconContent($"sv_icon_dot{(int)icon}_pix16_gizmo").image as Texture2D;
        SetIcon(gObj, iconAsTexture);
    }


    private static void SetIcon(GameObject gObj, Texture2D texture)
    {
        EditorGUIUtility.SetIconForObject(gObj, texture);
    }
}