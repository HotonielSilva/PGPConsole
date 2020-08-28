using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace BradescoPGP.Common
{
    public class CaptacaoLiquidaService
    {

        public static IEnumerable<CaptacaoLiquidaModel> ObterCaptacao()
        {
            var conStr = ConfigurationManager.ConnectionStrings["PGP"].ConnectionString;

            var result = new List<CaptacaoLiquidaModel>();

            using (var conn = new SqlConnection(conStr))
            {
                conn.Open();

                var command = new SqlCommand("dbo.sp_CapLiqCanDin", conn);

                command.CommandType = System.Data.CommandType.StoredProcedure;

                using (var rd = command.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var dado = new CaptacaoLiquidaModel();

                        dado.Agencia = rd["Agencia"].ToString();
                        dado.Conta = rd["Conta"].ToString();
                        dado.Produto = rd["Produto"].ToString();
                        dado.Especialista = rd["Especialista"].ToString();
                        dado.MatriculaConsultor = rd["MatriculaConsultor"].ToString();
                        decimal.TryParse(rd["TotalAplicacao"]?.ToString(), out var totalAplicado);
                        dado.TotalAplicacao = totalAplicado;
                        decimal.TryParse(rd["TotalResgate"]?.ToString(), out var totalResgate);
                        dado.TotalResgate = totalResgate;
                        decimal.TryParse(rd["CapLiq"]?.ToString(), out var capLiq);
                        dado.CapLiq = capLiq;
                        decimal.TryParse(rd["VL_DINHEIRO_NOVO"]?.ToString(), out var dinheiroNovo);
                        dado.VL_DINHEIRO_NOVO = dinheiroNovo;
                        decimal.TryParse(rd["VL_RESG_CDB"]?.ToString(), out var resgCDB);
                        dado.VL_RESG_CDB = resgCDB;
                        decimal.TryParse(rd["VL_RESG_ISENTOS"]?.ToString(), out var resgIsentos);
                        dado.VL_RESG_ISENTOS = resgIsentos;
                        decimal.TryParse(rd["VL_RESG_COMPROMISSADAS"]?.ToString(), out var resgCompr);
                        dado.VL_RESG_COMPROMISSADAS = resgCompr;
                        decimal.TryParse(rd["VL_RESG_LF"]?.ToString(), out var resgLF);
                        dado.VL_RESG_LF = resgLF;
                        decimal.TryParse(rd["VL_RESG_FUNDOS"]?.ToString(), out var resgFundos);
                        dado.VL_RESG_FUNDOS = resgFundos;
                        decimal.TryParse(rd["VL_RESG_CORRET"]?.ToString(), out var resgCorret);
                        dado.VL_RESG_CORRET = resgCorret;
                        decimal.TryParse(rd["VL_RESG_PREVI"]?.ToString(), out var resgPrevi);
                        dado.VL_RESG_PREVI = resgPrevi;

                        result.Add(dado);
                    }

                    rd.Close();
                }

                return result;
            }
        }
    }
}
