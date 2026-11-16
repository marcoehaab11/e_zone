using System.Globalization;

namespace SupplierDirectory.Infrastructure.Localization;

public interface IUiText
{
    string Get(string arabic, string english);
    bool IsEnglish { get; }
}

public sealed class UiText : IUiText
{
    public bool IsEnglish => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase);
    public string Get(string arabic, string english) => IsEnglish ? english : arabic;
}

public static class RequestLanguageExtensions
{
    public static bool PrefersEnglish(this HttpRequest request)
        => request.GetTypedHeaders().AcceptLanguage?.OrderByDescending(x => x?.Quality ?? 1)
            .Any(x => x is not null && x.Value.Value?.StartsWith("en", StringComparison.OrdinalIgnoreCase) == true) == true;
}
