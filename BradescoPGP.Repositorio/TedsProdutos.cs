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
    
    public partial class TedsProdutos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TedsProdutos()
        {
            this.TedsAplicacoes = new HashSet<TedsAplicacoes>();
        }
    
        public int Id { get; set; }
        public string Produto { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TedsAplicacoes> TedsAplicacoes { get; set; }
    }
}
