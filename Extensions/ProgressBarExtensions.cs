using System.Drawing.Drawing2D;

namespace HnsExplorer.Extensions
{
    public class ProgressBarExtended : ProgressBar
    {
        private SolidBrush? ForegroundBrush;
        private SolidBrush? BackgroundBrush;
        private Rectangle LoadingBarOuter;
        private Rectangle LoadingBarInner;
        public ProgressBarExtended()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (ForegroundBrush == null)
            {
                ForegroundBrush = new SolidBrush(ForeColor);
            }
            if (BackgroundBrush == null)
            {
                BackgroundBrush = new SolidBrush(BackColor);
            }
            if(LoadingBarOuter.IsEmpty)
            {
                LoadingBarOuter = new Rectangle(0, 0, Width, Height);
                DrawHorizontalBar(e.Graphics, BackgroundBrush, LoadingBarOuter);
            }

            double scaleFactor = (double)(Value - Minimum) / (Maximum - Minimum);
            int currentProgress = (int)((LoadingBarOuter.Width * scaleFactor) - 2);

            if (LoadingBarInner.IsEmpty)
            {
                LoadingBarInner = new Rectangle(1, 1, currentProgress, LoadingBarOuter.Height - 2);
            }

            LoadingBarInner.Width = currentProgress;
            DrawHorizontalBar(e.Graphics, ForegroundBrush, LoadingBarInner);
        }

        private void DrawHorizontalBar(Graphics g, Brush b, Rectangle bounds)
        {
            g.FillRectangle(b, bounds);
        }
    }
}
