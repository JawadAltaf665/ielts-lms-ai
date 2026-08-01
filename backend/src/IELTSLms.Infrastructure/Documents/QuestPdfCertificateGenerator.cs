using IELTSLms.Abstractions.Documents;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace IELTSLms.Infrastructure.Documents;

/// <summary>
/// Renders a one-page certificate PDF via QuestPDF (sprint requirement: "this is a solved problem,
/// don't build a PDF renderer"). QuestPDF's Community license is set once via
/// <see cref="ConfigureLicense"/>, called from <c>DependencyInjection.AddInfrastructure</c>.
/// </summary>
internal sealed class QuestPdfCertificateGenerator : ICertificatePdfGenerator
{
    public static void ConfigureLicense() => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] Generate(CertificatePdfModel model) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(14));

                page.Content().Column(column =>
                {
                    column.Spacing(16);
                    column.Item().AlignCenter().Text("Certificate of Achievement").FontSize(28).Bold();
                    column.Item().AlignCenter().Text("This certifies that").FontSize(14);
                    column.Item().AlignCenter().Text(model.LearnerName).FontSize(22).Bold();
                    column.Item().AlignCenter()
                        .Text($"has achieved an overall IELTS band of {model.OverallBand:0.0}").FontSize(16);
                    column.Item().AlignCenter().Text($"Issued {model.IssuedAtUtc:yyyy-MM-dd}").FontSize(12);
                    column.Item().PaddingTop(20).AlignCenter().Text($"Verification code: {model.VerificationCode}").FontSize(11);
                });
            });
        }).GeneratePdf();
}
