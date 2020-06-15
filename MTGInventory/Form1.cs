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


                var filePath = "";
                using (System.Windows.Forms.OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        filePath = openFileDialog.FileName;
                    }
                }


                Inventory i = new Inventory(filePath);
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
