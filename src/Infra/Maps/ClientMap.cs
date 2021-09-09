using FluentNHibernate.Mapping;
using Infra.Models;

namespace Infra.Maps
{
    public class ClientMap : ClassMap<Client>
    {
        public ClientMap()
        {
            Id(client => client.Id);
            Map(client => client.Name).Not.Nullable();
            Map(client => client.Age).Not.Nullable();
            Map(client => client.Active).Not.Nullable().Default("1");
        }
    }
}