using System.Text.Json;
using EasySave.EasyLog.Factories;
using EasySave.EasyLog.Interfaces;
using EasySave.EasyLog.Loggers;
using EasySave.EasyLog.Options;
using EasySave.EasyLog.Serialization;
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
    public void DailyLogger_WritesToExpectedFile()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var serializer = new FakeSerializer();
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
    }

    public sealed class SampleEntry
    {
        public string Message { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    private sealed class FakeSerializer : ILogSerializer
    {
        public string FileExtension => "json";

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
}
