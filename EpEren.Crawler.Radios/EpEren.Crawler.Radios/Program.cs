using Figgle;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using xNet;
using Console = Colorful.Console;

namespace EpEren.Crawler.Radios
{
    public class Donut
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Type{ get; set; }
        public string Url{ get; set; }
        public string Image { get; set; }
    }

    class Program
    {
        public static string Get(string url)
        {
            using (HttpRequest req = new HttpRequest())
            {
                req.UserAgent = "EpEren RadyoCrawler V0.1";
                return (req.Get(url)).ToString();
            }
        }
        static void Main(string[] args)
        {
            Go().GetAwaiter().GetResult();

            Console.ReadKey(true);
        }
        public static string GetID(string Uri)
        {
            var Html = Get(Uri);
            var Eslesme = Regex.Matches(Html, "\"radyo\":\"(.*?)\"");
            return Eslesme[0].Groups[1].Value;
        }
        public static Donut GetData(string _ID)
        {
            
            Donut Ret = null;
            var Html = Get("https://www.canli-radyo.biz/wp-admin/admin-ajax.php?action=radyocal&radyo=" + _ID);
            var JsonCikti = ((JObject.Parse(Html))["html"].ToString());
            var EslesmeBir = Regex.Matches(JsonCikti, "{\"type\":\"(.*?)\",.\"src\":\"(.*?)\"}");

            var Eslesmeiki = Regex.Matches(JsonCikti, "<source rel=\"nofollow\" src=\"(.*?)\" type=\"(.*?)\">");

            if (EslesmeBir.Count > 0)
            {
                Ret = new Donut()
                {
                    ID = _ID,
                    Type = EslesmeBir[0].Groups[1].Value.ToString(),
                    Url = EslesmeBir[0].Groups[2].Value.ToString()
                };
            }else if(Eslesmeiki.Count > 0)
            {
                Ret= new Donut()
                {
                    ID = _ID,
                    Type = Eslesmeiki[0].Groups[2].Value.ToString(),
                    Url = Eslesmeiki[0].Groups[1].Value.ToString()
                };
            }

            return Ret;
           
        }
        public static string SaveImage(string id, string image)
        {
            if (!Directory.Exists("resimler"))
            {
                Directory.CreateDirectory("resimler");
            }

            using (HttpRequest req = new HttpRequest())
            {
                req.UserAgent = "EpEren RadyoCrawler V0.1";
                req.Get(image).ToFile("resimler/"+id+".jpg");

                return "resimler/" + id + ".jpg";
            }


        }
        public static async Task Go()
        {
            Console.Clear();
            Console.WriteLine(FiggleFonts.Big.Render("==== EpEren  < 3 ===="), Color.Red);
            Console.SetCursorPosition((Console.WindowWidth - "İnstagram: Ep.Eren".Length) / 2, Console.CursorTop);
            Console.WriteLine("İnstagram: Ep.Eren", Color.Green);
            Console.SetCursorPosition((Console.WindowWidth - "Github: ErenKrt".Length) / 2, Console.CursorTop);
            Console.WriteLine("Github: ErenKrt", Color.Green);
            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------", Color.DarkOrange);

            Console.WriteLine("Devam etmek için bir tuşa basın");

            Console.ReadKey();
            Console.Clear();

            List<Donut> Donuts = new List<Donut>();

            HtmlAgilityPack.HtmlDocument Document = new HtmlAgilityPack.HtmlDocument();

            Document.LoadHtml(Get("https://www.canli-radyo.biz/"));

            var Mainler = Document.DocumentNode.SelectNodes("/html/body/div/div[3]/div/div[3]/div[1]/div/div[2]/div/ul/li");

            foreach (var Main in Mainler)
            {
                var ID = GetID(Main.ChildNodes[0].Attributes["href"].Value);
                var Data = GetData(ID);

                Data.Name= Main.ChildNodes[0].ChildNodes[2].InnerText;
                Data.Image= SaveImage(ID,Main.ChildNodes[0].ChildNodes[1].Attributes["data-src"].Value);
                Donuts.Add(Data);

                Console.WriteLine(Data.Name + " | Eklendi",Color.Green);
            }

            
            File.WriteAllText("liste.json", JsonConvert.SerializeObject(Donuts, Formatting.Indented));

            Console.WriteLine("Bitti !");
        }
    }
}
