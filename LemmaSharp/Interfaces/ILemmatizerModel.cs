/***************************************************************************
 *    LemmaGen / LemmaSharp Library: Fast and efficient lemmatization.
 *    Copyright (C) 2009  Matjaz Jursic @ Jozef Stefan Institute
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
