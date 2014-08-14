using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OCRUtil;
using LibUtil;
using TableOCR;

namespace LineOCR {
    public class TableBuilder {
        public static readonly int sideEgdeThreshold = 100;

        public PointF horizNormal;
        public PointF vertNormal;
        public PointF invHorizNormal;
        public PointF invVertNormal;

        public List<LineF> rowLines;

        public Line leftEdge;
        public Line rightEdge;

        List<RowInfo> rows;

        public Table table;

        public TableBuilder(LineNormalization lnorm) {
            double angle = lnorm.angle;
            
            horizNormal = new PointF((float) Math.Cos(angle), (float) Math.Sin(angle));
            vertNormal = new PointF((float) -Math.Sin(angle), (float) Math.Cos(angle));
            invHorizNormal = new PointF((float) Math.Cos(angle), (float) -Math.Sin(angle));
            invVertNormal = new PointF((float) Math.Sin(angle), (float) Math.Cos(angle));

            List<LineF> horizLines = lnorm.normHorizLines;
            List<LineF> vertLines = lnorm.normRotVertLines.OrderBy(ln => ln.p1.X).ToList();

            List<float> allLeftEndPoints = horizLines.Select(ln => TableX(ln.p1)).OrderBy(x => x).ToList();
            List<float> allRightEndPoints = horizLines.Select(ln => TableX(ln.p2)).OrderBy(x => x).ToList();

            float leftMedian = allLeftEndPoints[allLeftEndPoints.Count / 2];
            float rightMedian = allRightEndPoints[allRightEndPoints.Count / 2];

            rowLines =
                horizLines
                .Where(ln => Math.Abs(TableX(ln.p1) - leftMedian) < sideEgdeThreshold)
                .Where(ln => Math.Abs(TableX(ln.p2) - rightMedian) < sideEgdeThreshold)
                .OrderBy(ln => TableY(ln.p1)).ToList();

            List<PointF> leftEndPoints = rowLines.Select(ln => ln.p1).ToList();
            List<PointF> rightEndPoints = rowLines.Select(ln => ln.p2).ToList();

            float leftX = leftEndPoints.Select(pt => TableX(pt)).Average();
            float rightX = rightEndPoints.Select(pt => TableX(pt)).Average();

            PointF leftEdgeTop = new PointF(leftX, TableY(leftEndPoints.First()));
            PointF leftEdgeBottom = new PointF(leftX, TableY(leftEndPoints.Last()));
            leftEdge = new Line(PointOps.TruncPt(ToPicture(leftEdgeTop)), PointOps.TruncPt(ToPicture(leftEdgeBottom)));

            PointF rightEdgeTop = new PointF(rightX, TableY(rightEndPoints.First()));
            PointF rightEdgeBottom = new PointF(rightX, TableY(rightEndPoints.Last()));
            rightEdge = new Line(PointOps.TruncPt(ToPicture(rightEdgeTop)), PointOps.TruncPt(ToPicture(rightEdgeBottom)));

            rows = new List<RowInfo>();
            for (int r = 0; r < rowLines.Count - 1; r++) {
                List<float> dividers = new List<float>();
                LineF topLine = rowLines[r];
                LineF bottomLine = rowLines[r + 1];
                foreach (var ln in vertLines) {
                    if (TableX(ln.p1) - leftX > 10 && TableX(ln.p1) - rightX < -10) {
                        if (TableY(ln.p1) - 5 <= TableY(topLine.p1) && TableY(ln.p2) + 5 >= TableY(bottomLine.p1)) {
                            dividers.Add((TableX(ln.p1) + TableX(ln.p2)) / 2);
                        }
                    }
                }
                rows.Add(new RowInfo { topLine = topLine, bottomLine = bottomLine, dividers = dividers });
            }

            var rowClusters = ClusterRows();
            List<RowInfo> bestCluster = rowClusters[0];

            HealRowDividers(bestCluster);
            rowClusters = ClusterRows();
            bestCluster = rowClusters[0];

            int clusterStart = rows.IndexOf(bestCluster.First());
            int clusterEnd = rows.IndexOf(bestCluster.Last());
            for (int r = clusterStart + 1; r < rows.Count; r++) {
                // assert that cluster is contigious
                if (RowDividerDifferenceScore(bestCluster.First(), rows[r]) > 0) {
                    clusterEnd = r - 1;
                    break;
                }
                clusterEnd = r;
            }

            for (int r = clusterStart - 1; r >= 0; r--) {
                if (RowDividerDifferenceScore(bestCluster.First(), rows[r]) > 0) {
                    clusterStart = r + 1;
                    break;
                }
                clusterStart = r;
            }



            table = new Table();
            table.origin = ToPicture(new PointF(leftX, ToTable(rows[clusterStart].topLine.p1).Y));
            table.horizontalNormal = horizNormal;
            table.verticalNormal = vertNormal;

            table.rowHeights = new List<float>();
            for (int r = clusterStart; r <= clusterEnd; r++) {
                table.rowHeights.Add(RowHeight(rows[r]));
            }
            table.totalHeight = table.rowHeights.Sum();

            table.columnWidths = new List<float>();
            float prevColumn = leftX;
            foreach (float d in bestCluster[0].dividers) {
                table.columnWidths.Add(d - prevColumn);
                prevColumn = d;
            }
            table.columnWidths.Add(rightX - prevColumn);
            table.totalWidth = table.columnWidths.Sum();
        }

        private class RowInfo {
            public LineF topLine { get; set; }
            public LineF bottomLine { get; set; }
            public List<float> dividers { get; set; }
        }

        private List<List<RowInfo>> ClusterRows() {
            List<List<RowInfo>> clusters = new List<List<RowInfo>>();
            foreach (var row in rows) {
                List<RowInfo> bestCluster = clusters.Find(cluster => RowDifference(row, cluster[0]) < 0.1);
                if (bestCluster != null) {
                    bestCluster.Add(row);
                } else {
                    List<RowInfo> newCluster = new List<RowInfo>();
                    newCluster.Add(row);
                    clusters.Add(newCluster);
                }
            }
            clusters = clusters.OrderByDescending(clst => clst.Count).ToList();
            return clusters;
        }

        private RowInfo RowClusterCenter(List<RowInfo> rowCluster) {
            double[] distances = new double[rowCluster.Count];
            for (int r1 = 0; r1 < rowCluster.Count; r1++) {
                for (int r2 = 0; r2 < rowCluster.Count; r2++) {
                    distances[r1] += RowDifference(rowCluster[r1], rowCluster[r2]);
                }
            }
            double minDistance = distances.Min();
            for (int r = 0; r < rowCluster.Count; r++) {
                if (distances[r] == minDistance)
                    return rowCluster[r];
            }
            throw new ArgumentException("unable to found row info for min distance");
        }

        private double RowDifference(RowInfo r1, RowInfo r2) {
            double dividersScore = RowDividerDifferenceScore(r1, r2);
            double heightScore = RowHeightDifferenceScore(r1, r2);
            return dividersScore + heightScore;
        }

        private double RowDividerDifferenceScore(RowInfo r1, RowInfo r2) {
            HashSet<float> dividers1 = new HashSet<float>(r1.dividers);
            HashSet<float> dividers2 = new HashSet<float>(r2.dividers);
            return 1 - (double) dividers1.Intersect(dividers2).Count() / dividers1.Union(dividers2).Count();
        }

        private float RowHeight(RowInfo r) {
            return TableY(r.bottomLine.p1) - TableY(r.topLine.p1);
        }

        private double RowHeightDifferenceScore(RowInfo r1, RowInfo r2) {
            float height1 = RowHeight(r1);
            float height2 = RowHeight(r2);
            float avgHeight = (height1 + height2) / 2;
            return Math.Abs(height1 - height2) / avgHeight;
        }

        private void HealRowDividers(List<RowInfo> bestCluster) {
            RowInfo bestRow = RowClusterCenter(bestCluster);
            foreach (var row in rows) {
                if (RowDifference(bestCluster[0], row) < 0.3) {
                    row.dividers = new List<float>(bestRow.dividers);
                }
            }
        }

        public float TableX(PointF p) {
            return PointOps.DotProduct(p, horizNormal);
        }

        public float TableY(PointF p) {
            return PointOps.DotProduct(p, vertNormal);
        }

        public PointF ToTable(PointF p) {
            return new PointF(TableX(p), TableY(p));
        }

        public float PictureX(PointF p) {
            return PointOps.DotProduct(p, invHorizNormal);
        }

        public float PictureY(PointF p) {
            return PointOps.DotProduct(p, invVertNormal);
        }

        public PointF ToPicture(PointF p) {
            return new PointF(PictureX(p), PictureY(p));
        }

        public Bitmap DebugImage(Bitmap bw) {
            Bitmap res = new Bitmap(bw);

            Graphics g = Graphics.FromImage(res);

            g.DrawLine(new Pen(Color.Green, 4), leftEdge.p1, leftEdge.p2);
            g.DrawLine(new Pen(Color.Green, 4), rightEdge.p1, rightEdge.p2);
            foreach (var row in rowLines) {
                g.DrawLine(new Pen(Color.Red, 4), row.p1, row.p2);
            }

            foreach (var row in rows) {
                foreach (float d in row.dividers) {
                    g.DrawLine(new Pen(Color.Blue, 4),
                        PointOps.TruncPt(ToPicture(new PointF(d, ToTable(row.topLine.p1).Y))),
                        PointOps.TruncPt(ToPicture(new PointF(d, ToTable(row.bottomLine.p1).Y))));
                }
            }

            g.Dispose();

            return res;
        }

        public Bitmap ResultImage(Bitmap bw) {
            Bitmap res = new Bitmap(bw);
            Graphics g = Graphics.FromImage(res);
            table.DrawTable(g, new Pen(Color.Red, 4));
            g.Dispose();
            return res;
        }
    }
}
