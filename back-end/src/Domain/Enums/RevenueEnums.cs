namespace ShareFlow.Domain.Enums;

public enum RevenueImportSource
{
    Manual,
    Csv,
    AiScreenshot,
}

public enum RevenueStatus
{
    Pending,
    NeedsVerification,
    Approved,
    Rejected,
}
