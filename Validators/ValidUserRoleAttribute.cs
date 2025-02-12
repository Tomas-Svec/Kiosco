using System.ComponentModel.DataAnnotations;

namespace Kiosco.Validators
{
    public class ValidUserRoleAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string role && (role == "Empleado" || role == "Jefe"))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("El rol debe ser 'Empleado' o 'Jefe'.");
        }
    }
}