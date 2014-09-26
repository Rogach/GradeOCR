using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibUtil;

namespace ARCode {

    /**
     * Performs 1-d k-means clustering of integer values.
     */
    public static class ValueClustering {
        public static readonly int maxClusterSteps = 1000;

        class Cluster {
            public double Center;
            public List<int> Values = new List<int>();
            /** recalculates center value for cluster */
            public void Recenter() {
                if (Values.Count > 0) {
                    Center = (double) Values.Sum() / Values.Count;
                }
            }
        }

        /**
         * Clusters values into two clusters and returns threshold that divides
         * those clusters.
         */
        public static double DivThreshold(int[] values) {
            List<Cluster> clusters = RunClustering(values, 2);
            return (clusters[0].Center + clusters[1].Center) / 2;
        }

        static List<Cluster> RunClustering(int[] values, int count) {
            List<Cluster> clusters = InitClusters(values, count);
            for (int q = 0; q < 1000; q++) {
                List<Cluster> newClusters = new List<Cluster>();
                foreach (var c in clusters) {
                    newClusters.Add(new Cluster { Center = c.Center, Values = c.Values });
                }
                foreach (var c in newClusters) {
                    c.Recenter();
                    c.Values = new List<int>();
                }
                AddToClusters(newClusters, values);
                if (clusters == newClusters) {
                    return clusters;
                }
                clusters = newClusters;
            }
            return clusters;
        }


        static List<Cluster> InitClusters(int[] values, int count) {
            List<Cluster> clusters = new List<Cluster>();
            if (count < 2) {
                throw new Exception(String.Format("Clustering requires at least 2 clusters, got {0}", count));
            }
            double max = values.Max();
            double min = values.Min();
            
            clusters.Add(new Cluster { Center = min });
            
            double dv = (max - min) / (count - 1);
            double c = min + dv;
            for (int q = 0; q < count - 2; q++) {
                clusters.Add(new Cluster { Center = dv });
            }
            
            clusters.Add(new Cluster { Center = max });

            AddToClusters(clusters, values);

            return clusters;
        }

        static void AddToClusters(List<Cluster> clusters, int[] values) {
            foreach (var v in values) {
                clusters.MinBy(c => Math.Abs(c.Center - v)).Values.Add(v);
            }
        }
    }
}
