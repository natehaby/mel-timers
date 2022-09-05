using Microsoft.Extensions.Logging;

namespace MelTimers.Example;

public class LemonadeStand
{
    private readonly ILogger<LemonadeStand> _logger;

    public LemonadeStand(ILogger<LemonadeStand> logger)
    {
        _logger = logger;
    }


    /// <summary>
    /// Demonstrates the basic use of the <see cref="BeginOperation"/> method.
    /// </summary>
    public void ShopForIngredients()
    {
        using (var op = _logger.BeginOperation("Shopping for {ingredientCount} ingredients", 15))
        {
            // Go shopping.
            Task.Delay(500).Wait();

            op.Complete();
        }
    }

    /// <summary>
    /// Demonstrates the use of the <see cref="BeginOperation"/> method with a custom completion message.
    /// </summary>
    public void MakeLemonade()
    {
        using (var op = _logger.BeginOperation("Making Lemonade"))
        {
            // Make some lemonade
            Task.Delay(500).Wait();

            var litersMade = 10;

            op.Complete(litersMade, "LitersMade");
        }
    }
    
    /// <summary>
    /// Demonstrates the use of the <see cref="TimeOperation" /> method.
    /// </summary>
    public void SellLemonade()
    {
        using (_logger.TimeOperation("Selling lemonade"))
        {
            // Sell a glass of lemonaid.
            Task.Delay(500).Wait();
        }
    }
    
    /// <summary>
    /// Demonstrates abandoning an operation before completion.
    /// </summary>
    public void MakeSweetTea()
    {
        using (var op = _logger.BeginOperation("Making sweet tea"))
        {
            // Try to make Sweet Tea
            Task.Delay(500).Wait();
            var success = false;

            if (!success)
            {
                return;
            }

            op.Complete();
        }
    }

    /// <summary>
    /// Demonstrates changing <see cref="LogLevel"/> for completion and abandonment.
    /// </summary>
    public void MakeCoffee()
    {
        using (var op = _logger.BeginOperation(LogLevel.Trace, LogLevel.Warning, "Making coffee"))
        {
            // Make coffee
            Task.Delay(500).Wait();

            op.Complete();
        }
    }

    /// <summary>
    /// Demonstrates abandoning an operation and providing a reason.
    /// </summary>
    public void Advertise()
    {
        using(var op = _logger.BeginOperation(LogLevel.Trace, LogLevel.Warning, "Placing {signCount} signs", 20))
        {
            // Put out signs.
            Task.Delay(500).Wait();

            var isRaining = true;

            if(isRaining)
            {
                op.Abandon("rain", "weather");
            }

            op.Complete();
        }
    }

    /// <summary>
    ///  Demonstrates the <c>op.Cancel</c> method.
    /// </summary>
    /// <remarks>
    ///  Cancelled operations do not create a log entry.
    /// </remarks>
    public void Cleaning()
    {
        using(var op = _logger.BeginOperation("Cleaning up"))
        {
            // Clean up...
            Task.Delay(500).Wait();

            op.Cancel();
        }
    }



    /// <summary>
    /// Demonstrates providing destructured data for structured logging systems via ILogger.
    /// </summary>
    /// <remarks>
    /// If you are using Serilog (or possibly another logging platform), you can destructure objects with @ in the field name.
    /// </remarks>
    public void DemonstrateDestructuring()
    {
        using(var op = _logger.BeginOperation("Demostrating destructured objects."))
        {
            Customer customer = new()
            {
                Name = "John",
                Email = "john@nowhere.com"
            };
            
            op.Complete(customer, "@customer");

        }
    }

    public class Customer
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
