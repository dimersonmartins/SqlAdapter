using SqlAdapter.Migrations;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace SqlAdapter
{
    public class Build : Access
    {

        private List<Schema> _schemas_create = new List<Schema>();
        private List<Schema> _schemas_update = new List<Schema>();

        private string TableName { get; set; }

        private string Procedure { get; set; }
        public string PathProcedures { get; set; }

        private string Columns { get; set; }
        private string ColumnName { get; set; }

        private string Properties { get; set; }

        private bool IsUpdate { get; set; }

        /// <summary>
        /// schemas_create => Models a serem migradas
        /// schemas_update => MOdels a serem atualizadas
        /// ConnectionString => Conexão com o banco de dados
        /// </summary>
        public void Register(List<Schema> schemas_create, List<Schema> schemas_update, string ConnectionString)
        {
            base.ConnectionString   = ConnectionString;
            _schemas_create         = schemas_create;
            _schemas_update         = schemas_update;
        }

        /// <summary>
        /// Cria as tabelas na base de dados
        /// </summary>
        public void Create()
        {
            IsUpdate = false;
            for (int i = 0; i < _schemas_create.Count; i++)
            {
                TableName = _schemas_create[i].fields[0].table;

                if (HasTable())
                {
                    continue;
                }

                Properties = string.Empty;

                Properties += Constants.CREATE_TABLE + TableName + Constants.START_PARENTHESIS;

                MouthedLine(_schemas_create[i].fields);

                Properties += Columns;

                Console.WriteLine("Criando tabela: " + TableName);

                base.Execute(Properties);
                InsertMigrate();

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
            for (int i = 0; i < _schemas_update.Count; i++)
            {
                TableName = _schemas_update[i].fields[0].table;

                Properties = string.Empty;

                Properties += Constants.UPDATE_TABLE + TableName + Constants.SPACE + Constants.ADD_COLUMNS;

                MouthedLine(_schemas_update[i].fields);

                Properties += Columns;

                Console.WriteLine("Atualizando tabela: " + TableName);

                base.Execute(Properties);
                InsertMigrate();

                Console.WriteLine("Query: " + Properties);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Atualiza as tabelas na base de dados
        /// </summary>
        public void CreateProcedures()
        {
            foreach (string fullPath in Directory.GetFiles(PathProcedures + "Models", "*.SQL", SearchOption.AllDirectories)) // add para procurar em subDiretorios - SearchOption.AllDirectories
            {
                Procedure = Path.GetFileName(fullPath);

                if (HasProcedure() || !PathProcedures.Contains(".SQL"))
                {
                    continue;
                }

                string scriptProcedure = File.ReadAllText(PathProcedures);

                Console.WriteLine("Script: " + scriptProcedure);
                Console.WriteLine("Criando Procedure: " + Procedure);
               
                base.Execute(scriptProcedure);
            }

        }

        private bool HasProcedure()
        {
            DataSet ds = base.Query(Constants.START_EXIST_PROCEDURE
                    + Procedure.Replace(".SQL","")
                    + Constants.END_EXIST_PROCEDURE);

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0].Field<int>("response") > 0)
                {
                    return true;
                }
            }
            return false;
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
