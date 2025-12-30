# Sentinel Streamer 🛡️

**Sentinel Streamer** is a high-performance Custom App for [Technitium DNS Server](https://technitium.com/dns/) that streams real-time query metrics to InfluxDB via UDP.

Designed for cybersecurity labs and network monitoring, it allows you to visualize DNS traffic in Grafana without impacting DNS resolution speed (Fire-and-Forget).

## 🚀 Features
* **Zero-Latency Impact:** Uses asynchronous UDP packets so DNS resolution is never blocked.
* **InfluxDB Line Protocol:** Native support for InfluxDB v1/v2.
* **Dashboard Configuration:** Configure destination IP and Port directly from the Technitium Web UI.
* **Tagging:** Metrics are tagged by `Client IP`, `Query Type`, and `Domain` for granular filtering.

## 🛠️ Installation

### Option 1: Install via Zip (Recommended)
1.  Download the latest `SentinelStreamer.zip` from the [Releases Page](https://github.com/vicious-n-delicious/SentinelStreamer/releases).
2.  Open your Technitium Dashboard.
3.  Go to **Apps > Install App > Upload App Package**.
4.  Select the zip file and install.

### Option 2: Build from Source
To build this app, you need the `Technitium.DnsServer.Core.dll` from your current installation.

1.  Clone this repository.
2.  Copy `Technitium.DnsServer.Core.dll` (found in your Technitium install folder) into the root of this repo.
3.  Run the build command:
    ```bash
    dotnet publish -c Release
    ```
4.  Zip the contents of `bin/Release/net8.0/publish/` and upload to Technitium.

## ⚙️ Configuration
Once installed, go to **Apps > Installed Apps** and click the **Settings (Gear)** icon next to Sentinel Streamer.

* **InfluxServerAddress:** The IP of your InfluxDB instance (e.g., `172.18.1.100`).
* **InfluxServerPort:** The UDP listener port (Default: `8089`).

> **Note:** Ensure your InfluxDB container has `INFLUXDB_UDP_ENABLED=true` set in its environment variables.

## 📊 Grafana Query Example
To visualize the traffic heatmap:
```sql
SELECT count("count") FROM "dns_query" WHERE $timeFilter GROUP BY time($__interval), "client_ip"