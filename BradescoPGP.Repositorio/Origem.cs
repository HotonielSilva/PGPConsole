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
    
    public partial class Origem
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Origem()
        {
            this.Pipeline = new HashSet<Pipeline>();
        }
    
        public int Id { get; set; }
        public string Descricao { get; set; }
        public string Evento { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Pipeline> Pipeline { get; set; }
    }
}
