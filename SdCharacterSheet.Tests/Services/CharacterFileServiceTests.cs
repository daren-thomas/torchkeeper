using SdCharacterSheet.Services;
using Xunit;

namespace SdCharacterSheet.Tests.Services;

[Trait("Category", "Unit")]
public class CharacterFileServiceTests
{
    // FILE-02 + FILE-03: Round-trip save/load produces identical DTO
    [Fact(Skip = "Not yet implemented — Plan 03")]
    public async Task RoundTrip_SaveLoad_NoDataLoss() { }

    // FILE-03: Saved .sdchar JSON contains Version field
    [Fact(Skip = "Not yet implemented — Plan 03")]
    public async Task Save_ContainsVersionField() { }
}
