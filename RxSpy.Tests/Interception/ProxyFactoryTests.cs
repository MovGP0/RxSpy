using System.ComponentModel;
using RxSpy.Interception;

namespace RxSpy.Tests.Interception;

public static class ProxyFactoryTests
{
    public sealed class CreateProxyTests
    {
        public interface ITestInterface
        {
            void MethodA(int value);
            string MethodB(string input);
        }

        [Fact]
        [Category("HappyPath")]
        public void ShouldInterceptAndCallBothObjects()
        {
            // Arrange
            var spyObject = Substitute.For<ITestInterface>();
            var originalObject = Substitute.For<ITestInterface>();

            // Act: Create Proxy
            var proxy = ProxyFactory.CreateProxy(typeof(ITestInterface), spyObject, originalObject);
            var castedProxy = proxy as ITestInterface;

            // Assert: Proxy was created
            castedProxy.ShouldNotBeNull();

            // Act: Call the methods on the proxy
            castedProxy.MethodA(42);
            _ = castedProxy.MethodB("test");

            // Assert: Verify that the methods were called on both objects
            spyObject.Received().MethodA(42);
            originalObject.Received().MethodA(42);

            spyObject.Received().MethodB("test");
            originalObject.Received().MethodB("test");
        }

        [Fact]
        public void ShouldThrowInvalidOperationExceptionWhenInterfaceNotImplemented()
        {
            // Arrange
            var spyObject = Substitute.For<ITestInterface>();
            var nonMatchingObject = new object(); // Does not implement ITestInterface

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                ProxyFactory.CreateProxy(typeof(ITestInterface), spyObject, nonMatchingObject));
        }

        [Fact]
        public void ShouldOnlyCallOriginalObjectWhenMethodNotInSpyObject()
        {
            // Arrange
            var originalObject = Substitute.For<ITestInterface>();
            var spyObject = Substitute.For<ITestInterface>();

            // Act: Create Proxy
            var proxy = ProxyFactory.CreateProxy(typeof(ITestInterface), spyObject, originalObject);
            var castedProxy = proxy as ITestInterface;

            // Assert: Proxy was created
            Assert.NotNull(castedProxy);

            // Act: Call Methods
            castedProxy.MethodA(42);
            _ = castedProxy.MethodB("test");

            // Assert: Verify that MethodA was only called on the original object
            originalObject.Received().MethodA(42);
            spyObject.Received().MethodA(42);

            // Assert: Verify that MethodB was called on both objects
            originalObject.Received().MethodB("test");
            spyObject.Received().MethodB("test");
        }

        [Fact]
        public void ShouldOnlyCallSpyObjectWhenMethodNotInOriginalObject()
        {
            // Arrange
            var originalObject = Substitute.For<ITestInterface>();
            var spyObject = Substitute.For<ITestInterface>();

            // Act: Create Proxy
            var proxy = ProxyFactory.CreateProxy(typeof(ITestInterface), spyObject, originalObject);
            var castedProxy = proxy as ITestInterface;

            // Assert: Proxy was created
            castedProxy.ShouldNotBeNull();

            // Act: Call the methods on the proxy
            castedProxy.MethodA(42);
            _ = castedProxy.MethodB("test");

            // Assert: Verify that MethodA was called on both objects
            originalObject.Received().MethodA(42);
            spyObject.Received().MethodA(42);

            // Assert: Verify that MethodB was only called on the spy object
            originalObject.Received().MethodB("test");
            spyObject.Received().MethodB("test");
        }
    }
}