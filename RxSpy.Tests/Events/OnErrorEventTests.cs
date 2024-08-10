using RxSpy.Events;
using RxSpy.Factories;

namespace RxSpy.Tests.Events;

public sealed class OnErrorEventTests
{
    public sealed class ConstructorTests
    {
        [Fact]
        public void ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            const long expectedEventId = 1L;
            const long expectedEventTime = 123456789L;
            var expectedErrorType = TypeInfoFactory.Create(typeof(Exception));
            const string expectedMessage = "An error occurred";
            const long expectedOperatorId = 42L;
            const string expectedStackTrace = "Stack trace here";

            // Act
            var onErrorEvent = new OnErrorEvent
            {
                EventId = expectedEventId,
                EventTime = expectedEventTime,
                ErrorType = expectedErrorType,
                Message = expectedMessage,
                OperatorId = expectedOperatorId,
                StackTrace = expectedStackTrace
            };

            // Assert
            onErrorEvent.ShouldSatisfyAllConditions(
                e => e.EventId.ShouldBe(expectedEventId),
                e => e.EventTime.ShouldBe(expectedEventTime),
                e => e.ErrorType.ShouldBe(expectedErrorType),
                e => e.Message.ShouldBe(expectedMessage),
                e => e.OperatorId.ShouldBe(expectedOperatorId),
                e => e.StackTrace.ShouldBe(expectedStackTrace));
        }
    }
}