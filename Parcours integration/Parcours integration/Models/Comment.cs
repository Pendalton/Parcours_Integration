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
    
    public partial class Comment
    {
        public int CommentID { get; set; }
        public string CommentText { get; set; }
        public string DateHeure { get; set; }
        public int ParcoursID { get; set; }
        public int Rating { get; set; }
    
        public virtual Parcours Parcours { get; set; }
    }
}
