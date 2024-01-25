﻿using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MusicRandomizer
{
    public partial class PlaylistNameForm : Form
    {
        public string name;
        private bool pressedSave = false;

        public PlaylistNameForm()
        {
            InitializeComponent();
        }

        public PlaylistNameForm(string defaultName)
        {
            InitializeComponent();

            txtName.Text = defaultName;
            name = defaultName;
        }

        private void PlaylistNameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !pressedSave)
            {
                name = null;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
           name = txtName.Text;

            // Check to make sure the user actually entered a name
            if (name.Length == 0)
            {
                MessageBox.Show("Please enter in a name.");
                return;
            }

            // Check for invalid characters in the filename
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (name.Contains(c))
                {
                    MessageBox.Show("There are invalid characters in the playlist name.");
                    return;
                }
            }

            // Check to make sure this name isn't already taken
            if (File.Exists("playlists\\" + name + ".xml"))
            {
                MessageBox.Show("That playlist already exists.");
                return;
            }

            pressedSave = true;
            this.Close();
        }

    }
}
