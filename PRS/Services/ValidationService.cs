using System.Text.RegularExpressions;

namespace PRS.Services
{
    public class ValidationService
    {
        public static bool ValidEmail(string? email)
        {
            if (email == null) return true;
            Regex reg = new Regex("\"^([A-Za-z0-9+-_~.%]+@[a-zA-Z0-9.-]+\\\\.[a-zA-Z]{2,4})$");
            return reg.IsMatch(email);
        }
    }
}
