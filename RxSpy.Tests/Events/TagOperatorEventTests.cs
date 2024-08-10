using RxSpy.Events;

namespace RxSpy.Tests.Events;

public sealed class TagOperatorEventTests
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
            const string expectedTag = "Tag";

            // Act
            var tagOperatorEvent = new TagOperatorEvent
            {
                EventId = expectedEventId,
                EventTime = expectedEventTime,
                OperatorId = expectedOperatorId,
                Tag = expectedTag
            };

            // Assert
            tagOperatorEvent.ShouldSatisfyAllConditions(
                e => e.EventId.ShouldBe(expectedEventId),
                e => e.EventTime.ShouldBe(expectedEventTime),
                e => e.OperatorId.ShouldBe(expectedOperatorId),
                e => e.Tag.ShouldBe(expectedTag));
        }
    }
}
