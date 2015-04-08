﻿#region

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Containerizer.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using IronFoundry.Container;
using Containerizer.Models;
using System;
using Logger;

#endregion

namespace Containerizer.Controllers
{
    public class CreateResponse
    {
        [JsonProperty("id")]
        public string Handle { get; set; }
    }

    public class ContainersController : ApiController
    {
        private readonly IContainerService containerService;
        private readonly IContainerPropertyService propertyService;
        private readonly ILogger logger;


        public ContainersController(IContainerService containerService, IContainerPropertyService propertyService, ILogger logger)
        {
            this.containerService = containerService;
            this.propertyService = propertyService;
            this.logger = logger;
        }

        [Route("api/containers")]
        [HttpGet]
        public IReadOnlyList<string> Index()
        {
            logger.Info("Container#Index");
            return containerService.GetContainers().Select(x => x.Handle).ToList();
        }

        [Route("api/containers")]
        [HttpPost]
        public CreateResponse Create(ContainerSpecApiModel spec)
        {
            logger.Info("Container#Create: {0}", spec.Handle);
            var containerSpec = new ContainerSpec
            {
                Handle = spec.Handle,
            };

            var container = containerService.CreateContainer(containerSpec);
            if (container == null)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }

            propertyService.SetProperties(container, spec.Properties);

            logger.Info("Container#Create: {0} => {1}", spec.Handle, container.Id);
              
            return new CreateResponse
            {
                Handle = container.Handle
            };
        }

        [Route("api/containers/{handle}/stop")]
        [HttpPost]
        public IHttpActionResult Stop(string handle)
        {
            logger.Info("Container#Stop: {0}", handle);
            var container = containerService.GetContainerByHandle(handle);
            if (container != null)
            {
                logger.Info("Container#Stop: {0} => {1}", handle, container.Id);
                container.Stop(true);
                return Ok();
            }

            return NotFound();
        }

        [Route("api/containers/{handle}")]
        [HttpDelete]
        public IHttpActionResult Destroy(string handle)
        {
            logger.Info("Container#Destroy: {0}", handle);
            var container = containerService.GetContainerByHandle(handle);
            if (container != null)
            {
                logger.Info("Container#Destroy: {0} => {1}", handle, container.Id);
                containerService.DestroyContainer(handle);
                logger.Info("Container#Destroy (after): {0} => {1}", handle, container.Id);
                return Ok();
            }

            return NotFound();
        }
    }
}