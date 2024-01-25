﻿using System;
using System.Windows.Forms;

namespace MusicRandomizer
{
    public partial class RegionForm : Form
    {
        public SplatoonRegion chosenRegion;

        public RegionForm()
        {
            InitializeComponent();
        }

        private void VersionRequestForm_Load(object sender, EventArgs e)
        {
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!radNorthAmerica.Checked && !radEurope.Checked && !radJapan.Checked)
            {
                MessageBox.Show("Please choose a region.");
                return;
            }

            if (radNorthAmerica.Checked)
            {
                chosenRegion = SplatoonRegion.NorthAmerica;
            }
            else if (radEurope.Checked)
            {
                chosenRegion = SplatoonRegion.Europe;
            }
            else
            {
                chosenRegion = SplatoonRegion.Japan;
            }

            Close();
        }
    }
}
