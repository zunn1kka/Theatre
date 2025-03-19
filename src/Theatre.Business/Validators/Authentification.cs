using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theatre.Business
{
    public class Authentification
    {
        private int _count = 0;
        private string _lowerCaseChars = "abcdefghijklmnopqrstuvwxyz";
        private string _upperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string _numberChars = "0123456789";
        private string _specialSymbols = "!;%:?*.,";
        readonly Random random = new Random();

        public string LowerCaseChars { get => _lowerCaseChars; set => _lowerCaseChars = value; }
        public string UpperCaseChars { get => _upperCaseChars; set => _upperCaseChars = value; }
        public string NumberChars { get => _numberChars; set => _numberChars = value; }
        public string SpecialSymbols { get => _specialSymbols; set => _specialSymbols = value; }
        public int Count { get => _count; set => _count = value; }

        public Random Random => random;

    }
}
