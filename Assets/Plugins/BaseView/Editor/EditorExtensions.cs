using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.SceneManagement;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace BaseView.Plugins.Editor
{
    public class EditorExtensions : EditorWindow
    {
        private static AddRequest _addRequest;
        private const string PACKAGE_PATH = "git+ssh://git@github.com/Masorm/BaseView.git?path=Assets/Plugins/BaseView";
        private static string VERSION => $"v{PackageInfo.FindForAssetPath("Packages/com.masorm.baseview").version}";
        
        [MenuItem("Extension/ShowWindow")]
        private static void ShowWindow()
        {
            var window = GetWindow<EditorExtensions>();
            window.titleContent = new GUIContent("EditorExtensions");
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Update Package Version Button.", MessageType.Info);
            if (GUILayout.Button("Update Package"))
            {
                _addRequest = Client.Add(PACKAGE_PATH);
                EditorApplication.update += Progress;
            }
            
            GUILayout.Space(20f);
            EditorGUILayout.HelpBox("Set up Addressable Group and Profile Button.", MessageType.Info);
            if (GUILayout.Button("Addressable Settings"))
            {
                SetAddressable();
            }

            GUILayout.Space(20f);
            EditorGUILayout.HelpBox("Start Addressable Build Button.", MessageType.Info);
            if (GUILayout.Button("Build Addressable"))
            {
                StartAddressableBuild();
                
                EditorUtility.RevealInFinder(BUILD_PATH_DEFAULT_VALUE);

                var files = Directory.GetFiles(BUILD_PATH_DEFAULT_VALUE);
                foreach (var file in files)
                {
                    Debug.Log(Path.GetFileName(file));
                }
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
        private const string SCENE_LIST_GROUP_NAME = "SceneList";

        private static void SetAddressable()
        {
            if (!AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                // AddressableAssetSettingsDefaultObjectがなければ作成する
                settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
                settings.BuildRemoteCatalog = true;
                settings.OverridePlayerVersion = VERSION;
            }
            else
            {
                // // AddressableAssetSettingsDefaultObjectがあれば既存のものを使う
                settings = AddressableAssetSettingsDefaultObject.Settings;
            }

            activeSceneGroup = settings.groups.FirstOrDefault(x => x.Name == SCENE_LIST_GROUP_NAME);
            if (activeSceneGroup == null)
            {
                var groupTemplate = settings.GetGroupTemplateObject(0) as AddressableAssetGroupTemplate;
                activeSceneGroup = settings.CreateGroup(SCENE_LIST_GROUP_NAME, true, false, false, null, groupTemplate.GetTypes());
            }

            SetProfileSettings(settings);

            SetSchemaSettings(activeSceneGroup, settings);
            SetActiveSceneInGroupEntity(activeSceneGroup, settings);
        
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
        
        /// <summary>
        /// Add ActiveScene in SceneListGroup
        /// </summary>
        /// <param name="assetGroup"></param>
        /// <param name="assetSettings"></param>
        private static void SetActiveSceneInGroupEntity(AddressableAssetGroup assetGroup, AddressableAssetSettings assetSettings)
        {
            var guid = AssetDatabase.GUIDFromAssetPath(SceneManager.GetActiveScene().path).ToString();
            var assetEntry = assetSettings.CreateOrMoveEntry(guid, assetGroup);
            assetEntry.SetAddress(Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)));
        }

        /// <summary>
        /// Start Addressable Build
        /// </summary>
        private static void StartAddressableBuild()
        {
            AddressableAssetSettings.CleanPlayerContent();
            AddressableAssetSettings.BuildPlayerContent();
        }
    }
}
