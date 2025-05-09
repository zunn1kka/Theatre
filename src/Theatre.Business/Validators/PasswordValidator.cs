using System.Text;
using System.Text.RegularExpressions;

namespace Theatre.Business.Validators
{
    public class PasswordValidator
    {
        private const string LowerCaseChars = "abcdefghijklmnopqrstuvwxyz";
        private const string UpperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string NumberChars = "0123456789";
        private const string SpecialSymbols = "!@#$%^&*()_+-=[]{}|;:',.<>?";
        private static readonly Random Random = new();

        public string Check(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "Пароль не может быть пустым";

            if (password.Length < 10)
                return "Пароль простой";
            var strengthScore = 0;

            // Проверки с помощью регулярных выражений
            if (Regex.IsMatch(password, @"[A-Z]")) strengthScore++; // Есть заглавные буквы
            if (Regex.IsMatch(password, @"[a-z]")) strengthScore++; // Есть строчные буквы
            if (Regex.IsMatch(password, @"[0-9]")) strengthScore++; // Есть цифры
            if (Regex.IsMatch(password, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]")) strengthScore++; // Есть спецсимволы
            if (password.Length >= 14) strengthScore++; // Длина >= 14 символов

            return strengthScore switch
            {
                5 => "Пароль сложный",
                4 => "Пароль средний",
                _ => "Пароль простой"
            };
        }

        public string GeneratePassword(int length, bool includeUpperCase = true,
                                     bool includeNumbers = true,
                                     bool includeSpecial = true,
                                     bool includeLowerCase = true)
        {
            if (length < 8)
                throw new ArgumentException("Длина пароля должна быть не менее 8 символов");

            var charSet = new StringBuilder();
            if (includeLowerCase) charSet.Append(LowerCaseChars);
            if (includeUpperCase) charSet.Append(UpperCaseChars);
            if (includeNumbers) charSet.Append(NumberChars);
            if (includeSpecial) charSet.Append(SpecialSymbols);

            if (charSet.Length == 0)
                throw new ArgumentException("Должен быть выбран хотя бы один набор символов");

            return new string(Enumerable.Repeat(charSet.ToString(), length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}