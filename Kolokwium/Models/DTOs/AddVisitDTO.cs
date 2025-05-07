namespace Kolokwium.Models.DTOs;

public class AddVisitDTO
{
    public int VisitId { get; set; }
    public int ClientId { get; set; }
    public string MechanicLicenceNumber { get; set; }
    public List<ServiceDTO> Services { get; set; }
    
}

public class ServiceDTO
{
    public string ServiceName { get; set; }
    public double ServiceFee { get; set; }
}