/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          MaxEnt.cs
 *  Version:       1.0
 *  Desc:		   Maximum entropy classifier 
 *  Author:        Jan Rupnik, Miha Grcar
 *  Created on:    Sep-2009
 *  Last modified: Oct-2009
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Latino.Model
{
    internal static class MaxEnt
    {
        private class SumOperator : IBinaryOperator<double>
        {
            public double PerformOperation(double arg_1, double arg_2)
            {
                return arg_1 + arg_2; 
            }
        }

        private static SparseMatrix<double> CreateObservationMatrix<LblT>(IExampleCollection<LblT, BinaryVector<int>.ReadOnly> dataset, ref LblT[] idx_to_lbl)
        {
            SparseMatrix<double> mtx = new SparseMatrix<double>();
            ArrayList<LblT> tmp = new ArrayList<LblT>();
            Dictionary<LblT, int> lbl_to_idx = new Dictionary<LblT, int>();
            foreach (LabeledExample<LblT, BinaryVector<int>.ReadOnly> labeled_example in dataset)
            {
                if (!lbl_to_idx.ContainsKey(labeled_example.Label))
                {
                    lbl_to_idx.Add(labeled_example.Label, lbl_to_idx.Count);
                    tmp.Add(labeled_example.Label);
                }
            }
            int i = 0;
            foreach (LabeledExample<LblT, BinaryVector<int>.ReadOnly> labeled_example in dataset)
            {
                Utils.Verbose("{0} / {1}\r", ++i, dataset.Count);
                int lbl_idx = lbl_to_idx[labeled_example.Label];
                if (!mtx.ContainsRowAt(lbl_idx))
                {
                    mtx[lbl_idx] = ModelUtils.ConvertExample<SparseVector<double>>(labeled_example.Example);
                }
                else
                {
                    SparseVector<double> new_vec = ModelUtils.ConvertExample<SparseVector<double>>(labeled_example.Example);
                    new_vec.Merge(mtx[lbl_idx], new SumOperator()); 
                    mtx[lbl_idx] = new_vec;
                }
            }
            Utils.VerboseLine("");
            idx_to_lbl = tmp.ToArray();
            return mtx;
        }

        private static SparseMatrix<double> CreateObservationMatrix2<LblT>(IExampleCollection<LblT, BinaryVector<int>.ReadOnly> dataset, ref LblT[] idxToLbl)
        {
            ArrayList<LblT> tmp = new ArrayList<LblT>();
            Dictionary<LblT, int> lblToIdx = new Dictionary<LblT, int>();
            foreach (LabeledExample<LblT, BinaryVector<int>.ReadOnly> labeledExample in dataset)
            {
                if (!lblToIdx.ContainsKey(labeledExample.Label))
                {
                    lblToIdx.Add(labeledExample.Label, lblToIdx.Count);
                    tmp.Add(labeledExample.Label);
                }
            }            
            // prepare struct for fast computation
            Dictionary<int, int>[] counter = new Dictionary<int, int>[tmp.Count];
            for (int j = 0; j < counter.Length; j++) { counter[j] = new Dictionary<int, int>(); }
            // count features
            int i = 0;
            foreach (LabeledExample<LblT, BinaryVector<int>.ReadOnly> labeledExample in dataset)
            {
                Utils.Verbose("{0} / {1}\r", ++i, dataset.Count);
                int lblIdx = lblToIdx[labeledExample.Label];
                int val;
                foreach (int idx in labeledExample.Example)
                {
                    if (counter[lblIdx].TryGetValue(idx, out val))
                    {
                        counter[lblIdx][idx] = val + 1;
                    }
                    else
                    {
                        counter[lblIdx].Add(idx, 1);
                    }
                }
            }            
            // create sparse matrix
            SparseMatrix<double> mtx = new SparseMatrix<double>();
            for (int j = 0; j < counter.Length; j++)
            {
                SparseVector<double> vec = new SparseVector<double>();
                foreach (KeyValuePair<int, int> item in counter[j])
                {
                    vec.InnerIdx.Add(item.Key);
                    vec.InnerDat.Add(item.Value);
                }
                vec.Sort();
                mtx[j] = vec;
            }
            idxToLbl = tmp.ToArray();
            Utils.VerboseLine("");
            return mtx;
        }

        private static SparseMatrix<double> CutOff(SparseMatrix<double>.ReadOnly mtx, int cut_off)
        {
            SparseMatrix<double> new_mtx = new SparseMatrix<double>();
            foreach (IdxDat<SparseVector<double>.ReadOnly> row in mtx)
            {
                ArrayList<IdxDat<double>> tmp = new ArrayList<IdxDat<double>>();
                foreach (IdxDat<double> item in row.Dat)
                {
                    if (item.Dat > cut_off) { tmp.Add(item); }
                }
                new_mtx[row.Idx] = new SparseVector<double>(tmp);
            }
            return new_mtx;
        }

        //private static void SaveForMatlab(SparseMatrix<double>.ReadOnly mtx, string file_name)
        //{
        //    StreamWriter writer = new StreamWriter(file_name);
        //    foreach (IdxDat<SparseVector<double>.ReadOnly> row in mtx)
        //    {
        //        foreach (IdxDat<double> item in row.Dat)
        //        {
        //            writer.WriteLine("{0} {1} {2}", row.Idx + 1, item.Idx + 1, item.Dat);
        //        }
        //    }
        //    writer.Close();
        //}

        private static SparseMatrix<double> CopyStructure(SparseMatrix<double>.ReadOnly mtx)
        {
            SparseMatrix<double> new_mtx = new SparseMatrix<double>();
            foreach (IdxDat<SparseVector<double>.ReadOnly> row in mtx)
            {
                ArrayList<IdxDat<double>> tmp = new ArrayList<IdxDat<double>>();
                foreach (IdxDat<double> item in row.Dat)
                {
                    tmp.Add(new IdxDat<double>(item.Idx, 0));
                }
                new_mtx[row.Idx] = new SparseVector<double>(tmp);
            }
            return new_mtx;
        }

        private static void Reset(SparseMatrix<double> mtx)
        {
            foreach (IdxDat<SparseVector<double>> row in mtx)
            {
                for (int i = 0; i < row.Dat.Count; i++)
                {
                    row.Dat.SetDirect(i, 0);
                }
            }
        }

        private static void GisUpdate(SparseMatrix<double> lambda, SparseMatrix<double>.ReadOnly expectations, SparseMatrix<double>.ReadOnly observations, double f)
        {
            foreach (IdxDat<SparseVector<double>.ReadOnly> row in observations)
            {
                int i = 0;
                foreach (IdxDat<double> item in row.Dat)
                {
                    double new_val = lambda[row.Idx].GetDirect(i).Dat + 1.0 / f * Math.Log(observations[row.Idx].GetDirect(i).Dat / expectations[row.Idx].GetDirect(i).Dat);
                    //Utils.VerboseLine("{0} {1} {2} {3} {4}", lambda[row.Idx].GetDirect(i).Dat, f, observations[row.Idx].GetDirect(i).Dat, expectations[row.Idx].GetDirect(i).Dat, new_val);
                    lambda[row.Idx].SetDirect(i, new_val);
                    i++;
                }
            }
        }

        private static double GisFindMaxF<LblT>(IExampleCollection<LblT, BinaryVector<int>.ReadOnly> dataset)
        {
            double max_val = 0;
            foreach (LabeledExample<LblT, BinaryVector<int>.ReadOnly> item in dataset)
            {
                if (item.Example.Count > max_val) { max_val = item.Example.Count; }
            }
            return max_val;
        }

        private class RefInt
        {
            public int Val;
        }
        
        private static void UpdateExpectationMatrixPass1(object _args)
        {
            object[] args = (object[])_args;
            int start_idx = (int)args[0];
            int end_idx = (int)args[1];
            SparseMatrix<double>.ReadOnly train_mtx_tr = (SparseMatrix<double>.ReadOnly)args[2];
            IdxDat<SparseVector<double>.ReadOnly>[] rows = (IdxDat<SparseVector<double>.ReadOnly>[])args[3];
            double[][] mtx = (double[][])args[4];
            RefInt progress = (RefInt)args[5];
            for (int i = start_idx; i <= end_idx; i++)
            {
                IdxDat<SparseVector<double>.ReadOnly> row = rows[i];                
                foreach (IdxDat<double> item in row.Dat)
                {
                    if (train_mtx_tr.ContainsRowAt(item.Idx))
                    {
                        SparseVector<double>.ReadOnly train_mtx_row = train_mtx_tr[item.Idx];
                        foreach (IdxDat<double> train_mtx_item in train_mtx_row)
                        {
                            mtx[row.Idx][train_mtx_item.Idx] += train_mtx_item.Dat * item.Dat;
                        }
                    }
                }
                progress.Val++;
            }
        }

        private static void UpdateExpectationMatrixPass2(object _args)
        {
            object[] args = (object[])_args;
            int start_idx = (int)args[0];
            int end_idx = (int)args[1];
            SparseMatrix<double>.ReadOnly train_mtx_tr = (SparseMatrix<double>.ReadOnly)args[2];
            IdxDat<SparseVector<double>>[] rows = (IdxDat<SparseVector<double>>[])args[3];
            double[][] mtx = (double[][])args[4];
            double[] z = (double[])args[5];
            RefInt progress = (RefInt)args[6];
            for (int i = start_idx; i <= end_idx; i++)
            {
                IdxDat<SparseVector<double>> row = rows[i];     
                int item_idx = 0;
                foreach (IdxDat<double> item in row.Dat)
                {
                    SparseVector<double>.ReadOnly pom = train_mtx_tr[item.Idx];
                    foreach (IdxDat<double> pom_item in pom)
                    {
                        row.Dat.SetDirect(item_idx, row.Dat.GetDatDirect(item_idx) + mtx[row.Idx][pom_item.Idx] / z[pom_item.Idx] * pom_item.Dat);
                    }
                    item_idx++;
                }
                progress.Val++;
            }
        }

        private static void UpdateExpectationMatrix(int num_classes, int train_set_size, SparseMatrix<double>.ReadOnly train_mtx_tr, SparseMatrix<double>.ReadOnly lambda, SparseMatrix<double> expectations, int num_threads)
        {            
            double[][] mtx = new double[num_classes][];
            for (int j = 0; j < num_classes; j++) { mtx[j] = new double[train_set_size]; }
            double[] z = new double[train_set_size];
            Utils.VerboseLine("Initiating {0} threads ...", num_threads);
            int lambda_row_count = lambda.GetRowCount();
            IdxDat<SparseVector<double>.ReadOnly>[] aux = new IdxDat<SparseVector<double>.ReadOnly>[lambda_row_count];
            int i = 0;
            foreach (IdxDat<SparseVector<double>.ReadOnly> row in lambda)
            {
                aux[i++] = row;
            }
            int chunk_sz = (int)Math.Round((double)lambda_row_count / (double)num_threads); // *** this load balancing is not so good; should I count values instead of rows?
            Thread[] threads = new Thread[num_threads];
            RefInt[] progress_info = new RefInt[num_threads];
            int start_idx = 0;            
            for (i = 0; i < num_threads; i++)
            {
                int end_idx = start_idx + chunk_sz - 1;
                if (i == num_threads - 1) { end_idx = aux.Length - 1; }
                progress_info[i] = new RefInt();
                threads[i] = new Thread(new ParameterizedThreadStart(UpdateExpectationMatrixPass1));
                threads[i].Start(new object[] { start_idx, end_idx, train_mtx_tr, aux, mtx, progress_info[i] });
                //Console.WriteLine("{0}-{1}", start_idx, end_idx);
                start_idx += chunk_sz;
            }
            bool is_alive = true;
            while (is_alive)
            {
                int aggr_progress = 0;
                foreach (RefInt progress in progress_info)
                {
                    aggr_progress += progress.Val;
                }
                Utils.Verbose("Pass 1: {0} / {1}\r", aggr_progress, lambda_row_count);
                is_alive = false;
                foreach (Thread thread in threads)
                {
                    is_alive = is_alive || thread.IsAlive;
                }
                Thread.Sleep(100);               
            }
            Utils.VerboseLine("Pass 1: {0} / {0}\r", lambda_row_count);
            for (i = 0; i < num_classes; i++)
            {
                for (int j = 0; j < train_set_size; j++)
                {
                    mtx[i][j] = Math.Exp(mtx[i][j]);
                    z[j] += mtx[i][j];
                }
            }
            int expe_row_count = expectations.GetRowCount();
            IdxDat<SparseVector<double>>[] aux2 = new IdxDat<SparseVector<double>>[expe_row_count];
            i = 0;
            foreach (IdxDat<SparseVector<double>> row in expectations)
            {
                aux2[i++] = row;
            }
            start_idx = 0;
            for (i = 0; i < num_threads; i++)
            {
                int end_idx = start_idx + chunk_sz - 1;
                if (i == num_threads - 1) { end_idx = aux.Length - 1; }
                progress_info[i].Val = 0;
                threads[i] = new Thread(new ParameterizedThreadStart(UpdateExpectationMatrixPass2));
                threads[i].Start(new object[] { start_idx, end_idx, train_mtx_tr, aux2, mtx, z, progress_info[i] });
                start_idx += chunk_sz;
            }
            is_alive = true;
            while (is_alive)
            {
                int aggr_progress = 0;
                foreach (RefInt progress in progress_info)
                {
                    aggr_progress += progress.Val;
                }
                Utils.Verbose("Pass 2: {0} / {1}\r", aggr_progress, expe_row_count);
                is_alive = false;
                foreach (Thread thread in threads)
                {
                    is_alive = is_alive || thread.IsAlive;
                }
                Thread.Sleep(100);
            }
            Utils.VerboseLine("Pass 2: {0} / {0}\r", expe_row_count);
        }

        private static void UpdateExpectationMatrix(int num_classes, int train_set_size, SparseMatrix<double>.ReadOnly train_mtx_tr, SparseMatrix<double>.ReadOnly lambda, SparseMatrix<double> expectations)
        {
            double[][] mtx = new double[num_classes][];
            for (int j = 0; j < num_classes; j++) { mtx[j] = new double[train_set_size]; }
            double[] z = new double[train_set_size];
            foreach (IdxDat<SparseVector<double>.ReadOnly> row in lambda)
            {
                Utils.Verbose("Pass 1: {0} / {1}\r", row.Idx + 1, lambda.GetLastNonEmptyRowIdx() + 1);
                foreach (IdxDat<double> item in row.Dat)
                {
                    if (train_mtx_tr.ContainsRowAt(item.Idx))
                    {
                        SparseVector<double>.ReadOnly train_mtx_row = train_mtx_tr[item.Idx];
                        foreach (IdxDat<double> train_mtx_item in train_mtx_row)
                        {
                            mtx[row.Idx][train_mtx_item.Idx] += train_mtx_item.Dat * item.Dat;
                        }
                    }
                }
            }
            Utils.VerboseLine("");
            for (int i = 0; i < num_classes; i++)
            {
                for (int j = 0; j < train_set_size; j++)
                {
                    mtx[i][j] = Math.Exp(mtx[i][j]);
                    z[j] += mtx[i][j];
                }
            }
            foreach (IdxDat<SparseVector<double>> row in expectations)
            {
                Utils.Verbose("Pass 2: {0} / {1}\r", row.Idx + 1, expectations.GetLastNonEmptyRowIdx() + 1);
                int item_idx = 0;
                foreach (IdxDat<double> item in row.Dat)
                {
                    SparseVector<double>.ReadOnly pom = train_mtx_tr[item.Idx];
                    foreach (IdxDat<double> pom_item in pom)
                    {
                        row.Dat.SetDirect(item_idx, row.Dat.GetDatDirect(item_idx) + mtx[row.Idx][pom_item.Idx] / z[pom_item.Idx] * pom_item.Dat);
                    }
                    item_idx++;
                }
            }
            Utils.VerboseLine("");
        }

        private static SparseMatrix<double> TransposeDataset<LblT>(IExampleCollection<LblT, BinaryVector<int>.ReadOnly> dataset, bool clear_dataset)
        {
            SparseMatrix<double> aux = new SparseMatrix<double>();
            int i = 0;
            if (clear_dataset)
            {
                foreach (LabeledExample<LblT, BinaryVector<int>.ReadOnly> item in dataset)
                {
                    aux[i++] = ModelUtils.ConvertExample<SparseVector<double>>(item.Example);
                    item.Example.Inner.Clear(); // *** clear read-only vectors to save space
                }
            }
            else
            {
                foreach (LabeledExample<LblT, BinaryVector<int>.ReadOnly> item in dataset)
                {
                    aux[i++] = ModelUtils.ConvertExample<SparseVector<double>>(item.Example);
                }
            }
            return aux.GetTransposedCopy();
        }

        public static SparseMatrix<double> Gis<LblT>(IExampleCollection<LblT, BinaryVector<int>.ReadOnly> dataset, int cut_off, int num_iter, bool clear_dataset, string mtx_file_name, ref LblT[] idx_to_lbl, int num_threads) 
        {
            Utils.VerboseLine("Creating observation matrix ...");
            SparseMatrix<double> observations = null;
            if (Utils.VerifyFileNameOpen(mtx_file_name))
            {
                BinarySerializer reader = new BinarySerializer(mtx_file_name, FileMode.Open);
                idx_to_lbl = new ArrayList<LblT>(reader).ToArray();
                observations = new SparseMatrix<double>(reader);
                reader.Close();
            }
            else
            {
                observations = CreateObservationMatrix2(dataset, ref idx_to_lbl);
                //SparseMatrix<double> test = CreateObservationMatrix(dataset, ref idx_to_lbl);
                //Console.WriteLine(test.ContentEquals(observations));
                if (Utils.VerifyFileNameCreate(mtx_file_name))
                {
                    BinarySerializer writer = new BinarySerializer(mtx_file_name, FileMode.Create);
                    new ArrayList<LblT>(idx_to_lbl).Save(writer);
                    observations.Save(writer);
                    writer.Close();
                }
            }
            int num_classes = observations.GetLastNonEmptyRowIdx() + 1;
            int num_examples = dataset.Count;
            if (cut_off > 0)
            {
                Utils.VerboseLine("Performing cut-off ...");
                observations = CutOff(observations, cut_off);
            }
            Utils.VerboseLine("Preparing structures ...");
            SparseMatrix<double> lambda = CopyStructure(observations);
            SparseMatrix<double> expectations = CopyStructure(observations);
            double f = GisFindMaxF(dataset);
            SparseMatrix<double> train_mtx_tr = TransposeDataset(dataset, clear_dataset);            
            Utils.VerboseLine("Entering main loop ...");
            for (int i = 0; i < num_iter; i++)
            {
                Utils.VerboseLine("Iteration {0} / {1} ...", i + 1, num_iter);
                Utils.VerboseLine("Updating expectations ...");
                if (num_threads > 1)
                {
                    UpdateExpectationMatrix(num_classes, num_examples, train_mtx_tr, lambda, expectations, num_threads);
                }
                else
                {
                    UpdateExpectationMatrix(num_classes, num_examples, train_mtx_tr, lambda, expectations);
                }
                Utils.VerboseLine("Updating lambdas ...");
                GisUpdate(lambda, expectations, observations, f);
                //SaveForMatlab(expectations, "c:\\mec\\old\\expem.txt");
                Reset(expectations);
            }
            //SaveForMatlab(lambda, "c:\\mec\\old\\lamem.txt");
            //SaveForMatlab(observations, "c:\\mec\\old\\obsem.txt");
            return lambda;
        }

        public static ClassifierResult<LblT> Classify<LblT>(BinaryVector<int>.ReadOnly bin_vec, SparseMatrix<double>.ReadOnly lambdas, LblT[] idx_to_lbl)
        {
            DotProductSimilarity dot_prod = new DotProductSimilarity();
            SparseVector<double> vec = ModelUtils.ConvertExample<SparseVector<double>>(bin_vec);
            ArrayList<KeyDat<double, LblT>> scores = new ArrayList<KeyDat<double, LblT>>();
            foreach (IdxDat<SparseVector<double>.ReadOnly> row in lambdas)
            {
                double score = Math.Exp(dot_prod.GetSimilarity(row.Dat, vec));
                scores.Add(new KeyDat<double, LblT>(score, idx_to_lbl[row.Idx]));
            }
            return new ClassifierResult<LblT>(scores);
            // *** for some reason, the code below is slower than the one currently in use
            /*ClassifierResult<LblT> classifier_result = new ClassifierResult<LblT>();
            foreach (IdxDat<SparseVector<double>.ReadOnly> row in lambdas)
            {
                int i = 0, j = 0;
                int a_count = bin_vec.Count;
                int b_count = row.Dat.Count;
                double dot_prod = 0;
                List<int> a_idx = bin_vec.Inner.Inner;
                ArrayList<int> b_idx = row.Dat.Inner.InnerIdx;
                ArrayList<double> b_dat = row.Dat.Inner.InnerDat;
                int a_idx_i = a_idx[0];
                int b_idx_j = b_idx[0];
                while (true)
                {
                    if (a_idx_i < b_idx_j)
                    {
                        if (++i == a_count) { break; }
                        a_idx_i = a_idx[i];
                    }
                    else if (a_idx_i > b_idx_j)
                    {
                        if (++j == b_count) { break; }
                        b_idx_j = b_idx[j];
                    }
                    else
                    {
                        dot_prod += b_dat[j];
                        if (++i == a_count || ++j == b_count) { break; }
                        a_idx_i = a_idx[i];
                        b_idx_j = b_idx[j];
                    }
                }
                double score = Math.Exp(dot_prod);
                classifier_result.Inner.Add(new KeyDat<double, LblT>(score, idx_to_lbl[row.Idx]));
            }
            classifier_result.Inner.Sort(new DescSort<KeyDat<double, LblT>>());
            return classifier_result;*/
        }
    }
}