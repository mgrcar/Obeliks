/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          SloveneLemmatizer.cs
 *  Version:       1.0
 *  Desc:		   Slovene lemmatizer
 *  Author:        Miha Grcar
 *  Created on:    Jan-2009
 *  Last modified: Jan-2009
 *  Revision:      Aug-2009
 *
 ***************************************************************************/

using LemmaSharp;

namespace Latino.TextMining
{
    /* .-----------------------------------------------------------------------
       |
       |  Class SloveneLemmatizer
       |
       '-----------------------------------------------------------------------
    */
    public class SloveneLemmatizer : IStemmer 
    {
        private static Lemmatizer m_lemmatizer;
        static SloveneLemmatizer()
        {
            m_lemmatizer = new LemmatizerPrebuiltCompressed(LanguagePrebuilt.Slovene);
        }
        // *** IStemmer interface implementation ***
        public string GetStem(string word)
        {
            return m_lemmatizer.Lemmatize(word);
        }
        // *** ISerializable interface implementation ***
        public void Save(BinarySerializer writer)
        {
        }
    }
}