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
        static void Main()
        {
            //Application.SetHighDpiMode(HighDpiMode.SystemAware);
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            Inventory i = new Inventory("C:\\Users\\Chris\\source\\repos2020\\MTGInventory\\MTGInventory\\bin\\Debug\\netcoreapp3.1\\InventoryFile_07_05_2020.mtgi");
        }
    }

    class Inventory
    {
        private string privateKey = "48FF57BE-A076-45FD-A979-CB8CB232EB2E";
        private string publicKey = "4D483803-142D-4861-9103-B6CF5149AAF9";
        private string bearerToken;
        private Dictionary<string, string> editions;
        private Dictionary<string, string> editionNames;
        string[] extraColumns;
        ArrayList cards;
        string outPutFileName;



        public Inventory(string filePath)
        {

            loadConfig("config.JSON");
            setBearerTokenAsync().Wait();
            setGroupIDsAsync().Wait();
            processFile(filePath);
            outputFile();
            outputEditionsFile();
        }

        public void loadConfig(string filepath)
        {
            using (System.IO.StreamReader sr = File.OpenText(filepath))
            {
                JObject configJSON = JObject.Parse(sr.ReadToEnd());
                outPutFileName = configJSON.Property("OutputFileName").Value.ToString();
                extraColumns = JArray.Parse(configJSON.Property("Columns").First.ToString()).ToObject<string[]>();

            }
        }

        public async System.Threading.Tasks.Task setBearerTokenAsync()
        {

            //Retrive the current token and use it if it has at least 1 day left.
            if (File.Exists("token.txt"))
            {
                using (System.IO.StreamReader sr = File.OpenText("token.txt"))
                {
                    string token = sr.ReadLine();
                    string date = sr.ReadLine();
                    date = date.Substring(5, date.Length - 18);
                    DateTime validTill = DateTime.Parse(date);
                    if (validTill.AddDays(-1) > DateTime.Now)
                    {
                        bearerToken = token;
                        return;
                    }
                }
            }

            //Otherwise we need a new token
            using (var httpClient = new HttpClient())
            {

                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.tcgplayer.com/token"))
                {
                    request.Content = new StringContent("grant_type=client_credentials&client_id=" + publicKey + "&client_secret=" + privateKey);
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                   
                    var response = await httpClient.SendAsync(request);
                    
                    JObject responseJSON = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                    bearerToken = responseJSON.Property("access_token").Value.ToString();

                    string fName = "token.txt";
                    using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(fName))
                    {
                        file.WriteLine(bearerToken);
                        file.WriteLine(responseJSON.Property(".expires").Value.ToString());
                    }
                }

            }

        }


        public async System.Threading.Tasks.Task setGroupIDsAsync()
        {
            using (var httpClient = new HttpClient())
            {
                editions = new Dictionary<string, string>();
                editionNames = new Dictionary<string, string>();
                int limit = 10000;
                int offset = 0;

                while (offset < limit)
                {
                    
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), "http://api.tcgplayer.com/v1.32.0/catalog/groups?categoryId=1&limit=100&offset=" + offset))
                    {
                        request.Headers.Add("Authorization", "bearer " + bearerToken);
                       
                        
                        var response = await httpClient.SendAsync(request);

                        JObject responseJSON = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        limit = int.Parse(responseJSON.Property("totalItems").Value.ToString());


                        foreach (JToken j in responseJSON.Property("results").Value.Children())
                        {
                            JObject set = JObject.Parse(j.ToString());

                            if (set.Property("abbreviation").Value.ToString() != "")
                            {
                                editions.Add(set.Property("abbreviation").Value.ToString(), set.Property("groupId").Value.ToString());
                                editionNames.Add(set.Property("abbreviation").Value.ToString(), set.Property("name").Value.ToString());
                            }


                        }

                    }
                    offset += 100;
                }
            }
        }
        



        public void processFile(string filepath)
        {
            using (System.IO.StreamReader sr = File.OpenText(filepath))
            {
                //Toss the header
                sr.ReadLine();
                cards = new ArrayList();
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    cards.Add(new Card(s, extraColumns.Length, editions, bearerToken));

                }
            }

        }


        public void outputFile()
        {

            string fName = outPutFileName + "_" + DateTime.Now.ToString("HHmm_dd_MM_yyyy") + ".mtgi";
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(fName,false))
            {

                file.WriteLine(getHeader());
                foreach (Card c in cards)
                {

                    file.WriteLine(c.toString());

                }
            }
        }


        public void outputEditionsFile()
        {
            string fName = "SetAbbreviations.mtgi";
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(fName, false))
            {

                file.WriteLine("Abbreviation\tSet Name" );
                foreach (string k in editionNames.Keys)
                {

                    file.WriteLine(k + "\t" + editionNames[k]);

                }
            }
        }

        private string getHeader()
        {
            string header = "Card Name\tSet\tFoil(Y/n)\tTCGMid\tTCGMarket\tQuantity";
            foreach (string s in extraColumns) header = header + "\t" + s;
            return header;
        }
    }

    class Card
    {
        public string cardName;
        public string set;
        public Boolean foil;
        public string tcgMid;
        public string tcgMarket;
        public int quantity;
        public string[] extraColumns;
        Dictionary<string, string> editions;
        string bearerToken;

        public Card(string line, int numExtraColumns, Dictionary<string, string> editions, string bearerToken)
        {
            this.editions = editions;
            this.bearerToken = bearerToken;


            string[] data = line.Split("\t");
            cardName = data[0];
            if (cardName == "") return;
            set = data[1];
            foil = (data[2] == "Y" ? true : false);
            //3 is tcg mid
            //4 is tcg tcgMarket
            quantity = Int32.Parse(data[5]);
            extraColumns = new string[numExtraColumns];
            for (int i = 0; i < numExtraColumns; i++) extraColumns[i] = data[i + 6];
            try
            {
                setPricesAsync().Wait();
            }
            catch
            {
                tcgMarket = "NVD";
                tcgMid = "NVD";
            }
        }


        public async System.Threading.Tasks.Task setPricesAsync()
        {
            using (var httpClient = new HttpClient())
            {
                string productID;

                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "http://api.tcgplayer.com/v1.32.0/catalog/products?categoryId=1&productName=" + cardName.Replace(" ", "%20") + "&groupID=" + editions[set]))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", "bearer " + bearerToken);
                    var response = await httpClient.SendAsync(request);

                    JObject responseJSON = JObject.Parse(response.Content.ReadAsStringAsync().Result);


                    JObject card = null;

                    foreach (JToken j in responseJSON.Property("results").Value.Children())
                    {
                        card = JObject.Parse(j.ToString());
                    }

                    productID = card.Property("productId").Value.ToString();
                }


                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "http://api.tcgplayer.com/v1.32.0/pricing/product/" + productID))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", "bearer " + bearerToken);
                    var response = await httpClient.SendAsync(request);

                    JObject responseJSON = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                    foreach (JToken j in responseJSON.Property("results").Value.Children())
                    {
                        JObject pricing = JObject.Parse(j.ToString());

                        if (pricing.Property("subTypeName").Value.ToString() == "Normal" && !foil)
                        {
                            tcgMid = pricing.Property("midPrice").Value.ToString();
                            tcgMarket = pricing.Property("marketPrice").Value.ToString();
                            return;
                        }

                        if (pricing.Property("subTypeName").Value.ToString() == "Foil" && foil)
                        {
                            tcgMid = pricing.Property("midPrice").Value.ToString();
                            tcgMarket = pricing.Property("marketPrice").Value.ToString();
                            return;

                        }

                    }

                    tcgMarket = "NVD";
                    tcgMid = "NVD";
                }
            }


        }

        public string toString()
        {
            if (cardName == "") return "";
            string line = cardName + "\t" + set + "\t" + (foil ? "Y" : "N") + "\t" + tcgMid + "\t" + tcgMarket + "\t" + quantity;
            for (int i = 0; i < extraColumns.Length; i++) line += ("\t" + extraColumns[i]);
            return line;
        }


    }
}
