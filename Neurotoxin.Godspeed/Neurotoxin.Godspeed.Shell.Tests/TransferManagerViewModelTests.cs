using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using FakeItEasy;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neurotoxin.Godspeed.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Tests.Dummies;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Microsoft.Practices.ObjectBuilder2;

namespace Neurotoxin.Godspeed.Shell.Tests
{
    [TestClass]
    public class TransferManagerViewModelTests
    {
        private static IUnityContainer Container { get; set; }
        private static IEventAggregator eventAggregator { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            Container = new UnityContainer();
            Container.RegisterType<IWorkHandler, SyncWorkHandler>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IEventAggregator, EventAggregator>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IWindowManager, ConsoleWriter>(new ContainerControlledLifetimeManager());
            Container.RegisterInstance(A.Fake<IStatisticsViewModel>());
            Container.RegisterInstance(A.Fake<IUserSettings>());
            Container.RegisterInstance(A.Fake<ITitleRecognizer>());
            Container.RegisterInstance(A.Fake<IResourceManager>());
            Container.RegisterType<TransferManagerViewModel>();
            UnityInstance.Container = Container;
            eventAggregator = Container.Resolve<IEventAggregator>();
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
            Console.WriteLine("Source:");
            a.Items.ForEach(i => Console.WriteLine(i.Name));
            a.SelectAllCommand.Execute(null);

            var selection = a.SelectedItems.Select(i => i.Model).ToList();
            var b = GetDummyContentViewModel();
            Console.WriteLine("Target:");
            b.Items.ForEach(i => Console.WriteLine(i.Name));

            vm.Copy(a, b);

            var sb = new StringBuilder();
            foreach (var aitem in selection)
            {
                if (!b.Items.Any(bitem => IsCopy(aitem, bitem.Model)))
                    sb.AppendLine(aitem.Name + " is missing from target");
            }
            var errorMessage = sb.ToString();
            Assert.IsTrue(string.IsNullOrEmpty(errorMessage), errorMessage);
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
            var dummy = new DummyContentViewModel(new FakingRules
                                                          {
                                                              TreeDepth = 3,
                                                              ItemCount = new Range(5, 10),
                                                              ItemCountOnLevel = new Dictionary<int, Range>
                                                                                     {
                                                                                         {0, new Range(1,5)}
                                                                                     }
                                                          });
            dummy.Drive = dummy.Drives.First();
            return dummy;
        }

        private void WaitForProperty(INotifyPropertyChanged vm, string propertyName)
        {
            var changed = false;
            PropertyChangedEventHandler propDelegate = (sender, args) => { if (args.PropertyName == propertyName) changed = true; };
            vm.PropertyChanged += propDelegate;
            var sw = new Stopwatch();
            sw.Start();
            while (!changed)
            {
                Thread.Sleep(100);
                if (sw.ElapsedMilliseconds > 10000) throw new TimeoutException();
            }
            vm.PropertyChanged -= propDelegate;
        }

        private void WaitForEvent<TEvent, TPayload>(IViewModel vm = null, int timeout = 10000)
            where TEvent : CompositePresentationEvent<TPayload> 
            where TPayload : IPayload
        {
            var type = typeof (TEvent).Name;
            var fired = false;
            Console.WriteLine("[Event] Waiting for event " + type);
            Action<TPayload> action = payload => { if (vm == null || payload.Sender.Equals(vm)) fired = true; };
            eventAggregator.GetEvent<TEvent>().Subscribe(action);
            var sw = new Stopwatch();
            sw.Start();
            while (!fired)
            {
                Thread.Sleep(100);
                //if (sw.ElapsedMilliseconds > timeout) throw new TimeoutException(type);
                if (sw.ElapsedMilliseconds > timeout)
                {
                    Console.WriteLine("[Event] Timeout occured.");
                    timeout *= 2;
                    if (timeout > 100000) throw new TimeoutException(type);
                }
            }
            Console.WriteLine("[Event] {0} fired after {1}", type, sw.Elapsed);
            eventAggregator.GetEvent<TEvent>().Unsubscribe(action);
        }

    }
}