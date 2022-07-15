# MelTimers

MelTimers is an extension for *Microsoft.Extensions.Logging* that allows for timing operations. 
It is based on the most awesome _Serilog-Timings_ available at .....  


MelTimers is built with some specific requirements in mind:

 * One operation produces exactly one log event (events are raised at the completion of an operation)
 * Natural and fully-templated messages
 * Events for a single operation have a single event type, across both success and failure cases (only the logging level and `Outcome` properties change)

This keeps noise in the log to a minimum, and makes it easy to extract and manipulate timing 
information on a per-operation basis.

### Installation

This library is not yet available on NuGet.

### Getting started

Types are in the `MelTimers` namespace.

```csharp
using MelTimers;
```

The simplest use case is to time an operation, without explicitly recording success/failure:

```csharp
using (Operation.TimeOperation("Submitting payment for {OrderId}", order.Id))
{
    // Timed block of code goes here
}
```

At the completion of the `using` block, a message will be written to the log like:

```
[INF] Submitting payment for order-12345 completed in 456.7 ms
```

The operation description passed to `TimeOperation()` is a message template; the event written to the log
extends it with `" {Outcome} in {Elapsed} ms"`.

 * All events raised by MelTimers carry an `Elapsed` property in milliseconds
 * `Outcome` will always be `"completed"` when the `TimeOperation()` method is used

All of the properties from the description, plus the outcome and timing, will be recorded as
first-class properties on the log event.

Operations that can either _succeed or fail_, or _that produce a result_, can be created with
`Operation.BeginOperation()`:

```csharp
using (var op = Operation.BeginOperation("Retrieving orders for {CustomerId}", customer.Id))
{
	// Timed block of code goes here

	op.Complete();
}
```

Using `op.Complete()` will produce the same kind of result as in the first example:

```
[INF] Retrieving orders for customer-67890 completed in 7.8 ms
```

Additional methods on `Operation` allow more detailed results to be captured:

```csharp
    op.Complete(orders.Rows.Length, "Rows");
```

This will not change the text of the log message, but the property `Rows` will be attached to it for
later filtering and analysis.

If the operation is not completed by calling `Complete()`, it is assumed to have failed and a
warning-level event will be written to the log instead:

```
[WRN] Retrieving orders for customer-67890 abandoned in 1234.5 ms
```

In this case the `Outcome` property will be `"abandoned"`.

To suppress this message, for example when an operation turns out to be inapplicable, use
`op.Cancel()`. Once `Cancel()` has been called, no event will be written by the operation on
either completion or abandonment.

An operation can be manually abandoned with `op.Abandon()`. Optionally, a reason for abandonment 
can be provided.

```csharp
	op.Abandon("Order not found", "abandonReason");
```

### Caveats

One important usage note: because the event is not written until the completion of the `using` block
(or call to `Complete()`), arguments to `BeginOperation()` or `TimeOperation()` are not captured until then; don't
pass parameters to these methods that mutate during the operation.

### How does this relate to Serilog-Timings?

[Serilog-Timings](https://github.com/nblumhardt/serilog-timings) is the project which MelTimers is based.
MelTimers was created to provide similar functionality to Serilog-Timings, but without the dependency on Serilog.
MelTimers can still be used with Serilog via Microsoft.Extensions.Logging.ILogger and supports structured logging 
and object destructing using the standard serilog format. MelTimers does not support Serilogs Context and 
Enrichment options. If you are using Serilog and require these options, be sure to check out Serilog-Timings. 