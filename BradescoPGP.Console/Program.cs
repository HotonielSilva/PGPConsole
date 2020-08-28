using BradescoPGP.Repositorio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Configuration;
using BradescoPGPConsole;
using BradescoPGPConsole.Commands;
using BradescoPGP.Console.Cammands;

namespace BradescoPGP.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            #region InserçãoManual
            using (var db = new PGPEntities())
            {
                var entity = db.WindowsServiceConfig.FirstOrDefault(c => c.Tarefa == Comando.ImportarCaptacaoLiquida.ToString());

                var qu = new ImportarCaptacaoLiquida(new ServiceConfig
                {
                    Caminho = entity.CaminhoOrigem,
                    UltimaExecucao = entity.UltimaExecucao.AddMonths(-12),
                    IntervaloExecucao = TimeSpan.Parse(entity.IntervaloExecucao),
                    PadraoPesquisa = entity.PadraoPesquisa,
                    DataUltimaModificacao = entity.DataUltimaModificacao?.AddDays(-10),
                    Tarefa = entity.Tarefa,
                    Acrescentar = entity.Acrescentar.Value,
                    EmExecucao = false,
                    NomeArquivo = entity.NomeArquivo,
                    TeveFalha = false
                });

                if (qu.EstaPronto())
                {
                    qu.Importar();
                }
            }
            #endregion

            //ThreadPool.QueueUserWorkItem(Initialize);

            //System.Console.ReadLine();

            //Common.Logging.Log.Information("Finalizando PGP Service...");
        }

        static void Initialize(object state)
        {
            Common.Logging.Log.Information("Iniciando PGP Service...");

            while (true)
            {
                Execute();
                var sleepTime = TimeSpan.Parse(ConfigurationManager.AppSettings["IntervaloExecucao"]);
                Thread.Sleep(sleepTime);
            }
        }

        static void Execute()
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("pt-BR");

            var commands = new List<ICommand>();

            using (var db = new PGPEntities())
            {
                var tasks = Enum.GetNames(typeof(Comando));

                foreach (var task in tasks)
                {
                    var entity = db.WindowsServiceConfig.FirstOrDefault(c => c.Tarefa == task);

                    if (entity is null)
                        continue;

                    var exececoesCommandos = new string[]
                    {
                        Comando.CopiarArquivosParaDownload.ToString(),
                        Comando.ImportarAplicacaoResgate.ToString()
                    };

                    if (!exececoesCommandos.Contains(entity.Tarefa) &&
                        entity.UltimaExecucao.Date == DateTime.Now.Date)
                    {
                        continue;
                    }

                    var asm = typeof(Comando).Assembly;

                    var commandType = asm.GetType($"BradescoPGPConsole.Commands.{task}");

                    var command = (ICommand)Activator.CreateInstance(commandType,
                        args: new object[] {
                            new ServiceConfig {
                                Caminho = entity.CaminhoOrigem,
                                UltimaExecucao = entity.UltimaExecucao,
                                IntervaloExecucao = TimeSpan.Parse(entity.IntervaloExecucao),
                                PadraoPesquisa = entity.PadraoPesquisa,
                                Tarefa = entity.Tarefa,
                                Acrescentar = entity.Acrescentar.Value,
                                DataUltimaModificacao = entity.DataUltimaModificacao,
                                EmExecucao = entity.EmExecucao,
                                NomeArquivo = entity.NomeArquivo,
                                TeveFalha = entity.TeveFalha
                            }
                        });

                    commands.Add(command);
                }

                var invoker = new InvokerImports(commands);

                invoker.IniciarImportacoes();
            }
        }
    }
}
