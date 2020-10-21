using System;
using System.Collections.Generic;

namespace OTPManager.Helpers
{
    public static class Utils
    {
        static readonly Random _rnd = new Random();

        static readonly IReadOnlyList<char> BASE32_CHARSET = new char[]
        {
            '2', '3', '4', '5', '6', '7',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        /// <summary>
        /// Returns Base32 string. Base32 uses a 32-character set comprising the twenty-six upper-case letters A–Z, and the digits 2–7.
        /// </summary>
        public static string GenerateRandomBase32String(int length)
        {
            char[] result = new char[length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = BASE32_CHARSET[_rnd.Next(BASE32_CHARSET.Count)];
            }

            return new string(result);
        }
    }
}
