using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json; 
using TechnitiumLibrary.Net.Dns;
using DnsServerCore.Application;

namespace SentinelStreamer
{
    public class MetricApp : IDnsApplication
    {
        private UdpClient? _udpClient;
        private IPEndPoint? _influxEndpoint;
        private AppConfig _config;

        public Task InitializeAsync(IDnsApplicationContext context)
        {
            // Load Config or Default
            if (string.IsNullOrEmpty(context.Config))
            {
                _config = new AppConfig(); 
                context.SaveConfig(JsonConvert.SerializeObject(_config));
            }
            else
            {
                _config = JsonConvert.DeserializeObject<AppConfig>(context.Config) ?? new AppConfig();
            }

            // Setup UDP
            _udpClient = new UdpClient();
            
            // Resolve Hostname if user entered one
            IPAddress ip;
            if (!IPAddress.TryParse(_config.InfluxServerAddress, out ip))
            {
                try {
                    var addresses = Dns.GetHostAddresses(_config.InfluxServerAddress);
                    ip = addresses.FirstOrDefault() ?? IPAddress.Loopback;
                } catch {
                    ip = IPAddress.Loopback; // Fail safe
                }
            }

            _influxEndpoint = new IPEndPoint(ip, _config.InfluxServerPort);
            return Task.CompletedTask;
        }

        public Task ProcessRequestAsync(DnsRequest request, DnsResponse response, IDnsApplicationContext context)
        {
            // Performance: Fire & Forget
            _ = Task.Run(() => SendMetric(request, context));
            return Task.CompletedTask;
        }

        private void SendMetric(DnsRequest request, IDnsApplicationContext context)
        {
            if (_udpClient == null || _influxEndpoint == null) return;

            try
            {
                // Sanitize Data
                string domain = request.Question.Name.ToString().Replace(" ", "\\ ").Replace("\"", "\\\"");
                string qtype = request.Question.Type.ToString();
                string clientIp = context.RemoteEndPoint.Address.ToString();

                // InfluxDB Line Protocol
                // format: measurement,tags fields timestamp
                string lineProtocol = $"dns_query,client_ip={clientIp},qtype={qtype} domain=\"{domain}\",count=1i";

                byte[] data = Encoding.UTF8.GetBytes(lineProtocol);
                _udpClient.Send(data, data.Length, _influxEndpoint);
            }
            catch
            {
                // Telemetry must never crash the DNS service
            }
        }

        public Task ShutdownAsync()
        {
            _udpClient?.Close();
            _udpClient?.Dispose();
            return Task.CompletedTask;
        }
        
        public void Dispose()
        {
            _udpClient?.Dispose();
        }
    }

    // UI Configuration for Dashboard
    public class AppConfig
    {
        public string InfluxServerAddress { get; set; } = "172.18.1.100";
        public int InfluxServerPort { get; set; } = 8089;
    }
}

