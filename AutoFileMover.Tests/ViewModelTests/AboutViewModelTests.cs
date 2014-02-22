using System;
using AutoFileMover.Desktop.Interfaces;
using AutoFileMover.Desktop.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnityAutoMoq;

namespace AutoFileMover.Tests.ViewModelTests
{
    [TestClass]
    public class AboutViewModelTests
    {
        private UnityAutoMoqContainer _container = new UnityAutoMoqContainer();

        [TestMethod]
        public void NetworkDeployedTest()
        {
            var mock = _container.GetMock<IApplicationDeployment>();
            var mock2 = _container.GetMock<IApplicationContainer>();

            mock.SetupGet(s => s.IsNetworkDeployed).Returns(true);
            mock2.SetupAllProperties();

            var vm = new AboutViewModel(mock.Object, mock2.Object);

            Assert.IsTrue(vm.NetworkDeployed);
        }

        [TestMethod]
        public void CheckForUpdateTest()
        {
            var mock = _container.GetMock<IApplicationDeployment>();
            var mock2 = _container.GetMock<IApplicationContainer>();

            mock.SetupGet(s => s.IsNetworkDeployed).Returns(true);
            mock.Setup(ad => ad.CheckForUpdateAsync())
                .Raises(s => s.CheckForUpdateCompleted += null, 
                            new CheckForUpdateCompletedEventArgs(new Version(2, 0, 0, 0), true,
                                new Version(2, 0, 0, 0), true, 1024 * 1024, null, false, null));

            mock2.SetupAllProperties();

            var vm = new AboutViewModel(mock.Object, mock2.Object);

            //check that we are network deployed
            Assert.IsTrue(vm.NetworkDeployed);

            //check that we can execute the check for update command
            Assert.IsTrue(vm.CheckForUpdate.CanExecute(null));

            //execute the check for update command
            vm.CheckForUpdate.Execute(null);

            //check that an update is available
            Assert.IsTrue(vm.UpdateAvailable);

        }

    }
}
