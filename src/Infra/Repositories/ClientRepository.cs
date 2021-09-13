using System;
using System.Collections.Generic;
using Infra.Models;
using NHibernate;

namespace Infra.Repositories
{
    public class ClientRepository
    {
        private readonly ISession _session;

        public ClientRepository(ISession session)
        {
            _session = session;
        }

        public void Insert(Client client)
        {
            _session.Save(client);
        }

        public IEnumerable<Client> GetAll()
        {
            return _session.Query<Client>();
        }
    }
}