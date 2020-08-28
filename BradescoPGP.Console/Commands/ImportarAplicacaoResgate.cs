using BradescoPGP.Repositorio;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace BradescoPGPConsole.Commands
{
    public class ImportarAplicacaoResgate : AbstractCommand
    {
        private class MapeamentoConsultor
        {
            public String Matricula { get; set; }
        }

        public ImportarAplicacaoResgate(ServiceConfig config): base(config)
        {
        }

        protected override void PrepararDados()
        {
            TruncarTabela("AplicacaoResgate");
        }

        protected override void RealizarCarga()
        {
            BradescoPGP.Common.Logging.Log.Information("Iniciando importação de Aplicação e Resgate");

            var idAplicacoesResgateExistentes = new HashSet<int>();

            var encarteiramentoAtual = new Dictionary<String, string>();

            try
            {
                using (var db = new PGPEntities())
                {
                    //Obtem a lista de Id Registros já importados
                    idAplicacoesResgateExistentes = new HashSet<int>(db.AplicacaoResgate.Select(t => t.Id));

                    //Indexa o Encarteiramento por Agencia e Conta
                    foreach (var e in db.Encarteiramento)
                    {
                        encarteiramentoAtual[$"{e.Agencia}-{e.Conta}"] = e.Matricula;
                    }

                    var novasAplicacoesResgates = ObterAplicacaoResgate(idAplicacoesResgateExistentes, encarteiramentoAtual);

                    //Persistencia no banco
                    if (novasAplicacoesResgates.Count > 0)
                    {
                        var qtdRegistrosCadastrados = 0;

                        var maxRegistros = 10000;

                        if (novasAplicacoesResgates.Count > 10000)
                        {
                            while (qtdRegistrosCadastrados < novasAplicacoesResgates.Count)
                            {
                                var inserir = novasAplicacoesResgates.Skip(qtdRegistrosCadastrados).Take(maxRegistros).ToList();

                                db.BulkInsert(inserir);

                                qtdRegistrosCadastrados += inserir.Count;
                            }
                        }
                        else
                        {
                            db.BulkInsert(novasAplicacoesResgates);
                        }
                    }

                    BradescoPGP.Common.Logging.Log.Information("Importação de Aplicação e Resgate Finalizada com sucesso");
                }
            }
            catch (Exception ex)
            {
                Config.TeveFalha = false;

                BradescoPGP.Common.Logging.Log.Error("APLICAÇÃO RESGATE: Erro ao importar aplicações e resgates", ex);
            }
        }

        private List<AplicacaoResgate> ObterAplicacaoResgate(HashSet<int> idAplicacoesResgateExistentes, Dictionary<String, string> encarteiramento)
        {
            var result = new List<AplicacaoResgate>();

            var id = default(int);

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ExtratoPrime"].ConnectionString))
            {
                connection.Open();

                var dataAtual = DateTime.Now.Date;

                var minDate = new DateTime(dataAtual.Year, dataAtual.Month, 1).AddMonths(-3);

                var maxDate = new DateTime(dataAtual.Year, dataAtual.Month, DateTime.DaysInMonth(dataAtual.Year, dataAtual.Month));

                var paramMinDate = minDate.ToString("yyyy-MM-dd");
                var paramMaxDate = maxDate.ToString("yyyy-MM-dd");

                var query = $"SELECT * FROM tbmovimentacoes WHERE (valor <= -50000 or valor >= 50000) and [data] BETWEEN '{paramMinDate}' AND '{paramMaxDate}' ";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var db = new PGPEntities())
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            id = (int)reader["id_movimentacoes"];
                            var agencia = (int)reader["agencia"];
                            var conta = int.Parse(reader["conta"].ToString());

                            if (!idAplicacoesResgateExistentes.Contains(id) && encarteiramento.ContainsKey($"{agencia}-{conta}"))
                            {
                                result.Add(new AplicacaoResgate
                                {
                                    Id = id,
                                    advisor = reader["advisor"].ToString(),
                                    agencia = agencia,
                                    conta = conta,
                                    data = DateTime.Parse(reader["data"].ToString()),
                                    hora = TimeSpan.Parse(reader["hora"].ToString()),
                                    enviado = bool.Parse(reader["enviado"].ToString()),
                                    gerente = reader["gerente"].ToString(),
                                    operacao = reader["operacao"].ToString(),
                                    perif = reader["perif"].ToString(),
                                    produto = reader["produto"].ToString(),
                                    segmento = reader["segmento"].ToString(),
                                    terminal = reader["terminal"].ToString(),
                                    valor = decimal.Parse(reader["valor"].ToString()),
                                    MatriculaConsultor = encarteiramento[$"{agencia}-{conta}"]
                                });
                            }
                        }
                        reader.Close();
                    }
                }

                connection.Close();
            }
            return result;
        }
    }
}
