using WEBAPI.Entities;

namespace WEBAPI.Data.Repositories;

public class TestRepository(RmsDbContext context) : Repository<Test>(context), ITestRepository
{
    
}