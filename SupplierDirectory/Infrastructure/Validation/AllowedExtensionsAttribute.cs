using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SupplierDirectory.Infrastructure.Validation;

public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _extensions;

    public AllowedExtensionsAttribute(string[] extensions)
    {
        _extensions = extensions;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            if (file.Length == 0 || string.IsNullOrWhiteSpace(file.FileName))
            {
                return ValidationResult.Success;
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension) || !_extensions.Contains(extension.ToLower()))
            {
                return new ValidationResult(GetErrorMessage());
            }
        }
        else if (value is IEnumerable<IFormFile> files)
        {
            foreach (var f in files)
            {
                if (f == null || f.Length == 0 || string.IsNullOrWhiteSpace(f.FileName))
                {
                    continue;
                }

                var extension = Path.GetExtension(f.FileName);
                if (string.IsNullOrEmpty(extension) || !_extensions.Contains(extension.ToLower()))
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }
        }

        return ValidationResult.Success;
    }

    private string GetErrorMessage()
    {
        return $"صيغة الملف غير مدعومة. الصيغ المسموحة هي: {string.Join(", ", _extensions)}";
    }
}
