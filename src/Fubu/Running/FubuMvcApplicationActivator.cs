﻿using System;
using System.Collections.Generic;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Assets;
using FubuMVC.Core.Http.Hosting;
using FubuMVC.Core.Runtime;
using FubuMVC.Core.Services.Messaging;
using FubuMVC.Katana;
using KatanaRightsException = FubuMVC.Katana.KatanaRightsException;

namespace Fubu.Running
{
    public class FubuMvcApplicationActivator : IFubuMvcApplicationActivator
    {
        private IApplicationSource _applicationSource;
        private int _port;
        private string _physicalPath;
        private EmbeddedFubuMvcServer _server;

        public void Initialize(Type applicationType, int port, string physicalPath)
        {
            _applicationSource = Activator.CreateInstance(applicationType).As<IApplicationSource>();
            _port = PortFinder.FindPort(port);
            _physicalPath = physicalPath;

            StartUp();
        }

        public void StartUp()
        {
            try
            {
                FubuApplication.RootPath = _physicalPath;
                var application = _applicationSource.BuildApplication();
                var runtime = application.Bootstrap();
                _server = new EmbeddedFubuMvcServer(runtime, new KatanaHost(), _port);

                EventAggregator.SendMessage(new ApplicationStarted
                {
                    ApplicationName = _applicationSource.GetType().Name,
                    HomeAddress = _server.BaseAddress,
                    Timestamp = DateTime.Now,
                    Watcher = runtime.Factory.Get<AssetSettings>().CreateFileWatcherManifest(runtime.Files)
                });
            }
            catch (KatanaRightsException e)
            {
                EventAggregator.SendMessage(new InvalidApplication
                {
                    ExceptionText = e.Message,
                    Message = "Access denied."
                });               
            }
            catch (Exception e)
            {
                EventAggregator.SendMessage(new InvalidApplication
                {
                    ExceptionText = e.ToString(),
                    Message = "Bootstrapping {0} Failed!".ToFormat(_applicationSource.GetType().Name)
                });
            }
        }

        public void ShutDown()
        {
            _server.SafeDispose();
        }

        public void Recycle()
        {
            ShutDown();
            StartUp();
        }

    }
}