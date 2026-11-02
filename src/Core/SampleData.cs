using System.Collections.Generic;
using Core.Domain;


namespace Core;

public static class SampleData
{
    public static IEnumerable<Product> Products()
    {
        return new List<Product>
        {
            Product.Create("111", "SKU-001", "Роутер", "шт", 5), // Дефіцит
            Product.Create("112", "SKU-002", "Кабель UTP", "м", 100),
            Product.Create("113", "SKU-003", "Комутатор", "шт", 15),
            Product.Create("114", "SKU-004", "Конектор", "шт", 120),
            Product.Create("115", "SKU-005", "Стяжки", "шт", 200),
            Product.Create("116", "SKU-006", "Кабель живлення", "м", 8), // Дефіцит
            Product.Create("117", "SKU-007", "Серверна шафа", "шт", 2), // Дефіцит
            Product.Create("118", "SKU-008", "Патч-корд", "шт", 50),
            Product.Create("119", "SKU-009", "Антена", "шт", 12),
            Product.Create("120", "SKU-010", "Оптичний кабель", "м", 300),
            Product.Create("121", "SKU-011", "Блок живлення", "шт", 25),
            Product.Create("122", "SKU-012", "Кабель HDMI", "м", 45),
            Product.Create("123", "SKU-013", "Мишка", "шт", 80),
            Product.Create("124", "SKU-014", "Клавіатура", "шт", 60),
            Product.Create("125", "SKU-015", "Килимок", "шт", 150),
            Product.Create("126", "SKU-016", "Монітор", "шт", 10),
            Product.Create("127", "SKU-017", "Кабель USB", "м", 75),
        };
    }
}