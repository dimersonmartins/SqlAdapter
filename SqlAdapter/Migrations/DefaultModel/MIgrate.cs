using System;
using System.Collections.Generic;
using System.Text;

namespace SqlAdapter.Migrations
{
    public class Migrate
    {
        public Schema Schema()
        {
            Schema schema = new Schema("migrations");
            schema.Add(new Field()
            {
                name = "id",
                type = "BIGINT",
                nullable = false
            });

            schema.Add(new Field()
            {
                name = "name",
                type = "VARCHAR(255)",
                nullable = false
            });

            schema.Add(new Field()
            {
                name = "created_at",
                type = "DATETIME",
                nullable = false
            });

            schema.Add(new Field()
            {
                name = "updated_at",
                type = "DATETIME",
                nullable = false
            });

            return schema;
        }
    }
}
