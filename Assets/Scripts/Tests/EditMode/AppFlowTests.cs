using NUnit.Framework;
using TeleportNative.Core;

namespace TeleportNative.Tests
{
    [TestFixture]
    public class AppFlowTests
    {
        [Test]
        public void Valid_transitions() =>
            Assert.IsTrue(AppFlow.IsValidTransition(AppScreen.Onboarding, AppScreen.Library));

        [Test]
        public void Invalid_transition_blocked() =>
            Assert.IsFalse(AppFlow.IsValidTransition(AppScreen.Onboarding, AppScreen.Viewer));

        [Test]
        public void Request_advances_along_happy_path()
        {
            var f = new AppFlow();
            Assert.IsTrue(f.Request(AppScreen.Library));
            Assert.IsTrue(f.Request(AppScreen.Capture));
            Assert.IsTrue(f.Request(AppScreen.Processing));
            Assert.IsTrue(f.Request(AppScreen.Viewer));
            Assert.AreEqual(AppScreen.Viewer, f.Current);
        }

        [Test]
        public void Cannot_jump_back_to_onboarding()
        {
            var f = new AppFlow();
            f.ResetTo(AppScreen.Library);
            Assert.IsFalse(f.Request(AppScreen.Onboarding));
        }
    }
}
