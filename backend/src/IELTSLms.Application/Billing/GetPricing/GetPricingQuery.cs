using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Billing.GetPlans;

namespace IELTSLms.Application.Billing.GetPricing;

/// <summary>Flat list of all active prices across plans — the pricing engine's public read model (13 §4).</summary>
public sealed record GetPricingQuery : IQuery<IReadOnlyList<PriceResponse>>;
