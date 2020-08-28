using BradescoPGP.Repositorio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.XSSF.UserModel;
using BradescoPGP.Common.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using BradescoPGP.Common;

namespace BradescoPGPConsole.Commands
{
    public class ImportarCorretora: AbstractCommand
    {
        public ImportarCorretora(ServiceConfig config) : base(config)
        {
        }

        private void ImportarBradesco()
        {
            BradescoPGP.Common.Logging.Log.Information("Iniciando Importação de BRADESCO");
            int linha = 2;
            try
            {
                var file = new FileInfo(Config.Caminho);
                using (var package = new ExcelPackage(file,"basepgp"))
                using (var db = new PGPEntities())
                {
                    var corretoraLista = new List<Corretora>();
                    var wk = package.Workbook;
                    var sheet = wk.Worksheets.First();
                    
                    for (int i = 2; i < sheet.Dimension.Rows; i++)
                    {
                        if (sheet.Cells[i, 10].Value == null)
                            continue;

                        var corretora = new Corretora();

                        corretora.Agencia = sheet.Cells[i, 10].Value.ToString();
                        corretora.Conta = sheet.Cells[i, 11].Value.ToString();
                        corretora.Status = sheet.Cells[i, 12].Value.ToString();
                        corretora.Nome = "BRA";
                        
                        corretoraLista.Add(corretora);

                        if (corretoraLista.Count == 100000)
                        {
                            db.BulkInsert(corretoraLista);
                            corretoraLista.Clear();
                        }

                        linha = i;
                    }

                    if (corretoraLista.Count > 0)
                    {
                        db.BulkInsert(corretoraLista);
                    }
                }

                BradescoPGP.Common.Logging.Log.Information("Importação BRA finalizado");
            }
            catch (Exception e)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error($"DADOS CORRETORA: Erro ao importar Corretora Bradesco: LinhaExcel:{linha}", e);
            }
        }

        private void ImportarAgora()
        {
            BradescoPGP.Common.Logging.Log.Information("Iniciando importação de AGORA");
            try
            {
                var file = new FileInfo(Config.Caminho);
                using (var package = new ExcelPackage(file, "basepgp"))
                using (var db = new PGPEntities())
                {
                    var corretoraLista = new List<Corretora>();
                    var wk = package.Workbook;
                    var sheet = wk.Worksheets.Last();

                    for (int i = 2; i < sheet.Dimension.Rows; i++)
                    {
                        var corretora = new Corretora();

                        var cpfCnpj = sheet.Cells[i, 1].Value.ToString();

                        var cnpj = cpfCnpj.PadLeft(14,'0');
                        var cpf = cpfCnpj.PadLeft(11, '0');

                        if (Validador.IsCnpj(cnpj))
                        {
                            cpfCnpj = cnpj;
                        }
                        else if(Validador.IsCpf(cpf))
                        {
                            cpfCnpj = cpf;
                        }
                        else
                        {
                            //Log.Error($"Não é nem cpf válido e nem cnpj valorObtido: {cpfCnpj}, CPF:{cpf}, CPNJ: {cnpj}");
                            continue;
                        }

                        corretora.CPF = cpfCnpj;
                        corretora.Status = sheet.Cells[i, 11].Value.ToString().ToLower() == "a" ? "OK": "Vencido";
                        corretora.Nome = "AGO";

                        corretoraLista.Add(corretora);

                        if (corretoraLista.Count == 100000)
                        {
                            db.BulkInsert(corretoraLista);
                            corretoraLista.Clear();
                        }
                    }

                    if (corretoraLista.Count > 0)
                    {
                        db.BulkInsert(corretoraLista);
                    }
                }

                BradescoPGP.Common.Logging.Log.Information("Importação AGORA finalizado");
            }
            catch (Exception e)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error("DADOS CORRETORA: Erro ao importar AGORA", e);
            }
        }

        protected override void RealizarCarga()
        {
            BradescoPGP.Common.Logging.Log.Information("Iniciando importação de corretoras");

            ImportarBradesco();

            ImportarAgora();

            BradescoPGP.Common.Logging.Log.Information("Importação de corretoras finalizado");        }

        protected override void PrepararDados()
        {
            TruncarTabela("Corretora");
        }
    }
}
