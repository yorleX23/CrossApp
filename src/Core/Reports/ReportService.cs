using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain; // Перевір, щоб тут був правильний namespace твого Product

namespace Core.Reports;

public static class ReportService
{
    // 1) Фільтр + сортування + проєкція
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold) =>
        source.Where(p => p.Quantity < threshold)
              .OrderBy(p => p.Quantity)
              .ThenBy(p => p.Sku, StringComparer.Ordinal)
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
              .ToList();

    // 2) Топ зі стабільним порядком
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n) =>
        source.OrderByDescending(p => p.Quantity)
              .ThenBy(p => p.Sku, StringComparer.Ordinal)
              .Take(n)
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
              .ToList();

    // 3) Групування з підрахунком і сумою
    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source) =>
        source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
              .OrderByDescending(s => s.TotalQuantity)
              .ToList();

    // 4) Агрегати з обробкою порожньої послідовності (щоб не падало на Average/Max)
    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Product> source)
    {
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0, 0d, 0)
            : (list.Count, list.Sum(p => p.Quantity), list.Average(p => p.Quantity), list.Max(p => p.Quantity));
    }
}