using BookIt.DAL.Models;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Repositories;
using BookIt.BLL.DTOs;
using AutoMapper;

namespace BookIt.BLL.Services;

public class EstablishmentsService : IEstablishmentsService
{
    private readonly IMapper _mapper;
    private readonly IRatingsService _ratingsService;
    private readonly EstablishmentsRepository _repository;

    public EstablishmentsService(IMapper mapper, EstablishmentsRepository repository, IRatingsService ratingsService)
    {
        _mapper = mapper;
        _repository = repository;
        _ratingsService = ratingsService;
    }

    public async Task<IEnumerable<EstablishmentDTO>> GetAllAsync()
    {
        var establishmentsDomain = await _repository.GetAllAsync();
        var establishmentsDto = _mapper.Map<IEnumerable<EstablishmentDTO>>(establishmentsDomain);

        foreach (var establishment in establishmentsDto)
            establishment.Rating = await _ratingsService.CalculateRating(establishment);

        return establishmentsDto;
    }

    public async Task<EstablishmentDTO?> GetByIdAsync(int id)
    {
        var establishmentDomain = await _repository.GetByIdAsync(id);
        if (establishmentDomain is null) return null;
        var establishmentDto = _mapper.Map<EstablishmentDTO>(establishmentDomain);
        establishmentDto.Rating = await _ratingsService.CalculateRating(establishmentDto);
        return establishmentDto;
    }

    public async Task<EstablishmentDTO?> CreateAsync(EstablishmentDTO dto)
    {
        var establishmentDomain = _mapper.Map<Establishment>(dto);
        var addedEstablishment = await _repository.AddAsync(establishmentDomain);
        return await GetByIdAsync(addedEstablishment.Id);
    }

    public async Task<EstablishmentDTO?> UpdateAsync(int id, EstablishmentDTO dto)
    {
        var establishmentExists = await _repository.ExistsAsync(id);
        if (!establishmentExists) return null;
        var establishmentDomain = _mapper.Map<Establishment>(dto);
        establishmentDomain.Id = id;
        await _repository.UpdateAsync(establishmentDomain);
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var establishmentExists = await _repository.ExistsAsync(id);
        if (!establishmentExists) return false;
        await _repository.DeleteAsync(id);
        return true;
    }

    public List<string> GetFeatureList(Establishment establishment)
    {
        return Enum.GetValues<EstablishmentFeatures>()
            .Where(f => f != EstablishmentFeatures.None && establishment.Features.HasFlag(f))
            .Select(f => f.ToString())
            .ToList();
    }
}
