/*==========================================================================;
 *
 *  Projekt Sporazumevanje v slovenskem jeziku: 
 *    http://www.slovenscina.eu/Vsebine/Sl/Domov/Domov.aspx
 *  Project Communication in Slovene: 
 *    http://www.slovenscina.eu/Vsebine/En/Domov/Domov.aspx
 *    
 *  Avtorske pravice za to izdajo programske opreme ureja licenca 
 *    Creative Commons Priznanje avtorstva-Nekomercialno-Brez predelav 2.5
 *  This work is licenced under the Creative Commons 
 *    Attribution-NonCommercial-NoDerivs 2.5 licence
 *    
 *  File:    PosTagger.cs
 *  Desc:    Part-of-speech tagger 
 *  Created: Sep-2009
 *
 *  Authors: Miha Grcar
 *
 ***************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LemmaSharp;
using Latino;
using Latino.Model;

namespace PosTagger
{
    /* .-----------------------------------------------------------------------
       |
       |  Class PartOfSpeechTagger
       |
       '-----------------------------------------------------------------------
    */
    public class PartOfSpeechTagger 
    {
        private static Logger mLogger
            = Logger.GetLogger(typeof(PartOfSpeechTagger));

        public static Regex mNonWordRegex
            = new Regex(@"^\W+(\<eos\>)?$", RegexOptions.Compiled);

        private PatriciaTree mSuffixTree
            = null;
        private Dictionary<string, int> mFeatureSpace
            = null;
        private MaximumEntropyClassifierFast<string> mModel
            = null;
        private Lemmatizer mLemmatizer
            = null;
        private bool mConsiderTags
            = false;

        public PartOfSpeechTagger()
        { 
        }

        public PartOfSpeechTagger(string taggerModelFile, string lemmatizerModelFile)
        {
            LoadModels(taggerModelFile, lemmatizerModelFile); // throws ArgumentValueException
        }

        public PartOfSpeechTagger(BinarySerializer taggerModelSer, BinarySerializer lemmatizerModelSer)
        {
            LoadModels(taggerModelSer, lemmatizerModelSer); // throws ArgumentNullException
        }

        public void LoadModels(string taggerModelFile, string lemmatizerModelFile)
        {
            Utils.ThrowException(!Utils.VerifyFileNameOpen(taggerModelFile) ? new ArgumentValueException("taggerModelFile") : null);
            Utils.ThrowException((lemmatizerModelFile != null && !Utils.VerifyFileNameOpen(lemmatizerModelFile)) ? new ArgumentValueException("lemmatizerModelFile") : null);
            BinarySerializer taggerModelSer = new BinarySerializer(new FileStream(taggerModelFile, FileMode.Open, FileAccess.Read, FileShare.Read));
            BinarySerializer lemmatizerModelSer = null;
            if (lemmatizerModelFile != null)
            {
                lemmatizerModelSer = new BinarySerializer(new FileStream(lemmatizerModelFile, FileMode.Open, FileAccess.Read, FileShare.Read));
            }
            LoadModels(taggerModelSer, lemmatizerModelSer);
        }

        public void LoadModels(BinarySerializer taggerModelSer, BinarySerializer lemmatizerModelSer)
        {
            Utils.ThrowException(taggerModelSer == null ? new ArgumentNullException("taggerModelSer") : null);
            mLogger.Debug("Load", "Nalagam model za označevanje ...");
            mSuffixTree = new PatriciaTree(taggerModelSer);
            mFeatureSpace = Utils.LoadDictionary<string, int>(taggerModelSer);
            mModel = new MaximumEntropyClassifierFast<string>(taggerModelSer);
            if (lemmatizerModelSer != null)
            {
                mLogger.Debug("Load", "Nalagam model za lematizacijo ...");
                mConsiderTags = lemmatizerModelSer.ReadBool();
                mLemmatizer = new Lemmatizer(lemmatizerModelSer);
            }
        }

        // *** Dec-2011 ***

        private static Set<string> CreateFilterFromResult(Prediction<string> result)
        {
            Set<string> filter = new Set<string>();
            foreach (KeyDat<double, string> item in result)
            {
                if (item.Key > 0) { filter.Add(item.Dat); }
            }
            return filter;
        }

        private static Prediction<string> ProcessResult(Prediction<string> result, Set<string> filter)
        {
            ArrayList<KeyDat<double, string>> newResult = new ArrayList<KeyDat<double, string>>();
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i].Key > 0)
                {
                    if (!mNonWordRegex.Match(result[i].Dat).Success && (filter == null || filter.Contains(result[i].Dat)))
                    {
                        newResult.Add(result[i]);
                    }
                }
            }          
            return new Prediction<string>(newResult);
        }

        // *** End of Dec-2011 ***

        public void Tag(Corpus corpus, out int lemmaCorrect, out int lemmaCorrectLowercase, out int lemmaWords, bool xmlMode)
        {
            DateTime startTime = DateTime.Now;
            mLogger.Debug("Tag", "Označujem besedilo ...");
            lemmaCorrect = 0;
            lemmaCorrectLowercase = 0;
            lemmaWords = 0;
            for (int i = 0; i < corpus.TaggedWords.Count; i++)
            {
                mLogger.ProgressFast(Logger.Level.Info, /*sender=*/this, "Tag", "{0} / {1}", i + 1, corpus.TaggedWords.Count);
                BinaryVector featureVector = corpus.GenerateFeatureVector(i, mFeatureSpace, /*extendFeatureSpace=*/false, mSuffixTree);
                Prediction<string> result = mModel.Predict(featureVector);
                if ((corpus.TaggedWords[i].MoreInfo != null && corpus.TaggedWords[i].MoreInfo.Punctuation) || 
                    (corpus.TaggedWords[i].MoreInfo == null && mNonWordRegex.Match(corpus.TaggedWords[i].WordLower).Success)) // non-word
                {
                    bool flag = false;
                    foreach (KeyDat<double, string> item in result)
                    {
                        if (corpus.TaggedWords[i].Word == item.Dat || corpus.TaggedWords[i].Word + "<eos>" == item.Dat)
                        {
                            corpus.TaggedWords[i].Tag = item.Dat;
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        corpus.TaggedWords[i].Tag = corpus.TaggedWords[i].Word;
                    }
                }
                else // word
                {
                    string wordLower = corpus.TaggedWords[i].WordLower;
                    Set<string> filter = mSuffixTree.Contains(wordLower) ? mSuffixTree.GetTags(wordLower) : null;                    
                    result = ProcessResult(result, filter);//???!!!
                    string goldTag = corpus.TaggedWords[i].Tag;
                    string word = corpus.TaggedWords[i].Word;
                    string rule;
                    if (filter == null)
                    {
                        filter = Rules.ApplyTaggerRules(CreateFilterFromResult(result), word, out rule);
                    }
                    else
                    {
                        filter = Rules.ApplyTaggerRules(filter, word, out rule);                        
                        if (filter.Count == 0) { filter = Rules.ApplyTaggerRules(CreateFilterFromResult(result), word, out rule); }
                    }
                    result = ProcessResult(result, filter);//???!!!            
                    string predictedTag;
                    if (result.Count == 0)
                    {
                        predictedTag = Rules.GetMostFrequentTag(wordLower, filter);
                    }
                    else
                    {
                        predictedTag = result.BestClassLabel;
                    }
                    corpus.TaggedWords[i].Tag = predictedTag;
                    if (mLemmatizer != null)
                    {
                        string lemma;
                        lemma = mConsiderTags ? mLemmatizer.Lemmatize(wordLower, predictedTag) : mLemmatizer.Lemmatize(wordLower);
                        lemma = Rules.FixLemma(lemma, corpus.TaggedWords[i].Word, predictedTag);
                        if (string.IsNullOrEmpty(lemma)) { lemma = wordLower; }
                        if (xmlMode)
                        {
                            lemmaWords++;
                            if (lemma == corpus.TaggedWords[i].Lemma)
                            {
                                lemmaCorrect++;                                
                            }
                            if (corpus.TaggedWords[i].Lemma != null && lemma.ToLower() == corpus.TaggedWords[i].Lemma.ToLower())
                            {
                                lemmaCorrectLowercase++;
                            }
                        }
                        corpus.TaggedWords[i].Lemma = lemma;
                    }
                }
            }
            TimeSpan span = DateTime.Now - startTime;
            mLogger.Debug("Tag", "Trajanje označevanja: {0:00}:{1:00}:{2:00}.{3:000}.", span.Hours, span.Minutes, span.Seconds, span.Milliseconds);
        }

        public void Tag(Corpus corpus)
        {
            int foo, bar, foobar;
            Tag(corpus, out foo, out bar, out foobar, /*xmlMode=*/false); // throws InvalidOperationException, ArgumentNullException
        }

        public bool IsKnownWord(string word)
        {
            Utils.ThrowException(mSuffixTree == null ? new InvalidOperationException() : null);
            Utils.ThrowException(word == null ? new ArgumentNullException("word") : null);            
            return mSuffixTree.Contains(word.ToLower());
        }
    }
}
