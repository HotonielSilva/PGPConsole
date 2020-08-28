//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BradescoPGP.Repositorio
{
    using System;
    using System.Collections.Generic;
    
    public partial class Pipeline
    {
        public int Id { get; set; }
        public string NomeCliente { get; set; }
        public int Agencia { get; set; }
        public string Consultor { get; set; }
        public int Conta { get; set; }
        public bool BradescoPrincipalBanco { get; set; }
        public Nullable<decimal> ValoresNoMercado { get; set; }
        public decimal ValorDoPipe { get; set; }
        public Nullable<int> OrigemId { get; set; }
        public System.DateTime DataPrevista { get; set; }
        public Nullable<System.DateTime> DataDaConversao { get; set; }
        public Nullable<System.DateTime> DataProrrogada { get; set; }
        public Nullable<int> MotivoId { get; set; }
        public int StatusId { get; set; }
        public string Observacoes { get; set; }
        public Nullable<decimal> ValorAplicado { get; set; }
        public string MatriculaConsultor { get; set; }
        public bool Notificado { get; set; }
    
        public virtual Motivo Motivo { get; set; }
        public virtual Origem Origem { get; set; }
        public virtual Status Status { get; set; }
    }
}
