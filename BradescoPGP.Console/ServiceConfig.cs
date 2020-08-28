using System;

namespace BradescoPGPConsole
{
    public class ServiceConfig
    {
        public string Caminho { get; set; }
        public string PadraoPesquisa { get; set; }
        public DateTime UltimaExecucao { get; set; }
        public TimeSpan IntervaloExecucao { get; set; }
        public DateTime? DataUltimaModificacao { get; set; }
        public string Tarefa { get; set; }
        public bool Acrescentar { get; set; }
        public bool EmExecucao { get; set; }
        public string NomeArquivo { get; set; }
        public bool TeveFalha { get; set; } = false;
    }
}
