using System;
using System.IO;
using System.Threading.Tasks;
using EasySave.App.Services;
using EasySave.tests.Helpers.Assertions;
using Xunit;

namespace EasySave.Tests.App.Services;

public class CryptoSoftProcessServiceSemaphoreTests
{
    [Fact]
    public async Task EncryptFileAsync_ShouldReturnExitCode()
    {
        var exePath = "cmd.exe";
        var service = new CryptoSoftProcessService(exePath, "TestCryptoSoftSemaphore");

        var tempFile = Path.GetTempFileName();
        var exitCode = await service.EncryptFileAsync("/c exit 42", "key");

        Assert.Equal(42, exitCode);
    }

    [Fact]
    public void ProcessSemaphoreLock_ShouldThrow_WhenAlreadyLocked()
    {
        var semaphoreName = "TestSemaphoreForUnitTest";

        using var firstLock = new ProcessSemaphoreLock(semaphoreName, 1000);

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            using var secondLock = new ProcessSemaphoreLock(semaphoreName, 1000);
        });
        
    }
}