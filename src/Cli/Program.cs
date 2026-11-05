using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.Abstractions;
using Core.Services;
using Core.Storage;
using Core.Events;
using Core.Reports;
using Core.Domain;

// Токен скасування та перехоплення натискання Ctrl+C
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => 
{ 
    e.Cancel = true; 
    cts.Cancel(); 
};

// 1. Ініціалізація сховища
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

ICatalogStore store = useFile 
    ? new FileCatalogStore(dataPath) 
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);
Console.WriteLine($"=== Сховище: {store.GetType().Name} ===\n");

// Обробка команд із аргументів
var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

var items = service.All();

switch (command)
{
    case "import":
        try
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, timeoutCts.Token);

            var importer = new JsonCatalogImporter();

            var progress = new Progress<int>(p => 
            {
                Console.Write($"\rПрогрес: {p}%   ");
            });

            var added = await service.ImportAsync(importer, dataPath, progress, linkedCts.Token);
            
            // Чистий відступ, щоб підсумок не налізав на відсотки
            Console.WriteLine();
            Console.WriteLine($"Імпортовано записів: {added}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\nІмпорт скасовано (через Ctrl+C або тайм-аут).");
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"\nДані відхилено: {ex.Message}");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"\nФайл не знайдено: {dataPath}");
        }
        break;

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
        Console.WriteLine("Доступні команди: import | report-top | report-low | report-groups | report-sum");
        break;
}

static void Print(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
        Console.WriteLine($" {l.Sku,-10} {l.Name,-22} {l.Quantity,6} {l.Unit}");
}