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

            if (login.Length >= 10)
                return "Логин допустимый";
            else
                return "Логин недопустимый";
        }
    }
}
