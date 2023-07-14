using Fiorello.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace Fiorello.Areas.admin.ViewModels.Product
{
	public class ProductIndexVM
	{
        public ProductIndexVM()
        {
            Products = new List<Models.Product>();
            CategoriesIds = new List<int>();
            ProductTypes = new List<ProductType>();
        }

        public List<Models.Product> Products { get; set; }

        #region FilterProperties

        public string? Title { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }
        public List<SelectListItem> Categories { get; set; }
        [Display(Name = "Categories")]
        public List<int> CategoriesIds { get; set; }
        public List<ProductType> ProductTypes { get; set; }

        [Display(Name = "From")]
        public DateTime? CreatedAtStart { get; set; }

        [Display(Name = "To")]
        public DateTime? CreatedAtEnd { get; set; }
        #endregion
    }
}
