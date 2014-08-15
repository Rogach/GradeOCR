using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OCRUtil;
using LibUtil;

namespace TableOCR {

    /*
     * Builds a table for given set of horizontal/vertical lines.
     *
     * First, we find median for left and right end points of horizontal lines.
     * We take those points as X values for left and right edge lines of new table.
     *  
     * Afterwards, we retain only the rows lines that fall within certain threshold of those edge lines
     * (thus removing some of the artifacts).
     * Resulting row lines are paired with each other (from top to bottom) and form "rows" of our table.
     * 
     * Then, for each row, we find which vertical lines intersect it, thus retrieving cell borders.
     * 
     * We cluster resulting rows. Clustering distance is defined 
     * as differences in row height and set of vertical row dividers.
     * Biggest row cluster is then assumed to contain out target rows.
     * 
     * Those lines that are close to biggest row cluster, but do not exactly qualify,
     * are assumed to be "damaged" and are healed by replacing row dividers by those of main cluster.
     * 
     * Main cluster is then extended up and down, considering only row dividers when accepting new rows
     * (thus we allow several rows to be of wildly different width).
     * 
     * Rows that have height as integer multiple of median row height are assumed to be "damaged" and
     * are healed by splitting them into smaller rows (we only accept such split if there are real, but not
     * long enough lines in original image).
     */
    public class TableBuilder {

        public static Option<Table> ExtractTable(LineNormalization lnorm) {
            return NewBuilder(lnorm).table;
        }

        public static TableBuilder NewBuilder(LineNormalization lnorm) {
            return new TableBuilder(lnorm.angle, lnorm.normHorizLines, lnorm.normRotVertLines);
        }

        /* Threshold for distance of line end point to table edge.
         * Used when filtering horizontal lines.
         */
        public static readonly int sideEgdeThreshold = 100;

        PointF horizNormal;
        PointF vertNormal;
        PointF invHorizNormal;
        PointF invVertNormal;

        List<LineF> rowLines;

        float leftEdgeX;
        float rightEdgeX;

        LineF leftEdge;
        LineF rightEdge;

        private class RowInfo {
            public LineF topLine { get; set; }
            public LineF bottomLine { get; set; }
            public List<float> dividers { get; set; }
        }

        List<RowInfo> rows;

        public Option<Table> table = new None<Table>();

        public TableBuilder(double tableAngle, List<LineF> horizLines, List<LineF> vertLines) {
            horizNormal = new PointF((float) Math.Cos(tableAngle), (float) Math.Sin(tableAngle));
            vertNormal = new PointF((float) -Math.Sin(tableAngle), (float) Math.Cos(tableAngle));
            invHorizNormal = new PointF((float) Math.Cos(tableAngle), (float) -Math.Sin(tableAngle));
            invVertNormal = new PointF((float) Math.Sin(tableAngle), (float) Math.Cos(tableAngle));

            // convert incoming lines into table space
            horizLines = horizLines.ConvertAll(ln => new LineF(ToTable(ln.p1), ToTable(ln.p2)));
            vertLines = vertLines.ConvertAll(ln => new LineF(ToTable(ln.p1), ToTable(ln.p2)));

            BuildTable(horizLines, vertLines);
        }

        private void BuildTable(List<LineF> horizLines, List<LineF> vertLines) {
            rowLines = ExtractTableRowLines(horizLines);
            if (rowLines.Count < 2) return;

            CalculateEdgeLines();

            // build row objects
            rows = new List<RowInfo>();
            for (int r = 0; r < rowLines.Count - 1; r++) {
                rows.Add(new RowInfo { topLine = rowLines[r], bottomLine = rowLines[r + 1], dividers = new List<float>() });
            }

            // calculate vertical row dividers
            foreach (var row in rows) {
                foreach (var ln in vertLines) {
                    if (ln.p1.X - leftEdgeX > 10 && ln.p1.X - rightEdgeX < -10) {
                        if (ln.p1.Y - 5 <= row.topLine.p1.Y && ln.p2.Y + 5 >= row.bottomLine.p1.Y) {
                            row.dividers.Add((ln.p1.X + ln.p2.X) / 2);
                        }
                    }
                }
            }

            var rowClusters = ClusterRows();
            List<RowInfo> bestCluster = rowClusters[0];

            HealRowDividers(bestCluster);
            rowClusters = ClusterRows();
            bestCluster = rowClusters[0];

            int clusterStart = rows.IndexOf(bestCluster.First());
            int clusterEnd = rows.IndexOf(bestCluster.Last());

            // try extending cluster down
            for (int r = clusterStart + 1; r < rows.Count; r++) {
                if (RowDividerDifferenceScore(bestCluster.First(), rows[r]) > 0) {
                    clusterEnd = r - 1;
                    break;
                }
                clusterEnd = r;
            }

            // try extending cluster up
            for (int r = clusterStart - 1; r >= 0; r--) {
                if (RowDividerDifferenceScore(bestCluster.First(), rows[r]) > 0) {
                    clusterStart = r + 1;
                    break;
                }
                clusterStart = r;
            }

            rows = rows.GetRange(clusterStart, clusterEnd - clusterStart + 1);
            rows = HealRows(rows, horizLines, leftEdgeX, rightEdgeX);

            this.table = new Some<Table>(new Table());
            this.table.ForEach(table => {
                table.origin = ToPicture(new PointF(leftEdgeX, rows[0].topLine.p1.Y));
                table.horizontalNormal = horizNormal;
                table.verticalNormal = vertNormal;

                table.rowHeights = new List<float>();
                foreach (var row in rows) {
                    table.rowHeights.Add(RowHeight(row));
                }
                table.totalHeight = table.rowHeights.Sum();

                table.columnWidths = new List<float>();
                float prevColumn = leftEdgeX;
                foreach (float d in RowClusterCenter(bestCluster).dividers) {
                    table.columnWidths.Add(d - prevColumn);
                    prevColumn = d;
                }
                table.columnWidths.Add(rightEdgeX - prevColumn);
                table.totalWidth = table.columnWidths.Sum();
            });
        }

        public List<LineF> ExtractTableRowLines(List<LineF> horizLines) {
            // calculate X values for left and right table edge
            float leftMedian = horizLines.Select(ln => ln.p1.X).Median();
            float rightMedian = horizLines.Select(ln => ln.p2.X).Median();

            // retain only lines that have ends within threshold distance from table edges
            return
                horizLines
                .Where(ln => Math.Abs(ln.p1.X - leftMedian) < sideEgdeThreshold)
                .Where(ln => Math.Abs(ln.p2.X - rightMedian) < sideEgdeThreshold)
                .OrderBy(ln => ln.p1.Y)
                .ToList();
        }

        public void CalculateEdgeLines() {
            List<PointF> leftEndPoints = rowLines.Select(ln => ln.p1).ToList();
            List<PointF> rightEndPoints = rowLines.Select(ln => ln.p2).ToList();

            // recalculate X values for left and right table edges (minimizing RMSD from end points)
            leftEdgeX = leftEndPoints.Select(pt => pt.X).Average();
            rightEdgeX = rightEndPoints.Select(pt => pt.X).Average();

            PointF leftEdgeTop = new PointF(leftEdgeX, leftEndPoints.First().Y);
            PointF leftEdgeBottom = new PointF(leftEdgeX, leftEndPoints.Last().Y);
            leftEdge = new LineF(leftEdgeTop, leftEdgeBottom);

            PointF rightEdgeTop = new PointF(rightEdgeX, rightEndPoints.First().Y);
            PointF rightEdgeBottom = new PointF(rightEdgeX, rightEndPoints.Last().Y);
            rightEdge = new LineF(rightEdgeTop, rightEdgeBottom);
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

        /* 
         * Heal rows by calculating median row height
         * and splitting bigger rows by that row height
         */
        private List<RowInfo> HealRows(List<RowInfo> rows, List<LineF> horizLines, float leftX, float rightX) {
            List<float> rowHeights = rows.Select(row => RowHeight(row)).OrderBy(h => h).ToList();
            float medianRowHeight = rowHeights[rowHeights.Count / 2];

            return rows.SelectMany(row => {
                float possibleInnerRows = RowHeight(row) / medianRowHeight;
                int innerRowCount = (int) Math.Round(possibleInnerRows);
                float ratio = innerRowCount / possibleInnerRows;
                if (innerRowCount > 1 && ratio > 0.9 && ratio < 1.1) {
                    // this row can be divided into several smaller ones
                    // because we can fit whole number of median row heights into it
                    float innerRowHeight = RowHeight(row) / innerRowCount;
                    List<LineF> innerRowLines = new List<LineF>();
                    for (int ir = 1; ir < innerRowCount; ir++) {
                        float irY = row.topLine.p1.Y + ir * innerRowHeight;
                        if (horizLines.Find(hl => Math.Abs(hl.p1.Y - irY) < 5) != null) {
                            // we can find real (sub-threshold) line for this Y,
                            // so we confirm our guess
                            innerRowLines.Add(new LineF(new PointF(leftX, irY), new PointF(rightX, irY)));
                        }
                    }
                    // check that we were able to confirm our guess for all inner lines
                    if (innerRowLines.Count == innerRowCount - 1) {
                        // return guessed rows
                        List<RowInfo> innerRows = new List<RowInfo>();
                        innerRowLines.Add(row.bottomLine);
                        LineF prevLine = row.topLine;
                        for (int ir = 0; ir < innerRowCount; ir++) {
                            innerRows.Add(new RowInfo {
                                topLine = prevLine,
                                bottomLine = innerRowLines[ir],
                                dividers = new List<float>(row.dividers)
                            });
                            prevLine = innerRowLines[ir];
                        }
                        return innerRows;
                    } else {
                        // return orignal row
                        return new List<RowInfo> { row };
                    }
                } else {
                    return new List<RowInfo> { row };
                }
            }).ToList();
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

            g.DrawLine(new Pen(Color.Green, 4), ToPicture(leftEdge.p1), ToPicture(leftEdge.p2));
            g.DrawLine(new Pen(Color.Green, 4), ToPicture(rightEdge.p1), ToPicture(rightEdge.p2));
            foreach (var row in rowLines) {
                g.DrawLine(new Pen(Color.Red, 4), ToPicture(row.p1), ToPicture(row.p2));
            }

            foreach (var row in rows) {
                foreach (float d in row.dividers) {
                    g.DrawLine(new Pen(Color.Blue, 4),
                        ToPicture(new PointF(d, row.topLine.p1.Y)),
                        ToPicture(new PointF(d, row.bottomLine.p1.Y)));
                }
            }

            g.Dispose();

            return res;
        }
    }
}
