using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using MediaPanther.Aggregator.Reaper;
using MediaPanther.Aggregator.Reaper.Exceptions;

namespace MediaPanther.Aggregator.ReaperHarness
{
    public partial class Main : Form
    {
        #region members
        private Reaper.Reaper _reaper;
        #endregion

        #region constructors
        public Main() 
        {
            InitializeComponent();
        }
        #endregion

        private void Main_Load(object sender, EventArgs e) 
        {
            _reaper = new MediaPanther.Aggregator.Reaper.Reaper();
            Thread reaperThread = new Thread(new ThreadStart(_reaper.Start));

            reaperThread.IsBackground = true;
            reaperThread.Start();
        }

        private void StopProcessingBtn_Click(object sender, EventArgs e)
        {
            this._reaper.Stop();
            MessageBox.Show("Processing will stop after current cycle.");
        }
    }
}