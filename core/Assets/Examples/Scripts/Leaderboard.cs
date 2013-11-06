// ======================================================================================
// File         : Leaderboard.cs
// Author       : Wu Jie 
// Last Change  : 11/04/2013 | 21:32:44 PM | Monday,November
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
// \class 
// 
// \brief 
// 
///////////////////////////////////////////////////////////////////////////////

public class Leaderboard : MonoBehaviour {
    [System.Serializable]
    public class UserInfo {
        public Texture2D icon;
        public string iconURL;
        public string name;
        public int score;
    }

    public GameObject elementPrefab;
    public List<UserInfo> userInfos;
    public exLayer layer;

    exUIScrollView scrollView;
    exClipping clipping;
    int elementCount = 0;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Awake () {
        Application.targetFrameRate = 60;

        scrollView = GetComponentInChildren<exUIScrollView>();
        clipping = GetComponentInChildren<exClipping>();

        for ( int i = 0; i < userInfos.Count; ++i ) {
            GameObject el = Object.Instantiate( elementPrefab ) as GameObject;
            LeaderboardElement leaderboardEL = el.GetComponent<LeaderboardElement>();
            UserInfo userInfo = userInfos[i];

            if ( userInfo.icon != null ) {
                leaderboardEL.Init( userInfo.icon, userInfo.name, userInfo.score );
            }
            else {
                leaderboardEL.Init( userInfo.iconURL, userInfo.name, userInfo.score );
            }

            AddElement(el);
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void AddElement ( GameObject _el ) {
        Vector2 size = new Vector2( 480.0f, 110.0f );
        float margin = 10.0f;
        float curY = elementCount * size.y + (elementCount == 0 ? 0 : elementCount * margin);

        _el.transform.parent = scrollView.contentAnchor.transform;
        _el.transform.localPosition = new Vector3( 0.0f, -curY, _el.transform.position.z ); 

        scrollView.contentSize = new Vector2 ( 480.0f, curY + size.y + margin ); 
        layer.Add(_el);
        clipping.Add(_el);

        ++elementCount;
    }
}
