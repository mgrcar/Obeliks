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

        private static Regex mNonWordRegex
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
            BinarySerializer taggerModelSer = new BinarySerializer(taggerModelFile, FileMode.Open);
            BinarySerializer lemmatizerModelSer = null;
            if (lemmatizerModelFile != null)
            {
                lemmatizerModelSer = new BinarySerializer(lemmatizerModelFile, FileMode.Open);
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

        public void Tag(Corpus corpus, out int lemmaCorrect, out int lemmaCorrectLowercase, out int lemmaWords, bool xmlMode)
        {
            Utils.ThrowException(mModel == null ? new InvalidOperationException() : null);
            Utils.ThrowException(corpus == null ? new ArgumentNullException("corpus") : null);
            DateTime startTime = DateTime.Now;
            mLogger.Debug("Tag", "Označujem besedilo ...");
            lemmaCorrect = 0;
            lemmaCorrectLowercase = 0;
            lemmaWords = 0;
            for (int i = 0; i < corpus.TaggedWords.Count; i++)
            {
                mLogger.ProgressFast(/*sender=*/this, "Tag", "{0} / {1}", i + 1, corpus.TaggedWords.Count);
                BinaryVector<int> featureVector = corpus.GenerateFeatureVector(i, mFeatureSpace, /*extendFeatureSpace=*/false, mSuffixTree);
                Prediction<string> result = mModel.Predict(featureVector);
                if (mNonWordRegex.Match(corpus.TaggedWords[i].WordLower).Success) // non-word
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
                    Set<string> filter = mSuffixTree.Contains(corpus.TaggedWords[i].WordLower) ? mSuffixTree.GetTags(corpus.TaggedWords[i].WordLower) : null;
                    result = ProcessResult(result, filter);
                    corpus.TaggedWords[i].Tag = result.Count == 0 ? "*"/*unable to classify*/ : result.BestClassLabel;
                    if (mLemmatizer != null)
                    {
                        string tag = corpus.TaggedWords[i].Tag;
                        string wordLower = corpus.TaggedWords[i].WordLower;
                        //if (tag == "*")
                        //{
                        //    // *** TODO: take the most frequent tag from the filter (currently, frequency info not available)
                        //    logger.Info(null, tag);
                        //    logger.Info(null, filter);
                        //}
                        string lemma = (mConsiderTags && tag != "*") ? mLemmatizer.Lemmatize(wordLower, tag) : mLemmatizer.Lemmatize(wordLower);
                        if (string.IsNullOrEmpty(lemma) || (mConsiderTags && lemma == wordLower)) { lemma = wordLower; }
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
