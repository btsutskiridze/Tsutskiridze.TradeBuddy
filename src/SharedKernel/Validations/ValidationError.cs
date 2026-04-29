namespace SharedKernel.Validations;

public record ValidationError(string PropertyName, string ErrorMessage);