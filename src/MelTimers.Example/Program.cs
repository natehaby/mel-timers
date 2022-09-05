using MelTimers.Example;
using Microsoft.Extensions.Logging;


// Create an instance of ILogger for our Lemonade Stand and pass / inject it.
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Trace);  // Display all logs.
    builder.AddConsole();   // Display logs to the console.
});
var logger = loggerFactory.CreateLogger<LemonadeStand>();



LemonadeStand stand = new(logger);



stand.ShopForIngredients();

stand.MakeLemonade();

stand.SellLemonade();

stand.MakeSweetTea();

stand.MakeCoffee();

stand.Advertise();

stand.Cleaning();


// Results:  (Times may change slightly)
/*
info: MelTimers.Example.LemonadeStand[0]
      Shopping for 15 ingredients completed in 521.4 ms
info: MelTimers.Example.LemonadeStand[0]
      Making Lemonade completed with result of 10 in 506.5 ms.
info: MelTimers.Example.LemonadeStand[0]
      Selling lemonade completed in 510.2 ms
warn: MelTimers.Example.LemonadeStand[0]
      Making sweet tea abandoned in 506.1 ms
trce: MelTimers.Example.LemonadeStand[0]
      Making coffee completed in 512.6 ms
warn: MelTimers.Example.LemonadeStand[0]
      Placing 20 signs abandoned for rain in 502.2 ms.
*/



