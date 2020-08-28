using BradescoPGP.Common;
using BradescoPGP.Repositorio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BradescoPGPConsole.Commands
{
    public class ImportarCaminhoDinheiro : AbstractCommand
    {
        public ImportarCaminhoDinheiro(ServiceConfig config) : base(config)
        {
        }

        protected override void PrepararDados()
        {
            TruncarTabela("CaminhoDinheiro");
        }

        protected override void RealizarCarga()
        {
            BradescoPGP.Common.Logging.Log.Information("Iniciando importação Caminho do Dinheiro");

            int linhacount = 0;

            try
            {

                var listaCamDin = new List<CaminhoDinheiro>();

                using (var sr = new StreamReader(Config.Caminho))
                {
                    string linha = null;

                    string[] linhaDividida = null;

                    while ((linha = sr.ReadLine()) != null)
                    {
                        linhaDividida = linha.SplitWithQualifier(';', '\"', true);

                        if (linhacount > 0)
                        {
                            var camDin = new CaminhoDinheiro();

                            camDin.MesDataBase = linhaDividida[0];

                            if (string.IsNullOrEmpty(linhaDividida[1]) || string.IsNullOrEmpty(linhaDividida[2]))
                            {
                                var nomeMatriCord = ObterNomeCordenador(linhaDividida[3] ,ref listaCamDin);

                                camDin.MatriculaCordenador = nomeMatriCord[0];

                                camDin.Cordenador = nomeMatriCord[1];
                            }
                            else
                            {
                                camDin.MatriculaCordenador = linhaDividida[1];

                                camDin.Cordenador = linhaDividida[2];
                            }

                            camDin.MatriculaConsultor = linhaDividida[3];

                            camDin.Consultor = linhaDividida[4];

                            camDin.Agencia = linhaDividida[5];

                            camDin.Ag_Conta = linhaDividida[6];

                            camDin.Segmento = linhaDividida[7];

                            camDin.Produto = linhaDividida[8];

                            if (decimal.TryParse(linhaDividida[9], out decimal vlAplic)) camDin.VL_APLIC = vlAplic;

                            if (decimal.TryParse(linhaDividida[11], out decimal percDinNovo)) camDin.PERC_DINHEIRO_NOVO = percDinNovo;

                            if (decimal.TryParse(linhaDividida[12], out decimal vlResg)) camDin.VL_RESG = vlResg;

                            if (decimal.TryParse(linhaDividida[14], out decimal perCPMS)) camDin.PERC_COMPROMISSADAS = perCPMS;

                            if (decimal.TryParse(linhaDividida[16], out decimal percCorret)) camDin.PERC_CORRET = percCorret;

                            if (decimal.TryParse(linhaDividida[18], out decimal percFundos)) camDin.PERC_FUNDOS = percFundos;

                            if (decimal.TryParse(linhaDividida[20], out decimal percPrevi)) camDin.PERC_PREVI= percPrevi;

                            if (decimal.TryParse(linhaDividida[22], out decimal percCDB)) camDin.PERC_CDB = percCDB;

                            if (decimal.TryParse(linhaDividida[24], out decimal percIentos)) camDin.PERC_ISENTOS = percIentos;

                            if (decimal.TryParse(linhaDividida[26], out decimal percLF)) camDin.PERC_LF = percIentos;

                            if (decimal.TryParse(linhaDividida[28], out decimal percMSMTITENVTL)) camDin.PERC_MSM_TIT_ENV_TOTAL = percMSMTITENVTL;

                            if (decimal.TryParse(linhaDividida[30], out decimal percDIFTITENVTL)) camDin.PERC_DIF_TIT_ENV_TOTAL = percDIFTITENVTL;

                            if (decimal.TryParse(linhaDividida[32], out decimal percOutros)) camDin.PERC_OUTROS = percOutros;


                            listaCamDin.Add(camDin);

                            if (listaCamDin.Count > 100000)
                            {
                                using (var db = new PGPEntities())
                                {
                                    db.BulkInsert(listaCamDin);

                                    listaCamDin.Clear();
                                }
                            }
                        }

                        linhacount++;
                    }

                    if (listaCamDin.Count > 0)
                    {
                        using (var db = new PGPEntities())
                        {
                            db.BulkInsert(listaCamDin);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error("Caminho do Dinheiro: Erro ao importar Caminho do Dinheiro.", ex);
            }

            BradescoPGP.Common.Logging.Log.Information("improtacao Caminho do Dinheiro Finaliada");
        }

        private string[] ObterNomeCordenador(string matriculaConsultor, ref List<CaminhoDinheiro> listaImport)
        {
            if(listaImport.Count > 0)
            {
                var resultMemory = listaImport
                    .FirstOrDefault(s => s.MatriculaConsultor == matriculaConsultor && !string.IsNullOrEmpty(s.MatriculaCordenador) && !string.IsNullOrEmpty(s.Cordenador));

                if(resultMemory != null)
                {
                    return new string[] { resultMemory.MatriculaCordenador, resultMemory.Cordenador };
                }
            }

            using (var db = new PGPEntities())
            {

                var camdinImportado = db.CaminhoDinheiro
                    .FirstOrDefault(s => s.MatriculaConsultor == matriculaConsultor && !string.IsNullOrEmpty(s.MatriculaCordenador) && !string.IsNullOrEmpty(s.Cordenador));

                if (camdinImportado != null)
                {
                    return new string[] { camdinImportado.MatriculaCordenador, camdinImportado.Cordenador };
                }

                var usuario = db.Usuario.FirstOrDefault(s => s.Matricula == matriculaConsultor);

                return new string[] { usuario.MatriculaSupervisor, usuario.NomeSupervisor };
            }
        }

        public static ImportarCaminhoDinheiro ObterInstancia()
        {
            using (var db = new PGPEntities())
            {
                var entity = db.WindowsServiceConfig.FirstOrDefault(s => s.Tarefa == nameof(ImportarCaminhoDinheiro));

                return new ImportarCaminhoDinheiro(new ServiceConfig
                {
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
                });
            }
        }
    }
}

