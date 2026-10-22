using System;
using System.IO;
using System.Linq;
using Core;
using Core.Abstractions;
using Core.Services;
using Core.Storage;

// 1. Вибір реалізації (Composition Root)
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

ICatalogStore store = useFile 
    ? new FileCatalogStore(dataPath) 
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);

Console.WriteLine($"=== Сховище: {store.GetType().Name} ===");

// 2. Успішний сценарій
try
{
    var created = service.Add("SKU-999", "Новий товар", "шт", 10);
    Console.WriteLine($"Додано товар з ID: {created.Id}");
    
    service.Receive(created.Id, 5);
    Console.WriteLine("Оформлено прихід на 5 шт.");
}
catch (Exception ex)
{
    Console.WriteLine($"Помилка: {ex.Message}");
}

Console.WriteLine("\nПоточний каталог:");
foreach (var p in service.All())
{
    Console.WriteLine($" {p.Id} | {p.Sku,-10} | {p.Name,-15} | {p.Quantity,5} {p.Unit}");
}

// 3. Сценарій відмови (перехоплення винятку)
Console.WriteLine("\n=== Сценарій відмови ===");
try
{
    service.Receive("неіснуючий-id", 10);
}
catch (Exception ex)
{
    Console.WriteLine($"Перехоплено очікувану помилку: {ex.GetType().Name} - {ex.Message}");
}