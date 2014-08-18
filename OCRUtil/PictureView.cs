using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;

namespace OCRUtil {
    public class PictureView : UserControl {

        private Image _Image;
        public Image Image {
            get {
                return _Image;
            }
            set {
                Bitmap copy = new Bitmap((Bitmap) value);
                this.Invoke(new EventHandler(delegate {
                    _Image = copy;
                    this.Invalidate();
                    this.ZoomToFit();
                }));
            }
        }

        public bool AllowZoom { get; set; }

        public bool AllowMaximization { get; set; }

        public void SetImageKeepZoom(Image img) {
            Bitmap copy = new Bitmap((Bitmap) img);
            this.Invoke(new EventHandler(delegate {
                _Image = copy;
                this.Invalidate();
            }));
        }

        private float zoom = 1;
        private float offsetX = 0;
        private float offsetY = 0;
        
        private HScrollBar hScrollBar = new HScrollBar();
        private VScrollBar vScrollBar = new VScrollBar();
        private Panel plug = new Panel();

        public static readonly int scrollBarWidth = 15;

        public PictureView() : base() {
            this.BorderStyle = BorderStyle.FixedSingle;
            this.DoubleBuffered = true;

            this.AllowZoom = true;
            this.AllowMaximization = true;

            _Image = LoadPlaceholder();
            this.Controls.Add(plug);
            hScrollBar.Minimum = 0;
            hScrollBar.Maximum = 1000;
            this.Controls.Add(hScrollBar);
            vScrollBar.Minimum = 0;
            vScrollBar.Maximum = 1000;
            this.Controls.Add(vScrollBar);

            this.SizeChanged += new EventHandler(delegate {
                ClampOffsets();
                this.PerformLayout();
                this.Invalidate();
            });

            AddMouseScroll();
            AddMouseDrag();
            AddMouseZoom();
            AddDoubleClickListener();
            AddFullFrameListener();
        }

        public static PictureView InsertIntoPanel(Panel panel) {
            PictureView pv = new PictureView();
            pv.Size = panel.Size;
            pv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            pv.Location = new Point(0, 0);
            panel.Controls.Add(pv);
            return pv;
        }

        private void AddMouseScroll() {
            hScrollBar.Scroll += new ScrollEventHandler(delegate {
                offsetX = (_Image.Width - (this.Width - scrollBarWidth) / zoom) * ((float) hScrollBar.Value / 1000);
                this.Invalidate();
            });
            vScrollBar.Scroll += new ScrollEventHandler(delegate {
                offsetY = (_Image.Height - (this.Height - scrollBarWidth) / zoom) * ((float) vScrollBar.Value / 1000);
                this.Invalidate();
            });
        }

        private void AddMouseZoom() {
            this.MouseWheel += new MouseEventHandler(delegate(object sender, MouseEventArgs e) {
                if (AllowZoom) {
                    float origZoom = zoom;

                    for (int i = 0; i < e.Delta; i += 120) {
                        zoom *= 1.05f;
                    }
                    for (int i = 0; i > e.Delta; i -= 120) {
                        zoom *= 0.95f;
                    }

                    bool origFit = _Image.Width * origZoom <= this.Width && _Image.Height * origZoom <= this.Height;
                    bool fit = _Image.Width * zoom <= this.Width && _Image.Height * zoom <= this.Height;

                    if (fit) {
                        // if image fits into view, no action is required
                        offsetX = 0;
                        offsetY = 0;
                    } else if (origFit && !fit) {
                        // if image fit into frame before zoom, but stopped fitting after - need to center the image
                        if (_Image.Width * zoom > this.Width - scrollBarWidth) {
                            offsetX = (_Image.Width - (this.Width - scrollBarWidth) / zoom) / 2;
                        } else {
                            offsetX = 0;
                        }
                        if (_Image.Height * zoom > this.Height - scrollBarWidth) {
                            offsetY = (_Image.Height - (this.Height - scrollBarWidth) / zoom) / 2;
                        } else {
                            offsetY = 0;
                        }
                        ClampOffsets();
                    } else {
                        // image didn't fit before zoom or after
                        if (_Image.Width * zoom <= this.Width - scrollBarWidth) {
                            offsetX = 0;
                        } else {
                            offsetX += ((this.Width - scrollBarWidth) / origZoom - (this.Width - scrollBarWidth) / zoom) / 2;
                        }
                        if (_Image.Height * zoom <= this.Height - scrollBarWidth) {
                            offsetY = 0;
                        } else {
                            offsetY += ((this.Height - scrollBarWidth) / origZoom - (this.Height - scrollBarWidth) / zoom) / 2;
                        }
                        ClampOffsets();
                    }

                    UpdateScrollBars();

                    this.PerformLayout();
                    this.Invalidate();
                }
            });
        }

        private void AddMouseDrag() {
            Point dragStart = new Point();
            float startOffsetX = 0;
            float startOffsetY = 0;

            this.MouseDown += new MouseEventHandler(delegate(object sender, MouseEventArgs e) {
                if (e.Button == MouseButtons.Left) {
                    dragStart = e.Location;
                    startOffsetX = offsetX;
                    startOffsetY = offsetY;
                }
            });
            this.MouseMove += new MouseEventHandler(delegate(object sender, MouseEventArgs e) {
                if (e.Button == MouseButtons.Left) {
                    offsetX = startOffsetX + (dragStart.X - e.X) / zoom;
                    offsetY = startOffsetY + (dragStart.Y - e.Y) / zoom;
                    ClampOffsets();
                    UpdateScrollBars();
                    this.Cursor = Cursors.NoMove2D;
                    this.Invalidate();
                }
            });
            this.MouseUp += new MouseEventHandler(delegate(object sender, MouseEventArgs e) {
                if (e.Button == MouseButtons.Left) {
                    this.Cursor = Cursors.Default;
                }
            });
        }

        private void AddDoubleClickListener() {
            this.MouseDoubleClick += new MouseEventHandler(delegate(object sender, MouseEventArgs e) {
                if (e.Button == MouseButtons.Left) {
                    Point pt;
                    if (_Image.Width * zoom <= this.Width && _Image.Height * zoom <= this.Height) {
                        float dx = (this.Width - _Image.Width * zoom) / 2;
                        float dy = (this.Height - _Image.Height * zoom) / 2;
                        pt = new Point((int) Math.Floor((e.X - dx) / zoom), (int) Math.Floor((e.Y - dy) / zoom));
                    } else if (_Image.Width * zoom <= this.Width - scrollBarWidth) {
                        float dx = (this.Width - _Image.Width * zoom - scrollBarWidth) / 2;
                        pt = new Point((int) Math.Floor((e.X - dx) / zoom), (int) Math.Floor(offsetY + e.Y / zoom));
                    } else if (_Image.Height * zoom <= this.Height - scrollBarWidth) {
                        float dy = (this.Height - _Image.Height * zoom - scrollBarWidth) / 2;
                        pt = new Point((int) Math.Floor(offsetX + e.X / zoom), (int) Math.Floor((e.Y - dy) / zoom));
                    } else {
                        pt = new Point((int) Math.Floor(offsetX + e.X / zoom), (int) Math.Floor(offsetY + e.Y / zoom));
                    }
                    if (pt.X >= 0 && pt.X < _Image.Width && pt.Y >= 0 && pt.Y < _Image.Height) {
                        foreach (var listener in DoubleClickListeners) {
                            listener.Invoke(pt, e);
                        }
                    }
                }
            });
        }

        private void AddFullFrameListener() {
            this.MouseDoubleClick += new MouseEventHandler(delegate(object sender, MouseEventArgs e) {
                if (e.Button == MouseButtons.Middle && AllowMaximization) {
                    Form f = new Form();
                    f.Size = new Size(1000, 1000);

                    PictureView pv = new PictureView();
                    pv.Location = new Point(0, 0);
                    pv.Size = new Size(984, 962);
                    pv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
                    foreach (var mouseListener in DoubleClickListeners) {
                        pv.AddDoubleClickListener(mouseListener);
                    }
                    pv._Image = _Image;
                    pv.ZoomToFit();
                    pv.AllowMaximization = false;
                    f.Controls.Add(pv);
                    
                    f.ShowDialog();
                }
            });
        }

        public static Image placeholderImage = null;
        public static Image LoadPlaceholder() {
            if (placeholderImage == null) {
                placeholderImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("OCRUtil.hourglass.png"));
            }
            return placeholderImage;
        }

        protected override void OnLayout(LayoutEventArgs e) {
            base.OnLayout(e);
            if (_Image.Width * zoom <= this.Width && _Image.Height * zoom <= this.Height) {
                // the whole image fits into component, no scroll bars required
                vScrollBar.Visible = false;
                hScrollBar.Visible = false;
                plug.Visible = false;
            } else {
                // image doesn't fit into component, need to display scrolling gears
                vScrollBar.Visible = true;
                hScrollBar.Visible = true;
                plug.Visible = true;

                vScrollBar.Size = new Size(scrollBarWidth, this.Height - scrollBarWidth);
                vScrollBar.Location = new Point(this.Width - scrollBarWidth - 2, 0);
                hScrollBar.Size = new Size(this.Width - scrollBarWidth, scrollBarWidth);
                hScrollBar.Location = new Point(0, this.Height - scrollBarWidth - 2);
                plug.Size = new Size(scrollBarWidth, scrollBarWidth);
                plug.Location = new Point(this.Width - scrollBarWidth, this.Height - scrollBarWidth);
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            if (zoom > 1) {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
            } else {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;
            }

            if (_Image.Width * zoom <= this.Width && _Image.Height * zoom <= this.Height) {
                float zw = _Image.Width * zoom;
                float zh = _Image.Height * zoom;
                float x = (this.Width - zw) / 2;
                float y = (this.Height - zh) / 2;
                e.Graphics.DrawImage(
                    _Image, 
                    new RectangleF(x, y, zw, zh), 
                    new RectangleF(0 - 0.5f, 0 - 0.5f, _Image.Width, _Image.Height), 
                    GraphicsUnit.Pixel);
            } else if (_Image.Height * zoom <= this.Height - scrollBarWidth) {
                float destW = this.Width - scrollBarWidth;
                float destH = _Image.Height * zoom;
                float y = (this.Height - scrollBarWidth - destH) / 2;
                e.Graphics.DrawImage(
                    _Image,
                    new RectangleF(0, y, destW, destH),
                    new RectangleF(offsetX - 0.5f, 0 - 0.5f, destW / zoom, _Image.Height),
                    GraphicsUnit.Pixel
                );
            } else if (_Image.Width * zoom <= this.Width - scrollBarWidth) {
                float destW = _Image.Width * zoom;
                float x = (this.Width - scrollBarWidth - destW) / 2;
                float destH = this.Height - scrollBarWidth;
                e.Graphics.DrawImage(
                    _Image,
                    new RectangleF(x, 0, destW, destH),
                    new RectangleF(0 - 0.5f, offsetY - 0.5f, _Image.Width, destH / zoom),
                    GraphicsUnit.Pixel
                );
            } else {
                float destW = this.Width - scrollBarWidth;
                float destH = this.Height - scrollBarWidth;
                if (destW / zoom < 2 && destH / zoom < 2) {
                    destW = zoom * 2;
                    destH = zoom * 2;
                }
                e.Graphics.DrawImage(
                    _Image,
                    new RectangleF(0, 0, destW, destH),
                    new RectangleF(offsetX - 0.5f, offsetY - 0.5f, destW / zoom, destH / zoom),
                    GraphicsUnit.Pixel
                );
            }
        }

        private void UpdateScrollBars() {
            hScrollBar.Value = (int) Math.Min(1000, Math.Max(0, hScrollBar.Maximum * (offsetX / (_Image.Width - (this.Width - scrollBarWidth) / zoom))));
            vScrollBar.Value = (int) Math.Min(1000, Math.Max(0, vScrollBar.Maximum * (offsetY / (_Image.Height - (this.Height - scrollBarWidth) / zoom))));
        }

        private void ClampOffsets() {
            offsetX = Math.Min(_Image.Width - (this.Width - scrollBarWidth) / zoom, Math.Max(0, offsetX));
            offsetY = Math.Min(_Image.Height - (this.Height - scrollBarWidth) / zoom, Math.Max(0, offsetY));
        }

        public void ZoomToFit() {
            float zoomX = (float) this.Width / (float) _Image.Width;
            float zoomY = (float) this.Height / (float) _Image.Height;
            zoom = Math.Min(zoomX, zoomY) * 0.99f;
            this.PerformLayout();
            this.Invalidate();
        }

        private List<Action<Point, MouseEventArgs>> DoubleClickListeners = new List<Action<Point, MouseEventArgs>>();

        public void AddDoubleClickListener(Action<Point, MouseEventArgs> listener) {
            DoubleClickListeners.Add(listener);
        }

        public void RemoveDoubleClickListener(Action<Point, MouseEventArgs> listener) {
            DoubleClickListeners.Remove(listener);
        }
    }
}
