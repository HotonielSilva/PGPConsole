using BradescoPGP.Common.Logging;
using BradescoPGP.Repositorio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BradescoPGPConsole.Commands
{
    public class ImportarTemInvestFacil : AbstractCommand
    {
        public ImportarTemInvestFacil(ServiceConfig config) : base(config)
        {
        }

        protected override void PrepararDados()
        {
            TruncarTabela("TemInvestFacil");
        }

        protected override void RealizarCarga()
        {
            BradescoPGP.Common.Logging.Log.Information("Iniciando importação de TemInvesteFacil");
            try
            {
                using (var db = new PGPEntities())
                using (var file = new StreamReader(Config.Caminho))
                {
                    int count = 0;

                    var listaTemInvest = new List<TemInvestFacil>();

                    string linha = null;

                    while ((linha = file.ReadLine()) != null)
                    {
                        if (count > 0) //Pula primeira linha de cabeçalho
                        {
                            var temInvest = new TemInvestFacil();
                            var linhaDividida = linha.Split(';');

                            temInvest.Agencia = int.Parse(linhaDividida[0]);
                            temInvest.Conta = int.Parse(linhaDividida[1]);
                            if (!string.IsNullOrEmpty(linhaDividida[7])) temInvest.DataInicio = DateTime.Parse(linhaDividida[7]);
                            if (!string.IsNullOrEmpty(linhaDividida[8])) temInvest.DataFim = DateTime.Parse(linhaDividida[8]);
                            temInvest.Status = linhaDividida[10];

                            listaTemInvest.Add(temInvest);

                            if (listaTemInvest.Count == 100000)
                            {
                                db.BulkInsert(listaTemInvest);
                                listaTemInvest.Clear();
                            }
                        }
                        count++;
                    }

                    if (listaTemInvest.Count > 0)
                    {
                        db.BulkInsert(listaTemInvest);

                    }
                    BradescoPGP.Common.Logging.Log.Information("Importação concluida com sucesso");
                }
            }
            catch (Exception e)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error($"CADASTRO INVESTFÁCIL: Erro ao importar o arquivo {Config.Caminho}", e);
            }
        }
    }
}
