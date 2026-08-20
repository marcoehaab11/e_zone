using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SupplierDirectory.Infrastructure.Validation;

public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly int _maxFileSize;

    public MaxFileSizeAttribute(int maxFileSize)
    {
        _maxFileSize = maxFileSize;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            if (file.Length > _maxFileSize)
            {
                return new ValidationResult(GetErrorMessage(file.FileName));
            }
        }
        else if (value is IEnumerable<IFormFile> files)
        {
            foreach (var f in files)
            {
                if (f.Length > _maxFileSize)
                {
                    return new ValidationResult(GetErrorMessage(f.FileName));
                }
            }
        }

        return ValidationResult.Success;
    }

    private string GetErrorMessage(string fileName)
    {
        return $"حجم الملف ({fileName}) يتجاوز الحد الأقصى المسموح به وهو {_maxFileSize / (1024 * 1024)} ميجابايت.";
    }
}
