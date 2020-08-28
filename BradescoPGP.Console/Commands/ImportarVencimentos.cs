using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BradescoPGPConsole.ExtensionMethods;
using BradescoPGP.Repositorio;
using BradescoPGP.Common;
using System.Linq;

namespace BradescoPGPConsole.Commands
{
    public class ImportarVencimentos : AbstractCommand
    {
        public List<Vencimento> Backup { get; set; }

        public ImportarVencimentos(ServiceConfig config)
            : base(config)
        { }

        protected override void PrepararDados()
        {
            using (var db = new PGPEntities())
            {
                Backup = db.Vencimento.ToList();
            }
        }

        protected override void RealizarCarga()
        {
            var indexLinhaAtual = 0;
            
            PrepararDados();

            try
            {
                BradescoPGP.Common.Logging.Log.Information("Iniciando importação de Vencimentos.");

                using (StreamReader sd = new StreamReader(Config.Caminho, Encoding.UTF8))
                {
                    string linhaAtual = null;

                    string[] linhaDividida = null;

                    var vencimentos = new List<Vencimento>();

                    var clientes = default(List<vw_NomeCliente>);

                    var encarteirameto = new Dictionary<string, string>();

                    using (var db = new PGPEntities())
                    {
                        clientes = db.vw_NomeCliente.ToList();

                        foreach (var enca in db.Encarteiramento)
                        {
                            encarteirameto[$"{enca.Agencia}-{enca.Conta}"] = enca.CONSULTOR;
                        }
                    }

                    while ((linhaAtual = sd.ReadLine()) != null)
                    {
                        if (indexLinhaAtual > 0)
                        {
                            linhaDividida = linhaAtual.SplitWithQualifier(';', '\"', true);

                            var hasDataVenc = DateTime.TryParse(linhaDividida[2], out var dataVencimento);

                            if (encarteirameto.ContainsKey($"{linhaDividida[0]}-{linhaDividida[1]}") &&
                                hasDataVenc && dataVencimento.Month == DateTime.Now.Month)
                            {
                                Vencimento vencimento = new Vencimento();

                                vencimento.Cod_Agencia = int.Parse(linhaDividida[0]);

                                vencimento.Cod_Conta_Corrente = int.Parse(linhaDividida[1]);

                                vencimento.Dt_Vecto_Contratado = dataVencimento;

                                var nomeCliente = clientes.FirstOrDefault(c => c.Agencia == vencimento.Cod_Agencia && c.Conta == vencimento.Cod_Conta_Corrente);

                                if (nomeCliente != null)
                                    vencimento.Nm_Cliente_Contraparte = nomeCliente.NomeCliente;
                                else
                                    vencimento.Nm_Cliente_Contraparte = "Não disponivel no cockpit";

                                if (linhaDividida[3] != "") vencimento.Perc_Indexador = Convert.ToDouble(linhaDividida[3]);

                                if (linhaDividida[5] != "") vencimento.SALDO_ATUAL = decimal.Parse(linhaDividida[5]);

                                vencimento.Nome_produto_sistema_origem = linhaDividida[4];

                                vencimento.StatusId = 5;

                                vencimentos.Add(vencimento);

                                if (vencimentos.Count == 100000)
                                {
                                    using (var db = new PGPEntities())
                                    {
                                        db.BulkInsert(vencimentos);

                                        vencimentos.Clear();
                                    }
                                }
                            }
                        }
                        indexLinhaAtual++;
                    }

                    if (vencimentos.Count > 0)
                    {
                        using (var db = new PGPEntities())
                        {
                            db.BulkInsert(vencimentos);
                        }
                    }
                }

                Backup = null;

                BradescoPGP.Common.Logging.Log.Information("Importação de vencimentos finalizada.");
            }
            catch (Exception e)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error("VENCIMENTO: Erro ao importar Vencimentos.", e);

                TruncarTabela("Vencimento");

                using (var db = new PGPEntities())
                {
                    db.BulkInsert(Backup);
                }

            }
        }
    }
}
