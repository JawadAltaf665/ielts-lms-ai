using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Tests;

/// <summary>
/// Guards the invariants of the <see cref="Result"/> primitive that the whole error-handling
/// contract depends on (Development Standards §10).
/// </summary>
public sealed class ResultTests
{
    [Fact]
    public void Success_has_no_error()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_carries_the_error()
    {
        var error = Error.Validation("VALIDATION_REQUIRED", "Field is required.");

        var result = Result.Failure(error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Success_with_value_exposes_the_value()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Accessing_value_on_failure_throws()
    {
        Result<int> result = Error.NotFound("NOT_FOUND", "Missing.");

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Error_implicitly_converts_to_failed_result()
    {
        Result<string> result = Error.Conflict("CONFLICT_VERSION_MISMATCH", "Stale.");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void Successful_result_cannot_carry_an_error()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new TestResult(isSuccess: true, Error.NullValue));
    }

    private sealed class TestResult(bool isSuccess, Error error) : Result(isSuccess, error);
}
