using Microsoft.Extensions.Logging;
using Xunit;
using MelTimers;
using Dubstep.TestUtilities;
using FluentAssertions;
using System;

namespace MelTimers.Tests;

public class Tests
{

    TestLogger<Tests> logger;

    string completedMessage = "completed";
    string abandonedMessage = "abandoned";
           
 
    public Tests()
    {
        logger = new TestLogger<Tests>();
    }
    
    [Fact]
    public void LoggerExtensions_TimeOperation_Completed()
    {
        // Arrage
                       

        // Act
        using (logger.TimeOperation("Testing"))
        {
            // Do nothing.
        }

        // Assert

        logger.LogStatements.Should().HaveCount(1);
        logger.LastStatement.Level.Should().Be(LogLevel.Information);
        logger.LastStatement.Message.Should().Contain("Testing");
        logger.LastStatement.Message.Should().Contain(completedMessage);
        
    }

    [Fact]
    public void LoggerExtensions_TimeOperation_WithParameters()
    {
        // Arrange
        var parameterValue = "someText";

        // Act
        using (logger.TimeOperation("Testing with {parameter}", parameterValue))
        {
            // Do nothing.
        }

        // Assert
        logger.LogStatements.Should().HaveCount(1);
        logger.LastStatement.Level.Should().Be(LogLevel.Information);
        logger.LastStatement.Message.Should().Contain(completedMessage);
        logger.LastStatement.Message.Should().Contain("Testing");
        logger.LastStatement.Message.Should().Contain(parameterValue);
    }

    [Fact]
    public void LoggerExtensions_TimeOperation_LogAtLevel()
    {
        // Arrange
        string parameterValue = "someText";
        
        // Act
        using (logger.TimeOperation(LogLevel.Trace, "Testing {param}", parameterValue))
        {
            // Do nothing.
        }

        // Assert
        logger.LogStatements.Should().HaveCount(1);
        logger.LastStatement.Level.Should().Be(LogLevel.Trace);
        logger.LastStatement.Message.Should().Contain(completedMessage);
        logger.LastStatement.Message.Should().Contain("Testing");
        logger.LastStatement.Message.Should().Contain(parameterValue);
    }

    [Fact]
    public void LoggerExtensions_BeginOperation_Completed()
    {
        // Arrange
                    
        // Act
        using (var op = logger.BeginOperation("Testing"))
        {
            op.Complete();
        }

        // Assert
        logger.LogStatements.Should().HaveCount(1);
        logger.LastStatement.Level.Should().Be(LogLevel.Information);
        logger.LastStatement.Message.Should().Contain("Testing");
        logger.LastStatement.Message.Should().Contain(completedMessage);
    }

    [Fact]
    public void LoggerExtensions_BeginOperation_DidNotComplete()
    {
        // Arrange
        
        
        // Act
        using(var op = logger.BeginOperation("Testing"))
        {
            // Do nothing.
        }

        // Assert
        logger.LogStatements.Should().HaveCount(1);
        logger.LastStatement.Level.Should().Be(LogLevel.Warning);
        logger.LastStatement.Message.Should().Contain("Testing");
        logger.LastStatement.Message.Should().Contain(abandonedMessage);
    }


    [Fact]
    public void LoggerExtensions_BeginOperation_LogAtLevel_Completed()
    {
        // Arrange
        string parameterValue = "someText";

        // Act
        using (var op = logger.BeginOperation(LogLevel.Debug, LogLevel.Critical, "Testing {arg}", parameterValue))
        {
            op.Complete();
        }

        // Assert
        logger.LogStatements.Should().HaveCount(1);
        logger.LastStatement.Level.Should().Be(LogLevel.Debug);
        logger.LastStatement.Message.Should().Contain(completedMessage);
        logger.LastStatement.Message.Should().Contain("Testing");
        logger.LastStatement.Message.Should().Contain(parameterValue);
    }

    [Fact]
    public void LoggerExtensions_BeginOperation_LogAtLevel_DidNotComplete()
    {
        // Arrange
        string parameterValue = "someText";

        // Act
        using (var op = logger.BeginOperation(LogLevel.Debug, LogLevel.Critical, "Testing {arg}", parameterValue))
        {
            // Do nothing.
        }

        // Assert
        logger.LogStatements.Should().HaveCount(1);
        logger.LastStatement.Level.Should().Be(LogLevel.Critical);
        logger.LastStatement.Message.Should().Contain(abandonedMessage);
        logger.LastStatement.Message.Should().Contain("Testing");
        logger.LastStatement.Message.Should().Contain(parameterValue);
    }

    [Fact]
    public void LoggerExtensions_BeginOperation_CompleteWithResult()
    {
        // Arrange
        string parameterValue = "someText";
        string resultValue = "theResult";

        // Act
        using(var op = logger.BeginOperation("Testing {arg}", parameterValue))
        {
            op.Complete(resultValue);
        }

        // Assert
        logger.LogStatements.Should().HaveCount(1);
        logger.LastStatement.Level.Should().Be(LogLevel.Information);
        logger.LastStatement.Message.Should().Contain(completedMessage);
        logger.LastStatement.Message.Should().Contain(parameterValue);
        logger.LastStatement.Message.Should().Contain(resultValue);
    }

    [Fact]
    public void LoggerExtensions_BeginOperation_CompleteWithResultAndFieldName()
    {
        // Arrange
        string parameterValue = "someText";
        var resultValue = "theResult";

        // Act
        using (var op = logger.BeginOperation("Testing {arg}", parameterValue))
        {
            op.Complete(resultValue, "resultFieldName");
        }

        // Assert
        logger.LogStatements.Should().HaveCount(1);
        logger.LastStatement.Level.Should().Be(LogLevel.Information);
        logger.LastStatement.Message.Should().Contain(completedMessage);
        logger.LastStatement.Message.Should().Contain(parameterValue);
        logger.LastStatement.Message.Should().Contain(resultValue);
    }

    [Fact]
    public void LoggerExtensions_BeginOperation_CompleteWithValueDestructured()
    {
        // Arrange
        string propertyValue = "someText";
        var resultValue = "aResult";
        
        // Act
        using (var op = logger.BeginOperation("Test {arg}", propertyValue))
        {
            op.Complete(resultValue, "propertyName");
        }

        // Assert
        logger.LogStatements.Should().HaveCount(1);
        logger.LastStatement.Level.Should().Be(LogLevel.Information);
        logger.LastStatement.Message.Should().Contain(completedMessage);
        logger.LastStatement.Message.Should().Contain(resultValue);
    }

    [Fact]
    public void LoggerExtensions_BeginOperation_Cancelled()
    {
        // Arrange
        // Act
        using (var op = logger.BeginOperation("Testing"))
        {
            op.Cancel();
        }

        // Assert
        logger.LogStatements.Should().HaveCount(0);
    }

    [Fact]
    public void LoggerExtensions_BeginOperation_Abandoned()
    {
        // Arrange
        // Act
        using (var op = logger.BeginOperation("Testing"))
        {
            op.Abandon();
        }

        // Assert
        logger.LogStatements.Should().HaveCount(1);
        logger.LastStatement.Level.Should().Be(LogLevel.Warning);
        logger.LastStatement.Message.Should().Contain(abandonedMessage);
        logger.LastStatement.Message.Should().Contain("Testing");
    }

    [Fact]
    public void LoggerExtesion_BeginOperation_AbandonWithReason()
    {
        // Arrange
        string reason = "someReason";

        // Act
        using (var op = logger.BeginOperation("Testing"))
        {
            op.Abandon(reason);
        }

        // Assert
        logger.LogStatements.Should().HaveCount(1);
        logger.LastStatement.Level.Should().Be(LogLevel.Warning);
        logger.LastStatement.Message.Should().Contain(abandonedMessage);
        logger.LastStatement.Message.Should().Contain("Testing");
        logger.LastStatement.Message.Should().Contain(reason);
    }

    [Fact]
    public void LoggerExtension_BeginOperation_LogAtLevel_AbandonWithReason()
    {
        // Arrange
        string reason = "someReason";

        // Act
        using (var op = logger.BeginOperation(LogLevel.Trace, LogLevel.Debug, "Testing"))
        {
            op.Abandon(reason);
        }

        // Assert
        logger.LogStatements.Should().HaveCount(1);
        logger.LastStatement.Level.Should().Be(LogLevel.Debug);
        logger.LastStatement.Message.Should().Contain(abandonedMessage);
        logger.LastStatement.Message.Should().Contain("Testing");
        logger.LastStatement.Message.Should().Contain(reason);
    }
}
