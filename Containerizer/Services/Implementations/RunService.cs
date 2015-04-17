﻿using Containerizer.Controllers;
using Containerizer.Services.Interfaces;
using IronFrame;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Containerizer.Services.Implementations
{
    public class RunService : IRunService
    {
        public IContainer container { get; set; }

        public void Run(Controllers.IWebSocketEventSender websocket, Models.ApiProcessSpec apiProcessSpec)
        {

            var processSpec = NewProcessSpec(apiProcessSpec);
            var info = container.GetInfo();
            if (info != null)
            {
                CopyExecutorEnvVariables(processSpec, info);
                CopyProcessSpecEnvVariables(processSpec, apiProcessSpec.Env);
                OverrideEnvPort(processSpec, info);
            }

            try
            {
                var processIO = new ProcessIO(websocket);
                var process = container.Run(processSpec, processIO);
                websocket.SendEvent("pid", process.Id.ToString());
                var exitCode = process.WaitForExit();
                websocket.SendEvent("close", exitCode.ToString());
                websocket.Close(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "process finished");
            }
            catch (Exception e)
            {
                if (e.Message.Contains("System.OutOfMemoryException"))
                {
                    websocket.SendEvent("close", "-1");
                }
                else
                {
                    websocket.SendEvent("error", e.Message);
                }
                websocket.Close(System.Net.WebSockets.WebSocketCloseStatus.InternalServerError, e.Message);
            }
        }

        private static void OverrideEnvPort(ProcessSpec processSpec, ContainerInfo info)
        {
            if (info.ReservedPorts.Count > 0)
                processSpec.Environment["PORT"] = info.ReservedPorts[0].ToString();
        }

        private ProcessSpec NewProcessSpec(Models.ApiProcessSpec apiProcessSpec)
        {
            var processSpec = new ProcessSpec
            {
                DisablePathMapping = false,
                Privileged = false,
                WorkingDirectory = container.Directory.UserPath,
                ExecutablePath = apiProcessSpec.Path,
                Environment = new Dictionary<string, string>
                    {
                        { "ARGJSON", JsonConvert.SerializeObject(apiProcessSpec.Args) }
                    },
                Arguments = apiProcessSpec.Args
            };
            return processSpec;
        }

        private static void CopyProcessSpecEnvVariables(ProcessSpec processSpec, string[] envStrings)
        {
            if (envStrings == null) { return; }
            foreach (var kv in envStrings)
            {
                string[] arr = kv.Split(new Char[] { '=' }, 2);
                processSpec.Environment[arr[0]] = arr[1];
            }
        }

        private static void CopyExecutorEnvVariables(ProcessSpec processSpec, ContainerInfo info)
        {
            string varsJson = "";
            if (info.Properties.TryGetValue("executor:env", out varsJson))
            {
                var environmentVariables = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(varsJson);
                foreach (var dict in environmentVariables)
                {
                    processSpec.Environment[dict["name"]] = dict["value"];
                }
            }
        }

        private class WSWriter : TextWriter
        {
            private readonly string streamName;
            private readonly IWebSocketEventSender ws;

            public WSWriter(string streamName, IWebSocketEventSender ws)
            {
                this.streamName = streamName;
                this.ws = ws;
            }

            public override Encoding Encoding
            {
                get { return Encoding.Default; }
            }

            public override void Write(string value)
            {
                ws.SendEvent(streamName, value + "\r\n");
            }
        }

        private class ProcessIO : IProcessIO
        {
            public ProcessIO(IWebSocketEventSender ws)
            {
                StandardOutput = new WSWriter("stdout", ws);
                StandardError = new WSWriter("stderr", ws);
            }

            public TextWriter StandardOutput { get; set; }
            public TextWriter StandardError { get; set; }
            public TextReader StandardInput { get; set; }
        }
    }
}