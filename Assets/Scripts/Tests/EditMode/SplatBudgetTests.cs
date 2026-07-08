using NUnit.Framework;
using TeleportNative.Performance;

namespace TeleportNative.Tests
{
    [TestFixture]
    public class SplatBudgetTests
    {
        [Test]
        public void Tier_budgets_match_spec()
        {
            Assert.AreEqual(SplatBudget.Low, SplatBudget.For(Tier.Low));
            Assert.AreEqual(SplatBudget.Mid, SplatBudget.For(Tier.Mid));
            Assert.AreEqual(SplatBudget.High, SplatBudget.For(Tier.High));
        }

        [Test]
        public void DeviceProfiler_starts_at_base_budget()
        {
            var p = new DeviceProfiler();
            Assert.AreEqual(p.BaseSplatBudget, p.SplatBudget);
            Assert.AreEqual(1f, p.ThermalFactor);
        }
    }
}
