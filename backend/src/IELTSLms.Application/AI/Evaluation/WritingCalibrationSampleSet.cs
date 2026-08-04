namespace IELTSLms.Application.AI.Evaluation;

/// <summary>
/// One held-out, expert-scored calibration item (Testing Strategy §8's "curated, versioned calibration
/// set"): a Task 1 or Task 2 response plus the four criteria as an expert examiner scored them. Mirrors
/// <see cref="SpeakingCalibrationSample"/>'s shape.
/// </summary>
public sealed record WritingCalibrationSample(
    string TaskPrompt, string TaskType, string ResponseText, decimal ExpertTaskResponse, decimal ExpertCoherenceCohesion,
    decimal ExpertLexicalResource, decimal ExpertGrammaticalRangeAccuracy);

/// <summary>
/// ponytail: a small, developer-maintained in-code sample set rather than an admin-curated/queryable
/// store — same tradeoff as <see cref="SpeakingCalibrationSampleSet"/>, enough to prove the
/// calibration/gating mechanics end-to-end with a real statistical tolerance check. Move to a managed,
/// versioned store (with its own curation workflow) if the calibration set needs to grow past what
/// comfortably fits in a source file, or needs non-developer curation.
/// </summary>
public static class WritingCalibrationSampleSet
{
    public static IReadOnlyList<WritingCalibrationSample> Samples { get; } =
    [
        new(
            "The chart below shows the number of tourists visiting three museums between 2015 and 2020. Summarize the information.",
            "Task1",
            "The line graph illustrates visitor numbers at three museums from 2015 to 2020. Overall, the " +
            "City History Museum attracted the most visitors throughout the period, while the Science " +
            "Museum showed the sharpest rise. In 2015, the City History Museum received around 80,000 " +
            "visitors, roughly double the Art Gallery's figure. By 2020, however, the Science Museum had " +
            "overtaken the Art Gallery, climbing steadily each year to reach approximately 60,000 " +
            "visitors, whereas the Art Gallery's numbers remained largely flat throughout.",
            6.5m, 6.5m, 6.0m, 6.5m),
        new(
            "Some people think governments should spend money on public transportation rather than roads. Discuss both views and give your opinion.",
            "Task2",
            "In my opinion this is very important topic. Government must think about this carefully " +
            "because many people they use car every day and roads is very busy. Some people say bus and " +
            "train is better because it help environment, this is true I think. But other people say if " +
            "government dont build more roads then traffic will be more bad. In conclusion I think " +
            "government should spend on both but public transport is more important for future.",
            4.5m, 4.5m, 4.0m, 4.0m),
        new(
            "The table below shows household spending on different categories in four countries. Summarize the information.",
            "Task1",
            "The table presents household expenditure across four categories — food, housing, transport, " +
            "and leisure — in four countries. Housing accounts for the largest share of spending in every " +
            "country except Country C, where food dominates at 38%. Transport costs are broadly similar " +
            "across all four countries, ranging narrowly between 12% and 16%, while leisure spending " +
            "varies the most, from a low of 5% in Country A to a high of 19% in Country D. Overall, " +
            "spending priorities differ noticeably between the four countries.",
            7.0m, 7.0m, 7.0m, 6.5m),
        new(
            "Many people believe that social media has a negative effect on both individuals and society. To what extent do you agree or disagree?",
            "Task2",
            "The pervasive influence of social media on contemporary life has sparked considerable debate, " +
            "and while I acknowledge its benefits, I largely agree that its negative effects outweigh " +
            "them. On an individual level, excessive use has been linked to anxiety, disrupted sleep, and " +
            "diminished attention spans, particularly among adolescents who compare their lives " +
            "unfavourably to curated online personas. At a societal level, the rapid, unchecked spread of " +
            "misinformation undermines public discourse and can erode trust in legitimate institutions. " +
            "That said, social media also enables marginalized voices to be heard and allows communities " +
            "to organize around shared causes, benefits that should not be dismissed. On balance, however, " +
            "I believe the psychological and societal costs are significant enough that stronger " +
            "regulation and digital literacy education are warranted.",
            8.0m, 8.0m, 7.5m, 7.5m),
        new(
            "Describe a graph showing average monthly rainfall in two cities over a year.",
            "Task1",
            "Rain much in city A. City B have less rain. In summer month rain go down in both city. " +
            "Winter is more rain than summer. City A always more than city B every month. I think this " +
            "is because location different.",
            3.5m, 3.5m, 3.0m, 3.0m),
    ];
}
