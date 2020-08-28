using BradescoPGPConsole;
using BradescoPGPConsole.Commands;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BradescoPGPConsole.Commands
{
    public class ImportarTopTier : AbstractCommand
    {
        public ImportarTopTier(ServiceConfig config) : base(config)
        {
        }

        protected override void PrepararDados()
        {
            ExecuteQuery("DELETE FROM dbo.Usuario WHERE Equipe LIKE '%TOP TIER%'");
            ExecuteQuery("DELETE FROM dbo.Encarteiramento WHERE EQUIPE_MESA LIKE '%TOP TIER%'");

            ExecuteQuery("INSERT INTO dbo.Usuario(Nome,Matricula,NomeSupervisor,MatriculaSupervisor,Equipe,PerfilId,NomeUsuario,NotificacaoEvento,NotificacaoPipeline) " +
                         "VALUES ('CARLOS GOBBO', '9394050', 'CARLOS GOBBO', '9394050', 'TOP TIER', 2, 'i394050', 1, 1)");

            ExecuteQuery("INSERT INTO dbo.Usuario(Nome,Matricula,NomeSupervisor,MatriculaSupervisor,Equipe,PerfilId,NomeUsuario,NotificacaoEvento,NotificacaoPipeline) " +
                         "VALUES ('LEONARDO BARBOSA DE VASCONCELLOS ', '9396633', 'CARLOS GOBBO', '9394050', 'TOP TIER', 2, 'I396633', 1, 1)");

            ExecuteQuery("INSERT INTO dbo.Usuario(Nome,Matricula,NomeSupervisor,MatriculaSupervisor,Equipe,PerfilId,NomeUsuario,NotificacaoEvento,NotificacaoPipeline) " +
                         "VALUES ('BRUNO CAMARGO RODRIGUES', '8963614', 'CARLOS GOBBO', '9394050', 'TOP TIER', 2, 'H963614', 1, 1)");
        }

        protected override void RealizarCarga()
        {

            BradescoPGP.Common.Logging.Log.Information("Iniciando importação de Usuários TopTier.");

            DataTable dtTopTier = GetHierarquiaTopTier();
            DataTable dtFinal = new DataTable();
            DataTable dtTopTierDigital = GetHierarquiaTopTierDigital();

            dtFinal.Columns.Add("Nome");
            dtFinal.Columns.Add("Matricula");
            dtFinal.Columns.Add("NomeSupervisor");
            dtFinal.Columns.Add("MatriculaSupervisor");
            dtFinal.Columns.Add("Equipe");
            dtFinal.Columns.Add("PerfilId");
            dtFinal.Columns.Add("NomeUsuario");
            dtFinal.Columns.Add("NotificacaoEvento");
            dtFinal.Columns.Add("NotificacaoPipeline");

            foreach (System.Data.DataRow r in dtTopTier.Rows)
            {
                dtFinal.Rows.Add();
                dtFinal.Rows[dtFinal.Rows.Count - 1][0] = r["Nome"].ToString();
                dtFinal.Rows[dtFinal.Rows.Count - 1][1] = CodLetraToCodNum(r["Matricula"].ToString());
                dtFinal.Rows[dtFinal.Rows.Count - 1][2] = r["NomeSupervisor"].ToString();
                dtFinal.Rows[dtFinal.Rows.Count - 1][3] = CodLetraToCodNum(r["MatriculaSupervisor"].ToString());
                dtFinal.Rows[dtFinal.Rows.Count - 1][4] = r["Equipe"].ToString();
                dtFinal.Rows[dtFinal.Rows.Count - 1][5] = r["PerfilId"].ToString();
                dtFinal.Rows[dtFinal.Rows.Count - 1][6] = r["NomeUsuario"].ToString();
                dtFinal.Rows[dtFinal.Rows.Count - 1][7] = r["NotificacaoEvento"].ToString();
                dtFinal.Rows[dtFinal.Rows.Count - 1][8] = r["NotificacaoPipeline"].ToString();
            }

            foreach (System.Data.DataRow r in dtTopTierDigital.Rows)
            {
                dtFinal.Rows.Add();
                dtFinal.Rows[dtFinal.Rows.Count - 1][0] = r["Nome"].ToString();
                dtFinal.Rows[dtFinal.Rows.Count - 1][1] = CodLetraToCodNum(r["Matricula"].ToString());
                dtFinal.Rows[dtFinal.Rows.Count - 1][2] = r["NomeSupervisor"].ToString();
                dtFinal.Rows[dtFinal.Rows.Count - 1][3] = CodLetraToCodNum(r["MatriculaSupervisor"].ToString());
                dtFinal.Rows[dtFinal.Rows.Count - 1][4] = r["Equipe"].ToString();
                dtFinal.Rows[dtFinal.Rows.Count - 1][5] = r["PerfilId"].ToString();
                dtFinal.Rows[dtFinal.Rows.Count - 1][6] = r["NomeUsuario"].ToString();
                dtFinal.Rows[dtFinal.Rows.Count - 1][7] = r["NotificacaoEvento"].ToString();
                dtFinal.Rows[dtFinal.Rows.Count - 1][8] = r["NotificacaoPipeline"].ToString();
            }

            AddUsuariosTopTier(dtFinal);
            AddEncarteiramentoTopTier();
            AddEncarteiramentoTopTierDigital();

            BradescoPGP.Common.Logging.Log.Information("Fim da importação de Usuários TopTier.");
        }

        //Top Tier
        private static DataTable GetHierarquiaTopTier()
        {
            SqlConnection conn;
            ConexaoSqlPortal(out conn);
            DataTable results = new DataTable();

            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT AspUsr.NormalizedUserName as Nome, FuncConsultor as Matricula, AspLid.NormalizedUserName as NomeSupervisor, FuncLider as MatriculaSupervisor, " +
                                                        "'TOP TIER' as Equipe, 3 as PerfilId, FuncConsultor as NomeUsuario, 1 as NotificacaoEvento, 1 as NotificacaoPipeline " +
                                                        "FROM PortalSti.dbo.ComDivisaoEquipes " +
                                                        "INNER JOIN PortalSti.dbo.AspNetUsers AspUsr on AspUsr.Id = ComDivisaoEquipes.FuncConsultor " +
                                                        "INNER JOIN PortalSti.dbo.AspNetUsers AspLid on AspLid.Id = ComDivisaoEquipes.FuncLider " +
                                                        "where CategoriaId = 5", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                    adapter.Fill(results);

                    return results;
                }
            }
            catch (Exception ex)
            {
                BradescoPGP.Common.Logging.Log.Error("TOP TIER: Erro ao importar Hierarquias.", ex);
                return null;
            }
            finally
            { conn.Close(); }
        }
        private static void AddUsuariosTopTier(DataTable dt)
        {
            SqlConnection conn;
            ConexaoSqlDashboard(out conn);
            int cont = 1;
            try
            {
                StringBuilder sCommand = new StringBuilder("INSERT INTO dbo.Usuario(Nome,Matricula,NomeSupervisor,MatriculaSupervisor,Equipe,PerfilId,NomeUsuario,NotificacaoEvento,NotificacaoPipeline) VALUES ");
                using (conn)
                {
                    conn.Open();
                    List<string> Rows = new List<string>();
                    int contExec = 1;
                    foreach (DataRow r in dt.Rows)
                    {
                        Rows.Add(string.Format("('{0}','{1}','{2}','{3}','{4}',{5}, '{6}', {7}, {8})",
                            MySqlHelper.EscapeString(r["Nome"].ToString()), MySqlHelper.EscapeString(r["Matricula"].ToString()), MySqlHelper.EscapeString(r["NomeSupervisor"].ToString()),
                            MySqlHelper.EscapeString(r["MatriculaSupervisor"].ToString()), MySqlHelper.EscapeString(r["Equipe"].ToString()), r["PerfilId"],
                            MySqlHelper.EscapeString(r["NomeUsuario"].ToString()), r["NotificacaoEvento"], r["NotificacaoPipeline"]));
                        cont += 1;
                        contExec += 1;
                        if (contExec == 1001)
                        {
                            sCommand.Append(string.Join(",", Rows));
                            sCommand.Append(";");

                            using (SqlCommand myCmd = new SqlCommand(sCommand.ToString(), conn))
                            {
                                myCmd.CommandType = CommandType.Text;
                                myCmd.ExecuteNonQuery();
                            }
                            sCommand = new StringBuilder("INSERT INTO dbo.Usuario(Nome,Matricula,NomeSupervisor,MatriculaSupervisor,Equipe,PerfilId,NomeUsuario,NotificacaoEvento,NotificacaoPipeline) VALUES ");
                            contExec = 1;
                            Rows = new List<string>();
                        }
                    }
                    sCommand.Append(string.Join(",", Rows));
                    sCommand.Append(";");
                    using (SqlCommand myCmd = new SqlCommand(sCommand.ToString(), conn))
                    {
                        myCmd.CommandType = CommandType.Text;
                        myCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                BradescoPGP.Common.Logging.Log.Error("TOP TIER: Erro ao importar Hierarquias.", ex); 
            }
            finally
            { conn.Close(); }
        } 
        private static void AddEncarteiramentoTopTier()
        {
            SqlConnection conn;
            ConexaoSqlDashboard(out conn);

            try
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.Encarteiramento (CPF,Agencia,Conta,CONSULTOR,Matricula,EQUIPE_RESPONSAVEL,EQUIPE_MESA, AREA) " +
                    "SELECT cpfCli,idAgencia,conta, Usuario.Nome as CONSULTOR,matAdvisor, 'TOP TIER' AS EQUIPE_RESPONSAVEL, 'TOP TIER' AS EQUIPE_MESA, 'TOP TIER' AS AREA " +
                    "FROM BD_EXTRATO_PRIME.dbo.tbgerentes " +
                    "INNER JOIN dbo.Usuario on Usuario.Matricula = tbgerentes.matAdvisor " +
                    "where area = 'Prime Top Tier'", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                BradescoPGP.Common.Logging.Log.Error("TOP TIER: Erro ao importar Hierarquias.", ex);
            }
            finally
            { conn.Close(); }
        }

        //Top Tier Digital
        private static DataTable GetHierarquiaTopTierDigital()
        {
            SqlConnection conn;
            ConexaoSqlPortal(out conn);
            DataTable results = new DataTable();

            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT AspUsr.NormalizedUserName as Nome, FuncConsultor as Matricula, AspLid.NormalizedUserName as NomeSupervisor, FuncLider as MatriculaSupervisor, " +
                                                        "'TOP TIER DIGITAL' as Equipe, 3 as PerfilId, FuncConsultor as NomeUsuario, 1 as NotificacaoEvento, 1 as NotificacaoPipeline " +
                                                        "FROM PortalSti.dbo.ComDivisaoEquipes " +
                                                        "INNER JOIN PortalSti.dbo.AspNetUsers AspUsr on AspUsr.Id = ComDivisaoEquipes.FuncConsultor " +
                                                        "INNER JOIN PortalSti.dbo.AspNetUsers AspLid on AspLid.Id = ComDivisaoEquipes.FuncLider " +
                                                        "where CategoriaId = 8", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                    adapter.Fill(results);

                    return results;
                }
            }
            catch (Exception ex)
            {
                BradescoPGP.Common.Logging.Log.Error("TOP TIER: Erro ao importar Hierarquias.", ex);
                return null;
            }
            finally
            { conn.Close(); }
        }
        private static void AddEncarteiramentoTopTierDigital()
        {

            SqlConnection conn;
            ConexaoSqlDashboard(out conn);
            DataTable results = new DataTable();

            try
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.Encarteiramento (CPF,Agencia,Conta,CONSULTOR,Matricula,EQUIPE_RESPONSAVEL,EQUIPE_MESA, AREA) " +
                    "SELECT cpfCli,idAgencia,conta, Usuario.Nome as CONSULTOR,matAdvisor, 'TOP TIER DIGITAL' AS EQUIPE_RESPONSAVEL, 'TOP TIER DIGITAL' AS EQUIPE_MESA, 'TOP TIER DIGITAL' AS AREA  " +
                    "FROM BD_EXTRATO_PRIME.dbo.tbgerentes " +
                    "INNER JOIN dbo.Usuario on Usuario.Matricula = tbgerentes.matAdvisor " +
                    "where area like '%DIGITAL%'", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                BradescoPGP.Common.Logging.Log.Error("TOP TIER: Erro ao importar Hierarquias.", ex);
            }
        }

        //Conexao
        private static void ConexaoSqlPortal(out SqlConnection conn)
        {
            string connectionString = @"data source=D4898S000E888\INVESTIMENTOS;initial catalog=PortalSti;user id=sa;password=4898Bradesco;";

            conn = new SqlConnection(connectionString);

        }
        private static void ConexaoSqlDashboard(out SqlConnection conn)
        {
            //Homologação
            //string connectionString = @"data source=D4898S000E888\INVESTIMENTOS;initial catalog=BD_PGP_WEB_DESENV;user id=sa;password=4898Bradesco;";
            
            //Produção
            string connectionString = @"data source=D4898S000E888\INVESTIMENTOS;initial catalog=BD_PGP_WEB;user id=sa;password=4898Bradesco;";

            conn = new SqlConnection(connectionString);

        }

        //Util
        private string CodNunToCodLetra(string CodFunc)
        {
            try
            {
                string Cod = CodFunc.ToUpper().Trim().Substring(0, 1);
                string CodLetra = string.Empty;

                switch (Cod)
                {
                    case "1":
                        CodLetra = "A";
                        break;
                    case "2":
                        CodLetra = "B";
                        break;
                    case "3":
                        CodLetra = "C";
                        break;
                    case "4":
                        CodLetra = "D";
                        break;
                    case "5":
                        CodLetra = "E";
                        break;
                    case "6":
                        CodLetra = "F";
                        break;
                    case "7":
                        CodLetra = "G";
                        break;
                    case "8":
                        CodLetra = "H";
                        break;
                    case "9":
                        CodLetra = "I";
                        break;
                    default:
                        return CodFunc.ToUpper();
                }

                CodLetra = CodLetra + CodFunc.Trim().Substring(1);
                return CodLetra.ToUpper();
            }
            catch
            {
                return CodFunc.ToUpper();
            }
        }
        public static string CodLetraToCodNum(string CodFunc)
        {
            if (CodFunc == "")
            {
                return "";
            }


            string primeiraLetra = CodFunc.ToUpper().Trim().Substring(0, 1);
            string cod = "";

            switch (primeiraLetra)
            {
                case "A":
                    cod = "1";
                    break;
                case "B":
                    cod = "2";
                    break;
                case "C":
                    cod = "3";
                    break;
                case "D":
                    cod = "4";
                    break;
                case "E":
                    cod = "5";
                    break;
                case "F":
                    cod = "6";
                    break;
                case "G":
                    cod = "7";
                    break;
                case "H":
                    cod = "8";
                    break;
                case "I":
                    cod = "9";
                    break;
            }

            cod = cod + CodFunc.Trim().Substring(1);
            return cod;
        }
    }
}
