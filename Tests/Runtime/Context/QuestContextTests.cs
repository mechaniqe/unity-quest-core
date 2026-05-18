using System;
using NUnit.Framework;
using DynamicBox.Quest.Core;

namespace DynamicBox.Quest.Tests.Context
{
    [TestFixture]
    public class QuestContextTests
    {
        // Minimal interfaces used purely for registration tests
        private interface IFakeService { }
        private interface ISecondFakeService { }
        private class FakeServiceImpl : IFakeService { }
        private class SecondFakeServiceImpl : ISecondFakeService { }

        [Test]
        public void RegisterService_ThenGetService_ReturnsSameInstance()
        {
            var context = new QuestContext();
            var service = new FakeServiceImpl();

            context.RegisterService<IFakeService>(service);

            Assert.That(context.GetService<IFakeService>(), Is.SameAs(service));
        }

        [Test]
        public void GetService_NotRegistered_ReturnsNull()
        {
            var context = new QuestContext();

            Assert.That(context.GetService<IFakeService>(), Is.Null);
        }

        [Test]
        public void GetRequiredService_NotRegistered_ThrowsInvalidOperationException()
        {
            var context = new QuestContext();

            Assert.Throws<InvalidOperationException>(() => context.GetRequiredService<IFakeService>());
        }

        [Test]
        public void GetRequiredService_Registered_ReturnsInstance()
        {
            var context = new QuestContext();
            var service = new FakeServiceImpl();
            context.RegisterService<IFakeService>(service);

            var result = context.GetRequiredService<IFakeService>();

            Assert.That(result, Is.SameAs(service));
        }

        [Test]
        public void HasService_AfterRegister_ReturnsTrue()
        {
            var context = new QuestContext();
            context.RegisterService<IFakeService>(new FakeServiceImpl());

            Assert.That(context.HasService<IFakeService>(), Is.True);
        }

        [Test]
        public void HasService_BeforeRegister_ReturnsFalse()
        {
            var context = new QuestContext();

            Assert.That(context.HasService<IFakeService>(), Is.False);
        }

        [Test]
        public void UnregisterService_RemovesServiceFromContext()
        {
            var context = new QuestContext();
            context.RegisterService<IFakeService>(new FakeServiceImpl());

            context.UnregisterService<IFakeService>();

            Assert.That(context.HasService<IFakeService>(), Is.False);
        }

        [Test]
        public void MultipleServiceTypes_Coexist()
        {
            var context = new QuestContext();
            var svc1 = new FakeServiceImpl();
            var svc2 = new SecondFakeServiceImpl();

            context.RegisterService<IFakeService>(svc1);
            context.RegisterService<ISecondFakeService>(svc2);

            Assert.That(context.GetService<IFakeService>(), Is.SameAs(svc1));
            Assert.That(context.GetService<ISecondFakeService>(), Is.SameAs(svc2));
        }

        [Test]
        public void ConvenienceProperties_WhenNoServicesRegistered_ReturnNull()
        {
            var context = new QuestContext();

            Assert.That(context.AreaService, Is.Null);
            Assert.That(context.InventoryService, Is.Null);
            Assert.That(context.TimeService, Is.Null);
            Assert.That(context.FlagService, Is.Null);
        }

        [Test]
        public void RegisterService_ReplaceExisting_ReturnsNewInstance()
        {
            var context = new QuestContext();
            var original = new FakeServiceImpl();
            var replacement = new FakeServiceImpl();

            context.RegisterService<IFakeService>(original);
            context.RegisterService<IFakeService>(replacement);

            Assert.That(context.GetService<IFakeService>(), Is.SameAs(replacement));
        }
    }
}
