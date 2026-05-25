using Argon.Application.Counterparties.Common;

namespace Argon.Application.CounterpartyIdentifiers.Resolve;

[UsedImplicitly]
public class CounterpartyIdentifiersResolveHandler(
  ICounterpartyResolver resolver
) : IRequestHandler<CounterpartyIdentifiersResolveRequest, List<CounterpartyIdentifiersResolveResponse>>
{
  public async Task<List<CounterpartyIdentifiersResolveResponse>> Handle(
    CounterpartyIdentifiersResolveRequest request, CancellationToken cancellationToken)
  {
    List<CounterpartyResolution> resolutions = await resolver.ResolveAsync(request.RawText, cancellationToken);

    return resolutions
      .Select(r => new CounterpartyIdentifiersResolveResponse(r.Id, r.Name, r.MatchedByIdentifier, r.MatchedByName))
      .ToList();
  }
}
