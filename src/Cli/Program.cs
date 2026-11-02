using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Core;
using Core.Abstractions;
using Core.Services;
using Core.Storage;
using Core.Events;
using Core.Reports; // Обов'язково для звітів (Lab 7)
using Core.Domain;  // Обов'язково для Product

// 1. Твоя оригінальна ініціалізація сховища (без змін)
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

ICatalogStore store = useFile 
    ? new FileCatalogStore(dataPath) 
    : new InMemoryCatalogStore(SampleData.Products()); // Використовуємо твою правильну назву

var service = new CatalogService(store);
Console.WriteLine($"=== Сховище: {store.GetType().Name} ===\n");

var items = service.All(); // Отримуємо всі товари для звітів

// ==========================================
// Демонстрація відкладеного виконання LINQ
// ==========================================
Console.WriteLine("=== Демо відкладеного виконання ===");
var source = items.ToList();
var query = source.Where(p => p.Quantity < 10); // Запит створено, але не виконано
Console.WriteLine($"Дефіцитних до змін: {query.Count()}");

// Додаємо тимчасовий товар прямо в локальний список для перевірки
source.Add(Product.Create("tmp001", "SKU-999", "Тестовий товар", "шт", 3));
Console.WriteLine($"Дефіцитних після змін: {query.Count()}"); // Число зміниться, бо LINQ перерахує список

var snapshot = query.ToList(); // Фіксуємо результат
source.Clear();
Console.WriteLine($"У знімку лишилось: {snapshot.Count}\n"); 
// ==========================================

// Обробка команд для звітів
var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

switch (command)
{
    case "report-top":
        Print(ReportService.TopByQuantity(items, 5));
        break;

    case "report-low":
        Print(ReportService.LowStock(items, 10));
        break;

    case "report-groups":
        foreach (var g in ReportService.ByUnit(items))
            Console.WriteLine($" {g.Key,-6} позицій: {g.Count,3}  сумарно: {g.TotalQuantity,6}");
        break;

    case "report-sum":
        var t = ReportService.Totals(items);
        Console.WriteLine($"позицій {t.Items}, всього {t.Total}, середнє {t.Average:F2}, макс {t.Max}");
        break;

    default:
        Console.WriteLine("Доступні команди: report-top | report-low | report-groups | report-sum");
        break;
}

// Допоміжний метод для гарного виведення в консоль
static void Print(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
        Console.WriteLine($" {l.Sku,-10} {l.Name,-22} {l.Quantity,6} {l.Unit}");
}