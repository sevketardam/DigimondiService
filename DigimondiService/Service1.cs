using System;
using System.ServiceProcess;
using MySql.Data.MySqlClient;
using System.Timers;
using System.Net.NetworkInformation;
using System.Linq;
using System.Net.Http;

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
            timer.Interval = 30000;
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

                // API'nin adresini belirleyin
                var apiUrl = "https://dynastybdo.com/api/adddatabase.php?name=sevket&ip=" + localIpAddress;

                // HTTP GET isteği gönderin
                var response = await httpClient.GetAsync(apiUrl);

                // Yanıtın içeriğini alın ve konsol ekranında görüntüleyin
                var content = await response.Content.ReadAsStringAsync();

                ServiceController myService = new ServiceController("Service1");
                myService.Stop();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
