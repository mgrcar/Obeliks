/*==========================================================================;
 *
 *  (c) 2008-09 JSI.  All rights reserved.
 *
 *  File:          Stemmer.cs
 *  Version:       1.0
 *  Desc:		   Word stemmer interface and implementations
 *  Author:        Miha Grcar
 *  Created on:    Dec-2008
 *  Last modified: Jan-2009
 *  Revision:      Jan-2009
 *
 ***************************************************************************/

using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using LemmaSharp;

namespace Latino.TextMining
{
    /* .-----------------------------------------------------------------------
       |
       |  Interface IStemmer
       |
       '-----------------------------------------------------------------------
    */
    public interface IStemmer : ISerializable
    {
        string GetStem(string word);
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class PorterStemmer
       |
       '-----------------------------------------------------------------------
    **
       This code was originally written by Martin Porter in Java. The code was ported 
       to C# by André Hazelwood and Leif Azzopardi. Furthermore, it was slightly modified 
       to fit the LATINO coding style. See http://tartarus.org/~martin/PorterStemmer/
    
       This stemmer is for English and is faster than EnglishStemmer!
    */
    public class PorterStemmer : IStemmer
    {
        private char[] b;
        private int i, i_end, j, k; 
        private const int INC = 200;
        public PorterStemmer()
        {
            b = new char[INC];
            i = 0;
            i_end = 0;
        }
        public PorterStemmer(BinarySerializer reader) : this()
        { 
        }
        private bool Cons(int i)
        {
            switch (b[i])
            {
                case 'a':
                case 'e':
                case 'i':
                case 'o':
                case 'u': return false;
                case 'y': return (i == 0) ? true : !Cons(i - 1);
                default: return true;
            }
        }
        private int MeasConsSeq()
        {
            int n = 0;
            int i = 0;
            while (true)
            {
                if (i > j) { return n; }
                if (!Cons(i)) { break; }
                i++; 
            }
            i++;
            while (true)
            {
                while (true)
                {
                    if (i > j) { return n; }
                    if (Cons(i)) { break; }
                    i++;
                }
                i++;
                n++;
                while (true)
                {
                    if (i > j) { return n; }
                    if (!Cons(i)) { break; }
                    i++;
                }
                i++;
            }
        }
        private bool VowelInStem()
        {
            int i;
            for (i = 0; i <= j; i++)
            {
                if (!Cons(i)) { return true; }
            }
            return false;
        }
        private bool DoubleC(int j)
        {
            if (j < 1) { return false; }
            if (b[j] != b[j - 1]) { return false; }
            return Cons(j);
        }
        private bool ConsVowCons(int i)
        {
            if (i < 2 || !Cons(i) || Cons(i - 1) || !Cons(i - 2)) { return false; }
            int ch = b[i];
            if (ch == 'w' || ch == 'x' || ch == 'y') { return false; }
            return true;
        }
        private bool Ends(string s)
        {
            int l = s.Length;
            int o = k - l + 1;
            if (o < 0) { return false; }
            char[] sc = s.ToCharArray();
            for (int i = 0; i < l; i++)
            {
                if (b[o + i] != sc[i]) { return false; }
            }
            j = k - l;
            return true;
        }
        private void SetTo(string s)
        {
            int l = s.Length;
            int o = j + 1;
            char[] sc = s.ToCharArray();
            for (int i = 0; i < l; i++)
            {
                b[o + i] = sc[i];
            }
            k = j + l;
        }
        private void CondSetTo(string s)
        {
            if (MeasConsSeq() > 0) { SetTo(s); }
        }
        private void Step1()
        {
            if (b[k] == 's')
            {
                if (Ends("sses")) { k -= 2; }
                else if (Ends("ies")) { SetTo("i"); }
                else if (b[k - 1] != 's') { k--; }
            }
            if (Ends("eed"))
            {
                if (MeasConsSeq() > 0) { k--; }
            }
            else if ((Ends("ed") || Ends("ing")) && VowelInStem())
            {
                k = j; // *** j is computed by one of the two Ends calls
                if (Ends("at")) { SetTo("ate"); }
                else if (Ends("bl")) { SetTo("ble"); }
                else if (Ends("iz")) { SetTo("ize"); }
                else if (DoubleC(k))
                {
                    k--;
                    int ch = b[k];
                    if (ch == 'l' || ch == 's' || ch == 'z') { k++; }
                }
                else if (MeasConsSeq() == 1 && ConsVowCons(k)) { SetTo("e"); }
            }
        }
        private void Step2()
        {
            if (Ends("y") && VowelInStem()) { b[k] = 'i'; }
        }
        private void Step3()
        {
            if (k == 0) { return; }
            switch (b[k - 1])
            {
                case 'a':
                    if (Ends("ational")) { CondSetTo("ate"); break; }
                    if (Ends("tional")) { CondSetTo("tion"); break; }
                    break;
                case 'c':
                    if (Ends("enci")) { CondSetTo("ence"); break; }
                    if (Ends("anci")) { CondSetTo("ance"); break; }
                    break;
                case 'e':
                    if (Ends("izer")) { CondSetTo("ize"); break; }
                    break;
                case 'l':
                    if (Ends("bli")) { CondSetTo("ble"); break; }
                    if (Ends("alli")) { CondSetTo("al"); break; }
                    if (Ends("entli")) { CondSetTo("ent"); break; }
                    if (Ends("eli")) { CondSetTo("e"); break; }
                    if (Ends("ousli")) { CondSetTo("ous"); break; }
                    break;
                case 'o':
                    if (Ends("ization")) { CondSetTo("ize"); break; }
                    if (Ends("ation")) { CondSetTo("ate"); break; }
                    if (Ends("ator")) { CondSetTo("ate"); break; }
                    break;
                case 's':
                    if (Ends("alism")) { CondSetTo("al"); break; }
                    if (Ends("iveness")) { CondSetTo("ive"); break; }
                    if (Ends("fulness")) { CondSetTo("ful"); break; }
                    if (Ends("ousness")) { CondSetTo("ous"); break; }
                    break;
                case 't':
                    if (Ends("aliti")) { CondSetTo("al"); break; }
                    if (Ends("iviti")) { CondSetTo("ive"); break; }
                    if (Ends("biliti")) { CondSetTo("ble"); break; }
                    break;
                case 'g':
                    if (Ends("logi")) { CondSetTo("log"); break; }
                    break;
                default:
                    break;
            }
        }
        private void Step4()
        {
            switch (b[k])
            {
                case 'e':
                    if (Ends("icate")) { CondSetTo("ic"); break; }
                    if (Ends("ative")) { CondSetTo(""); break; }
                    if (Ends("alize")) { CondSetTo("al"); break; }
                    break;
                case 'i':
                    if (Ends("iciti")) { CondSetTo("ic"); break; }
                    break;
                case 'l':
                    if (Ends("ical")) { CondSetTo("ic"); break; }
                    if (Ends("ful")) { CondSetTo(""); break; }
                    break;
                case 's':
                    if (Ends("ness")) { CondSetTo(""); break; }
                    break;
            }
        }
        private void Step5()
        {
            if (k == 0) { return; }
            switch (b[k - 1])
            {
                case 'a':
                    if (Ends("al")) { break; } 
                    return;
                case 'c':
                    if (Ends("ance")) { break; }
                    if (Ends("ence")) { break; }
                    return;
                case 'e':
                    if (Ends("er")) { break; } 
                    return;
                case 'i':
                    if (Ends("ic")) { break; } 
                    return;
                case 'l':
                    if (Ends("able")) { break; }
                    if (Ends("ible")) { break; } 
                    return;
                case 'n':
                    if (Ends("ant")) { break; }
                    if (Ends("ement")) { break; }
                    if (Ends("ment")) { break; }
                    if (Ends("ent")) { break; } 
                    return;
                case 'o':
                    if (Ends("ion") && j >= 0 && (b[j] == 's' || b[j] == 't')) { break; }
                    if (Ends("ou")) { break; } 
                    return;
                case 's':
                    if (Ends("ism")) { break; } 
                    return;
                case 't':
                    if (Ends("ate")) { break; }
                    if (Ends("iti")) { break; } 
                    return;
                case 'u':
                    if (Ends("ous")) { break; } 
                    return;
                case 'v':
                    if (Ends("ive")) { break; } 
                    return;
                case 'z':
                    if (Ends("ize")) { break; } 
                    return;
                default:
                    return;
            }
            if (MeasConsSeq() > 1) { k = j; }
        }
        private void Step6()
        {
            j = k;
            if (b[k] == 'e')
            {
                int a = MeasConsSeq();
                if (a > 1 || a == 1 && !ConsVowCons(k - 1)) { k--; }
            }
            if (b[k] == 'l' && DoubleC(k) && MeasConsSeq() > 1) { k--; }
        }
        private void Stem()
        {
            k = i - 1;
            if (k > 1)
            {
                Step1();
                Step2();
                Step3();
                Step4();
                Step5();
                Step6();
            }
            i_end = k + 1;
            i = 0;
        }
        // *** IStemmer interface implementation ***
        public string GetStem(string word)
        {
            i = word.Length;
            char[] new_b = new char[i];
            for (int c = 0; c < i; c++)
            {
                new_b[c] = word[c];
            }
            b = new_b;
            Stem();
            return new string(b, 0, i_end);
        }
        // *** ISerializable interface implementation ***
        public void Save(BinarySerializer writer)
        {             
        }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class EnglishStemmer
       |
       '-----------------------------------------------------------------------
    */
    public class EnglishStemmer : IStemmer
    {
        private SF.Snowball.Ext.EnglishStemmer m_stemmer
            = new SF.Snowball.Ext.EnglishStemmer();
        public EnglishStemmer()
        {
        }
        public EnglishStemmer(BinarySerializer reader)
        {
        }
        // *** IStemmer interface implementation ***
        public string GetStem(string word)
        {
            m_stemmer.SetCurrent(word);
            m_stemmer.Stem();
            return m_stemmer.GetCurrent();
        }
        // *** ISerializable interface implementation ***
        public void Save(BinarySerializer writer)
        {
        }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class FrenchStemmer
       |
       '-----------------------------------------------------------------------
    */
    public class FrenchStemmer : IStemmer
    {
        private SF.Snowball.Ext.FrenchStemmer m_stemmer
            = new SF.Snowball.Ext.FrenchStemmer();
        public FrenchStemmer()
        { 
        }
        public FrenchStemmer(BinarySerializer reader)
        {         
        }
        // *** IStemmer interface implementation ***
        public string GetStem(string word)
        {
            m_stemmer.SetCurrent(word);
            m_stemmer.Stem();
            return m_stemmer.GetCurrent();
        }
        // *** ISerializable interface implementation ***
        public void Save(BinarySerializer writer)
        {             
        }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class SpanishStemmer
       |
       '-----------------------------------------------------------------------
    */
    public class SpanishStemmer : IStemmer
    {
        private SF.Snowball.Ext.SpanishStemmer m_stemmer
            = new SF.Snowball.Ext.SpanishStemmer();
        public SpanishStemmer()
        {
        }
        public SpanishStemmer(BinarySerializer reader)
        {
        }
        // *** IStemmer interface implementation ***
        public string GetStem(string word)
        {
            m_stemmer.SetCurrent(word);
            m_stemmer.Stem();
            return m_stemmer.GetCurrent();
        }
        // *** ISerializable interface implementation ***
        public void Save(BinarySerializer writer)
        {
        }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class GermanStemmer
       |
       '-----------------------------------------------------------------------
    */
    public class GermanStemmer : IStemmer
    {
        private SF.Snowball.Ext.German2Stemmer m_stemmer
            = new SF.Snowball.Ext.German2Stemmer();
        public GermanStemmer()
        {
        }
        public GermanStemmer(BinarySerializer reader)
        {
        }
        // *** IStemmer interface implementation ***
        public string GetStem(string word)
        {
            m_stemmer.SetCurrent(word);
            m_stemmer.Stem();
            return m_stemmer.GetCurrent();
        }
        // *** ISerializable interface implementation ***
        public void Save(BinarySerializer writer)
        {
        }
    }
}
