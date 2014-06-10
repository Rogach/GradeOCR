using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Grader.gui {
    class Separator : Control {

        public enum Direction {
            Horizontal, Vertical
        }

        private Direction direction;
        private float trimEnds = 0.01f;

        public Separator(Direction direction) {
            this.BackColor = Color.White;
            this.direction = direction;
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            Pen p = new Pen(Color.FromArgb(100, 0, 0, 0), 1);
            if (direction == Direction.Horizontal) {
                e.Graphics.DrawLine(p, 
                    new Point((int) (this.Width * trimEnds), this.Height / 2), 
                    new Point((int) (this.Width * (1 - trimEnds)), this.Height / 2));
            } else if (direction == Direction.Vertical) {
                e.Graphics.DrawLine(p,
                    new Point(this.Width / 2, (int) (this.Height * trimEnds)),
                    new Point(this.Width / 2, (int) (this.Height * (1 - trimEnds))));
            }
        }
    }
}
