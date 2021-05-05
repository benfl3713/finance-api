using System.Globalization;
using System.Threading;

namespace FinanceAPICore.Utilities
{
	public static class ExtensionMethods
	{
        public static string ToTitleCase(this string stringInput)
        {
            if (string.IsNullOrEmpty(stringInput))
                return stringInput;

            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(stringInput.ToLower());
        }
    }
}
