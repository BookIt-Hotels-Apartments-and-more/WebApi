using BookIt.BLL.DTOs;
using BookIt.DAL.Enums;

namespace BookIt.BLL.Interfaces;

public interface IClassificationService
{
    Task<VibeType?> ClassifyEstablishmentVibeAsync(EstablishmentDTO establishment);
    Task<VibeType?> UpdateEstablishmentVibeAsync(int id, EstablishmentDTO dto);
}
