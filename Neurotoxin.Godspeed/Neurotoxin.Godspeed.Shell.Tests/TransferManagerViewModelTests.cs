using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using FakeItEasy;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Tests.Dummies;
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
            a.SelectAllCommand.Execute(null);
            var selection = a.SelectedItems.Select(i => i.Model).ToList();
            var b = GetDummyContentViewModel();

            var eventAggregator = Container.Resolve<IEventAggregator>();
            var isBusy = true;
            eventAggregator.GetEvent<TransferFinishedEvent>().Subscribe(e => isBusy = false);
            vm.Copy(a, b);
            while (isBusy)
            {
                Thread.Sleep(100);
            }

            foreach (var aitem in selection)
            {
                Assert.IsTrue(b.Items.Any(bitem => IsCopy(aitem, bitem.Model)), aitem.Name + " is missing from target");
            }
        }

        private bool IsCopy(FileSystemItem a, FileSystemItem b)
        {
            //TODO
            return a.Name == b.Name;
        }

        private TransferManagerViewModel GetInstance()
        {
            return Container.Resolve<TransferManagerViewModel>();
        }

        private DummyContentViewModel GetDummyContentViewModel()
        {
            var fm = Container.Resolve<IFileManagerViewModel>();
            var dummy = new DummyContentViewModel(fm, new FakingRules
                                                          {
                                                              TreeDepth = new Range(3, 3),
                                                              ItemCount = new Range(0, 20),
                                                              ItemCountOnLevel = new Dictionary<int, Range>
                                                                                     {
                                                                                         {0, new Range(1,5)}
                                                                                     }
                                                          });
            dummy.Drive = dummy.Drives.First();
            return dummy;
        }

    }
}