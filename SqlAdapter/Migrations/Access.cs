using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace SqlAdapter.Migrations
{
    public class Access
    {
        public string ConnectionString
        {
            get; set;
        }



        public void SqlServerExecute(string pQuery)
        {
            SqlCommand comando = new SqlCommand();
            SqlConnection conexao = new SqlConnection(ConnectionString);
            comando.Connection = conexao;
            comando.CommandType = System.Data.CommandType.Text;
            comando.CommandText = pQuery;

            conexao.Open();
            try
            {
                comando.ExecuteNonQuery();
            }
            finally
            {
                conexao.Close();
            }
        }
        public DataSet SqlServerQuery(string pQuery)
        {
            SqlCommand comando = new SqlCommand();
            SqlConnection conexao = new SqlConnection(ConnectionString);

            comando.Connection = conexao;
            comando.CommandType = System.Data.CommandType.Text;
            comando.CommandText = pQuery;

            SqlDataAdapter adapter = new SqlDataAdapter(comando);
            DataSet ds = new DataSet();
            conexao.Open();

            try
            {
                adapter.Fill(ds);
            }
            finally
            {
                conexao.Close();
            }

            return ds;
        }
        public void SqlServerExecuteProcedure(string procedure)
        {
            SqlCommand comando = new SqlCommand();
            SqlConnection conexao = new SqlConnection(ConnectionString);

            comando.Connection = conexao;
            comando.CommandType = System.Data.CommandType.Text;
            comando.CommandText = procedure;

            conexao.Open();
            try
            {
                comando.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conexao.Close();
            }
        }

        public void MysqlExecute(string pQuery)
        {
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(pQuery, conn);
                cmd.CommandText = pQuery;
                cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            conn.Close();

        }

        public DataSet MysqlQuery(string pQuery)
        {
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            DataSet DS = new DataSet();
            try
            {
                conn.Open();
                var mySqlDataAdapter = new MySql.Data.MySqlClient.MySqlDataAdapter(pQuery, conn);
                mySqlDataAdapter.Fill(DS);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            conn.Close();

            return DS;
        }

        public void MysqlExecuteProcedure(string procedure)
        {
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(procedure, conn);
                cmd.CommandText = procedure;
                cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            conn.Close();
        }
    }
}
