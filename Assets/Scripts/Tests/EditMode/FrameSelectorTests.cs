using NUnit.Framework;
using UnityEngine;
using TeleportNative.Capture;

namespace TeleportNative.Tests
{
    [TestFixture]
    public class FrameSelectorTests
    {
        [Test]
        public void Rejects_blurry_frame()
        {
            var s = new FrameSelector(blurThreshold: 100f);
            var p = Matrix4x4.Translate(new Vector3(5, 0, 0));
            Assert.IsFalse(s.ShouldKeep(p, 10f));
        }

        [Test]
        public void Accepts_novel_and_sharp()
        {
            var s = new FrameSelector();
            var p = Matrix4x4.Translate(new Vector3(5, 0, 0));
            Assert.IsTrue(s.ShouldKeep(p, 500f));
        }

        [Test]
        public void Rejects_redundant_pose()
        {
            var s = new FrameSelector();
            var p = Matrix4x4.Translate(new Vector3(5, 0, 0));
            Assert.IsTrue(s.ShouldKeep(p, 500f));
            Assert.IsFalse(s.ShouldKeep(p, 500f)); // mesma posicao/rotacao
        }

        [Test]
        public void IsPoseNovel_skips_redundant()
        {
            var s = new FrameSelector();
            var a = Matrix4x4.Translate(new Vector3(5, 0, 0));
            var b = Matrix4x4.Translate(new Vector3(5, 0, 0));
            Assert.IsTrue(s.IsPoseNovel(a));
            s.ShouldKeep(a, 500f);
            Assert.IsFalse(s.IsPoseNovel(b));
        }
    }
}
