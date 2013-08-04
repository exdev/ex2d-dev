using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

    public string[] sceneNameList;

	// Use this for initialization
	void Start () {
	
	}
	
    // Update is called once per frame
    void OnGUI () {
        GUILayout.BeginVertical ();
        GUILayout.Space (50);
            foreach (var sceneName in sceneNameList) {
                if (GUILayout.Button("Test " + sceneName)) {
                    Main.RunTest (sceneName);
                }
            }
        GUILayout.EndVertical ();
    }
}
