using BradescoPGP.Console;
using BradescoPGP.Console.ModelsExport;
using BradescoPGP.Repositorio;
using BradescoPGPConsole.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;

namespace BradescoPGPConsole.Commands
{
    public class Expurgo : AbstractCommand
    {
        public Expurgo(ServiceConfig config) : base(config)
        { }

        protected override void PrepararDados()
        { }

        protected override void RealizarCarga()
        {
            ExpurgoTeds();

            ExpurgoVencimentos();

            ExpurgoAplicacaoResgate();

            ExpurgoPipeline();

            ExpurgoLog();

        }

        /// <summary>
        /// Expurgo dos dados apartir do 4 mês anterior
        /// </summary>
        private void ExpurgoTeds()
        {
            BradescoPGP.Common.Logging.Log.Information("Iniciando expurgo de TEDs");

            var dataAtual = DateTime.Now.Date;
            var mes = dataAtual.AddMonths(-4).Month;
            var ano = dataAtual.AddMonths(-4).Year;
            var destino = ConfigurationManager.AppSettings["DestinoExpurgoTED"];

            using (var db = new PGPEntities())
            {
                try
                {
                    var Teds = db.TED.Include(s => s.TedsAplicacoes).Include(s => s.TedsContatos)
                         .Where(r => r.Data.Month <= mes && r.Data.Year == ano)
                         .ToList();

                    var tedsParaExpurgo = Teds
                        .Join(db.Usuario,
                            t => new { matricula = t.MatriculaConsultor },
                            u => new { matricula = u.Matricula },
                             (t, u) => new { u.Equipe, TED = t })
                             .Where(r => r.TED.Data.Month <= mes && r.TED.Data.Year == ano)
                             .ToList();

                    var idsTedsEncontradas = tedsParaExpurgo.Select(s => s.TED.Id).ToList();

                    var tedsNaoRelacionadas = Teds
                             .Where(r => !idsTedsEncontradas.Contains(r.Id))
                             .ToList();

                    tedsParaExpurgo.AddRange(tedsNaoRelacionadas.ConvertAll(t => new { Equipe = "", TED = t }));

                    if (!tedsParaExpurgo.Any())
                        return;


                    Teds.Clear();

                    var tedsAgrupdas = tedsParaExpurgo.GroupBy(g => new { g.TED.Data.Month, g.TED.Data.Year }).ToList();

                    tedsAgrupdas.ForEach(t =>
                    {
                        var modelo = t.ToList().ConvertAll(ted => TedExportModel.Mapear(ted.TED, ted.Equipe));
                        var nomeArquivo = $"TED_ExpurgoPGP_{t.First().TED.Data.ToString("yyyyMM")}.xlsx";
                        var nomePlanilha = $"TED_ExpurgoPGP_{t.First().TED.Data.ToString("yyyyMM")}";
                        GerarExcel.Criar(modelo, nomePlanilha, nomeArquivo, destino);
                    });

                    var remover = tedsParaExpurgo.Select(s => s.TED).ToList();

                    ExpurgoService.ExpurgoTEDs(remover, db.Database.Connection.ConnectionString);

                }
                catch (Exception ex)
                {
                    Config.TeveFalha = true;

                    BradescoPGP.Common.Logging.Log.Error("EXPURGO DASHBOARD: Erro ao processar o expurgo de dados de TEDs.", ex);
                }

                BradescoPGP.Common.Logging.Log.Information("Expurgo de TEDs finalizado com sucesso.");

            }
        }

        /// <summary>
        /// Expurgo dos dados apartir do 4 mês anterior
        /// </summary>
        private void ExpurgoVencimentos()
        {
            BradescoPGP.Common.Logging.Log.Information("Iniciando expurgo de Vencimentos");

            var dataAtual = DateTime.Now;
            var mes = dataAtual.AddMonths(-4).Month;
            var ano = dataAtual.AddMonths(-4).Year;
            var destino = ConfigurationManager.AppSettings["DestinoExpurgoVencimento"];

            using (var db = new PGPEntities())
            {
                try
                {
                    var vencimentos = db.Vencimento
                    .Where(r => r.Dt_Vecto_Contratado.Month <= mes && r.Dt_Vecto_Contratado.Year <= ano)
                    .ToList();

                    var vencimentosParaExpurgo = vencimentos
                        .Join(db.Encarteiramento, v => new { agencia = v.Cod_Agencia.ToString(), conta = v.Cod_Conta_Corrente.ToString() },
                        enc => new { agencia = enc.Agencia, conta = enc.Conta }, (v, e) => new { v, e.Matricula })
                        .Join(db.Usuario, r => r.Matricula, u => u.Matricula, (r, u) => new { r.v, u.Nome, u.Matricula, u.Equipe })
                        .Where(r => r.v.Dt_Vecto_Contratado.Month <= mes && r.v.Dt_Vecto_Contratado.Year <= ano)
                        .ToList();

                    var idsTedsEncontradas = vencimentosParaExpurgo.Select(s => s.v.Id).ToList();

                    var vencimentosNaoRelacionados = vencimentos.Where(s => !idsTedsEncontradas.Contains(s.Id)).ToList();

                    vencimentosParaExpurgo
                        .AddRange(vencimentosNaoRelacionados.ConvertAll(v => new { v, Nome = "", Matricula = "", Equipe = "" }));

                    if (!vencimentosParaExpurgo.Any())
                        return;

                    var vencimentosAgrupados = vencimentosParaExpurgo
                        .GroupBy(g => new { g.v.Dt_Vecto_Contratado.Month, g.v.Dt_Vecto_Contratado.Year })
                        .ToList();

                    vencimentosAgrupados.ForEach(ven =>
                    {
                        var nomeArquivo = $"Vencimento_ExpurgoPGP_{ven.First().v.Dt_Vecto_Contratado.ToString("yyyyMM")}.xlsx";
                        var nomePlanilha = $"Vencimentos_{ven.First().v.Dt_Vecto_Contratado.ToString("yyyyMM")}";

                        var dadosExcel = ven.ToList().ConvertAll(v => VencimentoExportModel.MapearExcel(v.v, v.Nome, v.Matricula, v.Equipe));

                        GerarExcel.Criar(dadosExcel, nomePlanilha, nomeArquivo, destino);
                    });

                    var remover = vencimentosParaExpurgo.Select(s => s.v).ToList();

                    ExpurgoService.ExpurgoVencimentos(remover, db.Database.Connection.ConnectionString);
                }
                catch (Exception ex)
                {
                    Config.TeveFalha = true;

                    BradescoPGP.Common.Logging.Log.Error("EXPURGO DASHBOARD: Erro no processo de expurgo de dados de vencimentos.", ex);
                }

                BradescoPGP.Common.Logging.Log.Information("Expurgo de vencimentos finalizado com sucesso.");

            }
        }

        /// <summary>
        /// Expurgo de dados apartir do 13 mês anterior.
        /// </summary>
        private void ExpurgoPipeline()
        {
            BradescoPGP.Common.Logging.Log.Information("Iniciando expurgo de Pipeline");

            var dataAtual = DateTime.Now;
            var mes = dataAtual.AddMonths(-13).Month;
            var ano = dataAtual.AddMonths(-13).Year;
            var destino = ConfigurationManager.AppSettings["DestinoExpurgoPipeline"];

            using (var db = new PGPEntities())
            {
                try
                {
                    var entities = db.Pipeline.Include("Status").Include("Origem").Include("Motivo")
                    .Where(p => p.DataPrevista.Month <= mes && p.DataPrevista.Year <= ano)
                    .ToList();

                    if (!entities.Any())
                        return;

                    var vencimentosAgrupados = entities.GroupBy(g => new { g.DataPrevista.Month, g.DataPrevista.Year }).ToList();

                    vencimentosAgrupados.ForEach(pipe =>
                    {
                        var nomeArquivo = $"Pipelines_ExpurgoPGP_{pipe.First().DataPrevista.ToString("yyyyMM")}.xlsx";
                        var nomePlanilha = $"Pipelines_{pipe.First().DataPrevista.ToString("yyyyMM")}";

                        var dadosExcel = pipe.ToList().ConvertAll(p => PipelineExportModel.Mapear(p));

                        GerarExcel.Criar(dadosExcel, nomePlanilha, nomeArquivo, destino);
                    });


                    ExpurgoService.ExpurgoPipelines(entities, db.Database.Connection.ConnectionString);
                }
                catch (Exception ex)
                {
                    Config.TeveFalha = true;

                    BradescoPGP.Common.Logging.Log.Error("EXPURGO DASHBOARD: Erro no processo de expurgo de dados de pipelines.", ex);
                }

                BradescoPGP.Common.Logging.Log.Information("Expurgo de Pipeline finalizado com sucesso.");
            }
        }

        /// <summary>
        /// Expurgo dos dados apartir do 3 mês anterior
        /// </summary>
        public void ExpurgoLog()
        {
            BradescoPGP.Common.Logging.Log.Information("Iniciando expurgo de Log");

            var dataAtual = DateTime.Now;

            var dataExclusao = default(DateTime);
           
            using (var db = new PGPEntities())
            {
                try
                {
                    var comandos = Enum.GetNames(typeof(Comando));

                    var entities = new List<Log>();

                    foreach (var comando in comandos)
                    {
                        var entidadeConfi = db.WindowsServiceConfig.FirstOrDefault(c => c.Tarefa == comando);

                        if (entidadeConfi == null)
                            continue;

                        var intervaloExecucao = entidadeConfi?.IntervaloExecucao;

                        var nomeArquivoConfi = entidadeConfi?.NomeArquivo;
                        
                        if (!string.IsNullOrEmpty(intervaloExecucao) && TimeSpan.Parse(intervaloExecucao).Days == 30)
                        {
                            dataExclusao = dataAtual.AddMonths(-11);
                            
                            entities.AddRange(
                                db.Log.Where(p => p.Date <= dataExclusao && 
                                p.Message.StartsWith(nomeArquivoConfi)).ToList());
                        }
                        else
                        {
                            dataExclusao = dataAtual.AddDays(-11);
                            
                            entities.AddRange(db.Log.Where(p => p.Date <= dataExclusao &&
                            p.Message.StartsWith(nomeArquivoConfi)).ToList());
                        }
                    }

                    if (!entities.Any())
                        return;

                    var logsAgrupados = entities.GroupBy(g => new { g.Date.Month, g.Date.Year }).ToList();

                    logsAgrupados.ForEach(log =>
                    {
                        var nomeArquivo = $"Log_ExpurgoPGP_{log.First().Date.ToString("yyyyMM")}.txt";

                        var diretorio = ConfigurationManager.AppSettings["DestinoLog"];

                        var path = Path.Combine(diretorio, nomeArquivo);

                        var csv = CSV.Criar(";", log.ToList());

                        try
                        {
                            File.WriteAllText(path, csv);
                        }
                        catch (Exception ex)
                        {

                            Config.TeveFalha = true;

                            BradescoPGP.Common.Logging.Log.Error("EXPURGO DASHBOARD: Erro ao criar arquivo de expurgo de log", ex);

                            return;
                        }
                    });

                    ExpurgoService.ExpurgoLOGs(entities, db.Database.Connection.ConnectionString);

                }
                catch (Exception ex)
                {
                    Config.TeveFalha = true;

                    BradescoPGP.Common.Logging.Log.Error("EXPURGO DASHBOARD: Erro no processo de expurgo de dados de Log.", ex);
                }

                BradescoPGP.Common.Logging.Log.Information("Expurgo de Log finalizado com sucesso.");

            }
        }

        /// <summary>
        /// Expurgo dos dados apartir do 4 mês anterior
        /// </summary>
        private void ExpurgoAplicacaoResgate()
        {
            BradescoPGP.Common.Logging.Log.Information("Iniciando expurgo de Aplicação e Resgate");

            var dataAtual = DateTime.Now.Date;
            var mes = dataAtual.AddMonths(-4).Month;
            var ano = dataAtual.AddMonths(-4).Year;
            var destino = ConfigurationManager.AppSettings["DestinoExpurgoAplicacaoResgate"];

            using (var db = new PGPEntities())
            {
                try
                {
                    var aplicacoes = db.AplicacaoResgate.Where(s => s.data.Month <= mes && s.data.Year <= ano).ToList();

                    var aplicResgateExpurgo = aplicacoes
                            .Join(db.Usuario, ar => ar.MatriculaConsultor, u => u.Matricula, (ar, u) => new { ar, u.Nome })
                           .Where(s => s.ar.data.Month <= mes && s.ar.data.Year <= ano).ToList();

                    var idsRelacionados = aplicResgateExpurgo.Select(s => s.ar.Id).ToList();

                    var aplicacaoesNaoRelacionadas = aplicacoes
                        .Where(a => a.data.Month <= mes && a.data.Year <= ano && !idsRelacionados.Contains(a.Id)).ToList();

                    aplicResgateExpurgo.AddRange(aplicacaoesNaoRelacionadas.ConvertAll(t => new { ar = t, Nome = string.Empty }));

                    if (!aplicResgateExpurgo.Any())
                        return;

                    var aplicacoesAgrupdas = aplicResgateExpurgo.GroupBy(g => new { g.ar.data.Month, g.ar.data.Year }).ToList();

                    aplicacoesAgrupdas.ForEach(t =>
                    {
                        var model = t.ToList().ConvertAll(a => AplicacaoResgateExportModel.Mapear(a.ar, a.Nome));

                        var nomeArquivo = $"AplicacaoResgate_ExpurgoPGP_{t.First().ar.data.ToString("yyyyMM")}.xlsx";
                        var nomePlanilha = $"AplicacaoResgate_ExpurgoPGP_{t.First().ar.data.ToString("yyyyMM")}";
                        GerarExcel.Criar(model, nomePlanilha, nomeArquivo, destino);
                    });

                    var remover = aplicResgateExpurgo.Select(s => s.ar).ToList();

                    ExpurgoService.ExpurgoAplicacaoResgates(remover, db.Database.Connection.ConnectionString);

                }
                catch (Exception ex)
                {
                    Config.TeveFalha = true;

                    BradescoPGP.Common.Logging.Log.Error("APLICAÇÃO RESGATE: Erro ao processar o expurgo de dados de TEDs.", ex);
                }
            }
        }
    }
}
