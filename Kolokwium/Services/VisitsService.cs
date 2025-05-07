using System.Data.Common;
using Kolokwium.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace Kolokwium.Services;

public class VisitsService : IVisitsService
{

    private readonly string _connectionString =
        "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Kolokwium;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
    
    public async Task<VisitDTO?> GetVisit(int visitId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var cmdTxt = @"SELECT * FROM Visit WHERE visit_id = @VisitId";
        var cmd = new SqlCommand(cmdTxt, conn);
        cmd.Parameters.AddWithValue("@VisitId", visitId);
        var result = await cmd.ExecuteScalarAsync();
        if (result is null)
        {
            return null;
        }
        
        cmd.Parameters.Clear();

        cmdTxt =
            "SELECT v.date, c.first_name, c.last_name, c.date_of_birth, m.mechanic_id, m.licence_number, s.name, vs.service_fee " +
            "FROM Visit v JOIN Client c on v.client_id = c.client_id JOIN Mechanic m on v.mechanic_id = m.mechanic_id JOIN Visit_Service vs on v.visit_id = vs.visit_id JOIN Service s on s.service_id = vs.service_id WHERE v.visit_id = @VisitId";
        
        cmd.CommandText = cmdTxt;
        cmd.Parameters.AddWithValue("@VisitId", visitId);
        using var reader = await cmd.ExecuteReaderAsync();
        VisitDTO? visitDto = null;
        while (await reader.ReadAsync())
        {
            visitDto = new VisitDTO
            {
                Date = reader.GetDateTime(0),
                Clinet = new ClientDTO
                {
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    DateOfBirth = reader.GetDateTime(3),
                },
                Mechanic = new MechanicDTO
                {
                    MechanicId = reader.GetInt32(4),
                    LicenceNumber = reader.GetString(5),
                },
                VisitServices = new VisitServicesDTO
                {
                    Name = reader.GetString(6),
                    ServiceFee = (double) reader.GetDecimal(7),
                }
            };
        }
        
        return visitDto;

    }

    public async Task AddVist(AddVisitDTO addVisitDto)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand();
        await conn.OpenAsync();
        DbTransaction transaction = conn.BeginTransaction();
        cmd.Connection = conn;
        cmd.Transaction = transaction as SqlTransaction;
        try
        {
            var cmdTxt = @"SELECT 1 FROM Visit WHERE visit_id = @VisitId";
            cmd.CommandText = cmdTxt;
            cmd.Parameters.AddWithValue("@VisitId", addVisitDto.VisitId);
            var result = await cmd.ExecuteScalarAsync();
            if(!(result is null))
            {
                throw new Exception("Visit already exists");
            }
            cmd.Parameters.Clear();
            
            cmdTxt = @"SELECT 1 FROM Client WHERE client_id = @ClientId";
            cmd.CommandText = cmdTxt;
            cmd.Parameters.AddWithValue("@ClientId", addVisitDto.ClientId);
            result = await cmd.ExecuteScalarAsync();
            if(result is null)
            {
                throw new Exception("Client not found");
            }
            cmd.Parameters.Clear();
            
            cmdTxt = @"SELECT mechanic_id FROM Mechanic WHERE licence_number = @LicenceNumber";
            cmd.CommandText = cmdTxt;
            cmd.Parameters.AddWithValue("@LicenceNumber", addVisitDto.MechanicLicenceNumber);
            result = await cmd.ExecuteScalarAsync();
            if (result is null)
            {
                throw new Exception("Mechanic licence number not found");
            }
            var mechanicId = Convert.ToInt32(result);
            cmd.Parameters.Clear();
            
            cmdTxt = @"INSERT INTO Visit(visit_id, client_id, mechanic_id, date) VALUES(@VisitId, @ClientId, @MechanicId, @Date)";
            cmd.CommandText = cmdTxt;
            cmd.Parameters.AddWithValue("@VisitId", addVisitDto.VisitId);
            cmd.Parameters.AddWithValue("@ClientId", addVisitDto.ClientId);
            cmd.Parameters.AddWithValue("@MechanicId", mechanicId);
            cmd.Parameters.AddWithValue("@Date", DateTime.Now);

            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                throw new Exception("Unable to insert Visit record");
            }
            cmd.Parameters.Clear();


            foreach (var service in addVisitDto.Services)
            {
                cmdTxt = @"SELECT service_id FROM Service WHERE name = @ServiceName";
                cmd.CommandText = cmdTxt;
                cmd.Parameters.AddWithValue("@ServiceName", service.ServiceName);
                var serviceId = await cmd.ExecuteScalarAsync();
                if (serviceId is null)
                {
                    throw new Exception("Service not found");
                }
                cmd.Parameters.Clear();
                cmdTxt =
                    @"INSERT INTO Visit_Service(visit_id, service_id, service_fee) VALUES(@VisitId, @ServiceId, @ServiceFee)";
                cmd.CommandText = cmdTxt;
                cmd.Parameters.AddWithValue("@VisitId", addVisitDto.VisitId);
                cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                cmd.Parameters.AddWithValue("@ServiceFee", service.ServiceFee);

                await cmd.ExecuteNonQueryAsync();


            }
            
            
            
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw;
        }
        transaction.Commit();
        
    }
}