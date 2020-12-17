using System;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Dfe.Edis.SourceAdapter.Ukrlp.Application;
using Dfe.Edis.SourceAdapter.Ukrlp.FunctionApp.ChangeProcessing;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Logging;
using Moq;
using NCrontab;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Ukrlp.FunctionApp.UnitTests.ChangeProcessingTests
{
    public class WhenExecutingTimedCheckForChange
    {
        private Mock<IChangeProcessor> _changeProcessorMock;
        private Mock<ILogger<TimedCheckForChange>> _loggerMock;
        private TimedCheckForChange _function;
        private CancellationToken _cancellationToken;

        [SetUp]
        public void Arrange()
        {
            _changeProcessorMock = new Mock<IChangeProcessor>();

            _loggerMock = new Mock<ILogger<TimedCheckForChange>>();

            _function = new TimedCheckForChange(
                _changeProcessorMock.Object,
                _loggerMock.Object);

            _cancellationToken = new CancellationToken();
        }

        [Test]
        public async Task ThenItShouldCallChangeProcessor()
        {
            await _function.RunAsync(new TimerInfo(new CronSchedule(CrontabSchedule.Parse("0 0 1 1 *")), new ScheduleStatus()), _cancellationToken);
            
            _changeProcessorMock.Verify(processor => processor.ProcessChangesAsync(_cancellationToken), Times.Once);
        }

        [Test]
        public void ThenItShouldRethrowException()
        {
            var exception = new Exception("unit testing");
            _changeProcessorMock.Setup(processor => processor.ProcessChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var actual = Assert.ThrowsAsync<Exception>(async () =>
                await _function.RunAsync(new TimerInfo(new CronSchedule(CrontabSchedule.Parse("0 0 1 1 *")), new ScheduleStatus()), _cancellationToken));
            Assert.AreSame(exception, actual);
        }
    }
}