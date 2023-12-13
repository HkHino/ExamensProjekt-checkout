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

        /*
        // Simuler scanning af varer med et delay på 500 ms mellem hver vare
        scanner.ScanItem('A');
        Thread.Sleep(500);
        scanner.ScanItem('A');
        Thread.Sleep(500);
        scanner.ScanItem('A');
        Thread.Sleep(500);
        scanner.ScanItem('B');
        Thread.Sleep(500);
        scanner.ScanItem('B');
        Thread.Sleep(500);
        scanner.ScanItem('B');
        Thread.Sleep(500);
        scanner.ScanItem('R'); // Multipack af 'F'
        Thread.Sleep(500);
        scanner.ScanItem('C');
        Thread.Sleep(500);
        scanner.ScanItem('Z'); // Pantvare
        Thread.Sleep(500);
        */
        
        Console.WriteLine("Scanner is ready. Type 'stop' to stop.");

        // tjek for 'exit'
        while (true)
        {
            Console.Write("Enter item code: ");
            String userInput = Console.ReadLine().Trim();
            // Tjek efter exit parameter
            if (userInput.ToLower() == "stop")
            {
                break;
            }
    
            userInput = userInput.Substring(0, 1);
            char input = char.Parse(userInput);
            input = char.ToUpper(input);
           
            scanner.ScanItem(input);

            if (input == 'Z')
            {
                scanner.ScanItem('P');
            }
                                    
        }


        Console.WriteLine("Billig pris: " + cheapCalculator.TotalPrice);
        Console.WriteLine("Dyr pris:");
        expensiveCalculator.DisplaySoldItems();
        
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
            case 'F':
                return 8.0m;
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
        private Dictionary<char, int> itemCounts = new Dictionary<char, int>();
        private List<Item> soldItems = new List<Item>();

        public decimal TotalPrice
        {
            get { return soldItems.Sum(item => item.Price * item.Quantity); }
        }

        public void OnPriceCalculated(object sender, PriceCalculatedEventArgs e)
        {
            // Simpelt: Gem den solgte vare med pris og antal
            char itemCode = (char)sender; 
            int groupNumber = 1;

            // Tjek om vi har scannet denne vare allerede
            if (itemCounts.ContainsKey(itemCode))
            {
                itemCounts[itemCode]++;
            }
            else
            {
                itemCounts[itemCode] = 1;
            }
            
            // tilføj rabat for køb ved 3 x A
            if (itemCode == 'A' && itemCounts[itemCode] == 3)
            {
                e.TotalPrice = 0m; // buy 3 get one free
                itemCounts[itemCode] = 0;
            }
                       
            if (itemCode == 'A' || itemCode == 'B')
            {
                // håndtering ikke pant ting
                groupNumber = 3;
            }            

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
            
            double calcPrice = 0;

            foreach (var item in groupedItems)
            {
                Thread.Sleep(750);
                if (item.Code == 'B')
                {
                    int numberOfSetsOfThree = item.TotalQuantity / 3;

                    double itemPrice = (double)(item.TotalPrice/item.TotalQuantity);

                    double totalPriceWithDiscount = ((numberOfSetsOfThree * 3 * itemPrice) * 0.5) + ((item.TotalQuantity % 3) * itemPrice);

                    Console.WriteLine($"Vare: {item.Code}, Antal: {item.TotalQuantity}, Pris: {totalPriceWithDiscount} 50% Discount per group of 3 items, Gruppe: {item.groupNumber}");
                    calcPrice += totalPriceWithDiscount;

                   
                }
                else if (item.Code == 'A' && item.TotalQuantity >= 3)
                {
                    Console.WriteLine($"Vare: {item.Code}, Antal: {item.TotalQuantity}, Pris: {item.TotalPrice} Buy 3, get 1 free, Gruppe: {item.groupNumber}");
                    calcPrice += (double)item.TotalPrice;
                }
                else
                {
                    Console.WriteLine($"Vare: {item.Code}, Antal: {item.TotalQuantity}, Pris: {item.TotalPrice}, Gruppe: {item.groupNumber}");
                    calcPrice += (double)item.TotalPrice;
                }
            }
            Console.WriteLine("Total price: " + calcPrice);
        } 
    }
}