using System.Diagnostics;
using RxSpy.Events;
using RxSpy.Factories;

namespace RxSpy.Tests.Events;

public sealed class OperatorCreatedEventTests
{
    public sealed class ConstructorTests
    {
        [Fact]
        public void ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            const long expectedEventId = 1L;
            const long expectedEventTime = 123456789L;
            const long expectedId = 100L;
            const string expectedName = "OperatorName";
            var expectedCallSite = CallSiteFactory.Create(new StackFrame());
            var expectedOperatorMethod = MethodInfoFactory.Create(typeof(object).GetMethod("ToString")!);

            // Act
            var operatorCreatedEvent = new OperatorCreatedEvent
            {
                EventId = expectedEventId,
                EventTime = expectedEventTime,
                Id = expectedId,
                Name = expectedName,
                CallSite = expectedCallSite,
                OperatorMethod = expectedOperatorMethod
            };

            // Assert
            operatorCreatedEvent.ShouldSatisfyAllConditions(
                e => e.EventId.ShouldBe(expectedEventId),
                e => e.EventTime.ShouldBe(expectedEventTime),
                e => e.Id.ShouldBe(expectedId),
                e => e.Name.ShouldBe(expectedName),
                e => e.CallSite.ShouldBe(expectedCallSite),
                e => e.OperatorMethod.ShouldBe(expectedOperatorMethod));
        }
    }
}