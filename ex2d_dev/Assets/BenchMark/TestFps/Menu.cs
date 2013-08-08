using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {
    const int buttonWidth = 600;
    const int paramLabelWidth = 150;

    public static bool setted = false;
    public static bool enableAni = true;
    public static bool testMeshBuffer = false;

    public static float count = 1000;
    public static string inputCount = count.ToString();
    public static float showhide = 1;
    public static string inputShowhide = showhide.ToString();
    public static float stopmove = 3;
    public static string inputStopmove = stopmove.ToString();
    public static float speed = 200;
    public static string inputSpeed = speed.ToString();

    public static string inputVertexCount = exLayer.maxDynamicMeshVertex.ToString();

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
    void OnGuiSlider (string name, ref int val, float min, float max, ref string inputVal) {
        float t = val;
        OnGuiSlider (name, ref t, min, max, ref inputVal);
        val = (int)t;
    }
    void OnGuiSlider (string name, ref float val, float min, float max, ref string inputVal) {
        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label(name, textStyle, GUILayout.Width(paramLabelWidth));
            float size = (max - min) / 10;
            float newVal = GUILayout.HorizontalScrollbar(val, size, min, max + size);
            if (val != newVal && GUI.changed) {
                GUI.changed = false;
                val = newVal;
                inputVal = val.ToString();
            }
            inputVal = GUILayout.TextField(inputVal, textFieldStyle, GUILayout.Width(80));
            if (float.TryParse(inputVal, out newVal)) {
                val = newVal;
            }
        }
        GUILayout.EndHorizontal();
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
        GUILayout.BeginVertical();
        scrollPos = GUILayout.BeginScrollView(scrollPos, /*true, true, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, */scrollStyle, GUILayout.Width(buttonWidth + 30), GUILayout.Height(600));
        {
            foreach (var sceneName in sceneNameList) {
                if (GUILayout.Button("Test " + sceneName, btnStyle, GUILayout.Width(buttonWidth), GUILayout.Height(40))) {
                    Main.RunTest(sceneName);
                }
                GUILayout.Space(20);
            }
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            {
                exMesh.enableDoubleBuffer = GUILayout.Toggle(exMesh.enableDoubleBuffer, "ex2D: Double Mesh Buffer", toggleStyle);
                testMeshBuffer = GUILayout.Toggle(testMeshBuffer, "Test Mesh Buffer", toggleStyle);
            }
            GUILayout.EndHorizontal();
            OnGuiSlider("DynMesh Vertex:", ref exLayer.maxDynamicMeshVertex, 4, 4096, ref inputVertexCount);
            enableAni = GUILayout.Toggle(enableAni, "Sprite animation", toggleStyle);

            OnGuiSlider("Sprite Count:", ref count, 0, 1000, ref inputCount);
            OnGuiSlider("Sprite Speed:", ref speed, 0, 1000, ref inputSpeed);
            OnGuiSlider("Stop/Move Fps:", ref stopmove, 0, 70, ref inputStopmove);
            OnGuiSlider("Show/Hide Fps:", ref showhide, 0, 70, ref inputShowhide);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
