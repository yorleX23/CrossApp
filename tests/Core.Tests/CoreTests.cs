using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Reports;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class CoreTests
{
    [Fact]
    // 1) Інваріант: відмова і незмінний стан
    public void Receive_WithNegativeQuantity_Throws_AndKeepsState()
    {
        var store = new InMemoryCatalogStore();
        var service = new CatalogService(store);
        var p = service.Add("SKU-1", "Кабель", "м", 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => service.Receive(p.Id, -5));
        Assert.Equal(10, store.GetById(p.Id)!.Quantity);
    }

    [Fact]
    // 2) Подія як спосіб перевірки (викликається рівно 1 раз)
    public void Add_RaisesChangedExactlyOnce()
    {
        var service = new CatalogService(new InMemoryCatalogStore());
        var events = new List<CatalogChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        service.Add("SKU-2", "Гвинт", "шт", 7);

        Assert.Single(events);
        Assert.Equal(CatalogChangeKind.Added, events[0].Kind);
    }

    [Theory]
    [InlineData(10, 2)]
    [InlineData(1, 1)]
    // 3) Звіт на кількох наборах вхідних даних
    public void LowStock_CountsOnlyBelowThreshold(int threshold, int expected)
    {
        var count = ReportService.LowStock(TestData.Items(), threshold).Count;
        Assert.Equal(expected, count);
    }

    [Fact]
    // 4) Детермінований топ-N
    public void TopByQuantity_IsDeterministic()
    {
        var top = ReportService.TopByQuantity(TestData.Items(), 2)
                               .Select(i => i.Sku);

        Assert.Equal(new[] { "SKU-BIG", "SKU-MID" }, top);
    }

    [Fact]
    // 5) Скасування операції
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var service = new CatalogService(new InMemoryCatalogStore());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.ImportAsync(new JsonCatalogImporter(), "any.json", ct: cts.Token));
    }

    [Fact]
    // 6) Тест із некоректними даними та DomainException
    public async Task ImportAsync_InvalidData_ThrowsDomainException()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "[{\"Sku\":\"BAD\",\"Name\":\"Помилка\",\"Unit\":\"шт\",\"Quantity\":-10}]");
            var service = new CatalogService(new InMemoryCatalogStore());

            await Assert.ThrowsAsync<DomainException>(() =>
                service.ImportAsync(new JsonCatalogImporter(), tempFile));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    // Додаткове 1: потоковий імпорт читає елементи через IAsyncEnumerable
    public async Task ImportStreamAsync_WorksWithAsyncEnumerable()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "[{\"Sku\":\"S1\",\"Name\":\"Товар 1\",\"Unit\":\"шт\",\"Quantity\":5}]");
            var service = new CatalogService(new InMemoryCatalogStore());

            var added = await service.ImportStreamAsync(new JsonCatalogImporter(), tempFile);
            Assert.Equal(1, added);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    // Додаткове 2: доводимо, що спрацював саме таймаут зв'язаного токена
    public async Task ImportAsync_Timeout_TriggersCancellation()
    {
        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

        await Task.Delay(15);

        var service = new CatalogService(new InMemoryCatalogStore());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.ImportAsync(new JsonCatalogImporter(), "any.json", ct: linkedCts.Token));

        Assert.True(timeoutCts.IsCancellationRequested);
        Assert.False(userCts.IsCancellationRequested);
    }
}