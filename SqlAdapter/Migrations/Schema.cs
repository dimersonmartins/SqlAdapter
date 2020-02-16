using System.Collections.Generic;

namespace SqlAdapter.Migrations
{
    public class Schema
    {
        public Schema(string table)
        {
            _table = table;
        }

        private string _table { get; set; }

     

        public List<Field> fields = new List<Field>();
        public void Add(Field field)
        {
            field.table = _table;
            fields.Add(field);
        }
    }
}
