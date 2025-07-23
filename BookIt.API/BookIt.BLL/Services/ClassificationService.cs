using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Enums;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;
using GenerativeAI;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace BookIt.BLL.Services;

public class ClassificationService : IClassificationService
{
    private readonly GenerativeModel _geminiModel;
    private readonly EstablishmentsRepository _establishmentsRepository;

    public ClassificationService(IConfiguration configuration, EstablishmentsRepository establishmentsRepository)
    {
        string? apiKey = configuration["GeminiAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Gemini API Key is missing. Please configure 'GeminiAI:ApiKey'.");
        }

        string? modelName = configuration["GeminiAI:Model"];

        if (string.IsNullOrWhiteSpace(modelName))
        {
            throw new InvalidOperationException("Gemini model name is missing. Please configure 'GeminiAI:Model'.");
        }

        _geminiModel = new GenerativeModel(apiKey, model: modelName);
        _establishmentsRepository = establishmentsRepository;
    }

    public async Task<VibeType?> ClassifyEstablishmentVibeAsync(EstablishmentDTO establishment)
    {
        if (establishment is null) return null;

        string prompt = BuildVibeClassificationPrompt(establishment);

        try
        {
            var response = await _geminiModel.GenerateContentAsync(prompt);

            string? geminiOutput = response?.Text?.Trim();

            if (string.IsNullOrWhiteSpace(geminiOutput))
            {
                return VibeType.None;
            }

            return ParseGeminiOutputToVibeType(geminiOutput);
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task<VibeType?> UpdateEstablishmentVibeAsync(int id, EstablishmentDTO dto)
    {
        var currentEstablishment = await _establishmentsRepository.GetByIdAsync(id);

        var shouldReclassify = currentEstablishment!.Vibe is null
                            || currentEstablishment.Vibe.Value is VibeType.None
                            || EstablishmentDetailsChanged(currentEstablishment, dto);

        return shouldReclassify
            ? await ClassifyEstablishmentVibeAsync(dto)
            : currentEstablishment!.Vibe;
    }

    private string BuildVibeClassificationPrompt(EstablishmentDTO establishment)
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

        var featuresList = new List<string>();
        foreach (var prop in typeof(EstablishmentFeatures).GetEnumValues())
        {
            if (((EstablishmentFeatures)prop & establishment.Features) != 0)
            {
                string featureName = prop.ToString()!;
                if (featureName.StartsWith("Has")) featureName = featureName.Substring(3);
                if (featureName.StartsWith("Is")) featureName = featureName.Substring(2);
                featuresList.Add(featureName);
            }
        }
        promptBuilder.AppendLine($"Features: {(featuresList.Any() ? string.Join(", ", featuresList) : "No specific features listed")}");

        promptBuilder.AppendLine($"Location: {establishment.Geolocation?.City}, {establishment.Geolocation?.Country}");

        promptBuilder.AppendLine("\nVibe Type:");
        return promptBuilder.ToString();
    }

    private VibeType? ParseGeminiOutputToVibeType(string geminiOutput)
    {
        string cleanedOutput = geminiOutput.Trim();

        if (Enum.TryParse(cleanedOutput, true, out VibeType vibeType))
        {
            return vibeType;
        }

        switch (cleanedOutput.ToLowerInvariant())
        {
            case "sea":
            case "ocean":
                return VibeType.Beach;
            case "urban":
            case "downtown":
                return VibeType.City;
            case "wilderness":
            case "forest":
            case "park":
                return VibeType.Nature;
            case "calm":
            case "peaceful":
            case "tranquil":
                return VibeType.Relax;
            case "mountainous":
            case "alpine":
                return VibeType.Mountains;
            default:
                return VibeType.None;
        }
    }

    private bool EstablishmentDetailsChanged(Establishment dbEntity, EstablishmentDTO updatedDto)
    {
        return dbEntity.Features != updatedDto.Features ||
               dbEntity.Description != updatedDto.Description ||
               dbEntity.Geolocation?.City != updatedDto.Geolocation?.City;
    }
}
