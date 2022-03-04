﻿using System;
using System.Windows;
using Melville.IOC.IocContainers;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.MVVM.Wpf.RootWindows;
using Melville.Pdf.WpfViewer.Home;
using Melville.WpfAppFramework.StartupBases;

namespace Melville.Pdf.WpfViewer.CompositionRoot
{
    public class Startup:StartupBase
    {
        [STAThread]
        public static void Main(string[] arguments)
        {
            ApplicationRootImplementation.Run(new Startup(arguments));
        }

        private Startup(string[] arguments) : base(arguments)
        {
        }

        protected override void RegisterWithIocContainer(IBindableIocService service)
        {
            service.RegisterHomeViewModel<HomeViewModel>();
            service.Bind<IOpenSaveFile>().To<OpenSaveFileAdapter>();
            service.Bind<Window>().And<IRootNavigationWindow>().To<RootNavigationWindow>().AsSingleton();
        }
    }
}