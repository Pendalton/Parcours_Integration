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
    
    public partial class ModeleContrat
    {
        public int ID { get; set; }
        public int ID_Modele { get; set; }
        public int ID_Contrat { get; set; }
    
        public virtual Contrat Contrat { get; set; }
        public virtual Modele Modele { get; set; }
    }
}
