using BradescoPGP.Common.Logging;
using BradescoPGP.Repositorio;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BradescoPGPConsole.Commands
{
    public class ImportarAniversarios : AbstractCommand
    {
        public ImportarAniversarios(ServiceConfig config) : base(config)
        {
        }

        protected override void RealizarCarga()
        {
            int count = 0;
            try
            {
                BradescoPGP.Common.Logging.Log.Information("Iniciando importação de Aniversários.");

                using (StreamReader sr = new StreamReader(Config.Caminho, Encoding.UTF8))
                using (PGPEntities db = new PGPEntities())
                {
                    
                    var ListaAniversarios = new List<Aniversarios>();

                    string[] linhaDividida = null;
                    string linha = null;
                    while ((linha = sr.ReadLine()) != null)
                    {
                        if (count > 0)
                        {
                            linhaDividida = linha.Split(';');
                            var niver = new Aniversarios();

                            if (!linhaDividida[2].Contains("-"))
                                continue;

                            niver.CPF = linhaDividida[0];
                            niver.Agencia = int.Parse(linhaDividida[1]);

                            niver.Conta = int.Parse(linhaDividida[2].Split('-')[1]);

                            if (string.IsNullOrEmpty(linhaDividida[3]))
                                continue;

                            var data = default( DateTime);

                            if(DateTime.TryParse(linhaDividida[3], out data))
                            {
                                niver.DataNascimento = data;
                            }
                            else
                            {
                                niver.DataNascimento = DateTime.FromOADate(double.Parse(linhaDividida[3]));
                            }

                            ListaAniversarios.Add(niver);

                            if (ListaAniversarios.Count == 100000)
                            {
                                db.BulkInsert(ListaAniversarios);
                                ListaAniversarios.Clear();
                            }
                        }
                        count++;
                    }

                    if (ListaAniversarios.Count > 0)
                    {
                        db.BulkInsert(ListaAniversarios);
                    }
                }
                BradescoPGP.Common.Logging.Log.Information("Importação de Aniversários finalizada.");
            }
            catch (Exception ex)
            {
                Config.TeveFalha = true;
                BradescoPGP.Common.Logging.Log.Error("ANIVERSÁRIO: Erro ao importar Aniversários.", ex);
            }
        }


        

        protected override void PrepararDados()
        {
            TruncarTabela("Aniversarios");
        }
    }
}
