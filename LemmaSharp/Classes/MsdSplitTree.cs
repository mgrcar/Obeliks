using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LemmaSharp {
    // TODO this class need to be refractioned since it was assabled from staic methods and does not realy use OO principles.
    public class MsdSplitTree {
        public class BeamSearchParams {
            public Dictionary<int, int> beamsPerLevel = new Dictionary<int, int>();
        }

        private MsdSpec msdSpec;
        public int attrId;
        public List<LemmaExample> exampleList;
        public double ambigThis;
        public double ambigChild;
        public double ambigRecurs;
        public int subTreeSizeRecurs;
        public Dictionary<char, MsdSplitTree> subTrees;
        public List<MsdSplitTree> beamSiblings;

        public MsdSplitTree(MsdSpec msdSpec) {
            this.msdSpec = msdSpec;
        }
        public MsdSplitTree(List<LemmaExample> examples, MsdSpec msdSpec): this(examples, msdSpec, null) {
        }
        public MsdSplitTree(List<LemmaExample> examples, MsdSpec msdSpec, BeamSearchParams beamParams) {
            if (beamParams == null) beamParams = new BeamSearchParams();

            MsdSplitTree et = Split(PrepareExampleList(examples), msdSpec, beamParams);
            CopyVariablesToThis(et);
        }
        private void CopyVariablesToThis(MsdSplitTree et) {
            this.msdSpec = et.msdSpec;
            this.attrId = et.attrId;
            this.exampleList = et.exampleList;
            this.ambigThis = et.ambigThis;
            this.ambigChild = et.ambigChild;
            this.ambigRecurs = et.ambigRecurs;
            this.subTreeSizeRecurs = et.subTreeSizeRecurs;
            this.subTrees = et.subTrees;
            this.beamSiblings = et.beamSiblings;
        }

        public void OutputTree(int maxLevel) {
            OutputTree(this, this.msdSpec, 0, maxLevel, "");
        }
        public void OutputStatistics() {
            //count ambigities
            double wght = 0;
            double cnt = 0;
            for (int i = 0; i < exampleList.Count; i++) {
                wght += exampleList[i].Weight;
                cnt++;
            }
            Console.WriteLine("Number of ambiguity examples:");
            Console.WriteLine("    all: count={0}, weight={1}", cnt, wght);

            //unresolbavle ambiguities
            wght = 0;
            cnt = 0;
            for (int i = 1; i < exampleList.Count; i++)
                if (exampleList[i - 1].Word == exampleList[i].Word && exampleList[i - 1].Msd == exampleList[i].Msd && exampleList[i - 1].Lemma != exampleList[i].Lemma) {
                    wght += exampleList[i].Weight;
                    cnt++;
                }
            Console.WriteLine("    completley unresolvable (even if using MSD): count={0}, weight={1}", cnt, wght);

            //count real current ambigities
            wght = GetListAmbiguities(exampleList);
            Console.WriteLine("    partly (if not using MSD) unresolvable ambiguities: weight={0}", wght);
        }

        private static List<LemmaExample> CompactExamples(List<LemmaExample> examples) {
            Dictionary<string, LemmaExample> exampleDict = new Dictionary<string, LemmaExample>();
            Dictionary<string, double> weights = new Dictionary<string, double>();

            foreach (LemmaExample le in examples) {
                string signature = le.Word + "\t" + le.Lemma + "\t" + le.Msd;
                if (exampleDict.ContainsKey(signature))
                    weights[signature] += le.Weight;
                else {
                    exampleDict[signature] = le;
                    weights[signature] = le.Weight;
                }
            }

            List<LemmaExample> el = new List<LemmaExample>();
            foreach (KeyValuePair<string, LemmaExample> kvp in exampleDict) {
                string signature = kvp.Key;
                LemmaExample le = kvp.Value;
                el.Add(new LemmaExample(le.Word, le.Lemma, weights[signature], le.Msd, null, null));
            }

            return el;
        }
        private static List<LemmaExample> ExtractAmbiguities(List<LemmaExample> examples) {

            Dictionary<string, Dictionary<string, double>> wordLemmaWeights2 = new Dictionary<string, Dictionary<string, double>>();
            foreach (LemmaExample le in examples) {
                string word = le.Word;
                string lemma = le.Lemma + "\t" + le.Msd;
                double weight = le.Weight;

                if (!wordLemmaWeights2.ContainsKey(word))
                    wordLemmaWeights2.Add(word, new Dictionary<string, double>());

                Dictionary<string, double> lemmaWeights = wordLemmaWeights2[word];
                if (lemmaWeights.ContainsKey(lemma))
                    lemmaWeights[lemma] += weight;
                else
                    lemmaWeights[lemma] = weight;
            }

            List<LemmaExample> el = new List<LemmaExample>();

            foreach (KeyValuePair<string, Dictionary<string, double>> wlw in wordLemmaWeights2) {
                bool lemmaDif = false;

                string lemma = null;
                string msd = null;
                if (wlw.Value.Count > 1)
                    foreach (KeyValuePair<string, double> wl in wlw.Value) {
                        string[] aKey = wl.Key.Split(new char[] { '\t' });
                        if (lemma == null) lemma = aKey[0];
                        if (lemma != null && lemma != aKey[0]) lemmaDif = true;
                        if (msd == null) msd = aKey[1];
                    }
                if (lemmaDif)
                    foreach (KeyValuePair<string, double> wl in wlw.Value) {
                        string[] aKey = wl.Key.Split(new char[] { '\t' });
                        el.Add(new LemmaExample(wlw.Key, aKey[0], wl.Value, aKey[1], null, null));
                    }
            }
            return el;
        }
        private static List<LemmaExample> PrepareExampleList(List<LemmaExample> examples) {
            //create list of ambiguities
            List<LemmaExample> el = ExtractAmbiguities(CompactExamples(examples));
            //sorting that must be done since function GetListAmbiguities optimizes its work based on the sorting
            el.Sort(CompareExamplesWordMsdLemma);
            return el;
        }

        private static MsdSplitTree Split(List<LemmaExample> el, MsdSpec msdSpec, BeamSearchParams beamParams) {
            double weightInitial = GetListAmbiguities(el);
            return RecursiveSplitBeam(el, weightInitial, msdSpec, 0, beamParams);
        }
        private static MsdSplitTree RecursiveSplitBeam(List<LemmaExample> el, double weightInitial, MsdSpec msdSpec, int level, BeamSearchParams beamParams) {
            List<MsdSplitTree> splits = ProduceOrderedSplits(el, weightInitial, msdSpec);
            //OutputSplits(splits, el, weightInitial, msdSpec, level);

            List<MsdSplitTree> beamSplits = new List<MsdSplitTree>();

            int beamSize = 1;
            if (beamParams.beamsPerLevel != null && beamParams.beamsPerLevel.ContainsKey(level))
                beamSize = Math.Min(beamParams.beamsPerLevel[level], splits.Count);

            for (int beam = 0; beam < beamSize; beam++) {
                MsdSplitTree bestTree = splits[beam];
                if (bestTree.ambigChild < weightInitial) {
                    Dictionary<char, MsdSplitTree> newSubTrees = RecursiveSplit(bestTree, msdSpec, level, beamParams);
                    bestTree.subTrees = newSubTrees;
                    bestTree.ambigThis = weightInitial;
                    beamSplits.Add(bestTree);
                }
            }

            if (beamSplits.Count == 0) return null;
            if (beamSplits.Count == 1) return beamSplits[0];

            beamSplits.Sort(CompareTreesRecurSizeAsc);
            MsdSplitTree best = beamSplits[0];
            best.beamSiblings = beamSplits;
            return best;
        }
        private static Dictionary<char, MsdSplitTree> RecursiveSplit(MsdSplitTree bestTree, MsdSpec msdSpec, int level, BeamSearchParams beamParams) {
            Dictionary<char, MsdSplitTree> newSubTrees = new Dictionary<char, MsdSplitTree>();
            bestTree.ambigRecurs = 0;
            bestTree.ambigChild = 0;
            bestTree.subTreeSizeRecurs = 0;
            foreach (KeyValuePair<char, MsdSplitTree> kvp in bestTree.subTrees) {
                MsdSplitTree subTree = kvp.Value;
                MsdSplitTree newSubTree = subTree;
                if (subTree.ambigChild > 0) newSubTree = RecursiveSplitBeam(subTree.exampleList, subTree.ambigChild, msdSpec, level + 1, beamParams);
                if (newSubTree == null) newSubTree = subTree;
                newSubTrees.Add(kvp.Key, newSubTree);


                bestTree.ambigRecurs += newSubTree.ambigRecurs;
                bestTree.ambigChild += newSubTree.ambigThis;
                bestTree.subTreeSizeRecurs += newSubTree.subTreeSizeRecurs;
            }
            return newSubTrees;
        }

        private static List<MsdSplitTree> ProduceOrderedSplits(List<LemmaExample> el, double weightInitial, MsdSpec msdSpec) {
            List<MsdSplitTree> splits = new List<MsdSplitTree>();
            for (int attrId = 0; attrId < msdSpec.AttrCount; attrId++)
                splits.Add(SplitByMsdAttribute(el, attrId, msdSpec));
            splits.Sort(CompareTreesAbmibuitiesAsc);
            return splits;
        }
        private static int CompareTreesAbmibuitiesAsc(MsdSplitTree x, MsdSplitTree y) {
            if (x.ambigChild > y.ambigChild) return 1;
            if (x.ambigChild < y.ambigChild) return -1;
            return 0;
        }
        private static int CompareTreesRecurSizeAsc(MsdSplitTree x, MsdSplitTree y) {
            if (x.subTreeSizeRecurs > y.subTreeSizeRecurs) return 1;
            if (x.subTreeSizeRecurs < y.subTreeSizeRecurs) return -1;
            return 0;
        }

        private static void OutputSplits(List<MsdSplitTree> splits, List<LemmaExample> el, double weightInitial, MsdSpec msdSpec, int level) {
            Console.Write(new string(' ', level * 2));
            Console.WriteLine("Trying to split {0} examples with {1} ambiguities", el.Count, weightInitial);
            foreach (MsdSplitTree exmpTree in splits) {
                int attrId = exmpTree.attrId;

                string attrName = msdSpec.attrIdToNameMap[attrId];
                StringBuilder sbSubGroups = new StringBuilder();
                foreach (KeyValuePair<char, MsdSplitTree> elSub in exmpTree.subTrees) {
                    sbSubGroups.AppendFormat(" {0}:{1}", elSub.Key, elSub.Value.exampleList.Count);
                }
                Console.Write(new string(' ', level * 2));
                Console.WriteLine("  Attr: {0,2}, Ambig: Res={1,4} Rem={2,4}, AttrName: {3}, SplitTo: {4} classes: {5}",
                    attrId, weightInitial - exmpTree.ambigChild, exmpTree.ambigChild, attrName, exmpTree.subTrees.Count, sbSubGroups);
            }
        }
        private static void OutputTree(MsdSplitTree et, MsdSpec msdSpec, int level, int maxLevel, string attrSet) {
            if (level > maxLevel) return;
            int attrId = et.attrId;
            string attrName = msdSpec.attrIdToNameMap[attrId];

            StringBuilder sbSubGroups = new StringBuilder();
            if (et.subTrees != null) {
                sbSubGroups.AppendFormat(",SplitBy={0}({1}) To={2} classes:",
                    attrName, attrId, (et.subTrees == null ? "0" : et.subTrees.Count.ToString()));
                foreach (KeyValuePair<char, MsdSplitTree> sub in et.subTrees)
                    sbSubGroups.AppendFormat("|{0}:{1}", sub.Key, sub.Value.exampleList.Count);
            }
            StringBuilder sbBeam = new StringBuilder();
            if (et.beamSiblings != null) {
                sbSubGroups.AppendFormat(",BeamSibling=");
                foreach (MsdSplitTree beamSibl in et.beamSiblings)
                    sbSubGroups.AppendFormat("|{0}", beamSibl.subTreeSizeRecurs);
            }

            Console.Write(new string(' ', level * 2));
            Console.WriteLine("Examples={0},AttrSet=({1}),SubTree={2},Ambig:(T={3}/S={4}/R={5}){6}{7}",
                et.exampleList.Count, attrSet, et.subTreeSizeRecurs,
                et.ambigThis, et.ambigChild, et.ambigRecurs, sbSubGroups, sbBeam);

            if (et.subTrees != null)
                foreach (KeyValuePair<char, MsdSplitTree> sub in et.subTrees)
                    OutputTree(sub.Value, msdSpec, level + 1, maxLevel, attrSet + (attrSet.Length > 0 ? "&" : "") + attrName + "='" + sub.Key + "'");

        }

        private static MsdSplitTree SplitByMsdAttribute(List<LemmaExample> el, int attrId, MsdSpec msdSpec) {
            MsdSplitTree et = new MsdSplitTree(msdSpec);
            et.attrId = attrId;
            et.subTrees = new Dictionary<char, MsdSplitTree>();
            et.exampleList = el;

            //todo FIX IT
            MsdSplitTree etSubDef = new MsdSplitTree(msdSpec);
            etSubDef.exampleList = new List<LemmaExample>();
            et.subTrees['#'] = etSubDef;

            for (int i = 0; i < el.Count; i++) {
                LemmaExample e = el[i];
                char cls = msdSpec.GetAttrValue(e.Msd, attrId);
                if (et.subTrees.ContainsKey(cls))
                    et.subTrees[cls].exampleList.Add(e);
                else {
                    MsdSplitTree etSub = new MsdSplitTree(msdSpec);

                    et.subTrees[cls] = etSub;

                    etSub.exampleList = new List<LemmaExample>();
                    etSub.exampleList.Add(e);
                }
            }

            double ambigChild = 0;
            foreach (KeyValuePair<char, MsdSplitTree> sub in et.subTrees) {
                MsdSplitTree etSub = sub.Value;
                double ambig = GetListAmbiguities(sub.Value.exampleList);
                etSub.ambigThis = ambig;
                etSub.ambigChild = ambig;
                etSub.ambigRecurs = ambig;
                etSub.subTreeSizeRecurs = 1;
                ambigChild += ambig;
            }

            et.ambigChild = ambigChild;
            et.ambigRecurs = ambigChild;
            et.subTreeSizeRecurs = et.subTrees.Count;

            return et;
        }

        private static double GetRecursiveAmbiguities(MsdSplitTree et) {
            double weight = 0;
            if (et.subTrees == null)
                return GetChildsAmbiguities(et);

            foreach (MsdSplitTree etSub in et.subTrees.Values) {
                weight += GetRecursiveAmbiguities(etSub);
            }
            return weight;
        }
        private static double GetChildsAmbiguities(MsdSplitTree et) {
            double weight = 0;
            foreach (MsdSplitTree etSub in et.subTrees.Values) {
                double ambig = GetListAmbiguities(etSub.exampleList);
                etSub.ambigChild = ambig;
                weight += ambig;
            }
            return weight;
        }
        private static double GetListAmbiguities(List<LemmaExample> el) {
            Dictionary<string, Dictionary<string, Dictionary<string, double>>> wordLemmaMsdWeight = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();
            for (int i = 0; i < el.Count; i++) {
                LemmaExample exmp = el[i];
                if (!wordLemmaMsdWeight.ContainsKey(exmp.Word))
                    wordLemmaMsdWeight[exmp.Word] = new Dictionary<string, Dictionary<string, double>>();

                if (!wordLemmaMsdWeight[exmp.Word].ContainsKey(exmp.Lemma))
                    wordLemmaMsdWeight[exmp.Word][exmp.Lemma] = new Dictionary<string, double>();

                if (!wordLemmaMsdWeight[exmp.Word][exmp.Lemma].ContainsKey(exmp.Msd))
                    wordLemmaMsdWeight[exmp.Word][exmp.Lemma][exmp.Msd] = exmp.Weight;
                else
                    wordLemmaMsdWeight[exmp.Word][exmp.Lemma][exmp.Msd] += exmp.Weight;
            }

            double wghtAmbiguities = 0;
            foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, double>>> wordBase in wordLemmaMsdWeight) {
                double weightLemmaSum = 0;
                double weightLemmaMax = 0;
                foreach (KeyValuePair<string, Dictionary<string, double>> wordLemmaBase in wordBase.Value) {
                    double weightLemma = 0;
                    foreach (KeyValuePair<string, double> wordLemmaMsdBase in wordLemmaBase.Value) weightLemma += wordLemmaMsdBase.Value;
                    weightLemmaSum += weightLemma;
                    if (weightLemma > weightLemmaMax)
                        weightLemmaMax = weightLemma;
                }
                wghtAmbiguities += weightLemmaSum - weightLemmaMax;
            }

            return wghtAmbiguities;
        }

        private static int CompareExamplesWordMsdWeightLemma(LemmaExample x, LemmaExample y) {
            int ret = 0;
            ret = String.Compare(x.Word, y.Word);
            if (ret != 0) return ret;
            ret = String.Compare(x.Msd, y.Msd);
            if (ret != 0) return ret;
            ret = x.Weight > y.Weight ? -1 : (x.Weight < y.Weight ? 1 : 0);
            if (ret != 0) return ret;
            ret = String.Compare(x.Lemma, y.Lemma);
            return ret;
        }
        private static int CompareExamplesWordMsdLemma(LemmaExample x, LemmaExample y) {
            int ret = 0;
            ret = String.Compare(x.Word, y.Word);
            if (ret != 0) return ret;
            ret = String.Compare(x.Msd, y.Msd);
            if (ret != 0) return ret;
            ret = String.Compare(x.Lemma, y.Lemma);
            return ret;
        }
        private static int CompareExamplesWordLemmaMsd(LemmaExample x, LemmaExample y) {
            int ret = 0;
            ret = String.Compare(x.Word, y.Word);
            if (ret != 0) return ret;
            ret = String.Compare(x.Lemma, y.Lemma);
            if (ret != 0) return ret;
            ret = String.Compare(x.Msd, y.Msd);
            return ret;
        }

        public void CleanStructure(bool bDropExamples, bool bDropBeamSiblings) {
            if (bDropExamples) exampleList = null;
            if (bDropBeamSiblings) beamSiblings = null;
            if (subTrees != null) {
                foreach (KeyValuePair<char, MsdSplitTree> kvp in subTrees) {
                    kvp.Value.CleanStructure(bDropExamples, bDropBeamSiblings);
                }
            }
        }

        public string TransformMsd(string msd) {
            if (subTrees == null)
                return "";
            else {
                char attVal = msdSpec.GetAttrValue(msd, this.attrId);
                string thisTransform = msdSpec.attrIdToNameMap[attrId] + "=" + attVal;
                string subTreeTransform = "";

                if (!subTrees.ContainsKey(attVal))
                    attVal = '#'; //all others
                if (!subTrees.ContainsKey(attVal))
                    throw new Exception("Strange msd recieved for transformation? Only known msds can be used!"); //TODO check if this can be generalised

                subTreeTransform = subTrees[attVal].TransformMsd(msd);
                if (subTreeTransform!="") thisTransform += "&";
                return thisTransform + subTreeTransform;
            }
        }

        #region Serialization Functions (Binary)

        public void Serialize(BinaryWriter binWrt) {
            bool msdSpecExists = msdSpec != null;
            binWrt.Write(msdSpecExists);
            if (msdSpecExists)
            msdSpec.Serialize(binWrt);

            Dictionary<LemmaExample, int> exampleMapping = new Dictionary<LemmaExample, int>();

            if (exampleList == null)
                binWrt.Write(-1);
            else {
                binWrt.Write(exampleList.Count);
                int leId = 0;
                foreach (LemmaExample le in exampleList) {
                    le.Serialize(binWrt, false);
                    exampleMapping[le] = leId++;
                }
            }

            Serialize(binWrt, exampleMapping);
        }
        private void Serialize(BinaryWriter binWrt, Dictionary<LemmaExample, int> exampleMapping) {
            binWrt.Write(attrId);

            if (exampleList == null)
                binWrt.Write(-1);
            else {
                binWrt.Write(exampleList.Count);
                foreach (LemmaExample le in exampleList) {
                    binWrt.Write(exampleMapping[le]);
                }
            }

            binWrt.Write(ambigThis);
            binWrt.Write(ambigChild);
            binWrt.Write(ambigRecurs);
            binWrt.Write(subTreeSizeRecurs);

            if (subTrees == null)
                binWrt.Write(-1);
            else {
                binWrt.Write(subTrees.Count);
                foreach (KeyValuePair<char, MsdSplitTree> kvp in subTrees) {
                    binWrt.Write(kvp.Key);
                    kvp.Value.Serialize(binWrt, exampleMapping);
                }
            }

            if (beamSiblings == null)
                binWrt.Write(-1);
            else {
                binWrt.Write(beamSiblings.Count);
                foreach (MsdSplitTree mst in beamSiblings) {
                    if (mst == this)
                        binWrt.Write(true);
                    if (mst != this) {
                        binWrt.Write(false);
                        mst.Serialize(binWrt, exampleMapping);
                    }
                }
            }

        }

        public void Deserialize(BinaryReader binRead) {
            bool msdSpecExists = binRead.ReadBoolean();
            if (!msdSpecExists)
                msdSpec = null;
            else
                msdSpec = new MsdSpec(binRead);

            Dictionary<int, LemmaExample> exampleMapping = new Dictionary<int, LemmaExample>();

            int exampleListCount = binRead.ReadInt32();
            if (exampleListCount < 0)
                exampleList = null;
            else {
                exampleList = new List<LemmaExample>(exampleListCount);
                for (int leId = 0; leId < exampleListCount; leId++) {
                    LemmaExample le = new LemmaExample(binRead, null, null);
                    exampleMapping[leId] = le;
                    exampleList.Add(le);
                }
            }

            Deserialize(binRead, exampleMapping, msdSpec);
        }
        private void Deserialize(BinaryReader binRead, Dictionary<int, LemmaExample> exampleMapping, MsdSpec msdSpec) {
            this.msdSpec = msdSpec;

            attrId = binRead.ReadInt32();

            int exampleListCount = binRead.ReadInt32();
            if (exampleListCount < 0)
                exampleList = null;
            else {
                exampleList = new List<LemmaExample>(exampleListCount);
                for (int i = 0; i < exampleListCount; i++) {
                    int leId = binRead.ReadInt32();
                    LemmaExample le = exampleMapping[leId];
                    exampleList.Add(le);
                }
            }

            ambigThis = binRead.ReadDouble();
            ambigChild = binRead.ReadDouble();
            ambigRecurs = binRead.ReadDouble();
            subTreeSizeRecurs = binRead.ReadInt32();

            int subTreesCount = binRead.ReadInt32();
            if (subTreesCount < 0)
                subTrees = null;
            else {
                subTrees = new Dictionary<char, MsdSplitTree>();
                for (int i = 0; i < subTreesCount; i++) {
                    char key = binRead.ReadChar();
                    MsdSplitTree mst = new MsdSplitTree(binRead, exampleMapping, msdSpec);
                    subTrees.Add(key, mst);
                }
            }

            int beamSiblingsCount = binRead.ReadInt32();
            if (beamSiblingsCount < 0)
                beamSiblings = null;
            else {
                beamSiblings = new List<MsdSplitTree>(beamSiblingsCount);
                for (int i = 0; i < beamSiblingsCount; i++) {
                    bool bThisTree = binRead.ReadBoolean();
                    if (bThisTree)
                        beamSiblings.Add(this);
                    else {
                        MsdSplitTree mst = new MsdSplitTree(binRead, exampleMapping, msdSpec);
                        beamSiblings.Add(mst);
                    }
                }
            }

        }
        
        
        public MsdSplitTree(BinaryReader binRead) {
            Deserialize(binRead);
        }

        private MsdSplitTree(BinaryReader binRead, Dictionary<int, LemmaExample> exampleMapping, MsdSpec msdSpec) {
            Deserialize(binRead, exampleMapping, msdSpec);
        }

        #endregion
    }
}
