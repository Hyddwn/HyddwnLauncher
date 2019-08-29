using System;
using HyddwnLauncher.Core;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Patcher.NxLauncher;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HyddwnLauncher.Tests.PatcherTests
{
    [TestClass]
    public class GetIsNewPackFileTests
    {
        [TestMethod]
        public void GetIsNewPackFile_WithinRangeWithDistanceOfOne_CurrentToLatest()
        {
            var nxlPatcher = new NxlPatcher(new ClientProfile(), ServerProfile.OfficialProfile, new PatcherContext());

            var packPath = "\\package\\100_to_101.pack";
            var currentVersion = 100;
            var latestVersion = 101;

            var isWithinRange = nxlPatcher.GetIsNewPackFile(packPath, currentVersion, latestVersion);

            Assert.IsTrue(isWithinRange);
        }

        [TestMethod]
        public void GetIsNewPackFile_WithinRangeWithDistanceOfOne_PreviousToLatest()
        {
            var nxlPatcher = new NxlPatcher(new ClientProfile(), ServerProfile.OfficialProfile, new PatcherContext());

            var packPath = "\\package\\99_to_101.pack";
            var currentVersion = 100;
            var latestVersion = 101;

            var isWithinRange = nxlPatcher.GetIsNewPackFile(packPath, currentVersion, latestVersion);

            Assert.IsTrue(isWithinRange);
        }

        [TestMethod]
        public void GetIsNewPackFile_WithinRangeWithDistanceOfOne_PreviousToCurrent()
        {
            var nxlPatcher = new NxlPatcher(new ClientProfile(), ServerProfile.OfficialProfile, new PatcherContext());

            var packPath = "\\package\\99_to_100.pack";
            var currentVersion = 100;
            var latestVersion = 101;

            var isWithinRange = nxlPatcher.GetIsNewPackFile(packPath, currentVersion, latestVersion);

            Assert.IsFalse(isWithinRange);
        }

        [TestMethod]
        public void GetIsNewPackFile_WithinRangeWithDistanceOfMoreThanOne_BeforeRange()
        {
            var nxlPatcher = new NxlPatcher(new ClientProfile(), ServerProfile.OfficialProfile, new PatcherContext());

            var packPath = "\\package\\99_to_100.pack";
            var currentVersion = 100;
            var latestVersion = 103;

            var isWithinRange = nxlPatcher.GetIsNewPackFile(packPath, currentVersion, latestVersion);

            Assert.IsFalse(isWithinRange);
        }

        [TestMethod]
        public void GetIsNewPackFile_WithinRangeWithDistanceOfMoreThanOne_InitialFile()
        {
            var nxlPatcher = new NxlPatcher(new ClientProfile(), ServerProfile.OfficialProfile, new PatcherContext());

            var packPath = "\\package\\100_to_101.pack";
            var currentVersion = 100;
            var latestVersion = 103;

            var isWithinRange = nxlPatcher.GetIsNewPackFile(packPath, currentVersion, latestVersion);

            Assert.IsTrue(isWithinRange);
        }

        [TestMethod]
        public void GetIsNewPackFile_WithinRangeWithDistanceOfMoreThanOne_MiddleFile()
        {
            var nxlPatcher = new NxlPatcher(new ClientProfile(), ServerProfile.OfficialProfile, new PatcherContext());

            var packPath = "\\package\\101_to_102.pack";
            var currentVersion = 100;
            var latestVersion = 103;

            var isWithinRange = nxlPatcher.GetIsNewPackFile(packPath, currentVersion, latestVersion);

            Assert.IsTrue(isWithinRange);
        }

        [TestMethod]
        public void GetIsNewPackFile_WithinRangeWithDistanceOfMoreThanOne_LastFile()
        {
            var nxlPatcher = new NxlPatcher(new ClientProfile(), ServerProfile.OfficialProfile, new PatcherContext());

            var packPath = "\\package\\102_to_103.pack";
            var currentVersion = 100;
            var latestVersion = 103;

            var isWithinRange = nxlPatcher.GetIsNewPackFile(packPath, currentVersion, latestVersion);

            Assert.IsTrue(isWithinRange);
        }

        [TestMethod]
        public void GetIsNewPackFile_WithinRangeWithDistanceOfMoreThanOne_AfterRange()
        {
            var nxlPatcher = new NxlPatcher(new ClientProfile(), ServerProfile.OfficialProfile, new PatcherContext());

            var packPath = "\\package\\103_to_104.pack";
            var currentVersion = 100;
            var latestVersion = 103;

            var isWithinRange = nxlPatcher.GetIsNewPackFile(packPath, currentVersion, latestVersion);

            Assert.IsFalse(isWithinRange);
        }
    }
}
