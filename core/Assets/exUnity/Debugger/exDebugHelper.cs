// ======================================================================================
// File         : exDebugHelper.cs
// Author       : Wu Jie 
// Last Change  : 06/05/2011 | 11:08:21 AM | Sunday,June
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

// ------------------------------------------------------------------ 
// Desc: 
// ------------------------------------------------------------------ 

[ExecuteInEditMode]
public class exDebugHelper : MonoBehaviour {

    ///////////////////////////////////////////////////////////////////////////////
    // static
    ///////////////////////////////////////////////////////////////////////////////

    // static instance
    public static exDebugHelper instance = null;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void ScreenPrint ( string _text ) {
        if ( instance.showScreenPrint_ ) {
            instance.txtPrint = instance.txtPrint + _text + "\n"; 
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void ScreenPrint ( Vector2 _pos, string _text, GUIStyle _style = null ) {
        if ( instance.showScreenDebugText ) {
            TextInfo info = new TextInfo( _pos, _text, _style ); 
            instance.debugTextPool.Add(info);
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public enum LogType {
        None,
        Normal,
        Warning,
        Error,
    }

    public static void ScreenLog ( string _text, LogType _logType = LogType.None, GUIStyle _style = null, bool autoFadeOut = true) {
        LogInfo info = new LogInfo( _text, _style, autoFadeOut ? 5.0f : 0 );
        instance.pendingLogs.Enqueue(info);

        if ( _logType != LogType.None ) {
            switch ( _logType ) {
            case LogType.Normal: Debug.Log(_text); break;
            case LogType.Warning: Debug.LogWarning(_text); break;
            case LogType.Error: Debug.LogError(_text); break;
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void SetFPSColor ( Color _color ) {
        instance.fpsStyle.normal.textColor = _color;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static float GetFPS () { return instance.fps; }

    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////

    public Vector2 offset = new Vector2 ( 10.0f, 10.0f );

    public GUIStyle printStyle = null;
    public GUIStyle fpsStyle = null;
    public GUIStyle logStyle = null;
    public GUIStyle timeScaleStyle = null;

    protected string txtPrint = "screen print: ";
    protected string txtFPS = "fps: ";

    // for screen debug text
    public class TextInfo {
        public Vector2 screenPos = Vector2.zero;
        public string text;
        public GUIStyle style = null;

        public TextInfo ( Vector2 _screenPos, string _text, GUIStyle _style ) {
            screenPos = _screenPos;
            text = _text;
            style = _style;  
        }
    }
    protected List<TextInfo> debugTextPool = new List<TextInfo>();

    // for screen log
    public class LogInfo {
        public string text;
        public GUIStyle style = null;

        public float ratio {
            get {
                if (lifetime == 0) {
                    return 0;
                }
                return (timer >= lifetime - instance.logFadeOutDuration) ? (timer - (lifetime-instance.logFadeOutDuration))/instance.logFadeOutDuration : 0.0f; 
            } 
        }
        public bool canDelete { get { return timer > lifetime; } }

        // internal
        float speed = 1.0f;
        float timer = 0.0f;
        float lifetime = 5.0f;

        public LogInfo ( string _text, GUIStyle _style, float _lifetime ) {
            text = _text;
            style = _style;  
            lifetime = _lifetime;
        }

        public void Dead () {
            if (lifetime > 0) {
                float deadTime = lifetime - instance.logFadeOutDuration;
                if ( timer < deadTime - 1.0f) {
                    timer = deadTime - 1.0f;
                }
            }
        }

        public void Tick () {
            if (lifetime > 0) {
                timer += Time.deltaTime * speed;
            }
        }
    }
    // DISABLE { 
    // float logInterval = 0.05f;
    // float logTimer = 0.0f;
    // } DISABLE end 
    float logFadeOutDuration = 0.3f;
    protected List<LogInfo> logs = new List<LogInfo>();
    protected Queue<LogInfo> pendingLogs = new Queue<LogInfo>();

    // fps
    [SerializeField] protected bool showFps_ = true;
    public bool showFps {
        get { return showFps_; }
        set {
            if ( showFps_ != value ) {
                showFps_ = value;
            }
        }
    }
    public TextAnchor fpsAnchor = TextAnchor.UpperLeft;

    // timescale
    [SerializeField] protected bool enableTimeScaleDebug_ = true;
    public bool enableTimeScaleDebug {
        get { return enableTimeScaleDebug_; }
        set {
            if ( enableTimeScaleDebug_ != value ) {
                enableTimeScaleDebug_ = value;
            }
        }
    }

    // screen print
    [SerializeField] protected bool showScreenPrint_ = true;
    public bool showScreenPrint {
        get { return showScreenPrint_; }
        set {
            if ( showScreenPrint_ != value ) {
                showScreenPrint_ = value;
            }
        }
    }

    // screen log
    [SerializeField] protected bool showScreenLog_ = true;
    public bool showScreenLog {
        get { return showScreenLog_; }
        set {
            if ( showScreenLog_ != value ) {
                showScreenLog_ = value;
            }
        }
    }
    public int logCount = 10;

    // screen debug text
    public bool showScreenDebugText = false;

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    protected int frames = 0;
    protected float fps = 0.0f;
    protected float lastInterval = 0.0f;

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Awake () {
        if ( instance == null )
            instance = this;

        // DISABLE { 
        // logTimer = logInterval;
        // } DISABLE end 
        txtPrint = "";
        txtFPS = "";

        if ( showScreenDebugText ) {
            debugTextPool.Clear();
        }

        useGUILayout = false;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Start () {
        InvokeRepeating("UpdateFPS", 0.0f, 1.0f );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Update () {
        // count fps
        ++frames;

        //
        UpdateTimeScale ();

        // update log
        UpdateLog ();

        // NOTE: the OnGUI call multiple times in one frame, so we just clear text here.
        StartCoroutine ( CleanDebugText() );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnGUI () {
        GUIContent content = null;
        Vector2 size = Vector2.zero;
        float curX = offset.x;
        float curY = offset.y;

        if ( showFps ) {
            content = new GUIContent(txtFPS);
            size = fpsStyle.CalcSize(content);

            //
            switch ( fpsAnchor ) {
                //
            case TextAnchor.UpperLeft: 
                break;

            case TextAnchor.UpperCenter: 
                curX = curX + (Screen.width - size.x) * 0.5f; 
                break;

            case TextAnchor.UpperRight: 
                curX = Screen.width - size.x - curX; 
                break;

                //
            case TextAnchor.MiddleLeft: 
                curY = curY + (Screen.height - size.y) * 0.5f;
                break;

            case TextAnchor.MiddleCenter:
                curX = curX + (Screen.width - size.x) * 0.5f; 
                curY = curY + (Screen.height - size.y) * 0.5f;
                break;

            case TextAnchor.MiddleRight:
                curX = Screen.width - size.x - curX; 
                curY = curY + (Screen.height - size.y) * 0.5f;
                break;

                //
            case TextAnchor.LowerLeft:
                curY = Screen.height - size.y - curY;
                break;

            case TextAnchor.LowerCenter:
                curX = curX + (Screen.width - size.x) * 0.5f; 
                curY = Screen.height - size.y - curY;
                break;

            case TextAnchor.LowerRight:
                curX = Screen.width - size.x - curX; 
                curY = Screen.height - size.y - curY;
                break;
            }

            GUI.Label ( new Rect( curX, curY, size.x, size.y ), txtFPS, fpsStyle );

            curX = 10.0f;
            curY = 10.0f + size.y;
        }

        if ( enableTimeScaleDebug ) {
            string txtTimeScale = "TimeScale = " + Time.timeScale.ToString("f2");
            content = new GUIContent(txtTimeScale);
            size = timeScaleStyle.CalcSize(content);
            GUI.Label ( new Rect( curX, curY, size.x, size.y ), txtTimeScale, timeScaleStyle );
            curY += size.y;
        }

        if ( showScreenPrint ) {
            content = new GUIContent(txtPrint);
            size = printStyle.CalcSize(content);
            GUI.Label ( new Rect( curX, curY, size.x, size.y ), txtPrint, printStyle );
        }

        if ( showScreenLog ) {
            float y;
            bool downToUp = logStyle.alignment == TextAnchor.LowerLeft || logStyle.alignment == TextAnchor.LowerCenter || logStyle.alignment == TextAnchor.LowerRight;
            bool rightAlign = logStyle.alignment == TextAnchor.LowerRight || logStyle.alignment == TextAnchor.MiddleRight || logStyle.alignment == TextAnchor.UpperRight;
            if (downToUp) {
                y = Screen.height - 10;
            }
            else {
                y = 50;
            }
            for ( int i = logs.Count-1; i >= 0; --i ) {
                LogInfo info = logs[i];

                content = new GUIContent(info.text);
                GUIStyle style = (info.style == null) ? logStyle : info.style;
                size = style.CalcSize(content);

                //
                style.normal.textColor = new Color ( style.normal.textColor.r, 
                                                     style.normal.textColor.g, 
                                                     style.normal.textColor.b, 
                                                     1.0f - info.ratio );
                if (downToUp) {
                    y -= size.y;
                }
                else {
                    y += size.y;
                }
                if (rightAlign) {
                    GUI.Label(new Rect(Screen.width - 10.0f - size.x, y, size.x, size.y), info.text, style);
                }
                else {
                    GUI.Label(new Rect(10.0f, y, size.x, size.y), info.text, style);
                }
            }
        }

        if ( showScreenDebugText ) {
            for ( int i = 0; i < debugTextPool.Count; ++i ) {
                TextInfo info = debugTextPool[i];
                content = new GUIContent(info.text);
                GUIStyle style = (info.style == null) ? GUI.skin.label : info.style;
                size = style.CalcSize(content);

                Vector2 pos = new Vector2( info.screenPos.x, Screen.height - info.screenPos.y ) - size * 0.5f; 
                GUI.Label ( new Rect( pos.x, pos.y, size.x, size.y ), info.text, style );
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateFPS () {
        float timeNow = Time.realtimeSinceStartup;
        fps = frames / (timeNow - lastInterval);
        frames = 0;
        lastInterval = timeNow;
        txtFPS = "fps: " + fps.ToString("f2");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateTimeScale () {
        if ( enableTimeScaleDebug ) {
            if ( Input.GetKey(KeyCode.Minus) ) {
                Time.timeScale = Mathf.Max( Time.timeScale - 0.01f, 0.0f );
            }
            else if ( Input.GetKey(KeyCode.Equals) ) {
                Time.timeScale = Mathf.Min( Time.timeScale + 0.01f, 10.0f );
            }

            if ( Input.GetKey(KeyCode.Alpha0 ) ) {
                Time.timeScale = 0.0f;
            }
            else if ( Input.GetKey(KeyCode.Alpha9 ) ) {
                Time.timeScale = 1.0f;
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    IEnumerator CleanDebugText () {
        yield return new WaitForEndOfFrame();
        txtPrint = "";

        if ( showScreenDebugText ) {
            debugTextPool.Clear();
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateLog () {
        for ( int i = logs.Count-1; i >= 0; --i ) {
            LogInfo info = logs[i];
            info.Tick();
            if ( info.canDelete ) {
                logs.RemoveAt(i);
            }
        }

        // DISABLE { 
        // if ( logTimer < logInterval ) {
        //     logTimer += Time.deltaTime;
        // }
        // else {
        //     if ( pendingLogs.Count > 0 ) {
        //         logTimer = 0.0f;
        //         logs.Add(pendingLogs.Dequeue());

        //         if ( instance.logs.Count > instance.logCount ) {
        //             for ( int i = 0; i < instance.logs.Count - instance.logCount; ++i ) {
        //                 instance.logs[i].Dead();
        //             }
        //         }
        //     }
        // }
        // } DISABLE end 

        bool downToUp = logStyle.alignment == TextAnchor.LowerLeft || logStyle.alignment == TextAnchor.LowerCenter || logStyle.alignment == TextAnchor.LowerRight;

        if ( pendingLogs.Count > 0 ) {
            int count = Mathf.CeilToInt(pendingLogs.Count/2);

            do {
                if (downToUp) {
                    logs.Add(pendingLogs.Dequeue());
                }
                else {
                    logs.Insert(0, pendingLogs.Dequeue());
                }
                --count;

                if ( instance.logs.Count > instance.logCount ) {
                    for ( int i = 0; i < instance.logs.Count - instance.logCount; ++i ) {
                        instance.logs[i].Dead();
                    }
                }
            } while ( count > 0 );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void ClearScreen () {
        instance.pendingLogs.Clear ();
        instance.logs.Clear ();
//        for (int i = instance.logs.Count-1; i >= 0; --i) {
//            LogInfo info = logs [i];
//            info.Dead ();
//        }
    }
}
