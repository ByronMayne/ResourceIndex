using Microsoft.CodeAnalysis.Diagnostics;

namespace Resource.Index.Extensions
{
    internal static class AnalyzerConfigOptionsExtensions
    {
        /// <summary>
        /// Attempts the get the value of an option otherwise it sets the default value
        /// </summary>
        public static string GetValue(this AnalyzerConfigOptions instance, string key, string defaultValue)
        {
            if(instance.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
            return defaultValue;
        }
    }
}
