﻿using System.Threading.Tasks;
using RetroClash.Extensions;
using RetroClash.Logic;

namespace RetroClash.Protocol.Messages.Server
{
    public class LoginFailed : Message
    {
        public LoginFailed(Device device) : base(device)
        {
            Id = 20103;
            Version = 2;
        }

        public int ErrorCode { get; set; }
        public string Reason { get; set; }
        public string Fingerprint { get; set; }
        public string PatchUrl { get; set; }

        // Codes:
        // 7 = Content Update
        // 8 = Update Available
        // 10 = Maintenance
        // 11 = Banned
        // 12 = Played too long

        public override async Task Encode()
        {
            await Stream.WriteIntAsync(ErrorCode);
            await Stream.WriteStringAsync(Fingerprint); // Fingerprint
            await Stream.WriteStringAsync(null);
            await Stream.WriteStringAsync(PatchUrl); // Content URL
            await Stream.WriteStringAsync(null); // Update URL
            await Stream.WriteStringAsync(Reason);
        }
    }
}