using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Tests.Dummies;
using Neurotoxin.Godspeed.Shell.Tests.Helpers;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Tests
{
    [TestClass]
    public class TransferManagerViewModelTests
    {
        public static IUnityContainer Container { get; private set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            Container = new UnityContainer();
            Container.RegisterType<IEventAggregator, EventAggregator>(new ContainerControlledLifetimeManager());
            Container.RegisterInstance(A.Fake<IStatisticsViewModel>());
            Container.RegisterInstance(A.Fake<IUserSettings>());
            Container.RegisterInstance(A.Fake<ITitleRecognizer>());
            Container.RegisterType<TransferManagerViewModel>();
            Container.RegisterInstance(A.Fake<IFileManagerViewModel>());
            UnityInstance.Container = Container;
        }

        [TestInitialize]
        public void MyTestInitialize()
        {
            
        }

        [TestMethod]
        public void InstantiationTest()
        {
            var vm = GetInstance();
            Assert.IsNotNull(vm, "TransferManagerViewModel should not be null");
        }

        [TestMethod]
        public void BasicCopyTest()
        {
            var vm = GetInstance();
            var a = GetDummyContentViewModel();
            var b = GetDummyContentViewModel();
            vm.Copy(a, b);
        }

        private TransferManagerViewModel GetInstance()
        {
            return Container.Resolve<TransferManagerViewModel>();
        }

        private DummyContentViewModel GetDummyContentViewModel()
        {
            var fm = Container.Resolve<IFileManagerViewModel>();
            var dummy = new DummyContentViewModel(fm);
            dummy.Drive = dummy.Drives.First();
            //TODO: setup
            return dummy;
        }

    }
}