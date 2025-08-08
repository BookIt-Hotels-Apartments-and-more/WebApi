using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Configuration.Settings;
using BookIt.DAL.Enums;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;
using GenerativeAI;
using Microsoft.Extensions.Options;
using System.Text;

namespace BookIt.BLL.Services;

public class ClassificationService : IClassificationService
{
    private readonly GenerativeModel _geminiModel;
    private readonly GeminiAISettings _geminiSettings;
    private readonly EstablishmentsRepository _establishmentsRepository;

    public ClassificationService(
        IOptions<GeminiAISettings> geminiAiOptions,
        EstablishmentsRepository establishmentsRepository)
    {
        try
        {
            _geminiSettings = geminiAiOptions?.Value ?? throw new ArgumentNullException(nameof(geminiAiOptions));
            _establishmentsRepository = establishmentsRepository ?? throw new ArgumentNullException(nameof(establishmentsRepository));

            ValidateGeminiConfiguration(_geminiSettings);

            _geminiModel = new GenerativeModel(_geminiSettings.ApiKey, _geminiSettings.Model);
        }
        catch (Exception ex) when (!(ex is BookItBaseException))
        {
            throw new ExternalServiceException("Gemini AI", "Failed to initialize classification service", ex);
        }
    }

    public async Task<VibeType?> ClassifyEstablishmentVibeAsync(EstablishmentDTO establishment)
    {
        try
        {
            if (establishment is null)
                throw new ValidationException("Establishment", "Establishment data cannot be null");

            ValidateEstablishmentForClassification(establishment);

            string prompt = BuildVibeClassificationPrompt(establishment);

            return await CallGeminiForClassificationAsync(prompt);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Classification", "Failed to classify establishment vibe", ex);
        }
    }

    public async Task<VibeType?> UpdateEstablishmentVibeAsync(int id, EstablishmentDTO dto)
    {
        try
        {
            if (dto is null)
                throw new ValidationException("EstablishmentDTO", "Establishment data cannot be null");

            ValidateEstablishmentForClassification(dto);

            var currentEstablishment = await GetEstablishmentByIdAsync(id);

            return ShouldReclassifyEstablishment(currentEstablishment, dto)
                ? await ClassifyEstablishmentVibeAsync(dto)
                : currentEstablishment.Vibe;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Classification", "Failed to update establishment vibe", ex);
        }
    }

    private void ValidateGeminiConfiguration(GeminiAISettings settings)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            validationErrors.Add("ApiKey", new List<string> { "Gemini AI API key is required" });

        if (string.IsNullOrWhiteSpace(settings.Model))
            validationErrors.Add("Model", new List<string> { "Gemini AI model name is required" });

        if (validationErrors.Any())
            throw new ValidationException(validationErrors);
    }

    private void ValidateEstablishmentForClassification(EstablishmentDTO establishment)
    {
        var validationErrors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(establishment.Name))
            validationErrors.Add("Name", new List<string> { "Establishment name is required for classification" });

        if (string.IsNullOrWhiteSpace(establishment.Description))
            validationErrors.Add("Description", new List<string> { "Establishment description is required for classification" });

        if (establishment.Geolocation is null)
            validationErrors.Add("Geolocation", new List<string> { "Geolocation data is recommended for better classification accuracy" });

        if (validationErrors.Any())
            throw new ValidationException(validationErrors);
    }

    private async Task<Establishment> GetEstablishmentByIdAsync(int id)
    {
        try
        {
            var establishment = await _establishmentsRepository.GetByIdAsync(id);
            if (establishment is null)
                throw new EntityNotFoundException("Establishment", id);

            return establishment;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Database", "Failed to retrieve establishment for classification", ex);
        }
    }

    private bool ShouldReclassifyEstablishment(Establishment currentEstablishment, EstablishmentDTO updatedDto)
    {
        if (currentEstablishment.Vibe is null || currentEstablishment.Vibe == VibeType.None)
            return true;

        return EstablishmentDetailsChanged(currentEstablishment, updatedDto);
    }

    private async Task<VibeType?> CallGeminiForClassificationAsync(string prompt)
    {
        try
        {
            var response = await _geminiModel.GenerateContentAsync(prompt);

            if (response?.Text is null)
                throw new ExternalServiceException(
                    "Gemini AI",
                    "Received null or empty response from Gemini AI");

            string geminiOutput = response.Text.Trim();

            if (string.IsNullOrWhiteSpace(geminiOutput))
                return VibeType.None;

            return ParseGeminiOutputToVibeType(geminiOutput);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalServiceException(
                "Gemini AI",
                "Network error while calling Gemini AI service",
                ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ExternalServiceException(
                "Gemini AI",
                "Request to Gemini AI service timed out",
                ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ExternalServiceException(
                "Gemini AI",
                "Invalid API key or unauthorized access to Gemini AI",
                ex,
                401);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException(
                "Gemini AI",
                "Unexpected error while calling Gemini AI service",
                ex);
        }
    }

    private string BuildVibeClassificationPrompt(EstablishmentDTO establishment)
    {
        try
        {
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("Analyze the following establishment's details and determine its primary vibe type.");
            promptBuilder.AppendLine("Select *only one* of the following vibe types: 'Beach', 'Nature', 'City', 'Relax', 'Mountains'.");
            promptBuilder.AppendLine("If none of these are clearly applicable, respond with 'None'.");
            promptBuilder.AppendLine("Return only the name of the vibe type (e.g., 'Beach', 'City', 'None'). Do not include any other text, explanations, or punctuation.");
            promptBuilder.AppendLine();

            promptBuilder.AppendLine("Establishment Details:");
            promptBuilder.AppendLine($"Name: {establishment.Name}");
            promptBuilder.AppendLine($"Type: {establishment.Type}");
            promptBuilder.AppendLine($"Description: {establishment.Description}");

            var featuresList = BuildFeaturesList(establishment.Features);
            promptBuilder.AppendLine($"Features: {(featuresList.Any() ? string.Join(", ", featuresList) : "No specific features listed")}");

            if (establishment.Geolocation is not null)
                promptBuilder.AppendLine($"Location: {establishment.Geolocation.City}, {establishment.Geolocation.Country}");

            promptBuilder.AppendLine("\nVibe Type:");

            var prompt = promptBuilder.ToString();

            if (prompt.Length > 10_000)
            {
                throw new BusinessRuleViolationException(
                    "PROMPT_TOO_LONG",
                    "Establishment description is too long for classification");
            }

            return prompt;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("PromptGeneration", "Failed to build classification prompt", ex);
        }
    }

    private List<string> BuildFeaturesList(EstablishmentFeatures features)
    {
        try
        {
            var featuresList = new List<string>();

            foreach (var enumValue in Enum.GetValues<EstablishmentFeatures>())
            {
                if (enumValue != EstablishmentFeatures.None && features.HasFlag(enumValue))
                {
                    string featureName = enumValue.ToString();

                    if (featureName.StartsWith("Has", StringComparison.OrdinalIgnoreCase))
                        featureName = featureName.Substring(3);
                    else if (featureName.StartsWith("Is", StringComparison.OrdinalIgnoreCase))
                        featureName = featureName.Substring(2);

                    featuresList.Add(featureName);
                }
            }

            return featuresList;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("FeatureProcessing", "Failed to process establishment features", ex);
        }
    }

    private VibeType ParseGeminiOutputToVibeType(string geminiOutput)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(geminiOutput))
                return VibeType.None;

            string cleanedOutput = geminiOutput.Trim();

            if (Enum.TryParse(cleanedOutput, true, out VibeType vibeType) &&
                Enum.IsDefined(typeof(VibeType), vibeType))
                return vibeType;

            return cleanedOutput.ToLowerInvariant() switch
            {
                "sea" or "ocean" or "coastal" or "seaside" => VibeType.Beach,
                "urban" or "downtown" or "metropolitan" or "cityscape" => VibeType.City,
                "wilderness" or "forest" or "park" or "natural" or "countryside" => VibeType.Nature,
                "calm" or "peaceful" or "tranquil" or "quiet" or "serene" => VibeType.Relax,
                "mountainous" or "alpine" or "highland" or "peak" => VibeType.Mountains,
                "none" or "unknown" or "unclear" or "other" => VibeType.None,
                _ => VibeType.None
            };
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("OutputParsing", $"Failed to parse Gemini output: '{geminiOutput}'", ex);
        }
    }

    private bool EstablishmentDetailsChanged(Establishment dbEntity, EstablishmentDTO updatedDto)
    {
        try
        {
            return dbEntity.Features != updatedDto.Features ||
                   !string.Equals(dbEntity.Description, updatedDto.Description, StringComparison.Ordinal) ||
                   !string.Equals(dbEntity.Geolocation?.City, updatedDto.Geolocation?.City, StringComparison.OrdinalIgnoreCase) ||
                   !string.Equals(dbEntity.Geolocation?.Country, updatedDto.Geolocation?.Country, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("Comparison", "Failed to compare establishment details", ex);
        }
    }
}