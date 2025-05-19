using AutoMapper;
using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using BookIt.DAL.Models;
using BookIt.DAL.Repositories;

namespace BookIt.BLL.Services;

public class ApartmentsService : IApartmentsService
{
    private readonly IMapper _mapper;
    private readonly ApartmentsRepository _repository;

    public ApartmentsService(IMapper mapper, ApartmentsRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<IEnumerable<ApartmentDTO>> GetAllAsync()
    {
        var apartmentsDomain = await _repository.GetAllAsync();
        var apartmentsDto = _mapper.Map<IEnumerable<ApartmentDTO>>(apartmentsDomain);
        return apartmentsDto;
    }

    public async Task<ApartmentDTO?> GetByIdAsync(int id)
    {
        var apartmentDomain = await _repository.GetByIdAsync(id);
        if (apartmentDomain is null) return null;
        var apartmentDto = _mapper.Map<ApartmentDTO>(apartmentDomain);
        return apartmentDto;
    }

    public async Task<ApartmentDTO?> CreateAsync(ApartmentDTO dto)
    {
        var apartmentDomain = _mapper.Map<Apartment>(dto);
        var addedApartment = await _repository.AddAsync(apartmentDomain);
        return await GetByIdAsync(addedApartment.Id);
    }

    public async Task<ApartmentDTO?> UpdateAsync(int id, ApartmentDTO dto)
    {
        var apartmentExists = await _repository.ExistsAsync(id);
        if (!apartmentExists) return null;
        var apartmentDomain = _mapper.Map<Apartment>(dto);
        apartmentDomain.Id = id;
        await _repository.UpdateAsync(apartmentDomain);
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var apartmentExists = await _repository.ExistsAsync(id);
        if (!apartmentExists) return false;
        await _repository.DeleteAsync(id);
        return true;
    }
}
