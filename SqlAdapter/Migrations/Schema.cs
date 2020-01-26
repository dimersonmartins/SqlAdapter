using System.Collections.Generic;

namespace SqlAdapter.Migrations
{
    public class Schema
    {
        public List<Field> fields = new List<Field>();
        public void Add(Field field)
        {
            fields.Add(field);
        }
    }
}
