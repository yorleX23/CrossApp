using System.Collections.Generic;
using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IEnumerable<Product> Products()
    {
        yield return Product.Create("P-001", "SKU-101", "Кабель UTP", "м", 100);
        yield return Product.Create("P-002", "SKU-102", "Роутер", "шт", 5);
    }
}