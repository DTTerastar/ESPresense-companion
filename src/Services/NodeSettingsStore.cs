﻿using System.Collections.Concurrent;
using ESPresense.Models;

namespace ESPresense.Services
{
    public class NodeSettingsStore(MqttCoordinator mqtt, ILogger<NodeSettingsStore> logger) : BackgroundService
    {
        private static bool ParseBool(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            value = value.Trim().ToUpperInvariant();
            return value switch
            {
                "TRUE" or "1" or "ON" => true,
                "FALSE" or "0" or "OFF" => false,
                _ => bool.Parse(value)
            };
        }
        private readonly ConcurrentDictionary<string, NodeSettings> _storeById = new();

        public NodeSettings Get(string id)
        {
            return _storeById.TryGetValue(id, out var ns) ? ns.Clone() : new NodeSettings(id);
        }

        public async Task Set(string id, NodeSettings ds)
        {
            var old = Get(id);

            // Updating settings
            if (ds.Updating.AutoUpdate == null || ds.Updating.AutoUpdate != old.Updating.AutoUpdate)
                await mqtt.EnqueueAsync($"espresense/rooms/{id}/auto_update/set", ds.Updating.AutoUpdate == true ? "ON" : "OFF");
            if (ds.Updating.PreRelease == null || ds.Updating.PreRelease != old.Updating.PreRelease)
                await mqtt.EnqueueAsync($"espresense/rooms/{id}/pre_release/set", ds.Updating.PreRelease == true ? "ON" : "OFF");

            // Scanning settings
            if (ds.Scanning.ForgetAfterMs == null || ds.Scanning.ForgetAfterMs != old.Scanning.ForgetAfterMs)
                await mqtt.EnqueueAsync($"espresense/rooms/{id}/forget_after_ms/set", $"{ds.Scanning.ForgetAfterMs}");

            // Counting settings
            if (ds.Counting.IdPrefixes == null || ds.Counting.IdPrefixes != old.Counting.IdPrefixes)
                await mqtt.EnqueueAsync($"espresense/rooms/{id}/count_ids/set", $"{ds.Counting.IdPrefixes}");
            if (ds.Counting.StartCountingDistance == null || ds.Counting.StartCountingDistance != old.Counting.StartCountingDistance)
                await mqtt.EnqueueAsync($"espresense/rooms/{id}/count_min_dist/set", $"{ds.Counting.StartCountingDistance:0.00}");
            if (ds.Counting.StopCountingDistance == null || ds.Counting.StopCountingDistance != old.Counting.StopCountingDistance)
                await mqtt.EnqueueAsync($"espresense/rooms/{id}/count_max_dist/set", $"{ds.Counting.StopCountingDistance:0.00}");
            if (ds.Counting.IncludeDevicesAge == null || ds.Counting.IncludeDevicesAge != old.Counting.IncludeDevicesAge)
                await mqtt.EnqueueAsync($"espresense/rooms/{id}/include_devices_age/set", $"{ds.Counting.IncludeDevicesAge}");

            // Filtering settings
            if (ds.Filtering.IncludeIds == null || ds.Filtering.IncludeIds != old.Filtering.IncludeIds)
                await mqtt.EnqueueAsync($"espresense/rooms/{id}/include_ids/set", $"{ds.Filtering.IncludeIds}");
            if (ds.Filtering.ExcludeIds == null || ds.Filtering.ExcludeIds != old.Filtering.ExcludeIds)
                await mqtt.EnqueueAsync($"espresense/rooms/{id}/exclude_ids/set", $"{ds.Filtering.ExcludeIds}");
            if (ds.Filtering.MaxDistance == null || ds.Filtering.MaxDistance != old.Filtering.MaxDistance)
                await mqtt.EnqueueAsync($"espresense/rooms/{id}/max_distance/set", $"{ds.Filtering.MaxDistance:0.00}");
            if (ds.Filtering.EarlyReportDistance == null || ds.Filtering.EarlyReportDistance != old.Filtering.EarlyReportDistance)
                await mqtt.EnqueueAsync($"espresense/rooms/{id}/early_report_distance/set", $"{ds.Filtering.EarlyReportDistance:0.00}");
            if (ds.Filtering.SkipReportAge == null || ds.Filtering.SkipReportAge != old.Filtering.SkipReportAge)
                await mqtt.EnqueueAsync($"espresense/rooms/{id}/skip_report_age/set", $"{ds.Filtering.SkipReportAge}");

            // Calibration settings
            if (ds.Calibration.Absorption == null || ds.Calibration.Absorption != old.Calibration.Absorption)
                await mqtt.EnqueueAsync($"espresense/rooms/{id}/absorption/set", $"{ds.Calibration.Absorption:0.00}");
            if (ds.Calibration.RxAdjRssi == null || ds.Calibration.RxAdjRssi != old.Calibration.RxAdjRssi)
                await mqtt.EnqueueAsync($"espresense/rooms/{id}/rx_adj_rssi/set", $"{ds.Calibration.RxAdjRssi}");
            if (ds.Calibration.TxRefRssi == null || ds.Calibration.TxRefRssi != old.Calibration.TxRefRssi)
                await mqtt.EnqueueAsync($"espresense/rooms/{id}/tx_ref_rssi/set", $"{ds.Calibration.TxRefRssi}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            mqtt.NodeSettingReceivedAsync += arg =>
            {
                try
                {
                    var ns = Get(arg.NodeId);
                    switch (arg.Setting)
                    {
                        // Updating settings
                        case "auto_update":
                            ns.Updating.AutoUpdate = ParseBool(arg.Payload);
                            break;
                        case "pre_release":
                            ns.Updating.PreRelease = ParseBool(arg.Payload);
                            break;

                        // Scanning settings
                        case "forget_after_ms":
                            ns.Scanning.ForgetAfterMs = int.Parse(arg.Payload);
                            break;

                        // Counting settings
                        case "count_ids":
                            ns.Counting.IdPrefixes = arg.Payload;
                            break;
                        case "count_min_dist":
                            ns.Counting.StartCountingDistance = double.Parse(arg.Payload);
                            break;
                        case "count_max_dist":
                            ns.Counting.StopCountingDistance = double.Parse(arg.Payload);
                            break;
                        case "include_devices_age":
                            ns.Counting.IncludeDevicesAge = int.Parse(arg.Payload);
                            break;

                        // Filtering settings
                        case "include_ids":
                            ns.Filtering.IncludeIds = arg.Payload;
                            break;
                        case "exclude_ids":
                            ns.Filtering.ExcludeIds = arg.Payload;
                            break;
                        case "max_distance":
                            ns.Filtering.MaxDistance = double.Parse(arg.Payload);
                            break;
                        case "early_report_distance":
                            ns.Filtering.EarlyReportDistance = double.Parse(arg.Payload);
                            break;
                        case "skip_report_age":
                            ns.Filtering.SkipReportAge = int.Parse(arg.Payload);
                            break;

                        // Calibration settings
                        case "absorption":
                            ns.Calibration.Absorption = double.Parse(arg.Payload);
                            break;
                        case "rx_adj_rssi":
                            ns.Calibration.RxAdjRssi = int.Parse(arg.Payload);
                            break;
                        case "tx_ref_rssi":
                            ns.Calibration.TxRefRssi = int.Parse(arg.Payload);
                            break;

                        default:
                            return Task.CompletedTask;
                    }

                    _storeById.AddOrUpdate(arg.NodeId, _ => ns, (_, _) => ns);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error parsing {0} for {1}", arg.Setting, arg.NodeId);
                }
                return Task.CompletedTask;
            };

            await Task.Delay(-1, stoppingToken);
        }

        public async Task Update(string id)
        {
            await mqtt.EnqueueAsync($"espresense/rooms/{id}/update/set", "PRESS");
        }

        public async Task Arduino(string id, bool on)
        {
            await mqtt.EnqueueAsync($"espresense/rooms/{id}/arduino_ota/set", on ? "ON" : "OFF");
        }

        public async Task Restart(string id)
        {
            await mqtt.EnqueueAsync($"espresense/rooms/{id}/restart/set", "PRESS");
        }
    }
}
