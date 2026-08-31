using System;

namespace Core.Network.Abstractions.Types
{
    /// <summary>
    /// Produces and validates room codes. The alphabet drops characters that read alike in a
    /// shared screenshot or over voice: I, O, 0 and 1. A code doubles as the session name.
    /// </summary>
    public static class RoomCodeGenerator
    {
        public const int CodeLength = 6;
        public const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        private static readonly Random Random = new();

        public static string Generate()
        {
            var buffer = new char[CodeLength];

            for (var i = 0; i < CodeLength; i++)
            {
                buffer[i] = Alphabet[Random.Next(Alphabet.Length)];
            }

            return new string(buffer);
        }

        /// <summary>
        /// Brings user input to canonical form: trims, uppercases and drops separators people
        /// add when reading a code aloud. Does not guess at characters outside the alphabet.
        /// </summary>
        public static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var buffer = new char[input.Length];
            var length = 0;

            foreach (var character in input)
            {
                if (character == ' ' || character == '-' || character == '_')
                {
                    continue;
                }

                buffer[length++] = char.ToUpperInvariant(character);
            }

            return new string(buffer, 0, length);
        }

        public static bool IsValid(string code)
        {
            if (code == null || code.Length != CodeLength)
            {
                return false;
            }

            foreach (var character in code)
            {
                if (Alphabet.IndexOf(character) < 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
