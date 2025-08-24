using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Exceptions;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Configuration.Settings;
using BookIt.DAL.Enums;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;
using GenerativeAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace BookIt.BLL.Services;

public class ClassificationService : IClassificationService
{
    private readonly IMapper _mapper;
    private readonly GenerativeModel _geminiModel;
    private readonly GeminiAISettings _geminiSettings;
    private readonly ILogger<ClassificationService> _logger;
    private readonly EstablishmentsRepository _establishmentsRepository;

    public ClassificationService(
        IMapper mapper,
        ILogger<ClassificationService> logger,
        IOptions<GeminiAISettings> geminiAiOptions,
        EstablishmentsRepository establishmentsRepository)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("Initializing {ServiceName}", nameof(ClassificationService));

        try
        {
            _geminiSettings = geminiAiOptions?.Value ?? throw new ArgumentNullException(nameof(geminiAiOptions));
            _establishmentsRepository = establishmentsRepository ?? throw new ArgumentNullException(nameof(establishmentsRepository));

            _logger.LogInformation("Validating Gemini AI configuration");
            ValidateGeminiConfiguration(_geminiSettings);

            _logger.LogInformation("Creating Gemini AI model instance with model '{Model}'", _geminiSettings.Model);
            _geminiModel = new GenerativeModel(_geminiSettings.ApiKey, _geminiSettings.Model);

            _logger.LogInformation("{ServiceName} initialized successfully", nameof(ClassificationService));
        }
        catch (Exception ex) when (!(ex is BookItBaseException))
        {
            _logger.LogError(ex, "Failed to initialize {ServiceName}", nameof(ClassificationService));
            throw new ExternalServiceException("Gemini AI", "Failed to initialize classification service", ex);
        }
    }

    public async Task<VibeType?> ClassifyEstablishmentVibeAsync(EstablishmentDTO establishment)
    {
        _logger.LogInformation("Classifying vibe for establishment: {@Establishment}", establishment.Name);

        try
        {
            if (establishment is null)
                throw new ValidationException("Establishment", "Establishment data cannot be null");

            ValidateEstablishmentForClassification(establishment);

            string prompt = BuildVibeClassificationPrompt(establishment);

            var result = await CallGeminiForClassificationAsync(prompt);
            _logger.LogInformation("Classification result for {EstablishmentName}: {Vibe}", establishment.Name, result);

            return result;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying establishment vibe for {EstablishmentName}", establishment?.Name);
            throw new ExternalServiceException("Classification", "Failed to classify establishment vibe", ex);
        }
    }

    public async Task<VibeType?> UpdateEstablishmentVibeAsync(int id, EstablishmentDTO dto)
    {
        _logger.LogInformation("Updating vibe for establishment with ID {Id}", id);

        try
        {
            if (dto is null)
                throw new ValidationException("EstablishmentDTO", "Establishment data cannot be null");

            ValidateEstablishmentForClassification(dto);

            var currentEstablishment = await GetEstablishmentByIdAsync(id);

            var currentEstablishmentDto = _mapper.Map<EstablishmentDTO>(currentEstablishment);

            if (ShouldReclassifyEstablishment(currentEstablishmentDto, dto))
            {
                _logger.LogInformation("Reclassifying vibe for establishment {Id}", id);
                return await ClassifyEstablishmentVibeAsync(dto);
            }

            _logger.LogInformation("No classification change needed for establishment {Id}", id);
            return currentEstablishmentDto.Vibe;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vibe for establishment {Id}", id);
            throw new ExternalServiceException("Classification", "Failed to update establishment vibe", ex);
        }
    }

    private void ValidateGeminiConfiguration(GeminiAISettings settings)
    {
        _logger.LogInformation("Validating Gemini AI settings");

        var validationErrors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            validationErrors.Add("ApiKey", new List<string> { "Gemini AI API key is required" });

        if (string.IsNullOrWhiteSpace(settings.Model))
            validationErrors.Add("Model", new List<string> { "Gemini AI model name is required" });

        if (validationErrors.Any())
        {
            _logger.LogWarning("Gemini AI settings validation failed: {@Errors}", validationErrors);
            throw new Exception("Invalid Gemini configuration");
        }
    }

    private void ValidateEstablishmentForClassification(EstablishmentDTO establishment)
    {
        _logger.LogInformation("Validating establishment for classification: {Name}", establishment.Name);

        var validationErrors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(establishment.Name))
            validationErrors.Add("Name", new List<string> { "Establishment name is required for classification" });

        if (string.IsNullOrWhiteSpace(establishment.Description))
            validationErrors.Add("Description", new List<string> { "Establishment description is required for classification" });

        if (establishment.Geolocation is null)
            validationErrors.Add("Geolocation", new List<string> { "Geolocation data is recommended for better classification accuracy" });

        if (validationErrors.Any())
        {
            _logger.LogWarning("Validation failed for establishment: {@Errors}", validationErrors);
            throw new ValidationException(validationErrors);
        }
    }

    private async Task<Establishment> GetEstablishmentByIdAsync(int id)
    {
        _logger.LogInformation("Fetching establishment by ID: {Id}", id);

        try
        {
            var establishment = await _establishmentsRepository.GetByIdForVibeComparisonAsync(id);
            if (establishment is null)
            {
                _logger.LogWarning("Establishment with ID {Id} not found", id);
                throw new EntityNotFoundException("Establishment", id);
            }

            return establishment;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving establishment {Id}", id);
            throw new ExternalServiceException("Database", "Failed to retrieve establishment for classification", ex);
        }
    }

    private bool ShouldReclassifyEstablishment(EstablishmentDTO currentEstablishment, EstablishmentDTO updatedDto)
    {
        var shouldReclassify = currentEstablishment.Vibe is null || currentEstablishment.Vibe == VibeType.None ||
                               EstablishmentDetailsChanged(currentEstablishment, updatedDto);

        _logger.LogInformation("Should reclassify establishment {Id}? {Result}", currentEstablishment.Id, shouldReclassify);
        return shouldReclassify;
    }

    private async Task<VibeType?> CallGeminiForClassificationAsync(string prompt)
    {
        _logger.LogInformation("Sending classification request to Gemini AI");

        try
        {
            var response = await _geminiModel.GenerateContentAsync(prompt);

            if (response?.Text is null)
            {
                _logger.LogWarning("Gemini AI returned null/empty response");
                throw new ExternalServiceException("Gemini AI", "Received null or empty response from Gemini AI");
            }

            string geminiOutput = response.Text.Trim();
            _logger.LogInformation("Gemini AI response: {Output}", geminiOutput);

            if (string.IsNullOrWhiteSpace(geminiOutput))
                return VibeType.None;

            return ParseGeminiOutputToVibeType(geminiOutput);
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (HttpRequestException ex) when (ex.StatusCode.HasValue && ex.StatusCode.Value == HttpStatusCode.ServiceUnavailable)
        {
            _logger.LogWarning(ex, "Gemini AI is currently unavailable. Setting None Vibe type for establishment");
            return VibeType.None;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Network error while calling Gemini AI service");
            return VibeType.None;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Request to Gemini AI service timed out");
            return VibeType.None;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to Gemini AI service");
            return VibeType.None;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while calling Gemini AI service");
            return VibeType.None;
        }
    }

    private string BuildVibeClassificationPrompt(EstablishmentDTO establishment)
    {
        _logger.LogInformation("Building vibe classification prompt for {Name}", establishment.Name);

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
                _logger.LogError("Prompt too long: {Length} characters", prompt.Length);
                throw new BusinessRuleViolationException("PROMPT_TOO_LONG", "Establishment description is too long for classification");
            }

            return prompt;
        }
        catch (BookItBaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building classification prompt");
            throw new ExternalServiceException("PromptGeneration", "Failed to build classification prompt", ex);
        }
    }

    private List<string> BuildFeaturesList(EstablishmentFeatures features)
    {
        _logger.LogInformation("Processing features for establishment");

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

            _logger.LogInformation("Extracted features: {Features}", featuresList);
            return featuresList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing establishment features");
            throw new ExternalServiceException("FeatureProcessing", "Failed to process establishment features", ex);
        }
    }

    private VibeType ParseGeminiOutputToVibeType(string geminiOutput)
    {
        _logger.LogInformation("Parsing Gemini output to VibeType: {Output}", geminiOutput);

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
            _logger.LogError(ex, "Error parsing Gemini output: {Output}", geminiOutput);
            throw new ExternalServiceException("OutputParsing", $"Failed to parse Gemini output: '{geminiOutput}'", ex);
        }
    }

    private bool EstablishmentDetailsChanged(EstablishmentDTO currentDto, EstablishmentDTO updatedDto)
    {
        _logger.LogInformation("Comparing establishment details for ID {Id}", currentDto.Id);

        try
        {
            return currentDto.Features != updatedDto.Features ||
                   !string.Equals(currentDto.Description, updatedDto.Description, StringComparison.Ordinal) ||
                   !string.Equals(currentDto.Geolocation?.City, updatedDto.Geolocation?.City, StringComparison.OrdinalIgnoreCase) ||
                   !string.Equals(currentDto.Geolocation?.Country, updatedDto.Geolocation?.Country, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing establishment details for ID {Id}", currentDto.Id);
            throw new ExternalServiceException("Comparison", "Failed to compare establishment details", ex);
        }
    }
}
