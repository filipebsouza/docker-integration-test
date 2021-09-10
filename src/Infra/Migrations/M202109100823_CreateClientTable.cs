using FluentMigrator;
using Infra.Models;

namespace Infra.Migrations
{
    [Migration(202109100823)]
    public class M202109100823_CreateClientTable : Migration
    {
        private readonly string _tableName = nameof(Client);

        public override void Up()
        {
            Create.Table(_tableName)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString(150).NotNullable()
                .WithColumn("Age").AsInt32().NotNullable()
                .WithColumn("Active").AsBoolean().NotNullable().WithDefaultValue(true);
        }

        public override void Down()
        {
            Delete.Table(_tableName);
        }
    }
}