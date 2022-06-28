using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace BaseView.Plugins.Editor
{
    public class EditorExtensions : EditorWindow
    {
        private static AddRequest _addRequest;
        private const string PACKAGE_PATH = "https://github.com/Masorm/BaseView.git?path=Assets/Plugins/BaseView";
        
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

            if (GUILayout.Button("Update Package"))
            {
                _addRequest = Client.Add(PACKAGE_PATH);
                EditorApplication.update += Progress;
            }
        }

        private static void Progress()
        {
            if (_addRequest.IsCompleted)
            {
                if (_addRequest.Status == StatusCode.Success)
                {
                    Debug.Log($"Install完了: {_addRequest.Result.version}");
                }
                else if (_addRequest.Status >= StatusCode.Failure)
                {
                    Debug.LogError(_addRequest.Error.message);
                }

                EditorApplication.update -= Progress;
            }
        }
    }
}
