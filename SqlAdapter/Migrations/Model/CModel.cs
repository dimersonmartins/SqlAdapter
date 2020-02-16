using System;
using System.Collections.Generic;
using System.Text;

namespace SqlAdapter.Migrations.Migrations.Model
{
    class CModel
    {
        public const string model = @"
        using SqlAdapter.Migrations;
        namespace Models.Create
        {
            class Funcionarios
            {
                public Schema Schema()
                {
                    Schema schema = new Schema();
                    schema.Add(new Field()
                    {
                        table = "+"exemplo"+","+
                        "name = " + "id"+","+
                        "type = " + "BIGINT" + "," +
                        "nullable ="+" false"+ "," +
                        "create_index = "+ @"id_funcionario_cargo_fkx
                    });
                }
            }";
    }
}
