using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace MTGInventory
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Show();
            try
            {
                Inventory i = new Inventory();
                lblState.Text = "Loading Config";
                this.Refresh();
                i.loadConfig("config.JSON");
                lblState.Text = "Getting Token";
                this.Refresh();
                i.setBearerTokenAsync().Wait();
                lblState.Text = "Getting Set Info";
                this.Refresh();
                i.setGroupIDsAsync().Wait();
                lblState.Text = "Processing Input File";
                this.Refresh();
                i.processFile();
                lblState.Text = "Writing Output File";
                this.Refresh();
                i.outputFile();
                lblState.Text = "Making Set File";
                this.Refresh();
                i.outputEditionsFile();
                System.Windows.Forms.MessageBox.Show("Done");
                this.Close();
            } catch (Exception e)
            {
                lblState.Text = e.Message;
                this.Refresh();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
