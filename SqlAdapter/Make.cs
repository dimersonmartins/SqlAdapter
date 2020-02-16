using System;
using System.Collections.Generic;
using System.Text;

namespace SqlAdapter.Migrations
{
    public class Make
    {
       
        public Make()
        {
            register_schemas_create.Add(new Migrate().Schema());
            register_schemas_update.Add(new Migrate().Schema());
        }
       
        public DataBaseType DataBaseType { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string PathProcedures { get; set; }

        private string StringConnection { get; set; }

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

        private void Connect()
        {
            if (DataBaseType == DataBaseType.SQLServer)
            {
                if (Port > 0)
                {
                    StringConnection = "Server=" + Server + "," + Port + ";Database=" + Database + ";User Id="+ User + ";Password="+ Password;
                }
                else
                {
                    StringConnection = "Server=" + Server + ";Database=" + Database + ";User Id=" + User + ";Password=" + Password;
                }

            }
          
            if (DataBaseType == DataBaseType.MySql)
            {
                if (Port > 0)
                {
                    StringConnection = "server=" + Server + ";port=" + Port + ";database=" + Database + ";user=" + User + ";password=" + Password;
                }
                else
                {
                    StringConnection = "server=" + Server + ";database=" + Database + ";user=" + User + ";password=" + Password;
                }

            }

        }

        private MakeSqlServer makeSqlServer = new MakeSqlServer();
        private MakeMysql     makeMysql     = new MakeMysql();

        private void start()
        {
            Connect();

            if (DataBaseType == DataBaseType.SQLServer)
            {
                makeSqlServer.ConnectionString        = StringConnection;
                makeSqlServer.register_schemas_create = register_schemas_create;
                makeSqlServer.register_schemas_update = register_schemas_update;
                makeSqlServer.PathProcedures          = PathProcedures;
            }

            if (DataBaseType == DataBaseType.MySql)
            {
                makeMysql.ConnectionString           = StringConnection;
                makeMysql.register_schemas_create    = register_schemas_create;
                makeMysql.register_schemas_update    = register_schemas_update;
                makeMysql.PathProcedures             = PathProcedures;
            }

        }

        public void CommandSql(string pQuery)
        {
            Connect();

            if (DataBaseType == DataBaseType.SQLServer)
            {
                makeSqlServer.CommandSql(pQuery);
            }

            if (DataBaseType == DataBaseType.MySql)
            {
                makeMysql.CommandSql(pQuery);
            }
        }

        public void Create()
        {
            start();

            if (DataBaseType == DataBaseType.SQLServer)
            {
                makeSqlServer.Create();
            }

            if (DataBaseType == DataBaseType.MySql)
            {
                makeMysql.Create();
            }
        }

        public void Update()
        {
            start();

            if (DataBaseType == DataBaseType.SQLServer)
            {
                makeSqlServer.Update();
            }

            if (DataBaseType == DataBaseType.MySql)
            {
                makeMysql.Update();
            }
        }

        public void CreateProcedures()
        {
            start();

            if (DataBaseType == DataBaseType.SQLServer)
            {
                makeSqlServer.CreateProcedures();
            }

            if (DataBaseType == DataBaseType.MySql)
            {
                makeMysql.CreateProcedures();
            }
        }


        public void Seeder()
        {
            start();

            if (DataBaseType == DataBaseType.SQLServer)
            {
                makeSqlServer.Seeder();
            }

            if (DataBaseType == DataBaseType.MySql)
            {
                makeMysql.Seeder();
            }
        }

    }
}
