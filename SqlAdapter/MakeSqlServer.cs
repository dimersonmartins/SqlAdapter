using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;


namespace SqlAdapter.Migrations
{
    public class MakeSqlServer : Access
    {
        public MakeSqlServer()
        {
            register_schemas_create.Add(new Migrate().Schema());
            register_schemas_update.Add(new Migrate().Schema());
        }
        /// <summary>
        /// schemas_create => Models a serem migradas
        /// </summary>
        public List<Schema> register_schemas_create = new List<Schema>();
        /// <summary>
        /// schemas_update => Models a serem atualizadas
        /// </summary>
        public List<Schema> register_schemas_update = new List<Schema>();
        /// <summary>
        /// Conexão com o banco de dados
        /// </summary>
        public string ConnectionString {
            set 
            {
                base.ConnectionString = value;
            } 
        }

        private string TableName { get; set; }

        private string Procedure { get; set; }
        public string PathProcedures { get; set; }

        private string Columns { get; set; }
        private string Create_Indexs { get; set; }
        private string ColumnName { get; set; }
        private string SeederName { get; set; }

        private string Properties { get; set; }

        private bool IsUpdate { get; set; }

        public void CommandSql(string pQuery)
        {
            base.SqlServerExecute(pQuery);
        }

        /// <summary>
        /// Cria as tabelas na base de dados
        /// </summary>
        public void Create()
        {
            IsUpdate = false;
            for (int i = 0; i < register_schemas_create.Count; i++)
            {
                TableName = register_schemas_create[i].fields[0].table;

                if (HasTable())
                {
                    continue;
                }

                Properties = string.Empty;

                Properties += SqlServerConstants.CREATE_TABLE + TableName + SqlServerConstants.START_PARENTHESIS;

                MouthedLine(register_schemas_create[i].fields);

                Properties += Columns;

                Console.WriteLine("Criando tabela: " + TableName);

                base.SqlServerExecute(Properties);
                InsertMigrate();

                if (!string.IsNullOrWhiteSpace(Create_Indexs))
                {
                    CreateIndexs(register_schemas_create[i].fields);
                }

                Console.WriteLine("Query: " + Properties);
                Console.WriteLine();
            }
        }
        /// <summary>
        /// Atualiza as tabelas na base de dados
        /// </summary>
        public void Update()
        {
            IsUpdate = true;
            for (int i = 0; i < register_schemas_update.Count; i++)
            {
                TableName = register_schemas_update[i].fields[0].table;

                Properties = string.Empty;

                Properties += SqlServerConstants.UPDATE_TABLE + TableName + SqlServerConstants.SPACE + SqlServerConstants.ADD_COLUMNS;

                MouthedLine(register_schemas_update[i].fields);
                if (string.IsNullOrWhiteSpace(Columns))
                {
                    Console.WriteLine("Não há alterações na tabela: " + TableName);
                    continue;
                }
                Properties += Columns;

                Console.WriteLine("Atualizando tabela: " + TableName);

                base.SqlServerExecute(Properties);
                InsertMigrate();


                if (!string.IsNullOrWhiteSpace(Create_Indexs))
                {
                    CreateIndexs(register_schemas_update[i].fields);
                }


                Console.WriteLine("Query: " + Properties);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Criação das Procedures
        /// </summary>
        public void CreateProcedures()
        {
            foreach (string fullPath in Directory.GetFiles(PathProcedures, "*.sql", SearchOption.AllDirectories)) // add para procurar em subDiretorios - SearchOption.AllDirectories
            {
                try
                {

                    string fileName = Path.GetFileName(fullPath);
                    if (!fileName.Contains(".sql"))
                    {
                        continue;
                    }

                    string scriptProcedure = File.ReadAllText(fullPath, Encoding.Default);

                    Procedure = Between(scriptProcedure, "--NAME{", "}");

                    if (Procedure.Contains("SEEDER") || Procedure.Contains("SEEDER"))
                    {
                        continue;
                    }
                   
                    Console.WriteLine("Abrindo o Arquivo: " + Path.GetFileName(fileName));
                    HasProcedure();

                    Console.WriteLine("Criando Procedure: " + Procedure);
                    base.SqlServerExecuteProcedure(scriptProcedure);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
               
            }

        }

        /// <summary>
        /// Semeador de tabelas
        /// </summary>
        public void Seeder()
        {
            foreach (string fullPath in Directory.GetFiles(PathProcedures, "*.sql", SearchOption.AllDirectories)) // add para procurar em subDiretorios - SearchOption.AllDirectories
            {
                try
                {
                    string fileName = Path.GetFileName(fullPath);
                    if (!fileName.Contains(".sql"))
                    {
                        continue;
                    }

                    string scriptSeeder = File.ReadAllText(fullPath, Encoding.Default);

                    SeederName = Between(scriptSeeder, "--NAME{", "}");
                    if (!SeederName.Contains("SEEDER"))
                    {
                        continue;
                    }


                    Console.WriteLine("Abrindo o Arquivo: " + fileName);

                    Console.WriteLine("Semeando...");
                    base.SqlServerExecute(scriptSeeder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
               
            }

        }

        public void DropTables()
        {
            IsUpdate = false;
            for (int i = 0; i < register_schemas_create.Count; i++)
            {
                TableName = register_schemas_create[i].fields[0].table;

                Console.WriteLine("Droping Table: " + TableName);

                if (HasTable())
                {
                    base.SqlServerExecute(@"DROP TABLE "+ TableName);
                }

                Console.WriteLine();
            }
        }

        private void CreateIndexs(List<Field> schemas)
        {
            foreach (var reg in schemas)
            {
                string NULLABLE = string.Empty;

                if (!string.IsNullOrWhiteSpace(reg.create_index))
                {
                    base.SqlServerExecuteProcedure(@"CREATE INDEX "+ reg.create_index.ToLower() + " ON " + TableName + " ("+ reg.name.ToLower() + ")");
                }
            }
        }

        public string Between(string STR, string FirstString, string LastString)
        {
            string FinalString;
            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            int Pos2 = STR.IndexOf(LastString);
            FinalString = STR.Substring(Pos1, Pos2 - Pos1);
            return FinalString;
        }

        private void HasProcedure()
        {
           base.SqlServerExecute(SqlServerConstants.START_EXIST_PROCEDURE
                    + Procedure
                    + SqlServerConstants.MEIO_EXIST_PROCEDURE
                    + SqlServerConstants.END_EXIST_DROP_PROCEDURE
                    + Procedure
                    + SqlServerConstants.END_EXIST_PROCEDURE);
        }

        private bool HasTable()
        {
            if (!IsUpdate)
            {
                DataSet ds = base.SqlServerQuery(SqlServerConstants.START_EXIST_TABLE
                  + SqlServerConstants.SINGLE_QUOTES
                  + TableName
                  + SqlServerConstants.SINGLE_QUOTES
                  + SqlServerConstants.END_EXIST_TABLE);

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    if (ds.Tables[0].Rows[0].Field<int>("response") > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        private bool HasColumn()
        {
            DataSet ds = base.SqlServerQuery(@"IF(EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS  WHERE TABLE_NAME = '" + TableName + "' AND  COLUMN_NAME = '" + ColumnName + @"'))
                                         BEGIN
	                                        SELECT response = 1
                                         END
                                        ELSE
                                         BEGIN
                                         SELECT response = 0
                                         END");

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0].Field<int>("response") > 0)
                {
                    return true;
                }
            }

            return false;
        }
        private void InsertMigrate()
        {
            string date = DateTime.Now.ToString();
            if (!IsUpdate)
            {
                if (TableName != SqlServerConstants.MIGRATIONS)
                {
                    base.SqlServerExecute(SqlServerConstants.INSERT_MIGRATIONS
                  + SqlServerConstants.SINGLE_QUOTES
                  + TableName
                  + SqlServerConstants.SINGLE_QUOTES
                  + SqlServerConstants.COMMA
                  + SqlServerConstants.SPACE
                  + SqlServerConstants.SINGLE_QUOTES
                  + date
                  + SqlServerConstants.SINGLE_QUOTES
                  + SqlServerConstants.COMMA
                  + SqlServerConstants.SPACE
                  + SqlServerConstants.SINGLE_QUOTES
                  + date
                  + SqlServerConstants.SINGLE_QUOTES
                  + SqlServerConstants.END_PARENTHESIS);
                }
            }
            else
            {
                if (TableName != SqlServerConstants.MIGRATIONS)
                {
                    base.SqlServerExecute(SqlServerConstants.START_UPDATE_MIGRATIONS
                         + SqlServerConstants.SET
                         + SqlServerConstants.SPACE
                         + SqlServerConstants.UPDATED_AT
                         + SqlServerConstants.SPACE
                         + SqlServerConstants.EQUALS
                         + SqlServerConstants.SPACE
                         + SqlServerConstants.SINGLE_QUOTES
                         + date
                         + SqlServerConstants.SINGLE_QUOTES
                         + SqlServerConstants.SPACE
                         + SqlServerConstants.END_UPDATE_MIGRATIONS
                         + SqlServerConstants.SINGLE_QUOTES
                         + TableName
                         + SqlServerConstants.SINGLE_QUOTES);
                }
            }
        }
        private void MouthedLine(List<Field> schemas)
        {
            Columns = string.Empty;

            foreach (var reg in schemas)
            {
                ColumnName = reg.name.ToLower();

                if (HasColumn())
                {
                    continue;
                }

                string NULLABLE = string.Empty;

                if (!reg.nullable)
                {
                    NULLABLE = SqlServerConstants.NOTNULL;
                }


                if (!string.IsNullOrWhiteSpace(reg.create_index))
                {
                    Create_Indexs = reg.create_index + SqlServerConstants.COMMA;
                }

                if (ColumnName == SqlServerConstants.ID)
                {
                    Columns += ColumnName = ColumnName
                        + SqlServerConstants.SPACE
                        + reg.type
                        + NULLABLE
                        + SqlServerConstants.IDENTITY_PRIMARY_KEY
                        + SqlServerConstants.COMMA;
                }
                else
                {
                    Columns += ColumnName
                        + SqlServerConstants.SPACE
                        + reg.type
                        + NULLABLE
                        + SqlServerConstants.COMMA
                        + SqlServerConstants.SPACE;
                }
            }
            if (!IsUpdate)
            {
                Columns = Columns.TrimEnd(SqlServerConstants.SPACE).TrimEnd(SqlServerConstants.COMMA) + SqlServerConstants.END_PARENTHESIS + Environment.NewLine;
            }
            else
            {
                Columns = Columns.TrimEnd(SqlServerConstants.SPACE).TrimEnd(SqlServerConstants.COMMA);
            }

        }

    }
}
