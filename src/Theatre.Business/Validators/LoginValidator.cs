using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theatre.Business.Validators
{
    public class LoginValidator : Authentification
    {
        public string Check(string login)
        {
            bool found = false;
            foreach (char symbol in login)
            {
                if (SpecialSymbols.Contains(symbol))
                {
                    found = true;
                }
            }

            if (login.Length >= 10)
                Count++;
            if (login.Any(char.IsLower))
                Count++;
            if (login.Any(char.IsUpper))
                Count++;
            if (login.Any(char.IsDigit))
                Count++;
            if (found)
                Count++;
            if (Count == 5)
                return "Логин сложный";
            else if (Count == 4)
                return "Логин средний";
            else
                return "Логин простой";
        }
    }
}
