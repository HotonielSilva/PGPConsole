using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BradescoPGP.Repositorio;
using BradescoPGP.Common;

namespace BradescoPGPConsole.Commands
{
    public class ImportarInvestFacil : AbstractCommand
    {
        public ImportarInvestFacil(ServiceConfig config)
            : base(config)
        {
        }

        protected override void PrepararDados()
        {
            TruncarTabela("Investfacil");
        }

        protected override void RealizarCarga()
        {
            int linhaICount = 0;

            var currentObject = new Investfacil();
            try
            {
                BradescoPGP.Common.Logging.Log.Information("Iniciando importação de InvestFacil");

                using (var db = new PGPEntities())
                using (StreamReader sr = new StreamReader(Config.Caminho))
                {
                    string linhaAtual = null;

                    string[] linhaDividida = null;

                    var investFacilList = new List<Investfacil>();

                    while ((linhaAtual = sr.ReadLine()) != null)
                    {
                        if (linhaICount > 0)
                        {
                            linhaDividida = linhaAtual.SplitWithQualifier(';', '\"', true);

                            var investFacil = new Investfacil();

                            investFacil.SEGMENTO_CLIENTE = linhaDividida[0];
                            investFacil.AGENCIA = linhaDividida[1].Split('-')[0];
                            investFacil.CONTA = linhaDividida[1].Split('-')[1];
                            currentObject.AGENCIA = investFacil.AGENCIA;
                            currentObject.CONTA = investFacil.CONTA;
                            investFacil.NUM_CONTRATO = linhaDividida[3];
                            investFacil.MES_DT_BASE = linhaDividida[4];
                            if (!string.IsNullOrEmpty(linhaDividida[5])) investFacil.DT_EMISSAO = DateTime.Parse(linhaDividida[5]);
                            investFacil.PRAZO_PERMAN = int.Parse(linhaDividida[6]);
                            investFacil.FX_PERMANENCIA = linhaDividida[7];
                            investFacil.FX_VOLUME = linhaDividida[8];
                            investFacil.Vlr_Evento = decimal.Parse(linhaDividida[9]);
                            investFacil.SEGM_AGRUPADO = linhaDividida[10];
                            investFacil.SEGMENTO_MACRO = linhaDividida[11];
                            investFacil.MatriculaConsultor = linhaDividida[12];
                            investFacil.NomeConsultor = linhaDividida[13];
                            investFacil.MatriculaCordenador = linhaDividida[14];
                            investFacil.NomeCordenador= linhaDividida[15];
                            investFacil.Plataforma = linhaDividida[16];

                            investFacilList.Add(investFacil);

                            if (investFacilList.Count == 200000)
                            {
                                db.BulkInsert(investFacilList);

                                investFacilList.Clear();
                            }
                        }

                        linhaICount++;
                    }
                    if (investFacilList.Count > 0)
                    {
                        db.BulkInsert(investFacilList);
                    }
                }

                PreencherInvestFacilResumo();

                BradescoPGP.Common.Logging.Log.Information("Importação de InvestFacil finalizada");
            }
            catch (Exception ex)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error($"SALDO INVESTFÁCIL: Erro ao importar InvestFacil,Linha do registro {linhaICount}, Registro com erro: Agencia:{currentObject.AGENCIA},\nConta: {currentObject.CONTA}\nData: {currentObject.DT_EMISSAO}", ex);
            }
        }

        private void PreencherInvestFacilResumo()
        {
            var investisResumos = new List<InvestFacilResumo>();
            using (var db = new PGPEntities())
            {
                var resultado = db.Investfacil.ToList();

                var matriculas = resultado.Select(r => r.MatriculaConsultor).Distinct().ToList();

                decimal soma;

                matriculas.ForEach(m =>
                {
                    soma = resultado.Where(r => r.MatriculaConsultor == m).Sum(s => s.Vlr_Evento);
                    var investResumo = new InvestFacilResumo();
                    investResumo.Matricula = m;
                    investResumo.MatriculaCordenador = resultado.FirstOrDefault(r => r.MatriculaConsultor == m).MatriculaCordenador;
                    investResumo.Vlr_Evento = soma;
                    investisResumos.Add(investResumo);
                });

            }

            using (var db = new PGPEntities())
            {
                TruncarTabela("InvestFacilResumo");

                if (investisResumos.Count > 0)
                {
                    db.BulkInsert(investisResumos);
                }
            }
        }
    }
}
