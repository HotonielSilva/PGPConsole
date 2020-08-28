using BradescoPGP.Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BradescoPGP.Repositorio;
using System.Text.RegularExpressions;

namespace BradescoPGPConsole.Commands
{
    public class ImportarCockpit : AbstractCommand
    {

        public ImportarCockpit(ServiceConfig config)
            : base(config)
        {
        }

        protected override void PrepararDados()
        {
            //Copia do arquivo para pasta temporaria do usuario
            var nomeAquivo = new FileInfo(Config.Caminho).Name;

            var destino = Path.GetTempPath() + nomeAquivo;

            File.Copy(Config.Caminho, destino, true);

            Config.Caminho = destino;

            TruncarTabela("Cockpit");
        }

        protected override void RealizarCarga()
        {
            int count = 0;
            try
            {
                BradescoPGP.Common.Logging.Log.Information("Iniciando importação de Cockpit.");
                var ListaCockpit = new List<Cockpit>();
                var carteiras = default(Dictionary<string, string>);

                using (StreamReader sr = new StreamReader(Config.Caminho, Encoding.GetEncoding("ISO-8859-1")))
                {
                    using (PGPEntities db = new PGPEntities())
                    {
                        carteiras = db.Encarteiramento.GroupBy(g => new { g.Agencia, g.Conta })
                            .ToDictionary(x => $"{x.First().Agencia}-{x.First().Conta}", x => x.First().Matricula);
                    }

                    string[] linhaDividida = null;
                    string linha = null;
                    while ((linha = sr.ReadLine()) != null)
                    {
                        if (count > 0)
                        {
                            linhaDividida = linha.Split(';');

                            var cockpit = new Cockpit();

                            if (!carteiras.ContainsKey($"{linhaDividida[4]}-{linhaDividida[6]}"))
                            {
                                count++;
                                continue;
                            }

                            cockpit.CodFuncionalGerente = int.Parse(linhaDividida[0]);
                            cockpit.NomeGerente = linhaDividida[1];
                            cockpit.CPF = Regex.Replace(linhaDividida[2], @"\D", "");
                            cockpit.NomeCliente = linhaDividida[3];
                            cockpit.CodigoAgencia = int.Parse(linhaDividida[4]);
                            cockpit.NomeAgencia = linhaDividida[5];
                            cockpit.Conta = int.Parse(linhaDividida[6]);
                            cockpit.DataEncarteiramento = DateTime.Parse(linhaDividida[7]);
                            cockpit.DataContato = DateTime.Parse(linhaDividida[8]);
                            if (linhaDividida[9] != "") cockpit.DataRetorno = DateTime.Parse(linhaDividida[9]);
                            cockpit.Observacao = linhaDividida[10].Trim();
                            cockpit.ContatoTeveExito = linhaDividida[11].ToLower() == "sim";
                            cockpit.DataHoraEdicaoContato = DateTime.Parse(linhaDividida[12]);
                            cockpit.MeioContato = linhaDividida[13];
                            cockpit.ClienteNaoLocalizado = linhaDividida[14].ToLower() == "sim";
                            cockpit.TipoTransacao = linhaDividida[15];
                            cockpit.Finalizado = linhaDividida[16].ToLower() == "sim";
                            cockpit.GerenteRegistrouContato = int.Parse(linhaDividida[17]);
                            cockpit.MatriculaConsultor = carteiras[$"{linhaDividida[4]}-{linhaDividida[6]}"];

                            ListaCockpit.Add(cockpit);

                            if (ListaCockpit.Count == 100000)
                            {
                                using (var db = new PGPEntities())
                                {
                                    db.BulkInsert(ListaCockpit);

                                    ListaCockpit.Clear();
                                }
                            }
                        }

                        count++;
                    }

                }   

                if (ListaCockpit.Count > 0)
                {
                    using (var db = new PGPEntities())
                    {
                        db.BulkInsert(ListaCockpit);
                    }
                }
                BradescoPGP.Common.Logging.Log.Information("Importação de Cockpit finalizada.");
            }
            catch (Exception ex)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error("COCKPIT: Erro ao importar Cockpit.", ex);
            }
        }
    }
}
