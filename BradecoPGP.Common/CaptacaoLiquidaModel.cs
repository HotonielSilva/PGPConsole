namespace BradescoPGP.Common
{
    public class CaptacaoLiquidaModel
    {
        public string Especialista { get; set; }
        public string Produto { get; set; }
        public decimal CapLiq { get; set; }

        public decimal TotalAplicacao { get; set; }
        public decimal TotalResgate { get; set; }
        public decimal VL_DINHEIRO_NOVO { get; set; }
        public decimal VL_RESG_CDB { get; set; }
        public decimal VL_RESG_ISENTOS { get; set; }
        public decimal VL_RESG_COMPROMISSADAS { get; set; }
        public decimal VL_RESG_LF { get; set; }
        public decimal VL_RESG_FUNDOS { get; set; }
        public decimal VL_RESG_CORRET { get; set; }
        public decimal VL_RESG_PREVI { get; set; }

        public string Agencia { get; set; }
        public string Conta { get; set; }
        public string MatriculaConsultor { get; set; }
    }
}
