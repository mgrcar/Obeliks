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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace LemmaSharp {
    /// <summary>
    /// These are the lemmagen algorithm settings that affect speed/power of the learning and lemmatizing algorithm.
    /// TODO this class will be probbably removed in the future.
    /// </summary>
    [Serializable()]
    public class LemmatizerSettings : ISerializable {
        #region Constructor(s) & Destructor(s)

        public LemmatizerSettings() { } 

        #endregion

        #region Sub-Structures

        /// <summary>
        /// How algorithm considers msd tags.
        /// </summary>
        public enum MsdConsideration {
            /// <summary>
            /// Completely ignores mds tags (join examples with different tags and sum their weihgts).
            /// </summary>
            Ignore,
            /// <summary>
            /// Same examples with different msd's are not considered equal and joined.
            /// </summary>
            Distinct,
            /// <summary>
            /// Joins examples with different tags (concatenates all msd tags).
            /// </summary>
            JoinAll,
            /// <summary>
            /// Joins examples with different tags (concatenates just distinct msd tags - somehow slower).
            /// </summary>
            JoinDistinct,
            /// <summary>
            /// Joins examples with different tags (new tag is the left to right substring that all joined examples share).
            /// </summary>
            JoinSameSubstring
        }         

        #endregion

        #region Public Variables

        /// <summary>
        /// True if from string should be included in rule identifier ([from]->[to]). False if just length of from string is used ([#len]->[to]).
        /// </summary>
        public bool bUseFromInRules = true;
        /// <summary>
        /// Specification how algorithm considers msd tags.
        /// </summary>
        public MsdConsideration eMsdConsider = MsdConsideration.Distinct;
        /// <summary>
        /// How many of the best rules are kept in memory for each node. Zero means unlimited.
        /// </summary>
        public int iMaxRulesPerNode = 0;
        /// <summary>
        /// True: build proccess uses few more hevristics to build first left to right lemmatizer (lemmatizes front of the word);
        /// False: (default) do not create additional front lemmatizer;
        /// </summary>
        public bool bBuildFrontLemmatizer = false;
        /// <summary>
        /// True: the model will store all known words in full -> the same as having lexicon to check each word before using lemmatizer and then use the lemmatizer only for unknown words;
        /// False: (default) the model will try to mimimize the size and will store the full forms of words only in the case if they add new knowledge;
        /// </summary>
        public bool bStoreAllFullKnownWords = false;

        //TODO add to serialization and add description
        public bool bUseMsdSplitTreeOptimization = false;

        #endregion

        #region Cloneable functions

        public LemmatizerSettings CloneDeep() {
            return new LemmatizerSettings() {
                bUseFromInRules = this.bUseFromInRules,
                eMsdConsider = this.eMsdConsider,
                iMaxRulesPerNode = this.iMaxRulesPerNode,
                bBuildFrontLemmatizer = this.bBuildFrontLemmatizer,   
                bStoreAllFullKnownWords = this.bStoreAllFullKnownWords
            };            
        }

        #endregion

        #region Serialization Functions (ISerializable)

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("bUseFromInRules", bUseFromInRules);
            info.AddValue("eMsdConsider", eMsdConsider);
            info.AddValue("iMaxRulesPerNode", iMaxRulesPerNode);
            info.AddValue("bBuildFrontLemmatizer", bBuildFrontLemmatizer);
            info.AddValue("bStoreAllFullKnownWords", bStoreAllFullKnownWords);
            info.AddValue("bUseMsdSplitTreeOptimization", bUseMsdSplitTreeOptimization);
        }
        public LemmatizerSettings(SerializationInfo info, StreamingContext context) {
            bUseFromInRules = info.GetBoolean("bUseFromInRules");
            eMsdConsider = (MsdConsideration)info.GetValue("eMsdConsider", typeof(MsdConsideration));
            iMaxRulesPerNode = info.GetInt32("iMaxRulesPerNode");
            bBuildFrontLemmatizer = info.GetBoolean("bBuildFrontLemmatizer");
            bStoreAllFullKnownWords = info.GetBoolean("bStoreAllFullKnownWords");
            bUseMsdSplitTreeOptimization = info.GetBoolean("bUseMsdSplitTreeOptimization");
        }

        #endregion
        #region Serialization Functions (Binary)

        public void Serialize(BinaryWriter binWrt) {
            binWrt.Write(bUseFromInRules);
            binWrt.Write((int)eMsdConsider);
            binWrt.Write(iMaxRulesPerNode);
            binWrt.Write(bBuildFrontLemmatizer);
            binWrt.Write(bStoreAllFullKnownWords);
            binWrt.Write(bUseMsdSplitTreeOptimization);
        }
        public void Deserialize(BinaryReader binRead) {
            bUseFromInRules = binRead.ReadBoolean();
            eMsdConsider = (MsdConsideration)binRead.ReadInt32();
            iMaxRulesPerNode = binRead.ReadInt32();
            bBuildFrontLemmatizer = binRead.ReadBoolean();
            bStoreAllFullKnownWords = binRead.ReadBoolean();
            bUseMsdSplitTreeOptimization = binRead.ReadBoolean();
        }
        public LemmatizerSettings(System.IO.BinaryReader binRead) {
            this.Deserialize(binRead);
        }

        #endregion
        #region Serialization Functions (Latino)
        
        #if LATINO

        public void Save(Latino.BinarySerializer binWrt) {
            binWrt.WriteBool(bUseFromInRules);
            binWrt.WriteInt((int)eMsdConsider);
            binWrt.WriteInt(iMaxRulesPerNode);
            binWrt.WriteBool(bBuildFrontLemmatizer);
            binWrt.WriteBool(bStoreAllFullKnownWords);
            binWrt.WriteBool(bUseMsdSplitTreeOptimization);
        }

        public void Load(Latino.BinarySerializer binRead) {
            bUseFromInRules = binRead.ReadBool();
            eMsdConsider = (MsdConsideration)binRead.ReadInt();
            iMaxRulesPerNode = binRead.ReadInt();
            bBuildFrontLemmatizer = binRead.ReadBool();
            bStoreAllFullKnownWords = binRead.ReadBool();
            bUseMsdSplitTreeOptimization = binRead.ReadBool();
        }

        public LemmatizerSettings(Latino.BinarySerializer reader) {
            Load(reader);
        }

        #endif

        #endregion

    }
}
