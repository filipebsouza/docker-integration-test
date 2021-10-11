using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Infra.Models;

namespace Infra.Repositories
{
    public class ClientRepository
    {
        private readonly string _connectionString;

        public ClientRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Insert(Client client)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            const string sqlQuery = @"INSERT INTO Client (Name, Age, Active) 
                    VALUES(@Name, @Age, @Active)";
            db.Execute(sqlQuery, client);
        }

        public IEnumerable<Client> GetAll()
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            return db.Query<Client>("SELECT * FROM Client");
        }
    }
}