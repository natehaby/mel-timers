using MelTimers.Example;
using Microsoft.Extensions.Logging;


// Create an instance of ILogger for our Lemonaid Stand and pass / inject it.
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Trace);  // Display all logs.
    builder.AddConsole();   // Display logs to the console.
});
var logger = loggerFactory.CreateLogger<LemonaidStand>();



LemonaidStand stand = new(logger);



stand.ShopForIngrediants();

stand.MakeLemonaid();

stand.SellLemonade();

stand.MakeSweetTea();

stand.MakeCoffee();

stand.Advertise();

stand.Cleaning();


// Results:  (Times may change slightly)
/*
info: MelTimers.Example.LemonaidStand[0]
      Shopping for 15 ingredients completed in 521.4 ms
info: MelTimers.Example.LemonaidStand[0]
      Making Lemonaid completed with result of 10 in 506.5 ms.
info: MelTimers.Example.LemonaidStand[0]
      Selling lemonade completed in 510.2 ms
warn: MelTimers.Example.LemonaidStand[0]
      Making sweet tea abandoned in 506.1 ms
trce: MelTimers.Example.LemonaidStand[0]
      Making coffee completed in 512.6 ms
warn: MelTimers.Example.LemonaidStand[0]
      Placing 20 signs abandoned for rain in 502.2 ms.
*/



