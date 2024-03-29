﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAP.Sirecec.Data.Core
{
	public partial class CursosDocumentos : BaseEntity
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		[ForeignKey("Curso")]
		public int CursoId { get; set; }
		public int TipoDocumentoId { get; set; }
		public string? TipoDocumentoNombre { get; set; }
		public string? Nombre { get; set; }
		public string? ArchivoNombre { get; set; }
		public int? ArchivoPeso { get; set; }
		public virtual Curso? Curso { get; set; }
	}
}