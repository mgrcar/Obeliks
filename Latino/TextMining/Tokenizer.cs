/*==========================================================================;
 *
 *  (c) 2008-09 JSI.  All rights reserved.
 *
 *  File:          Tokenizer.cs
 *  Version:       1.0
 *  Desc:		   Text tokenizer interface and implementations
 *  Author:        Miha Grcar
 *  Created on:    Dec-2008
 *  Last modified: Jan-2009
 *  Revision:      Jan-2009
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections;

namespace Latino.TextMining
{
    /* .-----------------------------------------------------------------------
       |
       |  Interface ITokenizer
       |
       '-----------------------------------------------------------------------
    */
    public interface ITokenizer : IEnumerable<string>, ISerializable
    { 
        string Text { get; set; }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Enum TokenizerFilter
       |
       '-----------------------------------------------------------------------
    */
    public enum TokenizerFilter
    {
        AlphanumLoose,  // accept tokens that contain at least one alphanumeric character
        AlphanumStrict, // accept tokens that contain alphanumeric characters only
        AlphaLoose,     // accept tokens that contain at least one alphabetic character
        AlphaStrict,    // accept tokens that contain alphabetic characters only
        None            // accept all tokens
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class UnicodeTokenizer
       |
       '-----------------------------------------------------------------------  
    **
       This tokenizer (partially) follows the rules defined at http://www.unicode.org/reports/tr29/#Word_Boundaries
    */
    public class UnicodeTokenizer : ITokenizer, IEnumerator<string>
    {
        [Flags]
        private enum FilterFlags
        {
            ContainsAlpha   = 1,
            ContainsNumeric = 2,
            ContainsOther   = 4
        }
        private FilterFlags m_ff;
        private TokenizerFilter m_filter
            = TokenizerFilter.None;    
        private string m_text
            = "";
        private int m_start_idx
            = 0;
        private int m_end_idx
            = 0;
        private int m_min_token_len
            = 1;
        public UnicodeTokenizer()
        {
        }
        public UnicodeTokenizer(string text)
        {
            Utils.ThrowException(text == null ? new ArgumentNullException("text") : null);
            m_text = text;
        }
        public UnicodeTokenizer(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }
        private static bool IsNewline(char ch)
        {
            return ch == '\u000B' || ch == '\u000C' || ch == '\u0085' || ch == '\u2028' || ch == '\u2029' || 
                ch == '\r' || ch == '\n'; // *** \r and \n added for convenience (handled separately in the original rules)
        }
        private static bool IsMidLetter(char ch)
        {
            return ch == '\u00B7' || ch == '\u05F4' || ch == '\u2027' || ch == '\u003A' || ch == '\u0387' ||
                ch == '\uFE13' || ch == '\uFE55' || ch == '\uFF1A';
        }
        private static bool IsMidNum(char ch)
        {
            return ch == '\u002C' || ch == '\u003B' || ch == '\u037E' || ch == '\u0589' || ch == '\u060C' ||
                ch == '\u060D' || ch == '\u07F8' || ch == '\u2044' || ch == '\uFE10' || ch == '\uFE14' ||
                ch == '\u066C' || ch == '\uFE50' || ch == '\uFE54' || ch == '\uFF0C' || ch == '\uFF1B';
        }
        private static bool IsMidNumLet(char ch)
        {
            return ch == '\u0027' || ch == '\u002E' || ch == '\u2018' || ch == '\u2019' || ch == '\u2024' ||
                ch == '\uFE52' || ch == '\uFF07' || ch == '\uFF0E';
        }
        private static bool IsExtendNumLet(char ch)
        {
            return ch == '\u005F' || ch == '\u203F' || ch == '\u2040' || ch == '\u2054' || ch == '\uFE33' ||
                ch == '\uFE34' || ch == '\uFE4D' || ch == '\uFE4E' || ch == '\uFE4F' || ch == '\uFF3F';
        }
        private static bool IsALetter(char ch)
        {
            return char.IsLetter(ch);
        }
        private static bool IsNumeric(char ch)
        {
            return char.IsNumber(ch);
        }
        private bool AcceptToken()
        {
            return ((m_filter == TokenizerFilter.AlphanumLoose && (m_ff & (FilterFlags.ContainsAlpha | FilterFlags.ContainsNumeric)) != 0) ||
                (m_filter == TokenizerFilter.AlphanumStrict && (m_ff & FilterFlags.ContainsOther) == 0) ||
                (m_filter == TokenizerFilter.AlphaLoose && (m_ff & FilterFlags.ContainsAlpha) != 0) ||
                (m_filter == TokenizerFilter.AlphaStrict && m_ff == FilterFlags.ContainsAlpha) ||
                m_filter == TokenizerFilter.None) && m_end_idx - m_start_idx >= m_min_token_len;
        }
        private void GetNextToken()
        {
            m_ff = 0;
            for (int i = m_start_idx; i < m_text.Length - 1; i++)
            {
                char ch_1 = m_text[i];
                char ch_2 = m_text[i + 1];
                if (IsALetter(ch_1)) { m_ff |= FilterFlags.ContainsAlpha; }
                else if (IsNumeric(ch_1)) { m_ff |= FilterFlags.ContainsNumeric; }
                else { m_ff |= FilterFlags.ContainsOther; }
                if (ch_1 == '\r' && ch_2 == '\n') // WB3
                {
                }
                else if (IsNewline(ch_1) || IsNewline(ch_2)) // WB3a, WB3b
                {
                    m_end_idx = i + 1;
                    return;
                }
                else if (IsALetter(ch_1) && IsALetter(ch_2)) // WB5
                {
                }
                else if (i <= m_text.Length - 3 && IsALetter(ch_1) && (IsMidLetter(ch_2) || IsMidNumLet(ch_2)) && IsALetter(m_text[i + 2])) // WB6
                {
                }
                else if (i >= 1 && IsALetter(m_text[i - 1]) && (IsMidLetter(ch_1) || IsMidNumLet(ch_1)) && IsALetter(ch_2)) // WB7
                {
                }
                else if ((IsNumeric(ch_1) && IsNumeric(ch_2)) || (IsALetter(ch_1) && IsNumeric(ch_2)) || (IsNumeric(ch_1) && IsALetter(ch_2))) // WB8, WB9, WB10
                {
                }
                else if (i >= 1 && IsNumeric(m_text[i - 1]) && (IsMidNum(ch_1) || IsMidNumLet(ch_1)) && IsNumeric(ch_2)) // WB11
                {
                }
                else if (i <= m_text.Length - 3 && IsNumeric(ch_1) && (IsMidNum(ch_2) || IsMidNumLet(ch_2)) && IsNumeric(m_text[i + 2])) // WB12
                {
                }
                else if ((IsALetter(ch_1) || IsNumeric(ch_1) || IsExtendNumLet(ch_1)) && IsExtendNumLet(ch_2)) // WB13a
                {
                }
                else if (IsExtendNumLet(ch_1) && (IsALetter(ch_2) || IsNumeric(ch_2))) // WB13b
                {
                }
                else // WB14
                {
                    m_end_idx = i + 1;
                    return;
                }
            }
            if (m_end_idx == m_text.Length)
            {
                m_end_idx = -1;
            }
            else
            {
                char last_ch = m_text[m_text.Length - 1];
                if (IsALetter(last_ch)) { m_ff |= FilterFlags.ContainsAlpha; }
                else if (IsNumeric(last_ch)) { m_ff |= FilterFlags.ContainsNumeric; }
                else { m_ff |= FilterFlags.ContainsOther; }
                m_end_idx = m_text.Length;
            }
        }
        public TokenizerFilter Filter
        {
            get { return m_filter; }
            set { m_filter = value; }
        }
        public int MinTokenLen
        {
            get { return m_min_token_len; }
            set
            {
                Utils.ThrowException(value < 1 ? new ArgumentOutOfRangeException("MinTokenLen") : null);
                m_min_token_len = value;
            }
        }
        // *** IEnumerator<string> interface implementation ***
        public string Current
        {
            get
            {
                Utils.ThrowException(m_start_idx == m_end_idx ? new InvalidOperationException() : null);
                return m_text.Substring(m_start_idx, m_end_idx - m_start_idx);
            }
        }
        object IEnumerator.Current
        {
            get { return Current; } // throws InvalidOperationException
        }
        public bool MoveNext()
        {
            do
            {
                m_start_idx = m_end_idx;
                GetNextToken();
            }
            while (!AcceptToken() && m_end_idx != -1);
            if (m_end_idx == -1)
            {
                Reset();
                return false;
            }
            return true;
        }
        public void Reset()
        {
            m_start_idx = 0;
            m_end_idx = 0;
        }
        public void Dispose()
        {
        }
        // *** ITokenizer interface implementation ***
        public string Text
        {
            get { return m_text; }
            set
            {
                Utils.ThrowException(value == null ? new ArgumentNullException("Text") : null);
                Utils.ThrowException(m_start_idx != m_end_idx ? new InvalidOperationException() : null);
                m_text = value;
            }
        }
        public IEnumerator<string> GetEnumerator()
        {
            return this;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
        // *** ISerializable interface implementation ***
        public void Save(BinarySerializer writer) // *** current state and text are not saved
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions
            writer.WriteInt((int)m_filter);
            writer.WriteInt(m_min_token_len);
        }
        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            // the following statements throw serialization-related exceptions
            m_filter = (TokenizerFilter)reader.ReadInt();
            m_min_token_len = reader.ReadInt();
        }
    }
}