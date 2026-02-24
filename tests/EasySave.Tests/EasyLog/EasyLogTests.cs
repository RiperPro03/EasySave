using System.Text.Json;
using EasySave.EasyLog.Factories;
using EasySave.EasyLog.Interfaces;
using EasySave.EasyLog.Loggers;
using EasySave.EasyLog.Options;
using EasySave.EasyLog.Serialization;
using EasySave.EasyLog.WebSockets;
using EasySave.EasyLog.Utils;
using EasySave.EasyLog.Writers;
using JsonSerializer = EasySave.EasyLog.Serialization.JsonSerializer;

namespace EasySave.Tests.EasyLog;

public class EasyLogTests
{
    [Fact]
    public void DailyFileHelper_UsesExpectedName()
    {
        string path = DailyFileHelper.GetLogFilePath("logs", "json", new DateTime(2026, 2, 5));
        string expected = Path.Combine("logs", "2026-02-05.json");

        Assert.Equal(expected, path);
    }

    [Fact]
    public void JsonSerializer_WritesNdjsonLine()
    {
        var serializer = new JsonSerializer();

        string text = serializer.Serialize(new SampleEntry { Message = "hi", Count = 1 });

        Assert.EndsWith(Environment.NewLine, text);
        JsonDocument.Parse(text.TrimEnd());
    }

    [Fact]
    public void XmlSerializer_WritesXmlFragmentLine()
    {
        var serializer = new XmlSerializer();

        string text = serializer.Serialize(new SampleEntry { Message = "hi", Count = 1 });

        Assert.EndsWith(Environment.NewLine, text);
        Assert.False(text.StartsWith("<?xml", StringComparison.Ordinal));
        Assert.Contains("<SampleEntry", text, StringComparison.Ordinal);
        Assert.Contains("<Message>hi</Message>", text, StringComparison.Ordinal);
    }

    [Fact]
    public void FileLogWriter_AppendsToFile()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string filePath = Path.Combine(root, "log.txt");
        var writer = new FileLogWriter();

        writer.Write(filePath, "one\n");
        writer.Write(filePath, "two\n");

        string content = File.ReadAllText(filePath);
        Assert.Equal("one\ntwo\n", content);

        Directory.Delete(root, true);
    }

    [Fact]
    public void SafeLogger_ReturnsFalseWhenInnerThrows()
    {
        var logger = new SafeLogger<SampleEntry>(new ThrowingLogger());

        bool result = logger.Write(new SampleEntry());

        Assert.False(result);
    }

    [Fact]
    public void SafeLogger_ReturnsInnerResult()
    {
        var logger = new SafeLogger<SampleEntry>(new ResultLogger(true));

        bool result = logger.Write(new SampleEntry());

        Assert.True(result);
    }

    [Fact]
    public void LoggerFactory_ReturnsSafeLoggerByDefault()
    {
        var options = new LogOptions
        {
            LogDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
        };

        ILogger<SampleEntry> logger = LoggerFactory.Create<SampleEntry>(options);

        Assert.IsType<SafeLogger<SampleEntry>>(logger);
    }

    [Fact]
    public void LoggerFactory_CanReturnDailyLogger()
    {
        var options = new LogOptions
        {
            LogDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")),
            UseSafeLogger = false
        };

        ILogger<SampleEntry> logger = LoggerFactory.Create<SampleEntry>(options);

        Assert.IsType<DailyLogger<SampleEntry>>(logger);
    }

    [Fact]
    public void LoggerFactory_CanReturnWebSocketLogger_ForServerOnlyMode()
    {
        var options = new LogOptions
        {
            StorageMode = LogStorageMode.ServerOnly,
            UseSafeLogger = false,
            Server = new LogServerOptions
            {
                Host = "localhost",
                Port = 9696,
                WebSocketPath = "/ws/logs"
            }
        };

        ILogger<SampleEntry> logger = LoggerFactory.Create<SampleEntry>(options);

        Assert.IsType<WebSocketLogger<SampleEntry>>(logger);
    }

    [Fact]
    public void LogReaderFactory_CanCreateRemoteReader_ForServerOnlyMode()
    {
        var options = new LogOptions
        {
            StorageMode = LogStorageMode.ServerOnly,
            Server = new LogServerOptions
            {
                Host = "127.0.0.1",
                Port = 9696,
                WebSocketPath = "/ws/logs"
            }
        };

        ILogReader<SampleEntry> reader = LogReaderFactory.Create<SampleEntry>(options);

        Assert.NotNull(reader);
        Assert.Equal(string.Empty, reader.LogDirectory);
        Assert.Empty(reader.GetLogFiles());
    }

    [Fact]
    public void LogHubWebSocketClient_BuildsUriFromHostPortPath()
    {
        var client = new LogHubWebSocketClient(new LogServerOptions
        {
            Host = "10.0.0.42",
            Port = 8081,
            WebSocketPath = "custom/ws",
            UseTls = false
        });

        Uri uri = GetPrivateUri(client);

        Assert.Equal("ws://10.0.0.42:8081/custom/ws", uri.ToString());
    }

    [Fact]
    public void LogHubWebSocketClient_PrefersExplicitWebSocketUrl()
    {
        var client = new LogHubWebSocketClient(new LogServerOptions
        {
            WebSocketUrl = "wss://logs.example.com:9443/ws/logs",
            Host = "ignored-host",
            Port = 1
        });

        Uri uri = GetPrivateUri(client);

        Assert.Equal("wss://logs.example.com:9443/ws/logs", uri.ToString());
    }

    [Fact]
    public void LogReaderFactory_ReadsJsonEntries()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string filePath = Path.Combine(root, "2026-02-05.json");
        File.WriteAllText(filePath, "{\"Message\":\"hello\",\"Count\":2}\n");

        var reader = LogReaderFactory.Create<SampleEntry>(new LogOptions
        {
            LogDirectory = root
        });

        IReadOnlyList<SampleEntry> entries = reader.ReadEntriesFromFile(filePath);

        Assert.Single(entries);
        Assert.Equal("hello", entries[0].Message);
        Assert.Equal(2, entries[0].Count);

        Directory.Delete(root, true);
    }

    [Fact]
    public void LogReaderFactory_ReadsXmlEntries()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string filePath = Path.Combine(root, "2026-02-05.xml");
        File.WriteAllText(filePath, "<logs><SampleEntry><Message>hello</Message><Count>3</Count></SampleEntry></logs>");

        var reader = LogReaderFactory.Create<SampleEntry>(new LogOptions
        {
            LogDirectory = root
        });

        IReadOnlyList<SampleEntry> entries = reader.ReadEntriesFromFile(filePath);

        Assert.Single(entries);
        Assert.Equal("hello", entries[0].Message);
        Assert.Equal(3, entries[0].Count);

        Directory.Delete(root, true);
    }

    [Fact]
    public void LogReaderFactory_ThrowsWhenServerOptionsMissingInServerOnlyMode()
    {
        var options = new LogOptions
        {
            StorageMode = LogStorageMode.ServerOnly
        };

        Assert.Throws<ArgumentException>(() => LogReaderFactory.Create<SampleEntry>(options));
    }

    [Fact]
    public void LoggerFactory_ThrowsWhenServerOptionsMissingInServerOnlyMode()
    {
        var options = new LogOptions
        {
            StorageMode = LogStorageMode.ServerOnly,
            UseSafeLogger = false
        };

        Assert.Throws<ArgumentException>(() => LoggerFactory.Create<SampleEntry>(options));
    }

    [Fact]
    public void DailyLogger_WritesJsonWithoutWrapping()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var serializer = new FakeSerializer("json");
        var writer = new FakeWriter();
        var logger = new DailyLogger<SampleEntry>(
            root,
            serializer,
            writer,
            () => new DateTime(2026, 2, 5));

        logger.Write(new SampleEntry { Message = "hello" });

        string expectedPath = Path.Combine(root, "2026-02-05.json");
        Assert.Equal(expectedPath, writer.LastFilePath);

        Assert.Equal("line\n", writer.LastMessage);
        
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }

    [Fact]
    public void DailyLogger_WrapsXmlWithRoot()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var serializer = new FakeSerializer("xml");
        var writer = new FakeWriter();
        var logger = new DailyLogger<SampleEntry>(
            root,
            serializer,
            writer,
            () => new DateTime(2026, 2, 5));

        logger.Write(new SampleEntry { Message = "hello" });

        string expectedPath = Path.Combine(root, "2026-02-05.xml");
        Assert.Equal(expectedPath, writer.LastFilePath);
        
        string expectedContent = "<logs>\nline\n\n</logs>";
        Assert.Equal(expectedContent, writer.LastMessage);
        
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }

    public sealed class SampleEntry
    {
        public string Message { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    private sealed class FakeSerializer : ILogSerializer
    {
        private readonly string _extension;

        public FakeSerializer(string extension)
        {
            _extension = extension;
        }

        public string FileExtension => _extension;

        public string Serialize(object entry)
        {
            return "line\n";
        }
    }

    private sealed class FakeWriter : ILogWriter
    {
        public string? LastFilePath { get; private set; }
        public string? LastMessage { get; private set; }

        public bool Write(string filepath, string message)
        {
            LastFilePath = filepath;
            LastMessage = message;
            
            return true;
        }
    }

    private sealed class ThrowingLogger : ILogger<SampleEntry>
    {
        public bool Write(SampleEntry entry)
        {
            throw new InvalidOperationException("Boom");
        }
    }

    private sealed class ResultLogger : ILogger<SampleEntry>
    {
        private readonly bool _result;

        public ResultLogger(bool result)
        {
            this._result = result;
        }

        public bool Write(SampleEntry entry)
        {
            return _result;
        }
    }

    private static Uri GetPrivateUri(LogHubWebSocketClient client)
    {
        var field = typeof(LogHubWebSocketClient).GetField("_serverUri", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(field);

        var value = field!.GetValue(client);
        Assert.IsType<Uri>(value);
        return (Uri)value!;
    }
}
