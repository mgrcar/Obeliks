/*==========================================================================;
 *
 *  (c) 2009 mIHA.  All rights reserved.
 *
 *  File:          Tokenizer.cs
 *  Version:       1.0
 *  Desc:		   Text tokenizer based on regular expressions
 *  Author:        Miha Grcar
 *  Created on:    Apr-2009
 *  Last modified: Apr-2009
 *  Revision:      Apr-2009
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;

namespace Latino.TextMining
{
    /* .-----------------------------------------------------------------------
       |
       |  Class RegexTokenizer
       |
       '-----------------------------------------------------------------------  
    */
    public class RegexTokenizer : ITokenizer, IEnumerator<string>
    {
        private string m_text
            = "";
        private Queue<string> m_tokens
            = new Queue<string>();
        private Match m_token_match
            = null;
        private Regex m_token_regex
            = new Regex(@"[A-Za-z]+(-[A-Za-z]+)*", RegexOptions.Compiled);
        private Regex m_delim_regex
            = new Regex(@"\s+|$", RegexOptions.Compiled);
        private bool m_ignore_unknown_tokens
            = false;
        private void GetMoreTokens()
        {
            int start_idx = 0;
            if (m_token_match == null)
            {
                m_token_match = m_token_regex.Match(m_text);
            }
            else
            {
                start_idx = m_token_match.Index + m_token_match.Value.Length;
                m_token_match = m_token_match.NextMatch();                
            }
            if (m_token_match.Success)
            {
                if (!m_ignore_unknown_tokens)
                {
                    int len = m_token_match.Index - start_idx;
                    if (len > 0)
                    {
                        string glue = m_text.Substring(start_idx, len);
                        Match delim_match = m_delim_regex.Match(glue);
                        int inner_start_idx = 0;
                        while (delim_match.Success)
                        {
                            int inner_len = delim_match.Index - inner_start_idx;
                            if (inner_len > 0)
                            {
                                m_tokens.Enqueue(glue.Substring(inner_start_idx, inner_len));
                            }
                            inner_start_idx = delim_match.Index + delim_match.Value.Length;
                            delim_match = delim_match.NextMatch();
                        }
                    }
                }
                m_tokens.Enqueue(m_token_match.Value);
                if (!m_ignore_unknown_tokens && !m_token_match.NextMatch().Success) // tokenize tail
                {
                    start_idx = m_token_match.Index + m_token_match.Value.Length;
                    int len = m_text.Length - start_idx;
                    if (len > 0)
                    {
                        string glue = m_text.Substring(start_idx, len);
                        Match delim_match = m_delim_regex.Match(glue);
                        int inner_start_idx = 0;
                        while (delim_match.Success)
                        {
                            int inner_len = delim_match.Index - inner_start_idx;
                            if (inner_len > 0)
                            {
                                m_tokens.Enqueue(glue.Substring(inner_start_idx, inner_len));
                            }
                            inner_start_idx = delim_match.Index + delim_match.Value.Length;
                            delim_match = delim_match.NextMatch();
                        }
                    }
                }
            }
        }
        public string TokenRegex
        { 
            get { return m_token_regex.ToString(); }
            set { m_token_regex = new Regex(value, RegexOptions.Compiled); } // throws ArgumentNullException, ArgumentException
        }
        public bool IgnoreUnknownTokens
        {
            get { return m_ignore_unknown_tokens; }
            set { m_ignore_unknown_tokens = value; }
        }
        // *** IEnumerator<string> interface implementation ***
        public string Current
        {
            get 
            {
                Utils.ThrowException(m_tokens.Count == 0 ? new InvalidOperationException() : null); 
                return m_tokens.Peek();                
            }
        }
        object IEnumerator.Current
        {
            get { return Current; } // throws InvalidOperationException
        }
        public bool MoveNext()
        {
            if (m_tokens.Count > 0) { m_tokens.Dequeue(); }
            if (m_tokens.Count == 0) { GetMoreTokens(); }
            if (m_tokens.Count == 0) { Reset(); }
            return m_tokens.Count > 0;
        }
        public void Reset()
        {
            m_tokens.Clear();
            m_token_match = null;
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
                Utils.ThrowException(m_token_match != null ? new InvalidOperationException() : null);
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
        public void Save(BinarySerializer writer)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}