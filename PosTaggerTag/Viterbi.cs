/*==========================================================================;
 *
 *  (copyright)
 *
 *  File:          Viterbi.cs
 *  Version:       1.0
 *  Desc:		   Efficient sequence-tagging algorithm 
 *  Author:		   Miha Grcar
 *  Created on:    Sep-2009
 *  Last modified: Sep-2009
 *  Revision:      N/A
 *
 ***************************************************************************/

//using System; 
using System.Collections.Generic;
using Latino;
using Latino.Model;

namespace PosTagger
{
    public static class Viterbi
    {
        public delegate ClassifierResult<string> ViterbiDelegate(int word_idx, ArrayList<string>.ReadOnly tag_seq);

        private class TagNode
        {
            public ArrayList<string> TagSeq
                = new ArrayList<string>();
            public double SeqProb
                = 0;
        }

        public static ArrayList<string> TagSentence(int start_idx, ViterbiDelegate get_prob)
        { 
            // initialize 
            int word_idx = start_idx;
            ClassifierResult<string> prob = get_prob(word_idx, new ArrayList<string>());
            // create tag index
            Dictionary<string, TagNode> tag_index = new Dictionary<string, TagNode>();
            foreach (KeyDat<double, string> item in prob)
            {
                TagNode new_node = new TagNode();
                new_node.SeqProb = item.Key;
                tag_index.Add(item.Dat, new_node);
            }
            // loop over words            
            string best_end_tag = null;
            do
            {
                word_idx++;
                Dictionary<string, TagNode> new_tag_index = new Dictionary<string, TagNode>();
                double max_prob = 0;                
                // outer loop over tags                
                foreach (KeyValuePair<string, TagNode> prev_tag_info in tag_index)
                {
                    prev_tag_info.Value.TagSeq.Add(prev_tag_info.Key);
                    prob = get_prob(word_idx, prev_tag_info.Value.TagSeq);
                    prev_tag_info.Value.TagSeq.RemoveAt(prev_tag_info.Value.TagSeq.Count - 1);
                    if (prob == null) // end of corpus (end-of-sentence not detected)
                    {
                        new_tag_index = tag_index;
                        break;
                    }
                    // inner loop over tags
                    foreach (KeyDat<double, string> last_tag_info in prob)
                    {
                        if (!new_tag_index.ContainsKey(last_tag_info.Dat))
                        {
                            // create new node
                            TagNode new_node = new TagNode();
                            new_node.SeqProb = prev_tag_info.Value.SeqProb * last_tag_info.Key;
                            if (new_node.SeqProb > max_prob) { max_prob = new_node.SeqProb; best_end_tag = last_tag_info.Dat; }
                            new_node.TagSeq.AddRange(prev_tag_info.Value.TagSeq);
                            new_node.TagSeq.Add(prev_tag_info.Key);
                            new_tag_index.Add(last_tag_info.Dat, new_node);
                        }
                        else
                        {
                            // check if this sequence is better
                            TagNode node = new_tag_index[last_tag_info.Dat];
                            double seq_prob = prev_tag_info.Value.SeqProb * last_tag_info.Key;
                            if (seq_prob > node.SeqProb)
                            {
                                // update node
                                node.SeqProb = seq_prob;
                                if (seq_prob > max_prob) { max_prob = seq_prob; best_end_tag = last_tag_info.Dat; }
                                node.TagSeq.Last = prev_tag_info.Key;
                            }
                        }
                    }
                }
                tag_index = new_tag_index;
            } while (prob != null && !best_end_tag.EndsWith("<eos>")); // check if most probable sequence ends with end-of-sentence tag
            TagNode best_seq = tag_index[best_end_tag];
            best_seq.TagSeq.Add(best_end_tag);
            //Console.WriteLine(best_seq.TagSeq);
            return best_seq.TagSeq;
        }
    }
}
