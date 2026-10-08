using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace GroveGames.DependencyInjection.Unity.Editor
{
    internal sealed class DependencyInjectionSettingsBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (EditorBuildSettings.TryGetConfigObject<DependencyInjectionSettings>(DependencyInjectionSettings.GetConfigName(), out var settings) && settings != null)
            {
                DependencyInjectionSettingsProvider.AddToPreloadedAssets(settings);
            }
        }
    }
}
