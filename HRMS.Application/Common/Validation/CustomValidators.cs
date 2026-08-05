using FluentValidation;
using System.Text.RegularExpressions;

namespace HRMS.Application.Common.Validation
{
    /// <summary>
    /// Custom FluentValidation extensions for domain-specific formatting and business rule rules.
    /// </summary>
    public static class CustomValidators
    {
        private static readonly Regex PhoneRegex = new(@"^[6-9]\d{9}$", RegexOptions.Compiled);
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        private static readonly Regex PanRegex = new(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", RegexOptions.Compiled);
        private static readonly Regex IfscRegex = new(@"^[A-Z]{4}0[A-Z0-9]{6}$", RegexOptions.Compiled);
        private static readonly Regex AadhaarRegex = new(@"^\d{12}$", RegexOptions.Compiled);

        /// <summary>
        /// Validates Indian 10-digit mobile number format.
        /// </summary>
        public static IRuleBuilderOptions<T, string?> MustBeValidPhone<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .Must(phone => string.IsNullOrWhiteSpace(phone) || PhoneRegex.IsMatch(phone))
                .WithMessage("Phone number must be a valid 10-digit mobile number starting with 6, 7, 8, or 9.");
        }

        /// <summary>
        /// Validates email address format.
        /// </summary>
        public static IRuleBuilderOptions<T, string?> MustBeValidEmail<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .Must(email => string.IsNullOrWhiteSpace(email) || EmailRegex.IsMatch(email))
                .WithMessage("A valid email address format is required.");
        }

        /// <summary>
        /// Validates salary amount bounds.
        /// </summary>
        public static IRuleBuilderOptions<T, decimal> MustBeValidSalary<T>(this IRuleBuilder<T, decimal> ruleBuilder)
        {
            return ruleBuilder
                .GreaterThanOrEqualTo(0)
                .WithMessage("Salary cannot be negative.");
        }

        /// <summary>
        /// Validates PAN Card number format.
        /// </summary>
        public static IRuleBuilderOptions<T, string?> MustBeValidPan<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .Must(pan => string.IsNullOrWhiteSpace(pan) || PanRegex.IsMatch(pan))
                .WithMessage("PAN card number must follow the standard 10-character alphanumeric format (e.g. ABCDE1234F).");
        }

        /// <summary>
        /// Validates IFSC Code format.
        /// </summary>
        public static IRuleBuilderOptions<T, string?> MustBeValidIfsc<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .Must(ifsc => string.IsNullOrWhiteSpace(ifsc) || IfscRegex.IsMatch(ifsc))
                .WithMessage("IFSC code must follow the standard 11-character format (e.g. SBIN0001234).");
        }

        /// <summary>
        /// Validates Aadhaar card 12-digit number format.
        /// </summary>
        public static IRuleBuilderOptions<T, string?> MustBeValidAadhaar<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .Must(aadhaar => string.IsNullOrWhiteSpace(aadhaar) || AadhaarRegex.IsMatch(aadhaar))
                .WithMessage("Aadhaar number must be a valid 12-digit numeric sequence.");
        }
    }
}
