﻿using System.Reflection;
using Microsoft.Bot.Builder.M365.AI;
using Microsoft.Bot.Builder.M365.AI.Action;
using Microsoft.Bot.Builder.M365.AI.Moderator;
using Microsoft.Bot.Builder.M365.AI.Planner;
using Microsoft.Bot.Builder.M365.AI.Prompt;
using Microsoft.Bot.Builder.M365.Exceptions;
using Microsoft.Bot.Builder.M365.State;
using Microsoft.Bot.Builder.M365.Tests.TestUtils;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Moq;

namespace Microsoft.Bot.Builder.M365.Tests.AI
{
    public class DefaultActionsTests
    {
        [Fact]
        public void Test_DefaultActions_Are_Imported()
        {
            // Act
            IActionCollection<TestTurnState> actions = ImportDefaultActions<TestTurnState>();

            // Assert
            Assert.True(actions.HasAction(DefaultActionTypes.UnknownActionName));
            Assert.True(actions.HasAction(DefaultActionTypes.FlaggedInputActionName));
            Assert.True(actions.HasAction(DefaultActionTypes.FlaggedOutputActionName));
            Assert.True(actions.HasAction(DefaultActionTypes.RateLimitedActionName));
            Assert.True(actions.HasAction(DefaultActionTypes.PlanReadyActionName));
            Assert.True(actions.HasAction(DefaultActionTypes.DoCommandActionName));
            Assert.True(actions.HasAction(DefaultActionTypes.SayCommandActionName));
        }

        [Fact]
        public async Task Test_Execute_UnkownAction()
        {
            // Arrange
            var logs = new List<string>();
            IActionCollection<TestTurnState> actions = ImportDefaultActions<TestTurnState>(logs);
            var activity = MessageFactory.Text("hello");
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);
            var turnState = new TestTurnState();

            // Act
            var unknownAction = actions.GetAction(DefaultActionTypes.UnknownActionName);
            var result = await unknownAction.Handler.PerformAction(turnContext, turnState, null, "test-action");

            // Assert
            Assert.True(result);
            Assert.Equal(1, logs.Count);
            Assert.Equal("An AI action named \"test-action\" was predicted but no handler was registered", logs[0]);
        }

        [Fact]
        public async Task Test_Execute_FlaggedInputAction()
        {
            // Arrange
            var logs = new List<string>();
            IActionCollection<TestTurnState> actions = ImportDefaultActions<TestTurnState>(logs);
            var activity = MessageFactory.Text("hello");
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);
            var turnState = new TestTurnState();

            // Act
            var flaggedInputAction = actions.GetAction(DefaultActionTypes.FlaggedInputActionName);
            var result = await flaggedInputAction.Handler.PerformAction(turnContext, turnState, null, null);

            // Assert
            Assert.True(result);
            Assert.Equal(1, logs.Count);
            Assert.Equal("The users input has been moderated but no handler was registered for ___FlaggedInput___", logs[0]);
        }

        [Fact]
        public async Task Test_Execute_FlaggedOutputAction()
        {
            // Arrange
            var logs = new List<string>();
            IActionCollection<TestTurnState> actions = ImportDefaultActions<TestTurnState>(logs);
            var activity = MessageFactory.Text("hello");
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);
            var turnState = new TestTurnState();

            // Act
            var flaggedOutputAction = actions.GetAction(DefaultActionTypes.FlaggedOutputActionName);
            var result = await flaggedOutputAction.Handler.PerformAction(turnContext, turnState, null, null);

            // Assert
            Assert.True(result);
            Assert.Equal(1, logs.Count);
            Assert.Equal("The bots output has been moderated but no handler was registered for ___FlaggedOutput___", logs[0]);
        }

        [Fact]
        public async Task Test_Execute_RateLimitedAction()
        {
            // Arrange
            IActionCollection<TestTurnState> actions = ImportDefaultActions<TestTurnState>();
            var activity = MessageFactory.Text("hello");
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);
            var turnState = new TestTurnState();

            // Act
            var rateLimitedAction = actions.GetAction(DefaultActionTypes.RateLimitedActionName);
            var exception = await Assert.ThrowsAsync<AIException>(async () => await rateLimitedAction.Handler.PerformAction(turnContext, turnState, null, null));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("An AI request failed because it was rate limited", exception.Message);
        }

        [Fact]
        public async Task Test_Execute_PlanReadyAction()
        {
            // Arrange
            IActionCollection<TestTurnState> actions = ImportDefaultActions<TestTurnState>();
            var activity = MessageFactory.Text("hello");
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);
            var turnState = new TestTurnState();
            var plan0 = new Plan(new List<IPredictedCommand>());
            var plan1 = new Plan(new List<IPredictedCommand>()
            {
                new PredictedDoCommand("action"),
            });

            // Act
            var planReadyAction = actions.GetAction(DefaultActionTypes.PlanReadyActionName);
            var result0 = await planReadyAction.Handler.PerformAction(turnContext, turnState, plan0, null);
            var result1 = await planReadyAction.Handler.PerformAction(turnContext, turnState, plan1, null);
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await planReadyAction.Handler.PerformAction(turnContext, turnState, null, null));

            // Assert
            Assert.False(result0);
            Assert.True(result1);
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'plan')", exception.Message);
        }

        [Fact]
        public async Task Test_Execute_DoCommandAction()
        {
            // Arrange
            IActionCollection<TestTurnState> actions = ImportDefaultActions<TestTurnState>();
            var activity = MessageFactory.Text("hello");
            var turnContext = new TurnContext(new NotImplementedAdapter(), activity);
            var turnState = new TestTurnState();
            var handler = new TestActionHandler();
            var data = new DoCommandActionData<TestTurnState>
            {
                PredictedDoCommand = new PredictedDoCommand("test-action"),
                Handler = handler,
            };

            // Act
            var doCommandAction = actions.GetAction(DefaultActionTypes.DoCommandActionName);
            var result = await doCommandAction.Handler.PerformAction(turnContext, turnState, data, null);
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await doCommandAction.Handler.PerformAction(turnContext, turnState, null, null));

            // Assert
            Assert.True(result);
            Assert.Equal("test-action", handler.ActionName);
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'doCommandActionData')", exception.Message);
        }

        [Fact]
        public async Task Test_Execute_SayCommandAction()
        {
            // Arrange
            IActionCollection<TestTurnState> actions = ImportDefaultActions<TestTurnState>();
            var turnContextMock = new Mock<ITurnContext>();
            turnContextMock.Setup(tc => tc.Activity).Returns(new Activity { Type = ActivityTypes.Message });
            turnContextMock.Setup(tc => tc.SendActivityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new ResourceResponse()));
            var turnState = new TestTurnState();
            var command = new PredictedSayCommand("hello");

            // Act
            var sayCommandAction = actions.GetAction(DefaultActionTypes.SayCommandActionName);
            var result = await sayCommandAction.Handler.PerformAction(turnContextMock.Object, turnState, command, null);
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await sayCommandAction.Handler.PerformAction(turnContextMock.Object, turnState, null, null));

            // Assert
            Assert.True(result);
            turnContextMock.Verify(tc => tc.SendActivityAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'command')", exception.Message);
        }

        private static IActionCollection<TState> ImportDefaultActions<TState>(IList<string>? logs = null) where TState : ITurnState<StateBase, StateBase, TempState>
        {
            ILogger? logger = null;
            if (logs != null)
            {
                Mock<ILogger> loggerMock = new();
                loggerMock.Setup(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                    .Callback(new InvocationAction(invocation =>
                    {
                        var state = invocation.Arguments[2];
                        var exception = (Exception)invocation.Arguments[3];
                        var formatter = invocation.Arguments[4];

                        var invokeMethod = formatter.GetType().GetMethod("Invoke");
                        var logMessage = (string?)invokeMethod?.Invoke(formatter, new[] { state, exception });
                        if (logMessage != null)
                        {
                            logs.Add(logMessage);
                        }
                    }));
                logger = loggerMock.Object;
            }

            AIOptions<TState> options = new(
                new Mock<IPlanner<TState>>().Object,
                new PromptManager<TState>(),
                new Mock<IModerator<TState>>().Object);
            AI<TState> ai = new(options, logger);
            // get _actions field from AI class
            FieldInfo actionsField = typeof(AI<TState>).GetField("_actions", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance)!;
            return (IActionCollection<TState>)actionsField!.GetValue(ai)!;
        }

        private class TestActionHandler : IActionHandler<TestTurnState>
        {
            public string? ActionName { get; set; }

            public Task<bool> PerformAction(ITurnContext turnContext, TestTurnState turnState, object? entities = null, string? action = null)
            {
                ActionName = action;
                return Task.FromResult(true);
            }
        }
    }
}
