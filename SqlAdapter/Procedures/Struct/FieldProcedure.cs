using System.Collections.Generic;

namespace SqlAdapter.Migrations
{
    public class FieldProcedure
    {

        public string nameprocedure { get; set; }

        /// <summary>
        /// 0 - nome do parametro 1 - tipo do parametro 
        /// </summary>
        public Dictionary<string,string> parameters { get; set; }

        public string query { get; set; }

    }
}
