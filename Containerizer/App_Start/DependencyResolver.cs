﻿#region

using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Autofac;
using Containerizer.Controllers;
using Containerizer.Services.Implementations;
using Containerizer.Services.Interfaces;

#endregion

namespace Containerizer
{
    public class DependencyResolver : IDependencyResolver
    {
        private readonly IContainer container;

        public DependencyResolver()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<CreateContainerService>().As<ICreateContainerService>();
            containerBuilder.RegisterType<ContainerPathService>().As<IContainerPathService>();
            containerBuilder.RegisterType<NetInService>().As<INetInService>();
            containerBuilder.RegisterType<StreamInService>().As<IStreamInService>();
            containerBuilder.RegisterType<StreamOutService>().As<IStreamOutService>();
            containerBuilder.RegisterType<TarStreamService>().As<ITarStreamService>();
            containerBuilder.RegisterType<PropertyService>().As<IPropertyService>();
            containerBuilder.RegisterType<ContainersController>();
            containerBuilder.RegisterType<FilesController>();
            containerBuilder.RegisterType<NetController>();
            containerBuilder.RegisterType<PropertiesController>();
            containerBuilder.RegisterType<RunController>();
            container = containerBuilder.Build();
        }

        IDependencyScope IDependencyResolver.BeginScope()
        {
            return new DependencyResolver();
        }

        object IDependencyScope.GetService(Type serviceType)
        {
            return container.ResolveOptional(serviceType);
        }

        IEnumerable<object> IDependencyScope.GetServices(Type serviceType)
        {
            var collection = (IEnumerable<object>) container.ResolveOptional(serviceType);
            if (collection == null)
            {
                return new List<object>();
            }
            return collection;
        }

        void IDisposable.Dispose()
        {
            container.Dispose();
        }
    }
}