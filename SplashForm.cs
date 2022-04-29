using HnsExplorer.Data;

namespace HnsExplorer
{
    public partial class SplashForm : Form
    {
        private HnsDatasource datasource;
        public SplashForm(HnsDatasource datasource)
        {
            this.datasource = datasource;
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = datasource.LoadingState;
            var p = datasource.NumberOfStepsLoaded;
            var t = datasource.NumberOfStepsTotal;
            var perc = (double)p / t * 100;
            progressBar1.Value = (int)perc;
            progressBar1.Update();
            if (p == t)
            {
                timer1.Stop();
                Close();
            }
        }
    }
}
