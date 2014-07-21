﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace GradeOCR {
    public static class DigestExtractor {
        public static readonly int digestSize = 32;

        public static Bitmap ExtractDigestImage(Bitmap src) {
            Bitmap res = new Bitmap(digestSize, digestSize, PixelFormat.Format32bppArgb);
            res.SetResolution(src.HorizontalResolution, src.VerticalResolution);

            Graphics g = Graphics.FromImage(res);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;

            g.DrawImage(src,
                new Rectangle(0, 0, res.Width, res.Height),
                new Rectangle(0, 0, src.Width, src.Height),
                GraphicsUnit.Pixel);

            g.Dispose();

            return res;
        }
    }
}
