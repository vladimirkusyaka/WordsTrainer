using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace WordsTrainer.Mobile.Services
{
    public class UiTextService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _localizations;
        private string _languageCode;

        public UiTextService()
        {
            _localizations = LoadLocalizations();
            _languageCode = ResolveLanguageCode(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
        }

        public void SetLanguage(string? languageCode)
        {
            _languageCode = ResolveLanguageCode(languageCode);
        }

        public string T(string key)
        {
            if (_localizations.TryGetValue(_languageCode, out var current) &&
                current.TryGetValue(key, out var value))
            {
                return value;
            }

            if (_localizations.TryGetValue("en", out var fallback) &&
                fallback.TryGetValue(key, out var fallbackValue))
            {
                return fallbackValue;
            }

            return key;
        }

        public string Format(string key, params object[] args)
        {
            return string.Format(T(key), args);
        }

        private string ResolveLanguageCode(string? languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                return "en";

            var normalized = languageCode.Trim().ToLowerInvariant();

            if (_localizations.ContainsKey(normalized))
                return normalized;

            var shortCode = normalized.Split('-', '_')[0];

            return _localizations.ContainsKey(shortCode)
                ? shortCode
                : "en";
        }

        private static Dictionary<string, Dictionary<string, string>> LoadLocalizations()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly
                .GetManifestResourceNames()
                .Where(x =>
                    x.Contains(".Resources.Localization.", StringComparison.Ordinal) &&
                    x.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

            var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var resourceName in resources)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                    continue;

                var values = JsonSerializer.Deserialize<Dictionary<string, string>>(stream);
                if (values == null)
                    continue;

                var languageCode = GetLanguageCodeFromResourceName(resourceName);
                result[languageCode] = values;
            }

            return result;
        }

        private static string GetLanguageCodeFromResourceName(string resourceName)
        {
            var fileName = resourceName.Split('.').TakeLast(2).First();
            return fileName.Trim().ToLowerInvariant();
        }
    }
}
