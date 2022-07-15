// Copyright 2022 MelTimers Contributors.
// Original Code Copyright 2016 Serilog-Timings Contributors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Logging;

namespace MelTimers;


/// <summary>
/// Extensions for Microsoft.Extensions.Logging.ILogger
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Begin a new timed operation. The return value must be disposed to complete the operation.
    /// </summary>
    /// <param name="logger">The logger through which the timing will be recorded.</param>
    /// <param name="messageTemplate">A log message describing the operation, in message template format.</param>
    /// <param name="args">Arguments to the log message. These will be stored and captured only when the
    /// operation completes, so do not pass arguments that are mutated during the operation.</param>
    /// <returns>An <see cref="Operation"/> object.</returns>
    public static IDisposable TimeOperation(this ILogger logger, string messageTemplate, params object[] args)
    {
        return new Operation(logger, messageTemplate, args, CompletionBehaviour.Complete, LogLevel.Information, LogLevel.Warning);
    }

    /// <summary>
    /// Begin a new timed operation. The return value must be disposed to complete the operation.
    /// </summary>
    /// <param name="logger">The logger through which the timing will be recorded.</param>
    /// <param name="level">The LogLevel to assign to the log entry.</param>
    /// <param name="messageTemplate">A log message describing the operation, in message template format.</param>
    /// <param name="args">Arguments to the log message. These will be stored and captured only when the
    /// operation completes, so do not pass arguments that are mutated during the operation.</param>
    /// <returns>An <see cref="Operation"/> object.</returns>
    public static IDisposable TimeOperation(this ILogger logger, LogLevel level, string messageTemplate, params object[] args)
    {
        return new Operation(logger, messageTemplate, args, CompletionBehaviour.Complete, level, LogLevel.Warning);
    }

    /// <summary>
    /// Begin a new timed operation. The return value must be completed using <see cref="Operation.Complete()"/>,
    /// or disposed to record abandonment.
    /// </summary>
    /// <param name="logger">The logger through which the timing will be recorded.</param>
    /// <param name="messageTemplate">A log message describing the operation, in message template format.</param>
    /// <param name="args">Arguments to the log message. These will be stored and captured only when the
    /// operation completes, so do not pass arguments that are mutated during the operation.</param>
    /// <returns>An <see cref="Operation"/> object.</returns>
    public static Operation BeginOperation(this ILogger logger, string messageTemplate, params object[] args)
    {
        return new Operation(logger, messageTemplate, args, CompletionBehaviour.Abandon, LogLevel.Information, LogLevel.Warning);
    }

    /// <summary>
    /// Begin a new timed operation. The return value must be completed using <see cref="Operation.Complete()"/>,
    /// or disposed to record abandonment.
    /// </summary>
    /// <param name="logger">The logger through which the timing will be recorded.</param>
    /// <param name="completedLevel">The LogLevel used when the operation is completed.</param>
    /// <param name="abandonedLevel">The LogLevel used when the operation is not completed (abandoned)</param>
    /// <param name="messageTemplate">A log message describing the operation, in message template format.</param>
    /// <param name="args">Arguments to the log message. These will be stored and captured only when the
    /// operation completes, so do not pass arguments that are mutated during the operation.</param>
    /// <returns>An <see cref="Operation"/> object.</returns>
    public static Operation BeginOperation(this ILogger logger, LogLevel completedLevel, LogLevel abandonedLevel, string messageTemplate, params object[] args)
    {
        return new Operation(logger, messageTemplate, args, CompletionBehaviour.Abandon, completedLevel, abandonedLevel);
    }
}
