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

namespace LemmaSharp {
    [Serializable()]
    public class LemmaTreeNode: ILemmatizerModel {
        #region Private Variables

        //settings
        private LemmatizerSettings lsett; 

        //tree structure references
        private Dictionary<char, LemmaTreeNode> dictSubNodes;
        private LemmaTreeNode ltnParentNode;

        //essential node properties
        private int iSimilarity; //similarity among all words in this node
        private string sCondition; //suffix that must match in order to lemmatize
        private bool bWholeWord; //true if condition has to match to whole word

        //rules and weights;
        private LemmaRule lrBestRule; //the best rule to be applied when lemmatizing
        private RuleWeighted[] aBestRules; //list of best rules
        private Dictionary<string, RuleWeighted[]> dictMsdBestRules; //list of best rules for different msds
        private Dictionary<string, double> dictMsdWeights; //list of all msds and their weights
        private double dWeight;

        //source of this node
        private int iStart;
        private int iEnd;
        private ExampleList elExamples;

        #endregion

        #region Constructor(s) & Destructor(s)

        private LemmaTreeNode(LemmatizerSettings lsett) {
            this.lsett = lsett;
        }
        public LemmaTreeNode(LemmatizerSettings lsett, ExampleList elExamples)
            : this(lsett, elExamples, 0, elExamples.Count-1, null) {
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lsett"></param>
        /// <param name="elExamples"></param>
        /// <param name="iStart">Index of the first word of the current group</param>
        /// <param name="iEnd">Index of the last word of the current group</param>
        /// <param name="ltnParentNode"></param>
        private LemmaTreeNode(LemmatizerSettings lsett, ExampleList elExamples, int iStart, int iEnd, LemmaTreeNode ltnParentNode) : this(lsett) {
            this.ltnParentNode = ltnParentNode;
            this.dictSubNodes = null;

            this.iStart = iStart;
            this.iEnd = iEnd;
            this.elExamples = elExamples;

            if (iStart >= elExamples.Count || iEnd >= elExamples.Count || iStart > iEnd) {
                //just a precaution - should not happen - TODO check
                lrBestRule = elExamples.Rules.DefaultRule;
                aBestRules = new RuleWeighted[1];
                aBestRules[0] = new RuleWeighted(lrBestRule, 0);
                dWeight = 0;
                return;
            }

            this.iSimilarity = elExamples[iStart].Similarity(elExamples[iEnd]);
            int iConditionLength = Math.Min((ltnParentNode == null ? 0 : iSimilarity), elExamples[iStart].Word.Length);
            this.sCondition = elExamples[iStart].Word.Substring(elExamples[iStart].Word.Length - iConditionLength);
            this.bWholeWord = ltnParentNode == null ? false : elExamples[iEnd].Word.Length == ltnParentNode.iSimilarity;

            FindBestRules();
            AddSubAll();


            //TODO check this heuristics, can be problematic when there are more applicable rules
            if (lsett.eMsdConsider != LemmatizerSettings.MsdConsideration.Distinct)
            if (dictSubNodes != null) {
                List<KeyValuePair<char, LemmaTreeNode>> lReplaceNodes = new List<KeyValuePair<char, LemmaTreeNode>>();
                foreach (KeyValuePair<char, LemmaTreeNode> kvpChild in dictSubNodes)
                    if (kvpChild.Value.dictSubNodes != null && kvpChild.Value.dictSubNodes.Count == 1) {
                        IEnumerator<LemmaTreeNode> enumChildChild = kvpChild.Value.dictSubNodes.Values.GetEnumerator();
                        enumChildChild.MoveNext();
                        LemmaTreeNode ltrChildChild = enumChildChild.Current;
                        if (kvpChild.Value.lrBestRule == lrBestRule)
                            lReplaceNodes.Add(new KeyValuePair<char, LemmaTreeNode>(kvpChild.Key, ltrChildChild));
                    }
                foreach (KeyValuePair<char, LemmaTreeNode> kvpChild in lReplaceNodes) {
                    dictSubNodes[kvpChild.Key] = kvpChild.Value;
                    kvpChild.Value.ltnParentNode = this;
                }

            }

        }

        #endregion

        #region Public Properties

        public int TreeSize {
            get {
                int iCount = 1;
                if (dictSubNodes != null)
                    foreach (LemmaTreeNode ltnChild in dictSubNodes.Values)
                        iCount += ltnChild.TreeSize;
                return iCount;
            }
        }
        public double Weight {
            get {
                return dWeight;
            }
        }

        #endregion

        #region Essential Class Functions (building model)

        private void FindBestRules() {
            /*
             *  LINQ SPEED TEST (Slower than current metodology)
             * 
             
            List<LemmaExample> leApplicable = new List<LemmaExample>();
            for (int iExm = iStart; iExm <= iEnd; iExm++)
                if (elExamples[iExm].Rule.IsApplicableToGroup(sCondition.Length))
                    leApplicable.Add(elExamples[iExm]);

            List<KeyValuePair<LemmaRule, double>> lBestRules = new List<KeyValuePair<LemmaRule,double>>();
            lBestRules.AddRange(
            leApplicable.
                GroupBy<LemmaExample, LemmaRule, double, KeyValuePair<LemmaRule, double>>(
                    le => le.Rule,
                    le => le.Weight,
                    (lr, enumDbl) => new KeyValuePair<LemmaRule, double>(lr, enumDbl.Aggregate((acc, curr) => acc + curr))
                ).
                OrderBy(kvpLrWght=>kvpLrWght.Value)
            );

            if (lBestRules.Count > 0)
                lrBestRule = lBestRules[0].Key;
            else {
                lrBestRule = elExamples.Rules.DefaultRule;

            }
            */

            dWeight = 0;

            //calculate dWeight of whole node and calculates qualities for all rules
            Dictionary<LemmaRule, double> dictApplicableRules = new Dictionary<LemmaRule,double>();
            Dictionary<LemmaRule, Dictionary<string, double>> dictApplicableRulesMsd = null;
            if (lsett.eMsdConsider == LemmatizerSettings.MsdConsideration.Distinct) {
                dictApplicableRulesMsd = new Dictionary<LemmaRule, Dictionary<string, double>>();
                dictMsdWeights = new Dictionary<string,double>();
            }
            //dictApplicableRules.Add(elExamples.Rules.DefaultRule, 0);
            while (dictApplicableRules.Count == 0) {
                for (int iExm = iStart; iExm <= iEnd; iExm++) {
                    LemmaRule lr = elExamples[iExm].Rule;
                    double dExmWeight = elExamples[iExm].Weight;
                    dWeight += dExmWeight;

                    //Add msd weights if needed
                    if (lsett.eMsdConsider == LemmatizerSettings.MsdConsideration.Distinct) {
                        string msd = elExamples[iExm].Msd;

                        if (dictMsdWeights.ContainsKey(msd))
                            dictMsdWeights[msd] += dExmWeight;
                        else
                            dictMsdWeights.Add(msd, dExmWeight);
                    }

                    if (lr.IsApplicableToGroup(sCondition.Length)) {
                        if (dictApplicableRules.ContainsKey(lr))
                            dictApplicableRules[lr] += dExmWeight;
                        else {
                            dictApplicableRules.Add(lr, dExmWeight);
                            //do the same with rules separated by msd
                            if (lsett.eMsdConsider == LemmatizerSettings.MsdConsideration.Distinct)
                                dictApplicableRulesMsd.Add(lr, new Dictionary<string, double>());
                        }
                        //do the same (dictApplicableRules[lr] += dExmWeight;) with rules separated by msd
                        if (lsett.eMsdConsider == LemmatizerSettings.MsdConsideration.Distinct) {
                            Dictionary<string, double> dictApplicableRuleMsd = dictApplicableRulesMsd[lr];
                            string msd = elExamples[iExm].Msd;

                            if (dictApplicableRuleMsd.ContainsKey(msd))
                                dictApplicableRuleMsd[msd] += dExmWeight;
                            else
                                dictApplicableRuleMsd.Add(msd, dExmWeight);
                        }
                    }
                }
                //if none found then increase condition length or add some default appliable rule
                if (dictApplicableRules.Count == 0) {
                    if (this.sCondition.Length < iSimilarity)
                        this.sCondition = elExamples[iStart].Word.Substring(elExamples[iStart].Word.Length - (sCondition.Length + 1));
                    else {
                        //TODO preveri hevristiko, mogoce je bolje ce se doda default rule namesto rulea od starsa
                        dictApplicableRules.Add(ltnParentNode.lrBestRule, 0);
                        //do the same with rules separated by msd
                        if (lsett.eMsdConsider == LemmatizerSettings.MsdConsideration.Distinct) {
                            Dictionary<string, double> dictApplicableRuleMsd = new Dictionary<string,double>();
                            dictApplicableRulesMsd.Add(ltnParentNode.lrBestRule, dictApplicableRuleMsd);
                            for (int iExm = iStart; iExm <= iEnd; iExm++) {
                                string msd = elExamples[iExm].Msd;
                                if(!dictApplicableRuleMsd.ContainsKey(msd))
                                    dictApplicableRuleMsd.Add(msd, 0);
                            }
                        }
                    }
                }
            }
            
            //TODO can optimize this step using sorted list (dont add if it's worse than the worst)
            List<RuleWeighted> lSortedRules = new List<RuleWeighted>();
            foreach (KeyValuePair<LemmaRule, double> kvp in dictApplicableRules)
                lSortedRules.Add(new RuleWeighted(kvp.Key, kvp.Value / dWeight));
            lSortedRules.Sort();

            //keep just best iMaxRulesPerNode rules
            int iNumRules = lSortedRules.Count;
            if (lsett.iMaxRulesPerNode > 0) iNumRules = Math.Min(lSortedRules.Count, lsett.iMaxRulesPerNode);

            aBestRules = new RuleWeighted[iNumRules];
            for (int iRule = 0; iRule < iNumRules; iRule++) {
                aBestRules[iRule] = lSortedRules[iRule];
            }
            
            //set best rule
            lrBestRule = aBestRules[0].Rule;
            
            //TODO must check if this hevristics is OK (to privilige parent rule in case of tie)
            if (ltnParentNode != null)
                for (int iRule = 0; iRule < lSortedRules.Count && lSortedRules[iRule].Weight==lSortedRules[0].Weight; iRule++) {
                    if (lSortedRules[iRule].Rule == ltnParentNode.lrBestRule) {
                        lrBestRule = lSortedRules[iRule].Rule;
                        break;
                    }
                }

            //do the same with rules separated by msd
            if (lsett.eMsdConsider == LemmatizerSettings.MsdConsideration.Distinct) {
                Dictionary<string, List<RuleWeighted>>  dictMsdBestRulesTemp = new Dictionary<string, List<RuleWeighted>>();

                foreach (KeyValuePair<LemmaRule, Dictionary<string, double>> ruleMsdWeight in dictApplicableRulesMsd)
                    foreach (KeyValuePair<string, double> msdWeight in ruleMsdWeight.Value) {
                        if (!dictMsdBestRulesTemp.ContainsKey(msdWeight.Key))
                            dictMsdBestRulesTemp.Add(msdWeight.Key, new List<RuleWeighted>());
                        dictMsdBestRulesTemp[msdWeight.Key].Add(new RuleWeighted(ruleMsdWeight.Key, msdWeight.Value / dWeight));
                    }
                
                dictMsdBestRules = new Dictionary<string, RuleWeighted[]>();
                foreach (KeyValuePair<string, List<RuleWeighted>> msdRuleWeight in dictMsdBestRulesTemp) {
                    msdRuleWeight.Value.Sort();
                    dictMsdBestRules.Add(msdRuleWeight.Key, msdRuleWeight.Value.ToArray());
                }                    
            }


            
             
        }
        private void AddSubAll() {
            int iStartGroup = iStart;
            char chCharPrev = '\0';
            bool bSubGroupNeeded = false;
            for (int iWrd = iStart; iWrd <= iEnd; iWrd++) {
                string sWord = elExamples[iWrd].Word;
                char chCharThis = sWord.Length > iSimilarity ? sWord[sWord.Length - 1 - iSimilarity] : '\0';

                if (iWrd != iStart && chCharPrev != chCharThis) {
                    if (bSubGroupNeeded) {
                        AddSub(iStartGroup, iWrd - 1, chCharPrev);
                        bSubGroupNeeded = false;
                    }
                    iStartGroup = iWrd;
                }

                //TODO check out bSubGroupNeeded when there are multiple posible rules (not just lrBestRule)
                if (elExamples[iWrd].Rule != lrBestRule)
                    bSubGroupNeeded = true;
                if (lsett.eMsdConsider == LemmatizerSettings.MsdConsideration.Distinct) {
                    string msd = elExamples[iWrd].Msd;
                    if (dictMsdBestRules.ContainsKey(msd) && dictMsdBestRules[msd][0].Rule != elExamples[iWrd].Rule)
                        bSubGroupNeeded = true;
                }

                chCharPrev = chCharThis;
            }
            if (bSubGroupNeeded && iStartGroup != iStart) AddSub(iStartGroup, iEnd, chCharPrev);
        }
        private void AddSub(int iStart, int iEnd, char chChar) {
            LemmaTreeNode ltnSub = new LemmaTreeNode(lsett, elExamples, iStart, iEnd, this);
            
            //TODO - maybe not realy appropriate because loosing statisitcs from multiple possible rules

            if (lsett.eMsdConsider != LemmatizerSettings.MsdConsideration.Distinct)
            if (ltnSub.lrBestRule == lrBestRule && ltnSub.dictSubNodes == null) return;

            if (dictSubNodes == null) dictSubNodes = new Dictionary<char, LemmaTreeNode>();
            dictSubNodes.Add(chChar, ltnSub);
        }

        #endregion
        #region Essential Class Functions (running model = lemmatizing)

        public bool ConditionSatisfied(string sWord, bool ignoreCase, string sMsd) {
            //if (bWholeWord)
            //    return sWord == sCondition;
            //else 
            //    return sWord.EndsWith(sCondition);

            int iDiff = sWord.Length - sCondition.Length;
            if (iDiff < 0 || (bWholeWord && iDiff > 0)) return false;

            int iWrdEnd = sCondition.Length - ltnParentNode.sCondition.Length - 1;
            if (ignoreCase) {
                //try correct & inversed casing
                for (int iChar = 0; iChar < iWrdEnd; iChar++)
                    if (Char.ToLower(sCondition[iChar]) != Char.ToLower(sWord[iChar + iDiff]))
                        return false;
            }
            else {
                //try correct casing
                for (int iChar = 0; iChar < iWrdEnd; iChar++)
                    if (sCondition[iChar] != sWord[iChar + iDiff])
                        return false;
            }

            if (lsett.eMsdConsider == LemmatizerSettings.MsdConsideration.Distinct && sMsd != null)
                return dictMsdWeights.ContainsKey(sMsd);

            return true;
        }
        public string Lemmatize(string sWord) {
            return Lemmatize(sWord, false, null);
        }
        public string Lemmatize(string sWord, bool ignoreCase) {
            return Lemmatize(sWord, ignoreCase, null);
        }
        public string Lemmatize(string sWord, bool ignoreCase, string sMsd) {
            if (sWord.Length >= iSimilarity && dictSubNodes != null) {
                //try first correct casing
                char chChar = sWord.Length > iSimilarity ? sWord[sWord.Length - 1 - iSimilarity] : '\0';
                if (dictSubNodes.ContainsKey(chChar) && dictSubNodes[chChar].ConditionSatisfied(sWord, ignoreCase, sMsd))
                    return dictSubNodes[chChar].Lemmatize(sWord, ignoreCase, sMsd);
                
                //try also inversed casing
                if (ignoreCase && char.IsLetter(chChar)) {
                    char chCharInvert = char.IsLower(chChar) ? char.ToUpper(chChar) : char.ToLower(chChar);
                    if (dictSubNodes.ContainsKey(chCharInvert) && dictSubNodes[chCharInvert].ConditionSatisfied(sWord, ignoreCase, sMsd))
                        return dictSubNodes[chCharInvert].Lemmatize(sWord, ignoreCase, sMsd);
                }
            }
            if (lsett.eMsdConsider == LemmatizerSettings.MsdConsideration.Distinct && sMsd != null) {
                LemmaRule lrBestValid = null;
                LemmaTreeNode ltnValid = this;

                bool useNoMsd = false;
                while (lrBestValid == null && useNoMsd == false) {
                    if (ltnValid.dictMsdBestRules.ContainsKey(sMsd))
                        lrBestValid = ltnValid.dictMsdBestRules[sMsd][0].Rule;
                    else {
                        if (ltnValid.ltnParentNode != null)
                            ltnValid = ltnValid.ltnParentNode;
                        else
                            useNoMsd = true;
                    }
                }

                if (useNoMsd)
                    return ltnValid.lrBestRule.Lemmatize(sWord);
                else
                    return lrBestValid.Lemmatize(sWord);
            }
            else
                return lrBestRule.Lemmatize(sWord);
        }
        #endregion

        #region Output Functions (ToString)

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            ToString(sb, 0);
            return sb.ToString();
        }
        private void ToString(StringBuilder sb, int iLevel) {
            sb.Append(new string('\t', iLevel));
            sb.Append("Suffix=\"" + (bWholeWord?"^":"") + sCondition + "\"; ");
            sb.Append("BestRule=" + lrBestRule.ToString() + "; ");
            sb.Append("Weight=" + dWeight + "; ");
            if (aBestRules != null) {
                if (aBestRules.Length > 0)
                    sb.Append("Cover=" + aBestRules[0].Weight + "; ");
                if (aBestRules.Length > 1) {
                    sb.AppendFormat("AllRules={0}", aBestRules.Length);
                    foreach (RuleWeighted rw in aBestRules) {
                        sb.AppendLine();
                        sb.Append(new string('\t', iLevel + 2));
                        sb.Append(rw.ToString(dWeight));
                    }
                }
                else {
                    sb.Append("AllRules={only one/best rule}");
                }
            }
            sb.Append("; ");

            if (lsett.eMsdConsider == LemmatizerSettings.MsdConsideration.Distinct) {
                sb.AppendLine();
                sb.Append(new string('\t', iLevel + 2));
                foreach (KeyValuePair<string, double> kvp in dictMsdWeights) {
                    sb.AppendFormat("{0}:{1},", kvp.Key, kvp.Value);
                }
            }
            if (lsett.eMsdConsider == LemmatizerSettings.MsdConsideration.Distinct) {
                foreach (KeyValuePair<string, RuleWeighted[]> kvp in dictMsdBestRules) {
                    sb.AppendLine();
                    sb.Append(new string('\t', iLevel + 2));
                    sb.AppendFormat("Rules for msd: \"{0}\" (num:{1})", kvp.Key, kvp.Value.Length);
                    if (kvp.Value.Length == 1) {
                        sb.Append(kvp.Value[0].ToString(dWeight));
                    }
                    else {
                        foreach (RuleWeighted rw in kvp.Value) {
                            sb.AppendLine();
                            sb.Append(new string('\t', iLevel + 3));
                            sb.Append(rw.ToString(dWeight));
                        }
                    }
                }
            }
            sb.AppendLine();

            if (dictSubNodes != null)
                foreach (LemmaTreeNode ltnChild in dictSubNodes.Values)
                    ltnChild.ToString(sb, iLevel + 1);
        }

        #endregion

        #region Serialization Functions (Binary)

        public void Serialize(BinaryWriter binWrt) {
            //next variables are not serialized since serialization is handeled one level above (in a lemmatizer)
            //  - lsett
            //  - elExamples
            //  - ltnParentNode

            binWrt.Write(dictSubNodes != null);
            if (dictSubNodes != null) {
                binWrt.Write(dictSubNodes.Count);
                foreach (KeyValuePair<char, LemmaTreeNode> kvp in dictSubNodes) {
                    binWrt.Write(kvp.Key);
                    kvp.Value.Serialize(binWrt);
                }
            }

            binWrt.Write(iSimilarity);
            binWrt.Write(sCondition);
            binWrt.Write(bWholeWord);

            binWrt.Write(lrBestRule.Signature);
            binWrt.Write(aBestRules.Length);
            for (int i = 0; i < aBestRules.Length; i++) {
                binWrt.Write(aBestRules[i].Rule.Signature);
                binWrt.Write(aBestRules[i].Weight);
            }
            binWrt.Write(dWeight);

            //serialize dictMsdBestRules dictionary
            if (dictMsdBestRules == null)
                binWrt.Write(-1);
            else {
                binWrt.Write(dictMsdBestRules.Count);
                foreach (KeyValuePair<string, RuleWeighted[]> rules in dictMsdBestRules) {
                    binWrt.Write(rules.Key);
                    if (rules.Value == null)
                        binWrt.Write(-1);
                    else {
                        binWrt.Write(rules.Value.Length);
                        foreach (RuleWeighted rule in rules.Value) {
                            binWrt.Write(rule.Rule.Signature);
                            binWrt.Write(rule.Weight);
                        }
                    }
                }
            }

            //serialize dictMsdWeights dictionary
            if (dictMsdWeights == null)
                binWrt.Write(-1);
            else {
                binWrt.Write(dictMsdWeights.Count);
                foreach (KeyValuePair<string, double> msdWeight in dictMsdWeights) {
                    binWrt.Write(msdWeight.Key);
                    binWrt.Write(msdWeight.Value);
                }
            }

            binWrt.Write(iStart);
            binWrt.Write(iEnd);
        }
        public void Deserialize(BinaryReader binRead, LemmatizerSettings lsett, ExampleList elExamples, LemmaTreeNode ltnParentNode) {
            this.lsett = lsett;

            if (binRead.ReadBoolean()) {
                dictSubNodes = new Dictionary<char, LemmaTreeNode>();
                int iCount = binRead.ReadInt32();
                for (int i = 0; i < iCount; i++) {
                    char cKey = binRead.ReadChar();
                    LemmaTreeNode ltrSub = new LemmaTreeNode(binRead, this.lsett, elExamples, this);
                    dictSubNodes.Add(cKey, ltrSub);
                }
            }
            else
                dictSubNodes = null;

            this.ltnParentNode = ltnParentNode;

            iSimilarity = binRead.ReadInt32();
            sCondition = binRead.ReadString();
            bWholeWord = binRead.ReadBoolean();

            lrBestRule = elExamples.Rules[binRead.ReadString()];

            int iCountBest = binRead.ReadInt32();
            aBestRules = new RuleWeighted[iCountBest];
            for (int i = 0; i < iCountBest; i++)
                aBestRules[i] = new RuleWeighted(elExamples.Rules[binRead.ReadString()], binRead.ReadDouble());

            dWeight = binRead.ReadDouble();

            //deserialize dictMsdBestRules dictionary
            int dictMsdBestRulesCount = binRead.ReadInt32();
            if (dictMsdBestRulesCount == -1)
                dictMsdBestRules = null;
            else {
                dictMsdBestRules = new Dictionary<string, RuleWeighted[]>();
                for (int msdId = 0; msdId < dictMsdBestRulesCount; msdId++) {
                    string sMsd = binRead.ReadString();
                    RuleWeighted[] lRuleWeighted;
                    int ruleWeightedCount = binRead.ReadInt32();
                    if (ruleWeightedCount == -1)
                        lRuleWeighted = null;
                    else {
                        lRuleWeighted = new RuleWeighted[ruleWeightedCount];
                        for (int ruleId = 0; ruleId < ruleWeightedCount; ruleId++) {
                            string ruleSignature = binRead.ReadString();
                            double ruleWeight = binRead.ReadDouble();
                            LemmaRule rule = elExamples.Rules[ruleSignature];
                            lRuleWeighted[ruleId] = new RuleWeighted(rule, ruleWeight);
                        }
                    }
                    dictMsdBestRules.Add(sMsd, lRuleWeighted);
                }
            }

            //deserialize dictMsdWeights dictionary
            int dictMsdWeightsCount = binRead.ReadInt32();
            if (dictMsdWeightsCount == -1)
                dictMsdWeights = null;
            else {
                dictMsdWeights = new Dictionary<string, double>();
                for (int msdId = 0; msdId < dictMsdWeightsCount; msdId++) {
                    string sMsd = binRead.ReadString();
                    double dMsdWeight = binRead.ReadDouble();
                    dictMsdWeights.Add(sMsd, dMsdWeight);
                }
            }

            iStart = binRead.ReadInt32();
            iEnd = binRead.ReadInt32();
            this.elExamples = elExamples;
        }
        public LemmaTreeNode(BinaryReader binRead, LemmatizerSettings lsett, ExampleList elExamples, LemmaTreeNode ltnParentNode) {
            Deserialize(binRead, lsett, elExamples, ltnParentNode);
        }

        #endregion
        #region Serialization Functions (Latino)
        #if LATINO

        public void Save(Latino.BinarySerializer binWrt) {
            binWrt.WriteBool(dictSubNodes != null);
            if (dictSubNodes != null) {
                binWrt.WriteInt(dictSubNodes.Count);
                foreach (KeyValuePair<char, LemmaTreeNode> kvp in dictSubNodes) {
                    binWrt.WriteChar(kvp.Key);
                    kvp.Value.Save(binWrt);
                }
            }

            binWrt.WriteInt(iSimilarity);
            binWrt.WriteString(sCondition);
            binWrt.WriteBool(bWholeWord);

            binWrt.WriteString(lrBestRule.Signature);
            binWrt.WriteInt(aBestRules.Length);
            for (int i = 0; i < aBestRules.Length; i++) {
                binWrt.WriteString(aBestRules[i].Rule.Signature);
                binWrt.WriteDouble(aBestRules[i].Weight);
            }
            binWrt.WriteDouble(dWeight);

            binWrt.WriteInt(iStart);
            binWrt.WriteInt(iEnd);
        }
        public void Load(Latino.BinarySerializer binRead, LemmatizerSettings lsett, ExampleList elExamples, LemmaTreeNode ltnParentNode) {
            this.lsett = lsett;

            if (binRead.ReadBool()) {
                dictSubNodes = new Dictionary<char, LemmaTreeNode>();
                int iCount = binRead.ReadInt();
                for (int i = 0; i < iCount; i++) {
                    char cKey = binRead.ReadChar();
                    LemmaTreeNode ltrSub = new LemmaTreeNode(binRead, this.lsett, elExamples, this);
                    dictSubNodes.Add(cKey, ltrSub);
                }
            }
            else
                dictSubNodes = null;

            this.ltnParentNode = ltnParentNode;

            iSimilarity = binRead.ReadInt();
            sCondition = binRead.ReadString();
            bWholeWord = binRead.ReadBool();

            lrBestRule = elExamples.Rules[binRead.ReadString()];

            int iCountBest = binRead.ReadInt();
            aBestRules = new RuleWeighted[iCountBest];
            for (int i = 0; i < iCountBest; i++)
                aBestRules[i] = new RuleWeighted(elExamples.Rules[binRead.ReadString()], binRead.ReadDouble());

            dWeight = binRead.ReadDouble();

            iStart = binRead.ReadInt();
            iEnd = binRead.ReadInt();
            this.elExamples = elExamples;

        }
        public LemmaTreeNode(Latino.BinarySerializer binRead, LemmatizerSettings lsett, ExampleList elExamples, LemmaTreeNode ltnParentNode) {
            Load(binRead, lsett, elExamples, ltnParentNode);
        }

        #endif
        #endregion

        #region Other (Temporarly)

        //TODO - this is temp function, remove it
        public bool CheckConsistency() {
            bool bReturn = true;
            if (dictSubNodes != null)
                foreach (LemmaTreeNode ltnChild in dictSubNodes.Values)
                    bReturn = bReturn &&
                        ltnChild.CheckConsistency() &&
                        ltnChild.sCondition.EndsWith(sCondition);
            return bReturn;
        }

        public Dictionary<string, double> GetMsdCount() {
            Dictionary<string, double> thisMsdCount = new Dictionary<string, double>();

            /*if (dictSubNodes != null)
                foreach (LemmaTreeNode ltnChild in dictSubNodes.Values) {
                    Dictionary<string, double> childMsdCount = ltnChild.GetMsdCount();
                    foreach (KeyValuePair<string, double> msdCount in childMsdCount) {
                        if (thisMsdCount.ContainsKey(msdCount.Key))
                            thisMsdCount[msdCount.Key] += msdCount.Value;
                        else
                            thisMsdCount[msdCount.Key] = msdCount.Value;
                    }
                }
            else {*/
                for (int iExm = iStart; iExm <= iEnd; iExm++) {
                    string msd = elExamples[iExm].Msd ?? "{no MSD specified}";
                    
                    if (thisMsdCount.ContainsKey(msd))
                        thisMsdCount[msd] += elExamples[iExm].Weight;
                    else
                        thisMsdCount[msd] = elExamples[iExm].Weight;
                }
            //}

            return thisMsdCount;
        }

        #endregion
    }
}
