using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonCatalogImporter : ICatalogImporter
{
    public async Task<IReadOnlyList<Product>> LoadAsync(
        string path, 
        IProgress<int>? progress = null, 
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path, 
            FileMode.Open, 
            FileAccess.Read, 
            FileShare.Read, 
            4096, 
            useAsync: true);

        var dtos = await JsonSerializer.DeserializeAsync<List<ProductDto>>(
            stream, 
            cancellationToken: ct)
            ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");

        var result = new List<Product>(dtos.Count);
       for (var i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            
            // Додай це для демонстрації (щоб файл читався повільно):
            await Task.Delay(500, ct); 

            try
            {
                var p = Product.Create(dtos[i].Sku, dtos[i].Sku, dtos[i].Name, dtos[i].Unit, dtos[i].Quantity);
                result.Add(p);

                var percent = (int)((i + 1) / (double)dtos.Count * 100);
                progress?.Report(percent);
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex);
            }
        }

        return result;
    }

    // Додаткове 1: Потокове читання через IAsyncEnumerable
    public async IAsyncEnumerable<Product> LoadStreamAsync(
        string path, 
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path, 
            FileMode.Open, 
            FileAccess.Read, 
            FileShare.Read, 
            4096, 
            useAsync: true);

        var dtos = await JsonSerializer.DeserializeAsync<List<ProductDto>>(
            stream, 
            cancellationToken: ct)
            ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");

        for (var i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            Product p;
            try
            {
                p = Product.Create(dtos[i].Sku, dtos[i].Sku, dtos[i].Name, dtos[i].Unit, dtos[i].Quantity);
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex);
            }

            yield return p;
        }
    }
}