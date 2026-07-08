using NUnit.Framework;
using UnityEngine;
using TeleportNative.Capture;

namespace TeleportNative.Tests
{
    [TestFixture]
    public class CoverageTrackerTests
    {
        [Test]
        public void Full_rotation_covers_all_sectors()
        {
            var c = new CoverageTracker(12);
            for (int i = 0; i < 12; i++)
            {
                var q = Quaternion.Euler(0, i * 30 + 5, 0);
                c.Register(q * Vector3.forward);
            }
            Assert.GreaterOrEqual(c.Current.Coverage01, 0.99f);
        }

        [Test]
        public void Same_direction_covers_one_sector()
        {
            var c = new CoverageTracker(12);
            c.Register(Vector3.forward);
            c.Register(Vector3.forward);
            Assert.AreEqual(1, c.Current.SectorsCovered);
        }
    }
}
