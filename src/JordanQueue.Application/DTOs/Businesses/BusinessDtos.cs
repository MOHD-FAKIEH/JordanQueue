using JordanQueue.Domain.Enums;

namespace JordanQueue.Application.DTOs.Businesses;

public record BusinessDto(
    Guid Id,
    string NameArabic,
    string NameEnglish,
    string DescriptionArabic,
    string DescriptionEnglish,
    string PhoneNumber,
    string AddressArabic,
    string AddressEnglish,
    BusinessCategory Category,
    bool IsActive);

public record BusinessDetailDto(
    Guid Id,
    string NameArabic,
    string NameEnglish,
    string DescriptionArabic,
    string DescriptionEnglish,
    string PhoneNumber,
    string AddressArabic,
    string AddressEnglish,
    BusinessCategory Category,
    bool IsActive,
    IReadOnlyList<WorkingHoursDto> WorkingHours,
    IReadOnlyList<ServiceSummaryDto> Services,
    QueueSummaryDto? CurrentQueue);

public record WorkingHoursDto(DayOfWeek DayOfWeek, TimeOnly OpeningTime, TimeOnly ClosingTime, bool IsClosed);

public record ServiceSummaryDto(Guid Id, string NameArabic, string NameEnglish, int AverageServiceMinutes, bool IsActive);

public record QueueSummaryDto(Guid QueueId, string? NowServing, int WaitingCount, int EstimatedWaitMinutes, QueueStatus Status);

public record CreateBusinessRequest(
    string NameArabic,
    string NameEnglish,
    string DescriptionArabic,
    string DescriptionEnglish,
    string PhoneNumber,
    string AddressArabic,
    string AddressEnglish,
    BusinessCategory Category);

public record UpdateBusinessRequest(
    string NameArabic,
    string NameEnglish,
    string DescriptionArabic,
    string DescriptionEnglish,
    string PhoneNumber,
    string AddressArabic,
    string AddressEnglish,
    BusinessCategory Category,
    bool IsActive);

public record BusinessSearchRequest(string? Search, BusinessCategory? Category, int Page = 1, int PageSize = 20);
