using WordsTrainer.Contracts.Common;

namespace WordsTrainer.Mobile.Services;

public static class ApiResultExtensions
{
    public static string ToDisplayMessage<T>(
        this ApiResult<T> result,
        UiTextService texts,
        string fallbackKey)
    {
        if (!string.IsNullOrWhiteSpace(result.ErrorCode))
        {
            var localized = texts.T(result.ErrorCode);
            if (!string.Equals(localized, result.ErrorCode, StringComparison.Ordinal))
                return localized;
        }

        if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            return result.ErrorMessage;

        return texts.T(fallbackKey);
    }
}
