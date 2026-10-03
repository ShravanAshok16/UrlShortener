using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UrlShortener.Core.Services
{
    public class ShortCodeGenerator
    {
        private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int CodeLength = 7;

        public static string Generate()
        {
            var chars = new char[CodeLength];
            var random = Random.Shared;
            
            for (int i = 0; i < CodeLength; i++)
            {
                chars[i] = Alphabet[random.Next(Alphabet.Length)];
                
            }
            return new string(chars);
        }
    }
}