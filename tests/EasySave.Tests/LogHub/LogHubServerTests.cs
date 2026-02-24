using LogHub.Server.Contracts;
using LogHub.Server.Infrastructure.Queueing;
using LogHub.Server.Infrastructure.Storage;
using LogHub.Server.Options;
using Microsoft.Extensions.Options;

namespace EasySave.Tests.LogHub;

public sealed class LogHubServerTests
{
    [Fact]
    public async Task ChannelLogQueue_DequeuesItemsInOrder()
    {
        var queue = new ChannelLogQueue(Options.Create(new LogHubOptions
        {
            QueueCapacity = 8
        }));

        var first = new QueuedLogWrite
        {
            Extension = "json",
            SerializedEntry = "{\"a\":1}",
            TimestampUtc = new DateTime(2026, 2, 23, 10, 0, 0, DateTimeKind.Utc)
        };
        var second = new QueuedLogWrite
        {
            Extension = "xml",
            SerializedEntry = "<Entry><Value>2</Value></Entry>",
            TimestampUtc = new DateTime(2026, 2, 23, 10, 1, 0, DateTimeKind.Utc)
        };

        await queue.EnqueueAsync(first, CancellationToken.None);
        await queue.EnqueueAsync(second, CancellationToken.None);

        QueuedLogWrite d1 = await queue.DequeueAsync(CancellationToken.None);
        QueuedLogWrite d2 = await queue.DequeueAsync(CancellationToken.None);

        Assert.Equal(first.SerializedEntry, d1.SerializedEntry);
        Assert.Equal(second.SerializedEntry, d2.SerializedEntry);
    }

    [Fact]
    public async Task DailyFileLogWriter_WritesAndReadsJsonEntries()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var writer = CreateWriter(root);

            await writer.WriteAsync(new QueuedLogWrite
            {
                Extension = "json",
                SerializedEntry = "{\"Message\":\"one\"}\n",
                TimestampUtc = new DateTime(2026, 2, 23, 8, 0, 0, DateTimeKind.Utc)
            }, CancellationToken.None);

            await writer.WriteAsync(new QueuedLogWrite
            {
                Extension = ".json",
                SerializedEntry = "{\"Message\":\"two\"}\n",
                TimestampUtc = new DateTime(2026, 2, 23, 8, 1, 0, DateTimeKind.Utc)
            }, CancellationToken.None);

            IReadOnlyList<SerializedLogEntry> entries = writer.ReadEntries(maxFiles: 1, readAll: false);

            Assert.Equal(2, entries.Count);
            Assert.All(entries, e => Assert.Equal("json", e.Extension));
            Assert.Contains(entries, e => e.SerializedEntry.Contains("\"one\"", StringComparison.Ordinal));
            Assert.Contains(entries, e => e.SerializedEntry.Contains("\"two\"", StringComparison.Ordinal));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task DailyFileLogWriter_WritesAndReadsXmlEntriesAsFragments()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var writer = CreateWriter(root);

            await writer.WriteAsync(new QueuedLogWrite
            {
                Extension = "xml",
                SerializedEntry = "<SampleEntry><Message>alpha</Message></SampleEntry>\n",
                TimestampUtc = new DateTime(2026, 2, 23, 9, 0, 0, DateTimeKind.Utc)
            }, CancellationToken.None);

            await writer.WriteAsync(new QueuedLogWrite
            {
                Extension = "xml",
                SerializedEntry = "<SampleEntry><Message>beta</Message></SampleEntry>\n",
                TimestampUtc = new DateTime(2026, 2, 23, 9, 1, 0, DateTimeKind.Utc)
            }, CancellationToken.None);

            IReadOnlyList<SerializedLogEntry> entries = writer.ReadEntries(maxFiles: 1, readAll: false);
            string xmlFile = Path.Combine(root, "2026-02-23.xml");

            Assert.Equal(2, entries.Count);
            Assert.All(entries, e => Assert.Equal("xml", e.Extension));
            Assert.Contains(entries, e => e.SerializedEntry.Contains("<Message>alpha</Message>", StringComparison.Ordinal));
            Assert.Contains(entries, e => e.SerializedEntry.Contains("<Message>beta</Message>", StringComparison.Ordinal));

            string fileContent = File.ReadAllText(xmlFile);
            Assert.Contains("<logs>", fileContent, StringComparison.Ordinal);
            Assert.Contains("</logs>", fileContent, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task DailyFileLogWriter_ReadEntries_OrdersFilesByDescendingName()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var writer = CreateWriter(root);

            await writer.WriteAsync(new QueuedLogWrite
            {
                Extension = "json",
                SerializedEntry = "{\"Day\":\"older\"}\n",
                TimestampUtc = new DateTime(2026, 2, 22, 23, 0, 0, DateTimeKind.Utc)
            }, CancellationToken.None);

            await writer.WriteAsync(new QueuedLogWrite
            {
                Extension = "json",
                SerializedEntry = "{\"Day\":\"newer\"}\n",
                TimestampUtc = new DateTime(2026, 2, 23, 23, 0, 0, DateTimeKind.Utc)
            }, CancellationToken.None);

            IReadOnlyList<SerializedLogEntry> latestOnly = writer.ReadEntries(maxFiles: 1, readAll: false);

            Assert.Single(latestOnly);
            Assert.Contains("\"newer\"", latestOnly[0].SerializedEntry, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static DailyFileLogWriter CreateWriter(string root)
    {
        return new DailyFileLogWriter(Options.Create(new LogHubOptions
        {
            LogDirectory = root
        }));
    }
}
