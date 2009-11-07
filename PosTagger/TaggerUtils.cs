using System;
using Latino;

namespace PosTagger
{
    public static class TaggerUtils
    {
        public enum WordCaseType
        {
            CapitalLetter,
            Uppercase,
            Lowercase,
            Other
        }

        public static void ThrowException(Exception exception)
        {
            if (exception != null) { throw exception; }
        }

        public static bool IsLowercaseWord(string word)
        {
            ThrowException(word == null ? new ArgumentNullException("word") : null);
            ThrowException(word.Length == 0 ? new ArgumentValueException("word") : null);
            foreach (char ch in word)
            {
                if (!char.IsLower(ch)) { return false; }
            }
            return true;
        }

        public static bool IsUppercaseWord(string word)
        {
            ThrowException(word == null ? new ArgumentNullException("word") : null);
            ThrowException(word.Length == 0 ? new ArgumentValueException("word") : null);
            foreach (char ch in word)
            {
                if (!char.IsUpper(ch)) { return false; } 
            }
            return true;
        }

        public static bool IsCapitalLetterWord(string word)
        {
            ThrowException(word == null ? new ArgumentNullException("word") : null);
            ThrowException(word.Length == 0 ? new ArgumentValueException("word") : null);
            if (word.Length == 1) { return char.IsUpper(word[0]); }
            return char.IsUpper(word[0]) && IsLowercaseWord(word.Substring(1));
        }

        public static WordCaseType GetWordCaseType(string word)
        {
            ThrowException(word == null ? new ArgumentNullException("word") : null);
            ThrowException(word.Length == 0 ? new ArgumentValueException("word") : null);
            if (IsLowercaseWord(word))
            {
                return WordCaseType.Lowercase;
            }
            else if (IsCapitalLetterWord(word))
            {
                return WordCaseType.CapitalLetter;
            }
            else if (IsUppercaseWord(word))
            {
                return WordCaseType.Uppercase;
            }
            else
            {
                return WordCaseType.Other;
            }
        }

        public static string SetWordCaseType(string word, WordCaseType case_type)
        {
            ThrowException(word == null ? new ArgumentNullException("word") : null);
            ThrowException(word.Length == 0 ? new ArgumentValueException("word") : null);
            switch (case_type)
            { 
                case WordCaseType.Lowercase:
                    return word.ToLower();
                case WordCaseType.CapitalLetter:
                    if (word.Length >= 2)
                    {
                        return char.ToUpper(word[0]) + word.Substring(1).ToLower();
                    }
                    else
                    {
                        return word.ToUpper();
                    }
                case WordCaseType.Uppercase:
                    return word.ToUpper();
            }
            return word;
        }
    }
}
