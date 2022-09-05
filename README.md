# MelTimers

MelTimers extends `ILogger` from *Microsoft.Extensions.Logging* to provide timing operations. It can be used with the default `ILogger` providers or any 3rd party provider that supports `ILogger` such as [Serilog](https://github.com/serilog/serilog) and [NLog](https://github.com/NLog/NLog). 


MelTimers is based on [_Serilog-Timings_](https://github.com/nblumhardt/serilog-timings).


 MelTimers is built with some specific requirements in mind:
 * Keep logging providers interchangeable and their dependencies contained within the main application. Supporting class libraries only need to reference `ILogger`.
 * One operation produces exactly one log event (events are raised at the completion of an operation)
 * Natural and fully-templated messages
 * Events for a single operation have a single event type, across both success and failure cases (only the logging level and `Outcome` properties change)

This keeps noise in the log to a minimum, and makes it easy to extract and manipulate timing information on a per-operation basis.

## Installation

This library is not yet available on NuGet.

Until the library is available on NuGet, pull the code and add it to your solution.


## Getting started

Types are in the `MelTimers` namespace.

```csharp
using MelTimers;
```

The simplest use case is to time an operation, without explicitly recording success/failure:

```csharp
using (logger.TimeOperation("Submitting payment for {OrderId}", order.Id))
{
    // Timed block of code goes here
}
```

At the completion of the `using` block, a message will be written to the log like:

```
info: Submitting payment for order-12345 completed in 456.7 ms
```

The operation description passed to `TimeOperation()` is a message template; the event written to the log
extends it with `" {Outcome} in {Elapsed} ms"`.

 * All events raised by MelTimers carry an `Elapsed` property in milliseconds
 * `Outcome` will always be `"completed"` when the `TimeOperation()` method is used

All of the properties from the description, plus the outcome and timing, will be recorded as first-class properties on the log event.

Operations that can either _succeed or fail_, or _that produce a result_, can be created with
`Operation.BeginOperation()`:

```csharp
using (var op = logger.BeginOperation("Retrieving orders for {CustomerId}", customer.Id))
{
	// Timed block of code goes here

	op.Complete();
}
```

Using `op.Complete()` will produce the same kind of result as in the first example:

```
info: Retrieving orders for customer-67890 completed in 7.8 ms
```

Additional methods on `Operation` allow more detailed results to be captured:

```csharp
op.Complete(orders.Rows.Length, "Rows");
```

This will not change the text of the log message, but the property `Rows` will be attached to it for later filtering and analysis.

If the operation is not completed by calling `Complete()`, it is assumed to have failed and a warning-level event will be written to the log instead:

```
warn: Retrieving orders for customer-67890 abandoned in 1234.5 ms
```

In this case the `Outcome` property will be `"abandoned"`.

To suppress this message, for example when an operation turns out to be inapplicable, use `op.Cancel()`. Once `Cancel()` has been called, no event will be written by the operation on either completion or abandonment.

An operation can be manually abandoned with `op.Abandon()`. Optionally, a reason for abandonment can be provided.

```csharp
op.Abandon("Order not found", "abandonReason");
```

which will write the following to the log:

```
warn: Retrieving orders for customer-67890 abandoned for Order not found in 1234.5 ms
```

## Changing Log Level
You can change the default log levels for completion and abandonment:

```csharp
using (var op = logger.BeginOperation(LogLevel.Debug, LogLevel.Warning, "Retrieving orders for {CustomerId}", customer.Id))
{
	// Code...
	op.Complete(); 
}
```

## Serilog / NLog
When using _Serilog_ or _NLog_, object deconstruction is available for any field with the `@` prefix, the same as using those loggers directly.

```csharp
using (var op = logger.BeginOperation("Retrieving last order for {CustomerId}", customer.Id))
{
	// Code...
	op.Complete(order, "@order");
}
```

## Caveats

One important usage note: because the event is not written until the completion of the `using` block
(or call to `Complete()`), arguments to `BeginOperation()` or `TimeOperation()` are not captured until then; don't
pass parameters to these methods that mutate during the operation.

## How does this relate to Serilog-Timings?

MelTimers is based on [Serilog-Timings](https://github.com/nblumhardt/serilog-timings). MelTimers was created to provide similar functionality, but without the dependency on Serilog.
MelTimers does not support Serilog's Context and Enrichment options. If you are using Serilog and require these features, be sure to check out Serilog-Timings. 
