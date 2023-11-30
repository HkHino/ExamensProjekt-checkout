using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

class Program
{
    static void Main()
    {

        Scanner scanner = new Scanner();
        CheapPriceCalculator cheapCalculator = new CheapPriceCalculator();
        ExpensivePriceCalculator expensiveCalculator = new ExpensivePriceCalculator();

        scanner.PriceCalculated += cheapCalculator.OnPriceCalculated;
        scanner.PriceCalculated += expensiveCalculator.OnPriceCalculated;

        // Simuler scanning af varer med et delay på 500 ms mellem hver vare
        scanner.ScanItem('A');
        Thread.Sleep(500);
        scanner.ScanItem('B');
        Thread.Sleep(500);
        scanner.ScanItem('B');
        Thread.Sleep(500);
        scanner.ScanItem('R'); // Multipack af 'F'
        Thread.Sleep(500);
        scanner.ScanItem('Z'); // Pantvare
        Thread.Sleep(500);
        scanner.ScanItem('C');
        Thread.Sleep(500);

        Console.WriteLine("Billig pris: " + cheapCalculator.TotalPrice);
        Console.WriteLine("Dyr pris:");
        expensiveCalculator.DisplaySoldItems();
    }
}

// Delegate til event
public delegate void PriceCalculatedEventHandler(object sender, PriceCalculatedEventArgs e);

// Event arguments til PriceCalculated event
public class PriceCalculatedEventArgs : EventArgs
{
    public decimal TotalPrice { get; set; }
}

// Klasse for at repræsentere en vare
public class Item
{
    public char Code { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int Group { get; set; }
}

// Klasse for at repræsentere en scanner
public class Scanner
{
    // Event til at udsende når en pris er blevet beregnet
    public event PriceCalculatedEventHandler PriceCalculated;

    public void ScanItem(char itemCode)
    {
        // Simuler prisberegning baseret på varekoden
        decimal price = GetItemPrice(itemCode);

        // Udsend event med den beregnede pris
        OnPriceCalculated(itemCode, new PriceCalculatedEventArgs { TotalPrice = price });
    }

    protected virtual void OnPriceCalculated(char itemCode, PriceCalculatedEventArgs e)
    {
        PriceCalculated?.Invoke(itemCode, e);
    }

    private decimal GetItemPrice(char itemCode)
    {
        // Simuler prislogik baseret på varekoden
        // Implementer din egen prislogik her
        switch (itemCode)
        {
            case 'A':
                return 10.0m;
            case 'B':
                return 5.0m;
            case 'R':
                return 25.0m; // Prisen for multipack af 'F'
            case 'Z':
                return 10.0m;
            case 'P':
                return 1.0m; // Price for the pant
            case 'C':
                return 8.0m;
            default:
                return 0.0m; // Ugyldig varekode
        }
    }
}

// Interface til en prisberegner
public interface IPriceCalculator
{
    decimal TotalPrice { get; }
    void OnPriceCalculated(object sender, PriceCalculatedEventArgs e);
}

// Klasse for billig prisberegner
public class CheapPriceCalculator : IPriceCalculator
{
    public decimal TotalPrice { get; private set; }

    public void OnPriceCalculated(object sender, PriceCalculatedEventArgs e)
    {
        // Simpelt: Adder den beregnede pris til den samlede pris
        TotalPrice += e.TotalPrice;

        // Vis totalpris hver gang
        Console.WriteLine("Billig pris opdateret: " + TotalPrice);
    }
}

// Klasse for dyr prisberegner
public class ExpensivePriceCalculator : IPriceCalculator
{
    private List<Item> soldItems = new List<Item>();

    public decimal TotalPrice
    {
        get { return soldItems.Sum(item => item.Price * item.Quantity); }
    }

    public void OnPriceCalculated(object sender, PriceCalculatedEventArgs e)
    {
        // Simpelt: Gem den solgte vare med pris og antal
        char itemCode = (char)sender; // Assuming sender is char in this case
        int groupNumber = 1;

        if (itemCode == 'Z')
        {
            groupNumber = 2;
            // Handle 'Z' as a pant item
            //soldItems.Add(new Item { Code = 'Z', Price = 10.0m, Quantity = 1, Group = 1 });
            // Also handle 'P' as the pant price
            soldItems.Add(new Item { Code = 'P', Price = 1.0m, Quantity = 1, Group = groupNumber });
        }
        else if (itemCode == 'A' || itemCode == 'B')
        {
            // Handle other items normally
            groupNumber = 3;

        }
        else { }

        soldItems.Add(new Item { Code = itemCode, Price = e.TotalPrice, Quantity = 1, Group = groupNumber });

    }

    public void DisplaySoldItems()
    {
        // Grupper og sorter solgte varer og vis dem
        var groupedItems = soldItems.GroupBy(item => item.Code)
                                     .Select(group => new
                                     {
                                         Code = group.Key,
                                         TotalQuantity = group.Sum(item => item.Quantity),
                                         TotalPrice = group.Sum(item => item.Price * item.Quantity),
                                         groupNumber = group.First().Group
                                     })
                                     .OrderBy(item => item.Code).ThenBy(item => item.groupNumber);

        foreach (var item in groupedItems)
        {
            Console.WriteLine($"Vare: {item.Code}, Antal: {item.TotalQuantity}, Pris: {item.TotalPrice}, Gruppe: {item.groupNumber}");
        }
    }
}