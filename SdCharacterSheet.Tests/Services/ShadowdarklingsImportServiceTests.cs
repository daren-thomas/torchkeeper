using SdCharacterSheet.Services;
using Xunit;

namespace SdCharacterSheet.Tests.Services;

[Trait("Category", "Unit")]
public class ShadowdarklingsImportServiceTests
{
    // FILE-01: Full import produces correct Character
    [Fact(Skip = "Not yet implemented — Plan 02")]
    public async Task Import_BrimJson_ProducesCorrectCharacter() { }

    // FILE-01: Uses rolledStats, not stats
    [Fact(Skip = "Not yet implemented — Plan 02")]
    public async Task Import_UsesRolledStats() { }

    // FILE-01: Currency reads top-level gold/silver/copper
    [Fact(Skip = "Not yet implemented — Plan 02")]
    public async Task Import_Currency_TopLevelFields() { }

    // FILE-01: Currency falls back to ledger sum
    [Fact(Skip = "Not yet implemented — Plan 02")]
    public async Task Import_Currency_LedgerFallback() { }

    // FILE-01: bonusTo "AC:..." entries go to AC contributors
    [Fact(Skip = "Not yet implemented — Plan 02")]
    public async Task Import_AcBonuses() { }

    // FILE-01: Unknown JSON fields are silently ignored
    [Fact(Skip = "Not yet implemented — Plan 02")]
    public async Task Import_UnknownFields_Ignored() { }

    // FILE-01: currentHP = maxHP on import
    [Fact(Skip = "Not yet implemented — Plan 02")]
    public async Task Import_CurrentHP_EqualsMaxHP() { }
}
