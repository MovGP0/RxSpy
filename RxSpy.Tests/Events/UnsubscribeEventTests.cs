using RxSpy.Events;

namespace RxSpy.Tests.Events;

public sealed class UnsubscribeEventTests
{
    public sealed class ConstructorTests
    {
        [Fact]
        public void ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            const long expectedEventId = 1L;
            const long expectedEventTime = 123456789L;
            const long expectedSubscriptionId = 99L;

            // Act
            var unsubscribeEvent = new UnsubscribeEvent
            {
                EventId = expectedEventId,
                EventTime = expectedEventTime,
                SubscriptionId = expectedSubscriptionId
            };

            // Assert
            unsubscribeEvent.ShouldSatisfyAllConditions(
                e => e.EventId.ShouldBe(expectedEventId),
                e => e.EventTime.ShouldBe(expectedEventTime),
                e => e.SubscriptionId.ShouldBe(expectedSubscriptionId));
        }
    }
}