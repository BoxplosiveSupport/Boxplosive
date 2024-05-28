using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared
{
	public interface IProductListsModel
	{
		List<ProductModel> Triggers { get; set; }
		List<ProductModel> TargetProducts { get; set; }
		List<ProductModel> SelectedProducts { get; set; }
	}
}
