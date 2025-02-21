using System.Reflection;
using Windows.ApplicationModel;

namespace StudentManagementApp.Helpers
{
    public static class AppInfoHelper
    {
        public static string GetAppVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return versionAttribute?.InformationalVersion ?? "Unknown version";
        }
    }
}
