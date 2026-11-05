using System.Collections.Generic;
using Core.Domain;

namespace Core.Tests;

public static class TestData
{
    public static List<Product> Items() => new()
    {
        Product.Create("1", "SKU-BIG", "Товар 1", "шт", 100),
        Product.Create("2", "SKU-MID", "Товар 2", "шт", 50),
        Product.Create("3", "SKU-LOW", "Товар 3", "м", 5),
        Product.Create("4", "SKU-ZERO", "Товар 4", "м", 0)
    };
}