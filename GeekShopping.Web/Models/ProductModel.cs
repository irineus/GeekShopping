using System.ComponentModel.DataAnnotations;

namespace GeekShopping.Web.Models
{
    public class ProductModel
    {
        public long Id { get; set; }
        [Display(Name = "Nome")]
        [Required(ErrorMessage = "{0} é obrigatório")]
        [StringLength(150, ErrorMessage = "{0} deve ter no máximo {1} caracteres")]
        public string Name { get; set; }
        [Display(Name = "Preço")]
        [Required(ErrorMessage = "{0} é obrigatório")]
        [Range(1, 99999, ErrorMessage = "{0} deve ser um valor entre {1} e {2}")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
        [Display(Name = "Descrição")]
        [Required(ErrorMessage = "{0} é obrigatório")]
        [StringLength(500, ErrorMessage = "{0} deve ter no máximo {1} caracteres")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [Display(Name = "Categoria")]
        [Required(ErrorMessage = "{0} é obrigatório")]
        [StringLength(50, ErrorMessage = "{0} deve ter no máximo {1} caracteres")]
        public string CategoryName { get; set; }
        [Display(Name = "URL da Imagem")]
        [Required(ErrorMessage = "{0} é obrigatório")]
        [StringLength(300, ErrorMessage = "{0} deve ter no máximo {1} caracteres")]
        [DataType(DataType.ImageUrl)]
        public string ImageURL { get; set; }

        [Range(1, 100)]
        public int Count { get; set; } = 1;

        public string SubstringName()
        {
            if (Name.Length < 24) return Name;
            return Name.Substring(0, 21) + " ...";
        }

        public string SubstringDescription()
        {
            if (Description.Length < 355) return Description;
            return Description.Substring(0, 352) + " ...";
        }

    }
}
