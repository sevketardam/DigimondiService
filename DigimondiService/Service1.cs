using System;
using System.ServiceProcess;
using System.Timers;
using System.Net.NetworkInformation;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;

namespace DigimondiService
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            VeriTabaninaEkle();
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 5000;
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            VeriTabaninaEkle();
        }

        public async void VeriTabaninaEkle()
        {
            try
            {
                var localIpAddress = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up)
                    .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                    .Where(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .Select(a => a.Address.ToString())
                    .FirstOrDefault();

                var httpClient = new HttpClient();
                var name = await DosyaOku();
                var apiUrl = "https://dynastybdo.com/api/adddatabase.php?name=" + name + "&ip=" + localIpAddress;
                var response = await httpClient.GetAsync(apiUrl);
                var content = await response.Content.ReadAsStringAsync();

                DosyaYaz(content + " - " + DateTime.Now);

                if (content.Trim() == "eklendi")
                {
                    ServiceController myService = new ServiceController("Service1");
                    myService.Stop();
                }
            }
            catch (Exception ex)
            {
                DosyaYaz(ex.Message + " - " + DateTime.Now);
            }
        }

        public void DosyaYaz(string mesaj)
        {
            try
            {
                string dosyaYolu = AppDomain.CurrentDomain.BaseDirectory + "/Logs";
                if (!Directory.Exists(dosyaYolu))
                {
                    Directory.CreateDirectory(dosyaYolu);
                }

                string textYolu = AppDomain.CurrentDomain.BaseDirectory + "/Logs/servisim.txt";
                if (!File.Exists(textYolu))
                {
                    using (StreamWriter sw = File.CreateText(textYolu))
                    {
                        sw.WriteLine(mesaj);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(textYolu))
                    {
                        sw.WriteLine(mesaj);
                    }
                }
            }
            catch (Exception e)
            {
                DosyaYaz(e.Message + " - " + DateTime.Now);
            }

        }

        public async Task<string> DosyaOku()
        {
            string dosyaYolu = AppDomain.CurrentDomain.BaseDirectory + "/Logs";
            if (!Directory.Exists(dosyaYolu))
            {
                Directory.CreateDirectory(dosyaYolu);
            }

            string fileContent = "isim girilmemis";
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "/Logs/ComputerName.txt";
            if (!File.Exists(filePath))
            {
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    fileContent = ReadTextFile(filePath);
                }
            }
            else
            {
                fileContent = ReadTextFile(filePath);
            }

            return fileContent;
        }

        public string ReadTextFile(string filePath)
        {
            string text = "";

            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    text = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                DosyaYaz(e.Message + " - " + DateTime.Now);
            }

            return text;
        }
    }
}
