namespace SqlAdapter.Migrations
{
    public class MysqlConstants
    {
        public const string ID = "id";
        public const string CREATE_TABLE = "CREATE TABLE ";
        public const string UPDATE_TABLE = "ALTER TABLE ";
        public const string ADD_COLUMNS = "ADD ";
        public const string MIGRATIONS = "migrations";

        public const string INSERT_MIGRATIONS = "INSERT INTO migrations (name,created_at,updated_at) values (";
        public const string START_UPDATE_MIGRATIONS = "UPDATE migrations";
        public const string END_UPDATE_MIGRATIONS = "WHERE name = ";

        public const char SPACE = ' ';
        public const char EQUALS = '=';
        public const char START_PARENTHESIS = '(';
        public const char END_PARENTHESIS = ')';
        public const char COMMA = ',';
        public const char SINGLE_QUOTES = '\'';

        //SQL
        public const string SET = " SET";
        public const string NAME = "name";
        public const string CREATED_AT = "created_at";
        public const string UPDATED_AT = "updated_at";
        public const string NOTNULL = " NOT NULL";
        public const string IDENTITY_PRIMARY_KEY = " AUTO_INCREMENT PRIMARY KEY";

        public const string EXIST_TABLE = @"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = ";

        public const string EXIST_PROCEDURE = @"SELECT 1 from INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = ";

    }
}
