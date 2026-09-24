using Core.Dto;
using Core.Import;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

ImportResult<ProductDto> result = ProductCsvImporter.Load(path);

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (ProductDto p in result.Items.Take(5))
{
    Console.WriteLine($" {p.Id,-6} {p.Sku,-10} {p.Name,-26} {p.Quantity,5} {p.Unit}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
    {
        Console.WriteLine($" ! {e}");
    }
}

int total = result.Items.Count + result.Errors.Count;
double errorPercent = total > 0 ? (double)result.Errors.Count / total * 100 : 0;

Console.WriteLine($"\n[Статистика]: Усього: {total} | Прийнято: {result.Items.Count} | Пропущено: {result.Errors.Count} | Помилок: {errorPercent:F1}%");

return 0;