/***************************************************************************
 *    LemmaGen / LemmaSharp Library: Fast and efficient lemmatization.
 *    
 *    The MIT License (MIT)
 *    Copyright (C) 2009  Matjaz Jursic @ Jozef Stefan Institute
 *
 *    Permission is hereby granted, free of charge, to any person obtaining 
 *    a copy of this software and associated documentation files (the 
 *    "Software"), to deal in the Software without restriction, including 
 *    without limitation the rights to use, copy, modify, merge, publish, 
 *    distribute, sublicense, and/or sell copies of the Software, and to 
 *    permit persons to whom the Software is furnished to do so, subject to 
 *    the following conditions:
 *
 *    The above copyright notice and this permission notice shall be 
 *    included in all copies or substantial portions of the Software.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 *    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
 *    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
 *    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY 
 *    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
 *    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
 *    SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 ***************************************************************************/

using System;
namespace LemmaSharp {
    public interface ILemmatizerModel {
        /// <summary>
        /// Standard lemmatization interface (by default letter casing is NOT ignored).
        /// </summary>
        /// <param name="sWord">word to be lemmatized</param>
        /// <returns>Lemmatized word.</returns>
        string Lemmatize(string sWord);
        /// <summary>
        /// Extended lemamtization interface with more options
        /// </summary>
        /// <param name="sWord">word to be lemmatized</param>
        /// <param name="ignoreCase">If true than casing will be ignored. If set to false, than lemmatizer will match the longest rule it knows but requiering same casing of rule and word.</param>
        /// <returns>Lemmatized word.</returns>
        string Lemmatize(string sWord, bool ignoreCase);

        string ToString();
    }
}
