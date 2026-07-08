using System.Collections.Generic;
using NUnit.Framework;
using TeleportNative.Core;

namespace TeleportNative.Tests.EditMode
{
    public class RealtyFlowTests
    {
        [Test]
        public void Draft_Advances_Through_Rooms()
        {
            var d = new RealtyDraft("Casa", "",
                new List<RoomType> { RoomType.Sala, RoomType.Cozinha, RoomType.Quarto });

            Assert.AreEqual(3, d.Total);
            Assert.AreEqual(0, d.Done);
            Assert.AreEqual(RoomType.Sala, d.NextType);
            Assert.IsTrue(d.HasNext);

            d.Advance();
            Assert.AreEqual(1, d.Done);
            Assert.AreEqual(RoomType.Cozinha, d.NextType);

            d.Advance();
            d.Advance();
            Assert.IsFalse(d.HasNext);
            Assert.AreEqual(3, d.Done);
        }

        [Test]
        public void NextLabel_Includes_Title_And_Room()
        {
            var d = new RealtyDraft("Apto 101", "", new List<RoomType> { RoomType.Banheiro });
            Assert.AreEqual("Apto 101 — Banheiro", d.NextLabel());
        }

        [Test]
        public void Empty_Rooms_Is_Safe()
        {
            var d = new RealtyDraft("x", "", new List<RoomType>());
            Assert.IsFalse(d.HasNext);
            d.Advance(); // no-op
            Assert.AreEqual(0, d.Done);
        }

        [Test]
        public void Catalog_Has_Brazilian_Rooms()
        {
            Assert.IsTrue(RoomCatalog.All.Length >= 10);
            Assert.AreEqual("Suíte", RoomCatalog.Of(RoomType.Suite).Label);
            Assert.AreEqual("Área gourmet", RoomCatalog.Of(RoomType.AreaGourmet).Label);
        }

        [Test]
        public void Capture_Uses_Room_MinPhotos_When_Guided()
        {
            Assert.AreEqual(6, CaptureFlowLogic.EffectiveMinKeyframes(RoomType.Banheiro));
            Assert.AreEqual(10, CaptureFlowLogic.EffectiveMinKeyframes(RoomType.Sala));
            Assert.AreEqual(CaptureFlowLogic.MinKeyframes,
                CaptureFlowLogic.EffectiveMinKeyframes(null));
        }

        [Test]
        public void Hint_Shows_Room_Label_When_Guided()
        {
            var h = CaptureFlowLogic.Hint(true, false, 0, 0, RoomType.Cozinha);
            Assert.IsTrue(h.Contains("Cozinha"));
        }
    }
}
