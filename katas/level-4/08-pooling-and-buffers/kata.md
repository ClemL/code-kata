---
title: Pooling & Buffers
level: 4
minutes: 35
summary: Rent and return buffers with ArrayPool, build strings with string.Create, write UTF-8 into IBufferWriter, and scan text with vectorized SearchValues.
topics: [Performance, Memory, Pooling, Text encoding]
keywords: [try, finally, static, u8]
apis: ["ArrayPool<T>", string.Create, "IBufferWriter<T>", "ArrayBufferWriter<T>", "SearchValues<T>", CollectionsMarshal.AsSpan]
docs:
  - title: ArrayPool<T>
    url: https://learn.microsoft.com/dotnet/api/system.buffers.arraypool-1
  - title: String.Create
    url: https://learn.microsoft.com/dotnet/api/system.string.create
  - title: SearchValues<T>
    url: https://learn.microsoft.com/dotnet/api/system.buffers.searchvalues-1
  - title: IBufferWriter<T>
    url: https://learn.microsoft.com/dotnet/api/system.buffers.ibufferwriter-1
---

## Brief

Spans let you work on memory without copying; this kata is about where that memory comes from. `ArrayPool<T>` recycles large buffers instead of allocating new ones, `string.Create` writes directly into a new string's memory, `IBufferWriter<T>` lets a producer write into a buffer owned by the consumer (the model `System.Text.Json` and Kestrel use), and `SearchValues<T>` (.NET 8) precomputes a vectorized search for a set of characters.

## Tasks

Implement the static methods on `BufferKata`:

1. **`Adler32(Stream stream, ArrayPool<byte> pool, int bufferSize = 4096)`** computes the Adler-32 checksum of a stream, reading through **one** buffer rented from `pool`. The buffer must be returned even when the stream throws. Rented arrays can be larger than requested and may contain garbage from previous use. Adler-32: start with `a = 1, b = 0`; for each byte `a = (a + byte) % 65521; b = (b + a) % 65521`; the result is `(b << 16) | a`.
2. **`FormatClaimId(char prefix, int year, int sequence)`** returns `"{prefix}-{year}-{sequence:D6}"` (`'R', 2026, 42` → `"R-2026-000042"`) using `string.Create` with a `static` lambda. Years outside `1000..9999` and sequences outside `0..999999` throw `ArgumentOutOfRangeException`.
3. **`WriteUtf8Lines(IBufferWriter<byte> writer, IEnumerable<string> lines)`** encodes each line as UTF-8 directly into `writer`, followed by `\n`.
4. **`EscapeHtml(string text)`** escapes `<`, `>`, `&`, `"`, and `'` (as `&lt;`, `&gt;`, `&amp;`, `&quot;`, `&#39;`). Find the first special character with a `static readonly SearchValues<char>`. When there is none, return the **same string instance** (no allocation).
5. **`Sum(List<int> values)`** sums a list as a `long` by iterating its backing array through `CollectionsMarshal.AsSpan`.

## Hints

- `var buffer = pool.Rent(bufferSize); try { ... } finally { pool.Return(buffer); }`
- Read with `stream.Read(buffer, 0, bufferSize)` and only process `buffer.AsSpan(0, read)`.
- `string.Create(13, (prefix, year, sequence), static (span, state) => { ... })`. Inside, use `state.year.TryFormat(span[2..], out _, default, CultureInfo.InvariantCulture)` and the `"D6"` format.
- `Encoding.UTF8.GetBytes(line, writer)` is an extension in `System.Text.EncodingExtensions`; `writer.Write("\n"u8)` writes a UTF-8 literal.
- `private static readonly SearchValues<char> Special = SearchValues.Create("<>&\"'");` then `text.AsSpan().IndexOfAny(Special)`.
- Copy the clean prefix once, then switch on each remaining character.

## Notes

- **Always return what you rent, exactly once.** Returning twice lets two callers share a buffer; never returning just degrades the pool to plain allocation. `try`/`finally` is the minimum discipline.
- **Rented buffers are dirty.** `ArrayPool<T>.Shared` does not clear arrays by default. Never read beyond what you wrote, and pass `clearArray: true` when the buffer held sensitive data.
- **`string.Create` vs interpolation.** Since C# 10, `$"..."` uses `DefaultInterpolatedStringHandler`, which also allocates only the final string. `string.Create` still wins for custom formatting logic or when the length is known up front.
- **`SearchValues<T>`** chooses an algorithm (bitmaps, vectorized `IndexOfAny`, Aho-Corasick for `SearchValues<string>` in .NET 9) when created. Create it once and keep it in a static field.
- **`CollectionsMarshal.AsSpan(list)`** exposes the list's internal array; adding or removing items while you hold the span is undefined behavior.
