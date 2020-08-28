using BradescoPGP.Console;
using BradescoPGP.Repositorio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace BradescoPGPConsole.Commands
{
    public abstract class AbstractCommand : ICommand
    {
        public ServiceConfig Config { get; protected set; }

        public AbstractCommand(ServiceConfig config)
        {
            Config = config;
        }

        private bool ArquivoEmUso()
        {
            try
            {
                using (File.OpenRead(Config.Caminho)) { }
            }
            catch (Exception e)
            {
                var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);
                return errorCode == 32 || errorCode == 33;
            }

            return false;
        }

        public bool EstaPronto()
        {
            if (string.IsNullOrWhiteSpace(Config.Caminho))
            {
                if ((DateTime.Now - Config.UltimaExecucao) >= Config.IntervaloExecucao && Config.EmExecucao == false)
                {
                    using (var db = new PGPEntities())
                    {
                        var entity = db.WindowsServiceConfig.First(c => c.Tarefa == Config.Tarefa.ToString());

                        if (Config.Tarefa != Comando.CopiarArquivosParaDownload.ToString())
                            entity.EmExecucao = true;

                        db.SaveChanges();
                    }

                    return true;
                }

                return false;
            }

            if (Config.Tarefa == Comando.CopiarArquivosParaDownload.ToString() ||  !Config.TeveFalha)
            {
                var arquivos = default(string[]);

                try
                {
                    arquivos = Directory.GetFiles(Config.Caminho, Config.PadraoPesquisa);
                }
                catch (Exception ex)
                {
                    Config.TeveFalha = true;

                    AtualizarEstado();

                    BradescoPGP.Common.Logging.Log.Error($"{Config.NomeArquivo}: Erro ao tentar acessar os arquivos no diretorio {Config.Caminho}", ex);

                    return false;
                }

                if (arquivos.Length == 0)
                {
                    if ((DateTime.Now - Config.UltimaExecucao) >= Config.IntervaloExecucao)
                    {
                        Config.TeveFalha = true;

                        AtualizarEstado();

                        BradescoPGP.Common.Logging.Log.Error($"{Config.NomeArquivo}: {Config.Tarefa} está pronto para executar, porém não foi possível localizar o arquivo (Caminho: {Config.Caminho}, Padrão de pesquisa: {Config.PadraoPesquisa}).");
                    }

                    return false;
                }

                Config.Caminho = arquivos.Select(c => new FileInfo(c)).OrderByDescending(c => c.Name).First().FullName;

                if ((DateTime.Now - Config.UltimaExecucao) >= Config.IntervaloExecucao && Config.EmExecucao == false)
                {
                    var arquivoEmUso = ArquivoEmUso();

                    if (/*Config.Tarefa == Comando.ImportarCockpit.ToString() && */arquivoEmUso)
                    {
                        Config.TeveFalha = false;

                        AtualizarEstado();

                        BradescoPGP.Common.Logging.Log.Debug($"O arquivo '{Path.GetFileName(Config.Caminho)}' está em uso no momento e será importado na próxima execução");

                        return false;
                    }

                    if (Config.Tarefa != Comando.CopiarArquivosParaDownload.ToString())
                    {
                        var ultimaModificacaoArquivo = File.GetLastWriteTime(Config.Caminho);

                        //Truncate milessegundos para comparação.
                        ultimaModificacaoArquivo = ultimaModificacaoArquivo.AddTicks(-(ultimaModificacaoArquivo.Ticks % TimeSpan.TicksPerSecond));

                        var ultimaModificacaoBase = Config.DataUltimaModificacao ?? null;

                        if (ultimaModificacaoBase != null)
                        {
                            //Truncate milessegundos para comparação.
                            ultimaModificacaoBase = ultimaModificacaoBase.Value.AddTicks(-(ultimaModificacaoBase.Value.Ticks % TimeSpan.TicksPerSecond));

                            if (ultimaModificacaoBase >= ultimaModificacaoArquivo)
                            {
                                Config.TeveFalha = false;

                                AtualizarEstado(true);

                                var nomeArquivo = new FileInfo(Config.Caminho).Name;

                                //BradescoPGP.Common.Logging.Log.Information($"O arquivo {nomeArquivo} já foi importado e não houve alterações recentemente");

                                return false;
                            }
                        }
                    }

                    if (Config.Tarefa != Comando.CopiarArquivosParaDownload.ToString())
                    {
                        using (var db = new PGPEntities())
                        {
                            var entity = db.WindowsServiceConfig.First(c => c.Tarefa == Config.Tarefa.ToString());

                            entity.EmExecucao = true;

                            db.SaveChanges();
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        protected virtual void AtualizarEstado(bool atualizarApenasTeveFalha = false)
        {
            using (var db = new PGPEntities())
            {
                var entity = db.WindowsServiceConfig.First(c => c.Tarefa == Config.Tarefa);

                var comandosLiberados = new List<string>
                {
                    Comando.Expurgo.ToString(),
                    Comando.CopiarArquivosParaDownload.ToString(),
                    Comando.ImportarAplicacaoResgate.ToString()
                };

                if ((comandosLiberados.Contains(Config.Tarefa)  && !Config.TeveFalha ) ||
                    (!Config.TeveFalha && !atualizarApenasTeveFalha))
                {
                    var ultimaModificacao = string.IsNullOrEmpty(Config.Caminho) ? DateTime.Now : File.GetLastWriteTime(Config.Caminho);

                    entity.UltimaExecucao = DateTime.Now;

                    entity.DataUltimaModificacao = ultimaModificacao;

                    if (!string.IsNullOrEmpty(Config.Caminho))
                        entity.UltimoArquivo = new FileInfo(Config.Caminho).Name;
                }

                entity.TeveFalha = Config.TeveFalha;

                entity.EmExecucao = false;

                db.SaveChanges();
            }

            //Exclui arquivo Temporario
            if (Config.Tarefa == Comando.ImportarCockpit.ToString() && !Config.TeveFalha)
            {
                File.Delete(Config.Caminho);
            }
        }

        protected void TruncarTabela(string nomeTabela)
        {
            using (var db = new PGPEntities())
            {
               BradescoPGP.Common.Logging.Log.Information($"Iniciando limpeza da tabela '{nomeTabela}'.");

                db.Database.ExecuteSqlCommand($"TRUNCATE TABLE [{nomeTabela}]");

                BradescoPGP.Common.Logging.Log.Information($"Limpeza da tabela '{nomeTabela}' finalizada.");
            }
        }

        protected void ExecuteQuery(string query)
        {
            using (var db = new PGPEntities())
            {
                db.Database.ExecuteSqlCommand(query);

                BradescoPGP.Common.Logging.Log.Information($"Execução finalizada.");
            }
        }

        protected abstract void PrepararDados();

        protected abstract void RealizarCarga();

        private void LimparDiretorio()
        {
            if (Config.Caminho == null)
                return;

            var arquivoAtual = new FileInfo(Config.Caminho);

            var destino = @"\\d4898s001\D4898\Compartilhado\Entre_Secoes\D4898S022_Suporte\SCOPUS\DASHBOARD_PGP\Expurgo\Arquivos\" + arquivoAtual.Name;

            try
            {
                File.Copy(Config.Caminho, destino, true);
            }
            catch (Exception e)
            {
                BradescoPGP.Common.Logging.Log.Debug($"{Config.NomeArquivo}: Erro ao mover arquivo importado para o destino: {destino}", e);

                return;
            }

            var arquivos = Directory.GetFiles(arquivoAtual.DirectoryName, Config.PadraoPesquisa);

            if (arquivos.Length == 0)
                return;

            foreach (var arquivo in arquivos)
            {
                try
                {
                    File.Delete(arquivo);
                }
                catch (Exception e)
                {
                   BradescoPGP.Common.Logging.Log.Debug($"{Config.NomeArquivo}: Erro ao deletar arquivo na pasta raiz 'DASHBOARD'", e);
                }
            }
        }

        public virtual void Importar()
        {
            if (!Config.Acrescentar)
            {
                PrepararDados();
            }

            RealizarCarga();

            AtualizarEstado();

            if (!Config.TeveFalha)
            {
                LimparDiretorio();
            }
        }
    }
}
