﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAP.Sirecec.Data.Core
{
	public partial class CursoEncuesta : BaseEntity
	{
		[Key]
		public int Id { get; set; }
		public int CursoId { get; set; }
		public int EncuestaId { get; set; }
		public virtual Curso? Curso { get; set; }


	}
}