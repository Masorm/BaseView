using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            if (GUILayout.Button("Set&Build Addressable"))
            {
                SetAddressable();
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
                    Debug.Log($"Finished Update: {_addRequest.Result.version}");
                }
                else if (_addRequest.Status >= StatusCode.Failure)
                {
                    Debug.LogError(_addRequest.Error.message);
                }

                EditorApplication.update -= Progress;
            }
        }

        private static AddressableAssetSettings settings;
        private static AddressableAssetGroup activeSceneGroup;
        private const string DEFAULT_LOCAL_GROUP_NAME = "Default Local Group";
        private static string ACTIVE_SCENE_NAME => SceneManager.GetActiveScene().name;

        private static void SetAddressable()
        {
            if (!AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                // AddressableAssetSettingsDefaultObjectがなければ作成する
                settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
                settings.BuildRemoteCatalog = true;
                settings.OverridePlayerVersion = "v0.0.1";
            }
            else
            {
                // // AddressableAssetSettingsDefaultObjectがあれば既存のものを使う
                settings = AddressableAssetSettingsDefaultObject.Settings;
            }

            activeSceneGroup = settings.groups.FirstOrDefault(x => x.Name == ACTIVE_SCENE_NAME);
            if (activeSceneGroup == null)
            {
                var groupTemplate = settings.GetGroupTemplateObject(0) as AddressableAssetGroupTemplate;
                activeSceneGroup = settings.CreateGroup(ACTIVE_SCENE_NAME, true, false, false, null, groupTemplate.GetTypes());
            }

            SetProfileSettings(settings);

            SetSchemaSettings(activeSceneGroup, settings);
        
            DeleteDefaultGroup(settings);
            AssetDatabase.SaveAssets();
        }

        private const string BUILD_PATH_VARIABLE_NAME = "CustomBuildPath";
        private const string LOAD_PATH_VARIABLE_NAME = "CustomLoadPath";
        private const string BUILD_PATH_DEFAULT_VALUE = "ServerData/WebGL";
        private const string LOAD_PATH_DEFAULT_VALUE = "https://qiita.com";
    
        /// <summary>
        /// Set ProfileSettings
        /// </summary>
        /// <param name="settings"></param>
        private static void SetProfileSettings(AddressableAssetSettings settings)
        {
            settings.profileSettings.CreateValue(BUILD_PATH_VARIABLE_NAME, BUILD_PATH_DEFAULT_VALUE);
            settings.profileSettings.CreateValue(LOAD_PATH_VARIABLE_NAME, LOAD_PATH_DEFAULT_VALUE);
            settings.RemoteCatalogBuildPath.SetVariableByName(settings, BUILD_PATH_VARIABLE_NAME);
            settings.RemoteCatalogLoadPath.SetVariableByName(settings, LOAD_PATH_VARIABLE_NAME);
        }

        /// <summary>
        /// Set SchemaSettings
        /// </summary>
        /// <param name="group"></param>
        private static void SetSchemaSettings(AddressableAssetGroup group, AddressableAssetSettings settings)
        {
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
            schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
            schema.BuildPath.SetVariableByName(settings, BUILD_PATH_VARIABLE_NAME);
            schema.LoadPath.SetVariableByName(settings, LOAD_PATH_VARIABLE_NAME);
        }

        /// <summary>
        /// Delete DefaultGroup
        /// </summary>
        /// <param name="settings"></param>
        private static void DeleteDefaultGroup(AddressableAssetSettings settings)
        {
            var defaultGroup = settings.groups.FirstOrDefault(x => x.Name == DEFAULT_LOCAL_GROUP_NAME);
            if (defaultGroup != null) settings.RemoveGroup(defaultGroup);
        }
    }
}
