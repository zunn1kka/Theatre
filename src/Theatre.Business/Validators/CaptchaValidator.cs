using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theatre.Business.Validators
{
    public class CaptchaValidator : Authentification
    {
        private readonly string charSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        public string GenerateCaptcha()
        {
            return new string(Enumerable.Repeat(charSet, 6).Select(s => s[Random.Next(s.Length)]).ToArray());
            
        }
    }
}
