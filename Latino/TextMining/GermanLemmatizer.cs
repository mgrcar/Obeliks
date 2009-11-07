/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          GermanLemmatizer.cs
 *  Version:       1.0
 *  Desc:		   German lemmatizer
 *  Author:        Miha Grcar
 *  Created on:    Aug-2009
 *  Last modified: Aug-2009
 *  Revision:      Aug-2009
 *
 ***************************************************************************/

using LemmaSharp;

namespace Latino.TextMining
{
    /* .-----------------------------------------------------------------------
       |
       |  Class GermanLemmatizer
       |
       '-----------------------------------------------------------------------
    */
    public class GermanLemmatizer : IStemmer
    {
        private static Lemmatizer m_lemmatizer;
        static GermanLemmatizer()
        {
            m_lemmatizer = new LemmatizerPrebuiltCompressed(LanguagePrebuilt.German);
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