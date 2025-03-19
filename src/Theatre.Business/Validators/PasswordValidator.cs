using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theatre.Business.Validators
{
    public class PasswordValidator : Authentification
    {
        public string Check(string password)
        {

            bool found = false;
            foreach (char symbol in password)
            {
                if (SpecialSymbols.Contains(symbol))
                {
                    found = true;
                }
            }
            if (password.Length < 10)
                return "Пароль простой";
            if (password.Length >= 14)
                Count++;
            if (password.Any(char.IsLower))
                Count++;
            if (password.Any(char.IsUpper))
                Count++;
            if (password.Any(char.IsDigit))
                Count++;
            if (found)
                Count++;
            if (Count == 5)
                return "Пароль сложный";
            else if (Count == 4)
                return "Пароль средний";
            else
                return "Пароль простой";
        }
        public string GeneratePassword(int length, bool includeUpperCase, bool includeNumbers, bool includeSpecial, bool includeLowerCase)
        {
            string charSet = "";
            if (includeUpperCase == true) charSet += UpperCaseChars;
            if (includeNumbers == true) charSet += NumberChars;
            if (includeSpecial == true) charSet += SpecialSymbols;
            if (includeLowerCase == true) charSet += LowerCaseChars;
            StringBuilder password = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                password.Append(charSet[Random.Next(charSet.Length)]);
            }

            return password.ToString();
        }
    }
}
