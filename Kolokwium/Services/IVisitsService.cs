using Kolokwium.Models.DTOs;

namespace Kolokwium.Services;

public interface IVisitsService
{
    Task<VisitDTO?> GetVisit(int userId);
    Task AddVist(AddVisitDTO addVisitDto);
}