namespace JordanQueue.Application.DTOs.Services;

public record ServiceDto(
    Guid Id,
    Guid BusinessId,
    string NameArabic,
    string NameEnglish,
    string DescriptionArabic,
    string DescriptionEnglish,
    int AverageServiceMinutes,
    bool IsActive);

public record CreateServiceRequest(
    string NameArabic,
    string NameEnglish,
    string DescriptionArabic,
    string DescriptionEnglish,
    int AverageServiceMinutes);

public record UpdateServiceRequest(
    string NameArabic,
    string NameEnglish,
    string DescriptionArabic,
    string DescriptionEnglish,
    int AverageServiceMinutes,
    bool IsActive);
