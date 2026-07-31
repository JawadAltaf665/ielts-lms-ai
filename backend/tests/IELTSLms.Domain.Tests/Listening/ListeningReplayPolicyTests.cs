using IELTSLms.Domain.Listening;

namespace IELTSLms.Domain.Tests.Listening;

public sealed class ListeningReplayPolicyTests
{
    [Fact]
    public void Exam_mode_allows_zero_replays()
    {
        Assert.Equal(0, ListeningReplayPolicy.AllowedReplays(ListeningMode.Exam));
    }

    [Fact]
    public void Learning_mode_is_unlimited()
    {
        Assert.Equal(int.MaxValue, ListeningReplayPolicy.AllowedReplays(ListeningMode.Learning));
    }
}
