using System;
using System.Collections.Generic;
using System.Text;

namespace SqlAdapter.Migrations.Procedures.Struct
{
   
    class SchemaProcedure
    {
        public SchemaProcedure(string name)
        {
            _name = name;
        }

        public DataBaseType DataBaseType { get; set; }

        private string _name { get; set; }

        public List<FieldProcedure> fields = new List<FieldProcedure>();

        public void Add(FieldProcedure field)
        {
            field.nameprocedure = _name;
            fields.Add(field);
        }
    }
}
