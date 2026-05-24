using System.Data;

namespace Trendora.Services
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }

}
