using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;


namespace SqlAdapter.Migrations
{
    public class MakeMysql : Access
    {
        public MakeMysql()
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
            base.MysqlExecute(pQuery);
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

                Properties += MysqlConstants.CREATE_TABLE + TableName + MysqlConstants.START_PARENTHESIS;

                MouthedLine(register_schemas_create[i].fields);

                Properties += Columns;

                Console.WriteLine("Criando tabela: " + TableName);

                base.MysqlExecute(Properties);
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

                Properties += MysqlConstants.UPDATE_TABLE + TableName + MysqlConstants.SPACE + MysqlConstants.ADD_COLUMNS;

                MouthedLine(register_schemas_update[i].fields);
                if (string.IsNullOrWhiteSpace(Columns))
                {
                    Console.WriteLine("Não há alterações na tabela: " + TableName);
                    continue;
                }
                Properties += Columns;

                Console.WriteLine("Atualizando tabela: " + TableName);

                base.MysqlExecute(Properties);
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

                    Procedure = Between(scriptProcedure, "-- NAME{", "}");

                    if (Procedure.Contains("SEEDER") || Procedure.Contains("SEEDER"))
                    {
                        continue;
                    }
                   
                    Console.WriteLine("Abrindo o Arquivo: " + Path.GetFileName(fileName));
                    HasProcedure();

                    Console.WriteLine("Criando Procedure: " + Procedure);
                    base.MysqlExecuteProcedure(scriptProcedure);
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

                    SeederName = Between(scriptSeeder, "-- NAME{", "}");
                    if (!SeederName.Contains("SEEDER"))
                    {
                        continue;
                    }


                    Console.WriteLine("Abrindo o Arquivo: " + fileName);

                    Console.WriteLine("Semeando...");
                    base.MysqlExecute(scriptSeeder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
               
            }

        }

        private void DropTables()
        {
            IsUpdate = false;
            for (int i = 0; i < register_schemas_create.Count; i++)
            {
                TableName = register_schemas_create[i].fields[0].table;

                Console.WriteLine("Droping Table: " + TableName);

                if (HasTable())
                {
                    base.MysqlExecute(@"DROP TABLE "+ TableName);
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
                    base.MysqlExecuteProcedure(@"CREATE INDEX "+ reg.create_index.ToLower() + " ON " + TableName + " ("+ reg.name.ToLower() + ")");
                }
            }
        }

        private string Between(string STR, string FirstString, string LastString)
        {
            string FinalString;
            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            int Pos2 = STR.IndexOf(LastString);
            FinalString = STR.Substring(Pos1, Pos2 - Pos1);
            return FinalString;
        }

        private void HasProcedure()
        {
           base.MysqlExecute(MysqlConstants.EXIST_PROCEDURE
                + MysqlConstants.SINGLE_QUOTES
                + MysqlConstants.SPACE
                + Procedure
                + MysqlConstants.SPACE
                + MysqlConstants.SINGLE_QUOTES);
        }

        private bool HasTable()
        {
            if (!IsUpdate)
            {
                DataSet ds = base.MysqlQuery(MysqlConstants.EXIST_TABLE
                  + MysqlConstants.SINGLE_QUOTES
                  + TableName
                  + MysqlConstants.SINGLE_QUOTES);

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }


        private bool HasColumn()
        {
            DataSet ds = base.MysqlQuery(@"SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + TableName + "' AND  COLUMN_NAME = '" + ColumnName + @"'");

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                return true;
            }

            return false;
        }
        private void InsertMigrate()
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (!IsUpdate)
            {
                if (TableName != MysqlConstants.MIGRATIONS)
                {
                    base.MysqlExecute(MysqlConstants.INSERT_MIGRATIONS
                  + MysqlConstants.SINGLE_QUOTES
                  + TableName
                  + MysqlConstants.SINGLE_QUOTES
                  + MysqlConstants.COMMA
                  + MysqlConstants.SPACE
                  + MysqlConstants.SINGLE_QUOTES
                  + date
                  + MysqlConstants.SINGLE_QUOTES
                  + MysqlConstants.COMMA
                  + MysqlConstants.SPACE
                  + MysqlConstants.SINGLE_QUOTES
                  + date
                  + MysqlConstants.SINGLE_QUOTES
                  + MysqlConstants.END_PARENTHESIS);
                }
            }
            else
            {
                if (TableName != MysqlConstants.MIGRATIONS)
                {
                    base.MysqlExecute(MysqlConstants.START_UPDATE_MIGRATIONS
                         + MysqlConstants.SET
                         + MysqlConstants.SPACE
                         + MysqlConstants.UPDATED_AT
                         + MysqlConstants.SPACE
                         + MysqlConstants.EQUALS
                         + MysqlConstants.SPACE
                         + MysqlConstants.SINGLE_QUOTES
                         + date
                         + MysqlConstants.SINGLE_QUOTES
                         + MysqlConstants.SPACE
                         + MysqlConstants.END_UPDATE_MIGRATIONS
                         + MysqlConstants.SINGLE_QUOTES
                         + TableName
                         + MysqlConstants.SINGLE_QUOTES);
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
                    NULLABLE = MysqlConstants.NOTNULL;
                }


                if (!string.IsNullOrWhiteSpace(reg.create_index))
                {
                    Create_Indexs = reg.create_index + MysqlConstants.COMMA;
                }

                if (ColumnName == MysqlConstants.ID)
                {
                    Columns += ColumnName = ColumnName
                        + MysqlConstants.SPACE
                        + reg.type
                        + NULLABLE
                        + MysqlConstants.IDENTITY_PRIMARY_KEY
                        + MysqlConstants.COMMA;
                }
                else
                {
                    Columns += ColumnName
                        + MysqlConstants.SPACE
                        + reg.type
                        + NULLABLE
                        + MysqlConstants.COMMA
                        + MysqlConstants.SPACE;
                }
            }
            if (!IsUpdate)
            {
                Columns = Columns.TrimEnd(MysqlConstants.SPACE).TrimEnd(MysqlConstants.COMMA) + MysqlConstants.END_PARENTHESIS + Environment.NewLine;
            }
            else
            {
                Columns = Columns.TrimEnd(MysqlConstants.SPACE).TrimEnd(MysqlConstants.COMMA);
            }

        }

    }
}
