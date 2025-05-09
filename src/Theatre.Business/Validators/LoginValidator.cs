namespace Theatre.Business.Validators
{
    public class LoginValidator
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
