using System;
using System.Collections.Generic;
using System.Text;

namespace SqlAdapter.Migrations
{
    public class Migrate
    {
        public Schema Schema()
        {
            Schema schema = new Schema();
            schema.Add(new Field()
            {
                table = "migrations",
                name = "id",
                type = "BIGINT",
                nullable = false
            });

            schema.Add(new Field()
            {
                table = "migrations",
                name = "name",
                type = "VARCHAR(255)",
                nullable = false
            });

            schema.Add(new Field()
            {
                table = "migrations",
                name = "created_at",
                type = "DATETIME",
                nullable = false
            });

            schema.Add(new Field()
            {
                table = "migrations",
                name = "updated_at",
                type = "DATETIME",
                nullable = false
            });

            return schema;
        }
    }
}
