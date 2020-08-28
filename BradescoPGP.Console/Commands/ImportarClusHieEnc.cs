using BradescoPGP.Common;
using BradescoPGP.Repositorio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BradescoPGPConsole.Commands
{
    class ImportarClusHieEnc : AbstractCommand
    {
        private List<Encarteiramento> BackupEncarteiramento { get; set; }

        private List<Usuario> BackupHierarquia { get; set; }
        private List<Clusterizacoes> BackupClusterizacao { get; set; }

        public ImportarClusHieEnc(ServiceConfig config) : base(config)
        {
        }

        protected override void PrepararDados()
        {
            using (var db = new PGPEntities())
            {
                BackupEncarteiramento = db.Encarteiramento.ToList();

                BackupHierarquia = db.Usuario.Where(u => u.PerfilId > 1).ToList();

                BackupClusterizacao = db.Clusterizacoes.ToList();

                ExecuteQuery("DELETE FROM [dbo].[Encarteiramento] WHERE AREA = 'PGP'");

                ExecuteQuery("DELETE FROM [dbo].[Usuario] WHERE PerfilId > 1 AND EQUIPE NOT LIKE '%TOP TIER%'");

                ExecuteQuery("DELETE FROM [dbo].[Clusterizacoes] WHERE AREA = 'PGP'");
            }
        }

        protected override void RealizarCarga()
        {
            using (var sr = new StreamReader(Config.Caminho))
            {
                var resultClus = ImportarClusterizacao(sr);

                var resultEnc = ImportarEncarteiramento(sr);

                var resultHie = ImportarHierarquia(sr);

                
                if (!resultEnc || !resultHie || !resultClus)
                {
                    Restaurar();

                    DisposeBackup();

                    Config.TeveFalha = true;

                    return;
                }

                Config.TeveFalha = false;
            }
        }

        private bool ImportarEncarteiramento(StreamReader sr)
        {
            try
            {
                sr.DiscardBufferedData();

                sr.BaseStream.Seek(0, SeekOrigin.Begin);

                BradescoPGP.Common.Logging.Log.Information("Iniciando importação de Encarteiramento.");

                string linha;

                string[] linhaDividida = null;

                int linhacount = 0;

                List<Encarteiramento> encarteiramentos = new List<Encarteiramento>();

                while ((linha = sr.ReadLine()) != null)
                {
                    if (linhacount > 0)
                    {
                        linhaDividida = linha.SplitWithQualifier(';', '\"', true);

                        Encarteiramento encarteiramento = new Encarteiramento();

                        encarteiramento.Agencia = linhaDividida[0];
                        encarteiramento.Conta = linhaDividida[1].Split('-')[1];
                        encarteiramento.CPF = linhaDividida[2] != "" ? long.Parse(linhaDividida[2]).ToString() : linhaDividida[2];
                        encarteiramento.AG_PRINC = encarteiramento.Agencia;
                        encarteiramento.CONTA_PRINC = encarteiramento.Conta;
                        encarteiramento.CONSULTOR = linhaDividida[73];
                        encarteiramento.Matricula = linhaDividida[72];
                        encarteiramento.EQUIPE_RESPONSAVEL = linhaDividida[78];
                        encarteiramento.EQUIPE_MESA = linhaDividida[78];
                        encarteiramento.DIR_REG_AG_PRINC = linhaDividida[7];
                        if(int.TryParse(linhaDividida[4], out int codReg)) encarteiramento.COD_REG = codReg;
                        if(int.TryParse(linhaDividida[6], out int codDir)) encarteiramento.COD_DIR = codDir;
                        encarteiramento.AREA = "PGP";

                        encarteiramentos.Add(encarteiramento);

                        if (encarteiramentos.Count == 200000)
                        {
                            using (var db = new PGPEntities())
                            {
                                db.BulkInsert(encarteiramentos);
                                encarteiramentos.Clear();
                            }
                        }
                    }

                    linhacount++;
                }

                if (encarteiramentos.Count > 0)
                {
                    using (PGPEntities db = new PGPEntities())
                    {
                        db.BulkInsert(encarteiramentos);
                    }
                }

                BradescoPGP.Common.Logging.Log.Information("Importação de Encarteiramento finalizada.");

            }
            catch (Exception ex)
            {
                BradescoPGP.Common.Logging.Log.Error("ENCARTEIRAMENTO: Erro ao importar Encarteiramento.", ex);

                return false;
            }

            return true;
        }

        private bool ImportarHierarquia(StreamReader sr)
        {
            try
            {
                sr.DiscardBufferedData();

                sr.BaseStream.Seek(0, SeekOrigin.Begin);

                BradescoPGP.Common.Logging.Log.Information("Iniciando importação de Hierarquias.");

                int indexLinhaAtual = 0;

                var encarteiramento = new List<string>();

                using (var db = new PGPEntities())
                {
                    encarteiramento = db.Encarteiramento.Select(s => s.Matricula).Distinct().ToList();
                }

                string linhaAtual = null;

                string[] linhaDividida = null;

                var Usuarios = new List<Usuario>();

                while ((linhaAtual = sr.ReadLine()) != null)
                {
                    if (indexLinhaAtual > 0)
                    {
                        linhaDividida = linhaAtual.SplitWithQualifier(';', '\"', true);

                        var matriculasInclusas = Usuarios.Select(s => s.Matricula).ToList();

                        var matricula = linhaDividida[72];

                        if (!matriculasInclusas.Contains(matricula))
                        {

                            var especialista = new Usuario();

                            especialista.Nome = linhaDividida[73];

                            especialista.Matricula = linhaDividida[72];

                            especialista.NomeSupervisor = linhaDividida[75];

                            especialista.MatriculaSupervisor = linhaDividida[74];

                            especialista.Equipe = linhaDividida[78];

                            especialista.PerfilId = encarteiramento.Contains(especialista.Matricula) ? 3 : 2;

                            especialista.NomeUsuario = CodNunToCodLetra(especialista.Matricula);

                            Usuarios.Add(especialista);

                            matriculasInclusas = Usuarios.Select(s => s.Matricula).ToList();

                            if (!matriculasInclusas.Contains(especialista.MatriculaSupervisor))
                            {
                                var gestor = new Usuario();

                                gestor.Nome = especialista.NomeSupervisor;

                                gestor.Matricula = especialista.MatriculaSupervisor;

                                gestor.NomeSupervisor = linhaDividida[77];

                                gestor.MatriculaSupervisor = linhaDividida[76];

                                gestor.Equipe = linhaDividida[78];

                                gestor.PerfilId = 2;

                                gestor.NomeUsuario = CodNunToCodLetra(gestor.Matricula);

                                Usuarios.Add(gestor);
                            }

                            if (Usuarios.Count == 100000)
                            {
                                using (var db = new PGPEntities())
                                {
                                    db.BulkInsert(Usuarios.OrderBy(s => s.Nome));

                                    Usuarios.Clear();
                                }
                            }
                        }
                    }

                    indexLinhaAtual++;
                }

                if (Usuarios.Count > 0)
                {
                    using (var db = new PGPEntities())
                    {
                        db.BulkInsert(Usuarios.OrderBy(s => s.Nome).ToList());
                    }
                }

                BradescoPGP.Common.Logging.Log.Information("Importação de Hierarquias finalizada.");

            }
            catch (Exception ex)
            {

                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error("HIERARQUIA: Erro ao importar Hierarquias.", ex);

                return false;
            }

            return true;
        }

        private bool ImportarClusterizacao(StreamReader sr)
        {
            try
            {
                sr.DiscardBufferedData();

                sr.BaseStream.Seek(0, SeekOrigin.Begin);

                string linha = null;

                string[] linhaDividida = null;

                int linhacount = 0;

                List<Clusterizacoes> clusterizacaos = new List<Clusterizacoes>();

                while ((linha = sr.ReadLine()) != null)
                {
                    if (linhacount > 0)
                    {
                        linhaDividida = linha.SplitWithQualifier(';', '\"', true);

                        Clusterizacoes clusterizacao = new Clusterizacoes();

                        clusterizacao.AGENCIA = int.Parse(linhaDividida[0]);
                        clusterizacao.CONTA = int.Parse(linhaDividida[1].Split('-')[1]);
                        clusterizacao.CPF_CNPJ = linhaDividida[2];
                        clusterizacao.SALDO_CORRETORA_BRA = decimal.Parse(linhaDividida[48]);
                        clusterizacao.SALDO_CORRETORA_AGORA = decimal.Parse(linhaDividida[49]);
                        clusterizacao.SALDO_CORRETORA = decimal.Parse(linhaDividida[47]);
                        clusterizacao.SALDO_PREVIDENCIA = decimal.Parse(linhaDividida[50]);
                        clusterizacao.SALDO_POUPANCA = decimal.Parse(linhaDividida[37]);
                        clusterizacao.SALDO_INVESTS = decimal.Parse(linhaDividida[38]);
                        clusterizacao.SALDO_DAV_20K = decimal.Parse(linhaDividida[36]);
                        clusterizacao.SALDO_COMPROMISSADAS = decimal.Parse(linhaDividida[42]);
                        clusterizacao.SALDO_ISENTOS = decimal.Parse(linhaDividida[40]);
                        clusterizacao.SALDO_LF = decimal.Parse(linhaDividida[41]);
                        clusterizacao.SALDO_CDB = decimal.Parse(linhaDividida[39]);
                        clusterizacao.SALDO_TOTAL = decimal.Parse(linhaDividida[34]);
                        clusterizacao.SALDO_TOTAL_M3 = decimal.Parse(linhaDividida[35]);
                        clusterizacao.PERFIL_API = linhaDividida[21];
                        clusterizacao.MES_VCTO_API = linhaDividida[22];
                        clusterizacao.NIVEL_DESENQ_FX_RISCO = linhaDividida[20];
                        clusterizacao.COD_GER_RELC = !String.IsNullOrEmpty(linhaDividida[69]) ? linhaDividida[70] : string.Empty;
                        clusterizacao.GER_RELC = !String.IsNullOrEmpty(linhaDividida[70]) ? linhaDividida[71] : string.Empty;
                        clusterizacao.Segmento = linhaDividida[13];
                        clusterizacao.CodDiretoriaRegional = linhaDividida[6];
                        clusterizacao.DiretoriaRegional = linhaDividida[7];
                        var saldoFundosAcoes = decimal.Parse(linhaDividida[43]);
                        var saldoFundosCambial = decimal.Parse(linhaDividida[44]);
                        var saldoFundosMultim = decimal.Parse(linhaDividida[45]);
                        var saldoFundosDI = decimal.Parse(linhaDividida[46]);

                        var fundos = new List<decimal>
                        {
                            saldoFundosAcoes,saldoFundosCambial,saldoFundosMultim,saldoFundosDI
                        };

                        clusterizacao.SALDO_FUNDOS = fundos.Sum();

                        clusterizacao.AREA = "PGP";

                        clusterizacaos.Add(clusterizacao);

                        if (clusterizacaos.Count == 200000)
                        {
                            using (PGPEntities db = new PGPEntities())
                            {
                                db.BulkInsert(clusterizacaos);
                            }

                            clusterizacaos.Clear();
                        }
                    }

                    linhacount++;
                }

                if (clusterizacaos.Count > 0)
                {
                    using (PGPEntities db = new PGPEntities())
                    {
                        db.BulkInsert(clusterizacaos);
                    }
                }
            }
            catch (Exception ex)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error("CLUSTERIZAÇÃO: Erro ao importar Cluesterizacoes.", ex);

                return false;
            }

            return true;
        }

        private void Restaurar()
        {
            using (var db = new PGPEntities())
            {
                ExecuteQuery("DELETE FROM [dbo].[Encarteiramento] WHERE AREA = 'PGP'");

                ExecuteQuery("DELETE FROM [dbo].[Usuario] WHERE PerfilId > 1 AND EQUIPE NOT LIKE '%TOP TIER%'");

                ExecuteQuery("DELETE FROM [dbo].[Clusterizacoes] WHERE AREA = 'PGP'");

                db.BulkInsert(BackupEncarteiramento);

                db.BulkInsert(BackupHierarquia);

                db.BulkInsert(BackupClusterizacao);
            }

        }

        private void DisposeBackup()
        {
            BackupEncarteiramento.Clear();

            BackupHierarquia.Clear();

            BackupClusterizacao.Clear();
        }

        private string CodNunToCodLetra(string CodFunc)
        {
            try
            {
                string Cod = CodFunc.ToUpper().Trim().Substring(0, 1);
                string CodLetra = string.Empty;

                switch (Cod)
                {
                    case "1":
                        CodLetra = "A";
                        break;
                    case "2":
                        CodLetra = "B";
                        break;
                    case "3":
                        CodLetra = "C";
                        break;
                    case "4":
                        CodLetra = "D";
                        break;
                    case "5":
                        CodLetra = "E";
                        break;
                    case "6":
                        CodLetra = "F";
                        break;
                    case "7":
                        CodLetra = "G";
                        break;
                    case "8":
                        CodLetra = "H";
                        break;
                    case "9":
                        CodLetra = "I";
                        break;
                    default:
                        return CodFunc.ToUpper();
                }

                CodLetra = CodLetra + CodFunc.Trim().Substring(1);
                return CodLetra.ToUpper();
            }
            catch
            {
                return CodFunc.ToUpper();
            }
        }
    }
}
