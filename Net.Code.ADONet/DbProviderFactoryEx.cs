using System.Data;
using System.Data.Common;

namespace Net.Code.ADONet
{
    static class DbProviderFactoryEx 
    {
        public static IDbConnection CreateConnection(this DbProviderFactory factory, string connectionString)
        {
            var connection = factory.CreateConnection();
            // ReSharper disable once PossibleNullReferenceException
            connection.ConnectionString = connectionString;
            return connection;
        }
    }
}