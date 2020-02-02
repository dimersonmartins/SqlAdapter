using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace SqlAdapter.Migrations
{
    public class Make : Access
    {
        public Make()
        {
            register_schemas_create.Add(new Migrations.Migrate().Schema());
            register_schemas_update.Add(new Migrations.Migrate().Schema());
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

                Properties += Constants.CREATE_TABLE + TableName + Constants.START_PARENTHESIS;

                MouthedLine(register_schemas_create[i].fields);

                Properties += Columns;

                Console.WriteLine("Criando tabela: " + TableName);

                base.Execute(Properties);
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

                Properties += Constants.UPDATE_TABLE + TableName + Constants.SPACE + Constants.ADD_COLUMNS;

                MouthedLine(register_schemas_update[i].fields);
                if (string.IsNullOrWhiteSpace(Columns))
                {
                    Console.WriteLine("Não há alterações na tabela: " + TableName);
                    continue;
                }
                Properties += Columns;

                Console.WriteLine("Atualizando tabela: " + TableName);

                base.Execute(Properties);
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
                    Console.WriteLine("Abrindo o Arquivo: " + Path.GetFileName(fileName));

                    if (!fileName.Contains(".sql"))
                    {
                        continue;
                    }

                    string scriptProcedure = File.ReadAllText(fullPath, Encoding.Default);

                    Procedure = Between(scriptProcedure, "--NAME{", "}");

                    if (Procedure == "SEEDER")
                    {
                        continue;
                    }

                    HasProcedure();

                    Console.WriteLine("Criando Procedure: " + Procedure);
                    base.ExecuteProcedure(scriptProcedure);
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
                    if (SeederName != "SEEDER")
                    {
                        continue;
                    }


                    Console.WriteLine("Abrindo o Arquivo: " + fileName);

                    Console.WriteLine("Semeando...");
                    base.Execute(scriptSeeder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
               
            }

        }

        private void CreateIndexs(List<Field> schemas)
        {
            foreach (var reg in schemas)
            {
                string NULLABLE = string.Empty;

                if (!string.IsNullOrWhiteSpace(reg.create_index))
                {
                    base.ExecuteProcedure(@"CREATE INDEX "+ reg.create_index.ToLower() + " ON " + TableName + " ("+ reg.name.ToLower() + ")");
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
           base.Execute(Constants.START_EXIST_PROCEDURE
                    + Procedure
                    + Constants.MEIO_EXIST_PROCEDURE
                    + Constants.END_EXIST_DROP_PROCEDURE
                    + Procedure
                    + Constants.END_EXIST_PROCEDURE);
        }

        private bool HasTable()
        {
            if (!IsUpdate)
            {
                DataSet ds = base.Query(Constants.START_EXIST_TABLE
                  + Constants.SINGLE_QUOTES
                  + TableName
                  + Constants.SINGLE_QUOTES
                  + Constants.END_EXIST_TABLE);

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
            DataSet ds = base.Query(@"IF(EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS  WHERE TABLE_NAME = '" + TableName + "' AND  COLUMN_NAME = '" + ColumnName + @"'))
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
                if (TableName != Constants.MIGRATIONS)
                {
                    base.Execute(Constants.INSERT_MIGRATIONS
                  + Constants.SINGLE_QUOTES
                  + TableName
                  + Constants.SINGLE_QUOTES
                  + Constants.COMMA
                  + Constants.SPACE
                  + Constants.SINGLE_QUOTES
                  + date
                  + Constants.SINGLE_QUOTES
                  + Constants.COMMA
                  + Constants.SPACE
                  + Constants.SINGLE_QUOTES
                  + date
                  + Constants.SINGLE_QUOTES
                  + Constants.END_PARENTHESIS);
                }
            }
            else
            {
                if (TableName != Constants.MIGRATIONS)
                {
                    base.Execute(Constants.START_UPDATE_MIGRATIONS
                         + Constants.SET
                         + Constants.SPACE
                         + Constants.UPDATED_AT
                         + Constants.SPACE
                         + Constants.EQUALS
                         + Constants.SPACE
                         + Constants.SINGLE_QUOTES
                         + date
                         + Constants.SINGLE_QUOTES
                         + Constants.SPACE
                         + Constants.END_UPDATE_MIGRATIONS
                         + Constants.SINGLE_QUOTES
                         + TableName
                         + Constants.SINGLE_QUOTES);
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
                    NULLABLE = Constants.NOTNULL;
                }


                if (!string.IsNullOrWhiteSpace(reg.create_index))
                {
                    Create_Indexs = reg.create_index + Constants.COMMA;
                }

                if (ColumnName == Constants.ID)
                {
                    Columns += ColumnName = ColumnName
                        + Constants.SPACE
                        + reg.type
                        + NULLABLE
                        + Constants.IDENTITY_PRIMARY_KEY
                        + Constants.COMMA;
                }
                else
                {
                    Columns += ColumnName
                        + Constants.SPACE
                        + reg.type
                        + NULLABLE
                        + Constants.COMMA
                        + Constants.SPACE;
                }
            }
            if (!IsUpdate)
            {
                Columns = Columns.TrimEnd(Constants.SPACE).TrimEnd(Constants.COMMA) + Constants.END_PARENTHESIS + Environment.NewLine;
            }
            else
            {
                Columns = Columns.TrimEnd(Constants.SPACE).TrimEnd(Constants.COMMA);
            }

        }

    }
}
