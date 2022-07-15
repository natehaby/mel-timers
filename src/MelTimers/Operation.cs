// Copyright 2022 MelTimers Contributors
// Original Serilog-Timings code Copyright 2016 SerilogTimings Contributors
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
using System.Diagnostics;

namespace MelTimers;

/// <summary>
/// Records operation timings to the Serilog log.
/// </summary>
/// <remarks>
/// Static members on this class are thread-safe. Instances
/// of <see cref="Operation"/> are designed for use on a single thread only.
/// </remarks>
public class Operation : IDisposable
{
    /// <summary>
    /// Property names attached to events by <see cref="Operation"/>s.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public enum Properties
    {
        /// <summary>
        /// The timing, in milliseconds.
        /// </summary>
        Elapsed,

        /// <summary>
        /// Completion status, either <em>completed</em> or <em>discarded</em>.
        /// </summary>
        Outcome,

        /// <summary>
        /// A unique identifier added to the log context during
        /// the operation.
        /// </summary>
        OperationId
    };

    const string OutcomeCompleted = "completed", OutcomeAbandoned = "abandoned";
    static readonly double StopwatchToTimeSpanTicks = (double)Stopwatch.Frequency / TimeSpan.TicksPerSecond;

    ILogger _target;
    readonly string _messageTemplate;
    readonly object[] _args;
    readonly long _start;
    long? _stop;

    CompletionBehaviour _completionBehaviour;
    readonly LogLevel _completionLevel;
    readonly LogLevel _abandonmentLevel;
    readonly TimeSpan? _warningThreshold;
    Exception? _exception;

    internal Operation(ILogger target, string messageTemplate, object[] args,
        CompletionBehaviour completionBehaviour, LogLevel completionLevel, LogLevel abandonmentLevel,
        TimeSpan? warningThreshold = null)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));
        _messageTemplate = messageTemplate ?? throw new ArgumentNullException(nameof(messageTemplate));
        _args = args ?? throw new ArgumentNullException(nameof(args));
        _completionBehaviour = completionBehaviour;
        _completionLevel = completionLevel;
        _abandonmentLevel = abandonmentLevel;
        _warningThreshold = warningThreshold;
        _start = GetTimestamp();
    }

    static long GetTimestamp()
    {
        return unchecked((long)(Stopwatch.GetTimestamp() / StopwatchToTimeSpanTicks));
    }

    /// <summary>
    /// Returns the elapsed time of the operation. This will update during the operation, and be frozen once the
    /// operation is completed or canceled.
    /// </summary>
    public TimeSpan Elapsed
    {
        get
        {
            var stop = _stop ?? GetTimestamp();
            var elapsedTicks = stop - _start;

            if (elapsedTicks < 0)
            {
                // When measuring small time periods the StopWatch.Elapsed*  properties can return negative values.
                // This is due to bugs in the basic input/output system (BIOS) or the hardware abstraction layer
                // (HAL) on machines with variable-speed CPUs (e.g. Intel SpeedStep).
                return TimeSpan.Zero;
            }

            return TimeSpan.FromTicks(elapsedTicks);
        }
    }

    /// <summary>
    /// Complete the timed operation. This will write the event and elapsed time to the log.
    /// </summary>
    public void Complete()
    {
        if (_completionBehaviour == CompletionBehaviour.Silent)
            return;

        Write(_target, _completionLevel, OutcomeCompleted);
    }

    /// <summary>
    /// Complete the timed operation with an included result value.
    /// </summary>
    /// <param name="result">The result value.</param>
    /// <param name="resultPropertyName">The name for the property to attach to the event. Defaults to "result".</param>
    /// <remarks>
    /// If you are using a logger instance that handles object destructuring you may use that system's destructuring indicator. 
    /// For example, if you are using Serilog, you can use the <c>{@}</c> indicator with <paramref name="resultPropertyName"/>.
    /// </remarks>
    public void Complete(object result, string resultPropertyName = "result")
    {
        if (resultPropertyName == null) throw new ArgumentNullException(nameof(resultPropertyName));

        if (_completionBehaviour == CompletionBehaviour.Silent)
            return;

        Write(_target, _completionLevel, OutcomeCompleted, resultPropertyName, result);
    }

    /// <summary>
    /// Abandon the timed operation. This will write the event and elapsed time to the log.
    /// </summary>
    public void Abandon()
    {
        if (_completionBehaviour == CompletionBehaviour.Silent)
            return;

        Write(_target, _abandonmentLevel, OutcomeAbandoned);
    }


    /// <summary>
    /// Abandon the timed operation with an included abandonment reason.
    /// </summary>
    /// <param name="reason">Reason for abandonment.</param>
    /// <param name="reasonPropertyName">Optional field name for abandonment reason. Defaults to "reason".</param>
    public void Abandon(string reason, string reasonPropertyName = "reason")
    {
        if (reasonPropertyName == null) throw new ArgumentNullException(nameof(reasonPropertyName));

        if (_completionBehaviour == CompletionBehaviour.Silent)
            return;

        Write(_target, _abandonmentLevel, OutcomeAbandoned, reason, reasonPropertyName);
    }

    /// <summary>
    /// Cancel the timed operation. After calling, no event will be recorded either through
    /// completion or disposal.
    /// </summary>
    public void Cancel()
    {
        _completionBehaviour = CompletionBehaviour.Silent;
    }

    /// <summary>
    /// Dispose the operation. If not already completed or canceled, an event will be written
    /// with timing information. Operations started with TimeOperation will be completed through
    /// disposal. Operations started with BeginOperation will be recorded as abandoned.
    /// </summary>
    public void Dispose()
    {
        switch (_completionBehaviour)
        {
            case CompletionBehaviour.Silent:
                break;

            case CompletionBehaviour.Abandon:
                Write(_target, _abandonmentLevel, OutcomeAbandoned);
                break;

            case CompletionBehaviour.Complete:
                Write(_target, _completionLevel, OutcomeCompleted);
                break;

            default:
                throw new InvalidOperationException("Unknown underlying state value");
        }

        //PopLogContext();
    }

    void StopTiming()
    {
        _stop ??= GetTimestamp();
    }

    void Write(ILogger target, LogLevel level, string outcome)
    {
        StopTiming();
        _completionBehaviour = CompletionBehaviour.Silent;

        var elapsed = Elapsed.TotalMilliseconds;

        level = elapsed > _warningThreshold?.TotalMilliseconds && level < LogLevel.Warning
            ? LogLevel.Warning
            : level;

        target.Log(level, _exception, $"{_messageTemplate} {{{nameof(Properties.Outcome)}}} in {{{nameof(Properties.Elapsed)}:0.0}} ms", _args.Concat(new object[] { outcome, elapsed }).ToArray());

    }

    private void Write(ILogger target, LogLevel level, string outcome, string resultPropertyName, object result)
    {
        _completionBehaviour = CompletionBehaviour.Silent;

        var elapsed = Elapsed.TotalMilliseconds;

        target.Log(level, _exception, $"{_messageTemplate} {{{nameof(Properties.Outcome)}}} with result of {{{resultPropertyName}}} in {{{nameof(Properties.Elapsed)}:0.0}} ms.", _args.Concat(new object[] { outcome, result, elapsed }).ToArray());

    }
    
    private void Write(ILogger target, LogLevel level, string outcome, string reason, string reasonPropertyName = "reason")
    {
        _completionBehaviour = CompletionBehaviour.Silent;

        var elapsed = Elapsed.TotalMilliseconds;

        target.Log(level, _exception, $"{_messageTemplate} {{{nameof(Properties.Outcome)}}} for {{{reasonPropertyName}}} in {{{nameof(Properties.Elapsed)}:0.0}} ms.", _args.Concat(new object[] { outcome, reason, elapsed }).ToArray());

    }

    /// <summary>
    /// Enriches resulting log event with the given exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <returns>Same <see cref="Operation"/>.</returns>
    public Operation SetException(Exception exception)
    {
        _exception = exception;
        return this;
    }
}
