using BradescoPGP.Common.Logging;
using BradescoPGP.Console;
using BradescoPGP.Repositorio;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace BradescoPGPConsole.Commands
{
    public class CopiarArquivosParaDownload : AbstractCommand
    {
        public CopiarArquivosParaDownload(ServiceConfig config) : base(config)
        {
        }

        protected override void PrepararDados()
        { }

        protected override void RealizarCarga()
        {
            BradescoPGP.Common.Logging.Log.Information("Iniciando cópias dos arquivos para download.");

            using (var db = new PGPEntities())
            {
                var arquivos = db.Links.Where(l => l.Url.Contains("file")).Select(l => l.Url.Replace("file:///", string.Empty)).ToList();

                #region CapLiq
                //var configInvCap = db.WindowsServiceConfig.Where(a => a.Tarefa == Comando.ImportarInvestFacil.ToString() ||
                //    a.Tarefa == Comando.ImportarCaptacaoLiquida.ToString()
                //).ToList();
                //var invest = configInvCap.First(a => a.Tarefa == Comando.ImportarInvestFacil.ToString());

                //var cap = configInvCap.First(a => a.Tarefa == Comando.ImportarCaptacaoLiquida.ToString());

                //var arquivosInvest = Directory.GetFiles(invest.CaminhoOrigem, invest.UltimoArquivo);

                //var invUrl = arquivosInvest.Any() ? arquivosInvest.First() : null;

                //var arquivosCap = Directory.GetFiles(cap.CaminhoOrigem, cap.UltimoArquivo);

                //var capUrl = arquivosCap.Any() ? arquivosCap.First() : null;

                //var listaArquivosCorrigodo = new List<string>();

                //arquivos.ForEach(a =>
                //{
                //    if (a.Contains("BD_INVEST_FACIL"))
                //    {
                //        listaArquivosCorrigodo.Add(invUrl);
                //    }
                //    else if (a.Contains("Diária Captação Líquida Investimentos"))
                //    {
                //        listaArquivosCorrigodo.Add(capUrl);
                //    }
                //    else
                //    {
                //        listaArquivosCorrigodo.Add(a);
                //    }
                //});

                //listaArquivosCorrigodo.RemoveAll(s => s == null);

                //arquivos = listaArquivosCorrigodo;
                #endregion

                if (arquivos.Count == 0)
                {
                    BradescoPGP.Common.Logging.Log.Information("Nehum arquivo para realizar cópia no momento");

                    return;
                }

                foreach (var arquivo in arquivos)
                {
                    var caminhoNovoArquivo = Path.Combine(ConfigurationManager.AppSettings["PastaDestinoDownloads"], new FileInfo(arquivo).Name);

                    var arquivoControle = caminhoNovoArquivo + ".txt";

                    var ultimaModificacaoArquivoOriginal = File.GetLastWriteTime(arquivo).Ticks;

                    if (File.Exists(arquivoControle))
                    {
                        var ultimaModificacao = long.Parse(File.ReadAllText(arquivoControle));

                        if (ultimaModificacaoArquivoOriginal <= ultimaModificacao)
                        {
                            continue;
                        }

                        BradescoPGP.Common.Logging.Log.InformationFormat("Foi encontrado uma versão mais recente do arquivo '{0}'. Iniciando cópia...", arquivo);
                    }

                    try
                    {

                        File.Copy(arquivo, caminhoNovoArquivo, true);

                        File.WriteAllText(arquivoControle, ultimaModificacaoArquivoOriginal.ToString());

                        ////Altera o caminho do arquivo de captação líquida para fazer download no portal
                        //if (arquivo.Contains("Diária Captação Líquida Investimentos"))
                        //{
                        //    var link = db.Links.First(l => l.Titulo.ToLower() == "capliq");
                        //    link.Url = "file:///" + arquivo;
                        //    db.SaveChanges();
                        //}
                    }
                    catch (Exception ex)
                    {

                        Config.TeveFalha = true;

                        BradescoPGP.Common.Logging.Log.Error("DOWNLOAD DE ARQUIVOS: Error ao copiar arquivos.", ex);

                        continue;
                    }

                    BradescoPGP.Common.Logging.Log.InformationFormat("O arquivo '{0}' foi copiado com sucesso.", arquivo);
                }

                BradescoPGP.Common.Logging.Log.Information("Cópias finalizadas.");
            }


        }
    }
}
