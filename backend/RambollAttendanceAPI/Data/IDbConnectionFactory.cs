using System.Data;

namespace RambollAttendanceAPI.Data
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
