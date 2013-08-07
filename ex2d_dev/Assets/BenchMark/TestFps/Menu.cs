using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {
    const int buttonWidth = 450;

    public static bool setted = false;

    public static float count = 1000;
    public static bool enableAni = true;
    public static float showhide;
    public static float stopmove;
    public static float param4;

    public GUIStyle scrollStyle;
    public GUIStyle textStyle;
    public string[] sceneNameList;

    private Vector2 scrollPos;
    private GUIStyle btnStyle;
    private GUIStyle toggleStyle;
    private GUIStyle textFieldStyle;

	// Use this for initialization
	void Start () {
        setted = true;
	}
    void ApplyTextStyle (GUIStyle _style) {
        _style.font = textStyle.font;
        _style.fontSize = textStyle.fontSize;
        _style.fontStyle = textStyle.fontStyle;
        _style.normal.textColor = textStyle.normal.textColor;
        _style.hover.textColor = textStyle.hover.textColor;
        _style.active.textColor = textStyle.active.textColor;
        _style.focused.textColor = textStyle.focused.textColor;
        _style.onNormal.textColor = textStyle.onNormal.textColor;
        _style.onHover.textColor = textStyle.onHover.textColor;
        _style.onActive.textColor = textStyle.onActive.textColor;
        _style.onFocused.textColor = textStyle.onFocused.textColor;
    }
    // Update is called once per frame
    void OnGUI () {
        if (btnStyle == null) {
            btnStyle = new GUIStyle(GUI.skin.button);
            ApplyTextStyle(btnStyle);
        }
        if (toggleStyle == null) {
            toggleStyle = new GUIStyle(GUI.skin.toggle);
            ApplyTextStyle(toggleStyle);
        }
        if (textFieldStyle == null) {
            textFieldStyle = new GUIStyle(GUI.skin.textField);
            ApplyTextStyle(textFieldStyle);
        }
        GUILayout.BeginArea(new Rect(10, 10, 630, 950));
        scrollPos = GUILayout.BeginScrollView(scrollPos, /*true, true, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, */scrollStyle, GUILayout.Width(buttonWidth + 30), GUILayout.Height(600));
        //GUILayout.Space (50);
        GUILayout.BeginVertical ();
        foreach (var sceneName in sceneNameList) {
            if (GUILayout.Button("Test " + sceneName, btnStyle, GUILayout.Width(buttonWidth), GUILayout.Height(40))) {
                Main.RunTest (sceneName);
            }
        }
        exMesh.enableDoubleBuffer = GUILayout.Toggle(exMesh.enableDoubleBuffer, "ex2D: Double mesh buffer", toggleStyle);
        enableAni = GUILayout.Toggle(enableAni, "Sprite animation", toggleStyle);

        GUILayout.BeginHorizontal ();
        GUILayout.Label("count", textStyle, GUILayout.Width(100));
        count = GUILayout.HorizontalScrollbar (count, 180, 0, 15000);
        count = float.Parse(GUILayout.TextField(count.ToString(), textFieldStyle, GUILayout.Width(80)));
        GUILayout.EndHorizontal ();

        GUILayout.BeginHorizontal ();
        GUILayout.Label("showhide fps", textStyle, GUILayout.Width(100));
        showhide = GUILayout.HorizontalScrollbar (showhide, 180, 0, 1000);
        showhide = float.Parse(GUILayout.TextField(showhide.ToString(), textFieldStyle, GUILayout.Width(80)));
        GUILayout.EndHorizontal ();

        GUILayout.BeginHorizontal ();
        GUILayout.Label("stopmove fps", textStyle, GUILayout.Width(100));
        stopmove = GUILayout.HorizontalScrollbar (stopmove, 180, 0, 1000);
        stopmove = float.Parse(GUILayout.TextField(stopmove.ToString(), textFieldStyle, GUILayout.Width(80)));
        GUILayout.EndHorizontal ();

        GUILayout.BeginHorizontal ();
        GUILayout.Label("param4", textStyle, GUILayout.Width(100));
        param4 = GUILayout.HorizontalScrollbar (param4, 180, 0, 1500);
        param4 = float.Parse(GUILayout.TextField(param4.ToString(), textFieldStyle, GUILayout.Width(80)));
        GUILayout.EndHorizontal ();

        GUILayout.EndScrollView();
        GUILayout.EndVertical ();
        GUILayout.EndArea();
    }
}
