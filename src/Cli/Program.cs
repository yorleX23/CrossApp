using System;
using System.IO;
using System.Linq;
using Core;
using Core.Abstractions;
using Core.Services;
using Core.Storage;
using Core.Events; // Потрібно для CatalogChangedEventArgs

// 1. Вибір реалізації (Composition Root)
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

ICatalogStore store = useFile 
    ? new FileCatalogStore(dataPath) 
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);

Console.WriteLine($"=== Сховище: {store.GetType().Name} ===\n");

// --- КРОК 3 і 4: Налаштування логування та підписки ---
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    var line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} залишок {e.QuantityAfter}";
    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}

service.Changed += OnCatalogChanged; // Підписка локальним методом

int changes = 0;
service.Changed += (_, _) => changes++; // Друга підписка через лямбду-лічильник
// -----------------------------------------------------

// 2. Успішний сценарій (Крок 5)
try
{
    var created = service.Add("SKU-999", "Новий товар", "шт", 10);
    service.Receive(created.Id, 5);
    Console.WriteLine($"\nПодій отримано: {changes}");

    Console.WriteLine("\n--- Перевірка відписки ---");
    service.Changed -= OnCatalogChanged; // Відписуємось від консолі/файлу
    service.Receive(created.Id, 2); // Тиха операція: у консоль подія не виведеться
    Console.WriteLine($"Подій отримано: {changes}"); // Але лямбда працює далі (+1)
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

// 3. Сценарій відмови (Крок 6)
Console.WriteLine("\n=== Сценарій відмови ===");
try
{
    service.Receive("неіснуючий-id", 10);
}
catch (Exception ex)
{
    Console.WriteLine($"Перехоплено очікувану помилку: {ex.GetType().Name}\n{ex.Message}");
    Console.WriteLine($"Лічильник подій не змінився: {changes}"); // Подій не було
}

// 4. Виклик Func замість події (Крок 7)
Console.WriteLine("\n=== Перевірка Func: Товари, кількість яких < 15 ===");
var lowStock = service.FindAll(p => p.Quantity < 15);
foreach (var item in lowStock)
{
    Console.WriteLine($"{item.Id} - {item.Name} ({item.Quantity} {item.Unit})");
}