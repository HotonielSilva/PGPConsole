using BradescoPGP.Repositorio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BradescoPGP.Console
{
    public class ExpurgoService
    {
        public static void ExpurgoTEDs(List<TED> entities, string connString)
        {
            var tvp = new DataTable();
            tvp.Columns.Add("Id", typeof(int));

            var idsTeds = entities.Select(s => s.Id);

            foreach (var id in idsTeds)
                tvp.Rows.Add(id);

            using (var conn = new SqlConnection(connString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("sp_EXPURGO_TED", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd
                      .Parameters
                      .AddWithValue("@ids", tvp)
                      .SqlDbType = SqlDbType.Structured;

                    var linhasAfetadas = cmd.ExecuteNonQuery();
                }
            }

        }

        public static void ExpurgoVencimentos(List<Vencimento> entities, string connString)
        {
            var tvp = new DataTable();
            tvp.Columns.Add("Id", typeof(int));

            var idsTeds = entities.Select(s => s.Id);

            foreach (var id in idsTeds)
                tvp.Rows.Add(id);

            using (var conn = new SqlConnection(connString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("sp_EXPURGO_Vencimentos", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd
                      .Parameters
                      .AddWithValue("@ids", tvp)
                      .SqlDbType = SqlDbType.Structured;

                    var linhasAfetadas = cmd.ExecuteNonQuery();
                }
            }

        }

        public static void ExpurgoAplicacaoResgates(List<AplicacaoResgate> entities, string connString)
        {
            var tvp = new DataTable();
            tvp.Columns.Add("Id", typeof(int));

            var idsTeds = entities.Select(s => s.Id);

            foreach (var id in idsTeds)
                tvp.Rows.Add(id);

            using (var conn = new SqlConnection(connString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("sp_EXPURGO_AplicacaoResgate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd
                      .Parameters
                      .AddWithValue("@ids", tvp)
                      .SqlDbType = SqlDbType.Structured;

                    var linhasAfetadas = cmd.ExecuteNonQuery();
                }
            }

        }

        public static void ExpurgoPipelines(List<Pipeline> entities, string connString)
        {
            var tvp = new DataTable();
            tvp.Columns.Add("Id", typeof(int));

            var idsTeds = entities.Select(s => s.Id);

            foreach (var id in idsTeds)
                tvp.Rows.Add(id);

            using (var conn = new SqlConnection(connString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("sp_EXPURGO_Pipeline", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd
                      .Parameters
                      .AddWithValue("@ids", tvp)
                      .SqlDbType = SqlDbType.Structured;

                    var linhasAfetadas = cmd.ExecuteNonQuery();
                }
            }

        }

        internal static void ExpurgoLOGs(List<Log> entities, string connString)
        {
            var tvp = new DataTable();
            tvp.Columns.Add("Id", typeof(int));

            var idsTeds = entities.Select(s => s.Id);

            foreach (var id in idsTeds)
                tvp.Rows.Add(id);

            using (var conn = new SqlConnection(connString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("sp_EXPURGO_LOGS", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd
                      .Parameters
                      .AddWithValue("@ids", tvp)
                      .SqlDbType = SqlDbType.Structured;

                    var linhasAfetadas = cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
