using System;
using Core.Domain;
using Core.Import;

Console.WriteLine("=== Сценарій 1: успіх ===");
Product product = Product.Create("P-001", "sku-001", "Цемент М400 25кг", "шт", 100);
Console.WriteLine(product);

product.RegisterArrival(50);
product.Issue(30);
Console.WriteLine(product);

Console.WriteLine("\n=== Сценарій 2: порушення інваріантів ===");
TryDo("видача більша за залишок", () => product.Issue(1000));
TryDo("порожній SKU", () => Product.Create("P-002", "", "Пісок", "т", 10));
TryDo("від'ємний залишок", () => Product.Create("P-003", "SKU-003", "Цегла", "шт", -5));

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" {title}: {ex.GetType().Name} — {ex.Message}");
    }
}

Console.WriteLine("\n=== Додаткове завдання: Зв'язок з імпортом тижня 3 ===");

// 1. Завантажуємо DTO з файлу (як у 3-й лабі)
var importResult = ProductCsvImporter.Load(Path.Combine("data", "sample.csv"));
var validEntities = new List<Product>();
var domainErrors = new List<string>();

// 2. Пробуємо перетворити кожен DTO на повноцінну доменну сутність
foreach (var dto in importResult.Items)
{
    try
    {
        // FromDto викликає Create, де прописані всі наші інваріанти
        validEntities.Add(Product.FromDto(dto));
    }
    catch (Exception ex)
    {
        // Якщо DTO мав від'ємну кількість чи пустий SKU - записуємо помилку
        domainErrors.Add($"SKU {dto.Sku}: {ex.Message}");
    }
}

// 3. Виводимо результати
Console.WriteLine($"Успішно створено доменних сутностей: {validEntities.Count}");
Console.WriteLine($"Відхилено через порушення інваріантів: {domainErrors.Count}");

foreach (var error in domainErrors)
{
    Console.WriteLine($" - {error}");
}