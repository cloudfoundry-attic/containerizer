using IronFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Containerizer.Models;
using Containerizer.Services.Interfaces;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Containerizer.Factories;

namespace Containerizer.Controllers
{
    public class Capacity
    {
        [JsonProperty("memory_in_bytes")]
        public ulong MemoryInBytes;

        [JsonProperty("disk_in_bytes")]
        public ulong DiskInBytes;

        [JsonProperty("max_containers")]
        public ulong MaxContainers;
    }

    public class CapacityController : ApiController
    {
        [Route("api/capacity")]
        public Capacity Index()
        {
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            if (!GlobalMemoryStatusEx(memStatus))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var drive = new System.IO.DriveInfo(ContainerServiceFactory.GetContainerRoot());

            return new Capacity
            {
                MemoryInBytes = memStatus.ullTotalPhys,
                DiskInBytes = (ulong)drive.TotalSize,
                MaxContainers = Int32.MaxValue,
            };
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
    }
}
