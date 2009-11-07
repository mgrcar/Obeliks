/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          KMeans.cs
 *  Version:       1.0
 *  Desc:		   K-means clustering algorithm
 *  Author:        Miha Grcar 
 *  Created on:    Aug-2009
 *  Last modified: Aug-2009
 *  Revision:      Oct-2009
 * 
 ***************************************************************************/

using System;

namespace Latino.Model
{
    public class KMeans<LblT> : IClustering<LblT, SparseVector<double>.ReadOnly> 
    {        
        private ISimilarity<SparseVector<double>.ReadOnly> m_similarity
            = new CosineSimilarity();
        private Random m_rnd
            = new Random();
        private CentroidType m_centroid_type
            = CentroidType.NrmL2;
        private double m_eps
            = 0.0005;
        private int m_trials
            = 1;
        private int m_k;

        public KMeans(int k)
        {
            Utils.ThrowException(k < 2 ? new ArgumentOutOfRangeException("k") : null);
            m_k = k;
        }

        public Random Random
        {
            get { return m_rnd; }
            set
            {
                Utils.ThrowException(value == null ? new ArgumentNullException("Random") : null);
                m_rnd = value;
            }
        }

        public ISimilarity<SparseVector<double>.ReadOnly> Similarity
        {
            get { return m_similarity; }
            set
            {
                Utils.ThrowException(value == null ? new ArgumentNullException("Similarity") : null);
                m_similarity = value;
            }
        }

        public CentroidType CentroidType
        {
            get { return m_centroid_type; }
            set { m_centroid_type = value; }
        }

        public double Eps
        {
            get { return m_eps; }
            set
            {
                Utils.ThrowException(value < 0 ? new ArgumentOutOfRangeException("Eps") : null);
                m_eps = value;
            }
        }

        public int Trials
        {
            get { return m_trials; }
            set
            {
                Utils.ThrowException(value < 1 ? new ArgumentOutOfRangeException("Trials") : null);
                m_trials = value;
            }
        }

        // *** IClustering<LblT, SparseVector<double>.ReadOnly> interface implementation ***

        public Type RequiredExampleType
        {
            get { return typeof(SparseVector<double>.ReadOnly); }
        }

        public ClusteringResult Cluster(IExampleCollection<LblT, SparseVector<double>.ReadOnly> dataset)
        {
            Utils.ThrowException(dataset == null ? new ArgumentNullException("dataset") : null);
            Utils.ThrowException(dataset.Count < m_k ? new ArgumentValueException("dataset") : null);
            ClusteringResult clustering = null;
            ClusteringResult best_clustering = null;
            double global_best_clust_qual = 0;
            for (int trial = 1; trial <= m_trials; trial++)
            {
                Utils.VerboseLine("*** CLUSTERING TRIAL {0} OF {1} ***", trial, m_trials);
                ArrayList<SparseVector<double>.ReadOnly> centroids = null;
                clustering = new ClusteringResult();
                for (int i = 0; i < m_k; i++) { clustering.Roots.Add(new Cluster()); }
                // select seed items
                double min_sim = double.MaxValue;
                ArrayList<int> tmp = new ArrayList<int>(dataset.Count);
                for (int i = 0; i < dataset.Count; i++) { tmp.Add(i); }
                for (int k = 0; k < 3; k++)
                {
                    ArrayList<SparseVector<double>.ReadOnly> seeds = new ArrayList<SparseVector<double>.ReadOnly>(m_k);
                    tmp.Shuffle(m_rnd);
                    for (int i = 0; i < m_k; i++)
                    {
                        seeds.Add(ModelUtils.ComputeCentroid(new SparseVector<double>.ReadOnly[] { dataset[tmp[i]].Example }, m_centroid_type));
                    }
                    // assess quality of seed items
                    double sim_avg = 0;
                    foreach (SparseVector<double>.ReadOnly seed_1 in seeds)
                    {
                        foreach (SparseVector<double>.ReadOnly seed_2 in seeds)
                        {
                            if (seed_1 != seed_2)
                            {
                                sim_avg += m_similarity.GetSimilarity(seed_1, seed_2);
                            }
                        }
                    }
                    sim_avg /= (double)(m_k * m_k - m_k);
                    //Console.WriteLine(sim_avg);
                    if (sim_avg < min_sim)
                    {
                        min_sim = sim_avg;
                        centroids = seeds;
                    }
                }
                // main loop
                int iter = 0;
                double best_clust_qual = 0;
                double clust_qual;
                while (true)
                {
                    iter++;
                    clust_qual = 0;
                    // assign items to clusters
                    foreach (Cluster cluster in clustering.Roots) { cluster.Items.Clear(); }
                    for (int i = 0; i < dataset.Count; i++)
                    {
                        SparseVector<double>.ReadOnly example = dataset[i].Example;
                        double max_sim = double.MinValue;
                        ArrayList<int> candidates = new ArrayList<int>();
                        for (int j = 0; j < m_k; j++)
                        {
                            SparseVector<double>.ReadOnly centroid = centroids[j];
                            double sim = m_similarity.GetSimilarity(example, centroid);
                            if (sim > max_sim)
                            {
                                max_sim = sim;
                                candidates.Clear();
                                candidates.Add(j);
                            }
                            else if (sim == max_sim)
                            {
                                candidates.Add(j);
                            }
                        }
                        if (candidates.Count > 1)
                        {
                            candidates.Shuffle(m_rnd);
                        }
                        if (candidates.Count > 0) // *** is this always true? 
                        {
                            clustering.Roots[candidates[0]].Items.Add(new Pair<double, int>(1, i));
                            clust_qual += max_sim;
                        }
                    }
                    clust_qual /= (double)dataset.Count;
                    Utils.VerboseLine("*** Iteration {0} ***", iter);
                    Utils.VerboseLine("Quality: {0:0.0000}", clust_qual);
                    // check if done
                    if (iter > 1 && clust_qual - best_clust_qual <= m_eps)
                    {
                        break;
                    }
                    best_clust_qual = clust_qual;
                    // compute new centroids
                    for (int i = 0; i < m_k; i++)
                    {
                        centroids[i] = clustering.Roots[i].ComputeCentroid(dataset, m_centroid_type);
                    }
                }
                if (trial == 1 || clust_qual > global_best_clust_qual)
                {
                    global_best_clust_qual = clust_qual;
                    best_clustering = clustering;
                }
            }
            return best_clustering;
        }

        ClusteringResult IClustering<LblT>.Cluster(IExampleCollection<LblT> dataset)
        {
            Utils.ThrowException(dataset == null ? new ArgumentNullException("dataset") : null);
            Utils.ThrowException(!(dataset is IExampleCollection<LblT, SparseVector<double>.ReadOnly>) ? new ArgumentTypeException("dataset") : null);
            return Cluster((IExampleCollection<LblT, SparseVector<double>.ReadOnly>)dataset); // throws ArgumentValueException
        }
    }
}
