using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SqlAdapter.Migrations
{
    public class Access
    {
        public string ConnectionString
        {
            get; set;
        }

        public void Execute(string pQuery)
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
        public DataSet Query(string pQuery)
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
        public DataSet PROC(string NomeProcedure, List<SqlParameter> parametros)
        {
            SqlCommand comando = new SqlCommand();
            SqlConnection conexao = new SqlConnection(ConnectionString);

            comando.Connection = conexao;
            comando.CommandType = System.Data.CommandType.StoredProcedure;
            comando.CommandText = NomeProcedure;

            if (parametros != null)
            {
                foreach (var item in parametros)
                {
                    comando.Parameters.Add(item);
                }
            }

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
    }
}
