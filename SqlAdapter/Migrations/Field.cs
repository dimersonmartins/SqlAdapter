namespace SqlAdapter.Migrations
{
    public class Field
    {

        public string table { get; set; }

        public string name { get; set; }

        public string type { get; set; }

        public bool nullable { get; set; }

        public string create_index { get; set; }
    }
}
