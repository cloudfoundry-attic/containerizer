using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NSpec;
using Containerizer.Controllers;
using System.Web.Http;
using Containerizer.Models;
using System.Web.Http.Results;
using IronFrame;
using Containerizer.Services.Interfaces;
using Containerizer.Services.Implementations;

namespace Containerizer.Tests.Specs.Controllers
{
    class CapacityControllerSpec : nspec
    {
        private void describe_()
        {
            describe[Controller.Index] = () =>
            {
                CapacityController controller = null;

                before = () =>
                {
                    controller = new CapacityController();
                };

                it["returns positive capacity for MemoryInBytes"] = () =>
                {
                    var capacity = controller.Index();
                    capacity.MemoryInBytes.should_be_greater_than(0);
                };

                it["returns positive capacity for DiskInBytes"] = () =>
                {
                    var capacity = controller.Index();
                    capacity.DiskInBytes.should_be_greater_than(0);
                };

                it["returns positive capacity for MaxContainers"] = () =>
                {
                    var capacity = controller.Index();
                    capacity.MaxContainers.should_be_greater_than(0);
                };
            };
        }
    }
}