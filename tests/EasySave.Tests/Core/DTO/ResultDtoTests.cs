using EasySave.Core.DTO;

namespace EasySave.Tests.Core.DTO;

public class ResultDtoTests
{
    [Fact]
    public void Ok_ShouldReturnSuccessTrue()
    {
        var res = ResultDto.Ok("done");

        Assert.True(res.Success);
        Assert.Equal("done", res.Message);
        Assert.Null(res.ErrorCode);
    }

    [Fact]
    public void Ok_ShouldAllowEmptyMessage()
    {
        var res = ResultDto.Ok();

        Assert.True(res.Success);
        Assert.Equal(string.Empty, res.Message);
        Assert.Null(res.ErrorCode);
    }

    [Fact]
    public void Fail_ShouldReturnSuccessFalse_AndSetFields()
    {
        var res = ResultDto.Fail("boom", "E001");

        Assert.False(res.Success);
        Assert.Equal("boom", res.Message);
        Assert.Equal("E001", res.ErrorCode);
    }

    [Fact]
    public void Fail_ShouldAllowNullErrorCode()
    {
        var res = ResultDto.Fail("boom");

        Assert.False(res.Success);
        Assert.Equal("boom", res.Message);
        Assert.Null(res.ErrorCode);
    }
}