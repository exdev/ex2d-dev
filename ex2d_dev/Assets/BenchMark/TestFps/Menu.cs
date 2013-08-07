using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

    public GUIStyle scrollStyle;
    public GUIStyle textStyle;
    public string[] sceneNameList;

    private Vector2 scrollPos;
    private GUIStyle btnStyle;
    private GUIStyle toggleStyle;

	// Use this for initialization
	void Start () {
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
        GUILayout.BeginArea(new Rect(10, 10, 630, 950));
        scrollPos = GUILayout.BeginScrollView(scrollPos, /*true, true, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, */scrollStyle, GUILayout.Width(260), GUILayout.Height(600));
            //GUILayout.Space (50);
            GUILayout.BeginVertical ();
                foreach (var sceneName in sceneNameList) {
                    if (GUILayout.Button("Test " + sceneName, btnStyle, GUILayout.Width(230), GUILayout.Height(40))) {
                        Main.RunTest (sceneName);
                    }
                }
                exMesh.enableDoubleBuffer = GUILayout.Toggle(exMesh.enableDoubleBuffer, "ex2D: enable double mesh buffer", toggleStyle);
            GUILayout.EndVertical ();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
