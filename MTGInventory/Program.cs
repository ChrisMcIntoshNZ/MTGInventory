using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
//using System.Windows.Forms;

namespace MTGInventory
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //Application.SetHighDpiMode(HighDpiMode.SystemAware);
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            if (args.Length == 0) {
                args = new string[1];
                args[0] = "C:\\Users\\Chris\\Documents\\Bleh\\Gonti.mtgi";
            }

            Inventory i = new Inventory(args[0]);
        }
    }
}

