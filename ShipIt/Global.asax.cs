﻿using System;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Castle.Core.Resource;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using ShipIt.Startup;

namespace ShipIt
{
    public class Global : HttpApplication
    {
        private readonly IWindsorContainer _container;

        public Global()
        {
            _container =
                new WindsorContainer(new XmlInterpreter(new ConfigResource("castle")));
        }

        public override void Dispose()
        {
            _container.Dispose();
            base.Dispose();
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            GlobalConfiguration.Configuration.Services.Replace(
                typeof(IHttpControllerActivator),
                new WindsorCompositionRoot(_container));
            GlobalConfiguration.Configuration.Formatters.Remove(GlobalConfiguration.Configuration.Formatters
                .JsonFormatter);
        }
    }
}