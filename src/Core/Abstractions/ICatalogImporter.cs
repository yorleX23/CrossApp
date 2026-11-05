using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Core.Domain;

namespace Core.Abstractions;

public interface ICatalogImporter
{
    Task<IReadOnlyList<Product>> LoadAsync(
        string path, 
        IProgress<int>? progress = null, 
        CancellationToken ct = default);

    // Додаткове 1: Потоковий імпорт
    IAsyncEnumerable<Product> LoadStreamAsync(
        string path, 
        CancellationToken ct = default);
}