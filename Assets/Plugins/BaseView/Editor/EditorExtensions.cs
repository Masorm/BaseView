using UnityEditor;
using UnityEngine;

namespace BaseView.Plugins.Editor
{
    public class EditorExtensions : EditorWindow
    {
        [MenuItem("Extension/ShowWindow")]
        private static void ShowWindow()
        {
            var window = GetWindow<EditorExtensions>();
            window.titleContent = new GUIContent("EditorExtensions");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Button"))
            {
                Debug.Log("Pushed!!");
            }
        }
    }
}
