//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Parcours_integration.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Utilisateurs_Services
    {
        public int ID { get; set; }
        public int ID_Service { get; set; }
        public int ID_Utilisateur { get; set; }
    
        public virtual Service Service { get; set; }
        public virtual Utilisateurs Utilisateurs { get; set; }
    }
}
