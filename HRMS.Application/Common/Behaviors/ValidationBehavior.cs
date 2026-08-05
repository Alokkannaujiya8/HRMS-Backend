using FluentValidation;
using HRMS.Application.Common.Results;
using MediatR;

namespace HRMS.Application.Common.Behaviors
{
    /// <summary>
    /// Pipeline behavior executing FluentValidation rules on incoming MediatR requests before handler execution.
    /// </summary>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators ?? throw new ArgumentNullException(nameof(validators));
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
                var error = Error.Validation("Validation.Failure", errorMessage);

                if (typeof(TResponse).IsGenericType &&
                    typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var resultType = typeof(TResponse).GetGenericArguments()[0];
                    var failureMethod = typeof(Result)
                        .GetMethods()
                        .First(m => m.Name == nameof(Result.Failure) && m.IsGenericMethod)
                        .MakeGenericMethod(resultType);

                    return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
                }

                if (typeof(TResponse) == typeof(Result))
                {
                    return (TResponse)(object)Result.Failure(error);
                }

                throw new ValidationException(failures);
            }

            return await next();
        }
    }
}
