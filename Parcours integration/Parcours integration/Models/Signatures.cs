//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré à partir d'un modèle.
//
//     Des modifications manuelles apportées à ce fichier peuvent conduire à un comportement inattendu de votre application.
//     Les modifications manuelles apportées à ce fichier sont remplacées si le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Parcours_integration.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Signatures
    {
        public int ID { get; set; }
        public int ID_Signataire { get; set; }
        public System.DateTime Date_Signature { get; set; }
        public int ID_Parcours { get; set; }
        public string Role { get; set; }
    
        public virtual Parcours Parcours { get; set; }
        public virtual Utilisateurs Utilisateurs { get; set; }
    }
}
