using BradescoPGP.Common.Logging;
using BradescoPGP.Repositorio;
using BradescoPGPConsole.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace BradescoPGPConsole.Commands
{
    public class ImportarTEDs : AbstractCommand
    {
        private class MapeamentoConsultor
        {
            public String Matricula { get; set; }

            public String Nome { get; set; }
        }

        public ImportarTEDs(ServiceConfig config)
            : base(config)
        {
        }

        protected override void RealizarCarga()
        {
            try
            {
                BradescoPGP.Common.Logging.Log.Information("Iniciando importação de TEDs.");

                var idTEDsExistentes = new HashSet<int>();

                var encarteiramentoAtual = new Dictionary<String, MapeamentoConsultor>();

                var statusInicialTED = 11;

                using (var db = new PGPEntities())
                {
                    //Obtem a lista de TEDs já importadas
                    idTEDsExistentes = new HashSet<int>(db.TED.Select(t => t.Id));

                    //Indexa o Encarteiramento por Agencia e Conta
                    foreach (var e in db.Encarteiramento)
                    {
                        encarteiramentoAtual[CriarChave(e.Agencia, e.Conta)] = new MapeamentoConsultor
                        {
                            Matricula = e.Matricula,
                            Nome = e.CONSULTOR
                        };
                    }
                }

                var novasTEDs = ObterNovasTEDs(idTEDsExistentes, encarteiramentoAtual);

                PreencherDadosAdicionais(encarteiramentoAtual, statusInicialTED, novasTEDs);

                novasTEDs = RemoverAnterioresADezembro(novasTEDs);

                Persistir(novasTEDs);

                BradescoPGP.Common.Logging.Log.Information($"Importação de TEDs finalizada. | {novasTEDs.Count} incluidas");
            }
            catch (Exception ex)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error("TED: Erro ao importar TEDs.", ex);
            }
        }

        private List<TED> RemoverAnterioresADezembro(List<TED> novasTEDs)
        {
            using (var db = new PGPEntities())
            {
                // remove teds da equipe varejo menor que 01/12/2019
                var dataInicial = new DateTime(2019, 12, 1);

                var tedsVarejoApartirDezembro2019 = novasTEDs.Join(db.Usuario,
                    t => t.MatriculaConsultor,
                    u => u.Matricula,
                    (t, u) => new { t, u.Equipe })
                    .Where(s => s.Equipe.ToLower() == "varejo" && s.t.Data < dataInicial).Select(s => s.t).ToList();

                tedsVarejoApartirDezembro2019.ForEach(t =>
                {
                    novasTEDs.Remove(t);
                });

                //Altera status de teds excluidas par não ser inclusas em próximas execuções
                if (tedsVarejoApartirDezembro2019.Any())
                {
                    using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ExtratoPrime"].ConnectionString))
                    {
                        connection.Open();

                        AtualizarStatusTeds(tedsVarejoApartirDezembro2019.Select(s => s.Id).ToList(), connection);

                        connection.Close();
                    }
                }
            }

            return novasTEDs;
        }

        private static List<TED> ObterNovasTEDs(HashSet<int> idTEDsExistentes, Dictionary<String, MapeamentoConsultor> encarteiramentoAtual)
        {
            var novasTEDs = new List<TED>();

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ExtratoPrime"].ConnectionString))
            {
                connection.Open();

                novasTEDs.AddRange(ObterNovasTEDsPGP(idTEDsExistentes, encarteiramentoAtual, connection));

                var idsTedsNovas = novasTEDs.Select(s => s.Id).ToList();

                if (idsTedsNovas.Any())
                    //AtualizarStatusTeds(idsTedsNovas, connection); //Usar apenas em prducao

                connection.Close();
            }

            return novasTEDs;
        }

        private static void AtualizarStatusTeds(List<int> idsTeds, SqlConnection connection)
        {

            var ids = idsTeds.GerarString();

            using (var cmd = new SqlCommand("UPDATE TbTedsRecebidas SET ImpDash = 1 WHERE Id in (" + ids + ")", connection))
            {
                cmd.ExecuteNonQuery();

            }
        }

        private static void Persistir(List<TED> novasTEDs)
        {
            if (novasTEDs.Count == 0)
                return;

            using (var db = new PGPEntities())
            {
                db.BulkInsert(novasTEDs);
            }
        }

        /// <summary>
        /// Verifica quais são as agencia e conta que já tem consultor para realizar a gestão do cliente e associa no Objeto da TED
        /// </summary>
        /// <param name="encarteiramentoAtual"></param>
        /// <param name="statusInicialTED"></param>
        /// <param name="novasTEDs"></param>
        private static void PreencherDadosAdicionais(IDictionary<string, MapeamentoConsultor> encarteiramentoAtual, int statusInicialTED, List<TED> novasTEDs)
        {
            var chaveAgenciaConta = String.Empty;

            foreach (var ted in novasTEDs)
            {
                chaveAgenciaConta = CriarChave(ted.Agencia, ted.Conta);
                
                ted.NomeConsultor = encarteiramentoAtual[chaveAgenciaConta].Nome;
                
                ted.NomeCliente = ObterNomeCliente(int.Parse(ted.Agencia), int.Parse(ted.Conta));

                ted.StatusId = statusInicialTED;
            }
        }

        private static String CriarChave(String agencia, String conta)
        {
            return String.Format("{0}.{1}", agencia, conta);
        }

        private static String ObterNomeCliente(int agencia, int conta)
        {
            using (var db = new PGPEntities())
            {
                var cliente = db.Cockpit.FirstOrDefault(c => c.CodigoAgencia == agencia && c.Conta == conta);

                if (cliente != null)
                {
                    return cliente.NomeCliente;
                }

                return null;
            }
        }

        /// <summary>
        /// Obtem uma lista de das novas TEDs no valor abaixo de 500.000 (PGP)
        /// </summary>
        /// <param name="idTEDsExistentes"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        private static List<TED> ObterNovasTEDsPGP(HashSet<int> idTEDsExistentes, IDictionary<string, MapeamentoConsultor> encarteiramentoAtual, SqlConnection connection)
        {
            var novasTEDs = new List<TED>();

            using (var cmd = new SqlCommand("Select Id, DataTed 'Data', Agencia, Conta, Valor, Area, Funcional , Gestor, Gerente FROM TbTedsRecebidas WHERE ImpDash = 0 and AREA LIKE '%PGP%'", connection))
            {
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        if (!idTEDsExistentes.Contains(int.Parse(dataReader["Id"].ToString())))
                        {
                            var agencia = dataReader["Agencia"].ToString();

                            var conta = dataReader["Conta"].ToString();

                            if (encarteiramentoAtual.ContainsKey(CriarChave(agencia,conta)))
                            {
                                novasTEDs.Add(new TED
                                {
                                    Id = Convert.ToInt32(dataReader["Id"]),
                                    Data = Convert.ToDateTime(dataReader["Data"]),
                                    Agencia = dataReader["Agencia"].ToString(),
                                    Conta = dataReader["Conta"].ToString(),
                                    Valor = Convert.ToDecimal(dataReader["Valor"]),
                                    Area = dataReader["Area"].ToString(),
                                    MatriculaConsultor = dataReader["Funcional"].ToString(),
                                    MatriculaSupervisor = dataReader["Gestor"].ToString(),
                                    Notificado = false
                                });
                            }
                        }

                    }

                    dataReader.Close();
                }

                var tedsExlusao = novasTEDs.Where(s => s.Area == "PGP PJ" && s.Valor < 500000).ToList();

                tedsExlusao.ForEach(t =>
                {
                    novasTEDs.Remove(t);
                });

                //Atualiza status de teds excluidas para não ser inclusas em próximas execucoes
                if (tedsExlusao.Any())
                    AtualizarStatusTeds(tedsExlusao.Select(s => s.Id).ToList(), connection);
            }

            return novasTEDs;

            #region CodigoDeric
            //foreach (var codigoGerente in codigosGerentesArea)
            //{
            //    var ultimasTEDsPorGerente = ObterListaTEDsPorGerente(idTEDsExistentes, false, codigoGerente.Key, codigoGerente.Value, connection);

            //    foreach (var ted in ultimasTEDsPorGerente)
            //    {
            //        idTEDsExistentes.Add(ted.Id);
            //    }

            //    novasTEDs.AddRange(ultimasTEDsPorGerente);

            //    ultimasTEDsPorGerente = ObterListaTEDsPorGerente(idTEDsExistentes, true, codigoGerente.Key, codigoGerente.Value, connection);

            //    foreach (var ted in ultimasTEDsPorGerente)
            //    {
            //        idTEDsExistentes.Add(ted.Id);
            //    }

            //    novasTEDs.AddRange(ultimasTEDsPorGerente);
            //}
            #endregion
        }

        protected override void PrepararDados()
        {
            TruncarTabela("TED");
        }
    }
}

#region Codigos não utilizado
/// <summary>
/// Obtem uma lista de das novas TEDs no valor acima de 500.000 (PRIME INVEST)
/// </summary>
/// <param name="idTEDsExistentes"></param>
/// <param name="connection"></param>
/// <returns></returns>
//private static List<TED> ObterNovasTEDsPrimeInvest(HashSet<int> idTEDsExistentes, SqlConnection connection)
//{
//    var idTed = default(int);

//    var novasTEDs = new List<TED>();

//    using (var sqlCommand = new SqlCommand("SELECT Id, Data, Agencia, Conta, Valor, CodConsultor 'MatriculaConsultor', Consultor 'NomeConsultor', NULL 'MatriculaSupervisor', NULL 'NomeSupervisor', 'PRIME INVEST' 'Area' FROM ViewRegistrosPgp", connection))
//    {
//        using (PGPEntities db = new PGPEntities())
//        using (var dataReader = sqlCommand.ExecuteReader())
//        {

//            while (dataReader.Read())
//            {
//                idTed = Convert.ToInt32(dataReader["Id"]);

//                if (!idTEDsExistentes.Contains(idTed)) //TED Nova
//                {
//                    var agencia = dataReader["Agencia"].ToString();
//                    var conta = dataReader["Conta"].ToString();


//                    novasTEDs.Add(new TED
//                    {
//                        Id = Convert.ToInt32(dataReader["Id"]),
//                        Data = Convert.ToDateTime(dataReader["Data"]),
//                        Agencia = dataReader["Agencia"].ToString(),
//                        Conta = dataReader["Conta"].ToString(),
//                        Valor = Convert.ToDecimal(dataReader["Valor"]),
//                        MatriculaConsultor = dataReader["MatriculaConsultor"].ToString(),
//                        NomeConsultor = dataReader["NomeConsultor"].ToString(),
//                        Area = dataReader["Area"].ToString(),
//                        Notificado = false
//                    });
//                }
//            }

//            dataReader.Close();
//        }
//    }

//    return novasTEDs;
//}


//private static Dictionary<int, String> ObterListaGerentesArea(SqlConnection connection)
//{
//    var codigoGerenteArea = new Dictionary<int, String>();

//    using (var sqlCommand = new SqlCommand("SP_TbTedsRecebidas", connection))
//    {
//        sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;

//        sqlCommand.Parameters.AddWithValue("@OPERACAO", "SelectGerentesConsolidadaPgp");

//        //OBTEM A LISTA DE GERENTES DA PGP
//        using (var dataReader = sqlCommand.ExecuteReader())
//        {
//            while (dataReader.Read())
//            {
//                if (int.TryParse(dataReader["Funcional"]?.ToString(), out var funcional))
//                    codigoGerenteArea[funcional] = dataReader["Area"]?.ToString();
//            }

//            dataReader.Close();
//        }
//    }

//    return codigoGerenteArea;
//}


//private static List<TED> ObterListaTEDsPorGerente(HashSet<int> idTEDsExistentes, bool notificado, int codigoFuncionalGerente, string area, SqlConnection connection)
//{
//    var novasTEDs = new List<TED>();

//    using (var sqlCommand = new SqlCommand("SP_TbTedsRecebidas", connection))
//    {
//        sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;

//        sqlCommand.Parameters.AddWithValue("@OPERACAO", "SelectRegistrosPendentesPgp");
//        sqlCommand.Parameters.AddWithValue("@Notificado", notificado);
//        sqlCommand.Parameters.AddWithValue("@Funcional", codigoFuncionalGerente);

//        var idTed = default(int);

//        using (var dataReader = sqlCommand.ExecuteReader())
//        {
//            while (dataReader.Read())
//            {
//                idTed = Convert.ToInt32(dataReader["Id"]);

//                if (!idTEDsExistentes.Contains(idTed)) //TED Nova
//                {
//                    novasTEDs.Add(new TED
//                    {
//                        Id = Convert.ToInt32(dataReader["Id"]),
//                        Data = Convert.ToDateTime(dataReader["Data"]),
//                        Agencia = dataReader["Agencia"].ToString(),
//                        Conta = dataReader["Conta"].ToString(),
//                        Valor = Convert.ToDecimal(dataReader["Valor"]),
//                        Area = area,
//                        MatriculaSupervisor = codigoFuncionalGerente.ToString(),
//                        NomeSupervisor = dataReader["Gerente"].ToString(),
//                        Notificado = false
//                    });
//                }
//            }

//            dataReader.Close();
//        }
//    }

//    return novasTEDs;
//}

#endregion