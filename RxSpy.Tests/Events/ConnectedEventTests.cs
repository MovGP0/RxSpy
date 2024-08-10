using RxSpy.Events;

namespace RxSpy.Tests.Events;

public sealed class ConnectedEventTests
{
    public sealed class ConstructorTests
    {
        [Fact]
        public void ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            const long expectedEventId = 1L;
            const long expectedEventTime = 123456789L;
            const long expectedOperatorId = 42L;

            // Act
            var connectedEvent = new ConnectedEvent
            {
                EventId = expectedEventId,
                EventTime = expectedEventTime,
                OperatorId = expectedOperatorId
            };

            // Assert
            connectedEvent.ShouldSatisfyAllConditions(
                e => e.EventId.ShouldBe(expectedEventId),
                e => e.EventTime.ShouldBe(expectedEventTime),
                e => e.OperatorId.ShouldBe(expectedOperatorId));
        }
    }
}
