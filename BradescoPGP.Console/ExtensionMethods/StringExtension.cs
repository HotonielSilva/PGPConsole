using System.Globalization;
using System.Text;

namespace BradescoPGPConsole 
{
    public static class StringExtension
    {
        public static string RemoverAcentos(this string text)
        {
            string sbReturn = string.Empty;

            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();

            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn += letter;
            }

            return sbReturn.ToString();
        }
    }
}
