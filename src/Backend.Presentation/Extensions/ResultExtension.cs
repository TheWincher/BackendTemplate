using Backend.Domain.Errors;
using Backend.Presentation.Models;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Presentation.Extensions;

public static class ResultExtensions
{
    public static ErrorResponse ToResponse(
        this IEnumerable<IError> errors,
        int status)
    {
        var (type, title) = status switch
        {
            400 => ("bad_request",        "La requête est invalide."),
            404 => ("not_found",          "La ressource est introuvable."),
            409 => ("conflict",           "Un conflit existe avec la ressource."),
            422 => ("validation_error",   "Un ou plusieurs erreurs de validation."),
            _   => ("internal_error",     "Une erreur interne est survenue.")
        };

        return new ErrorResponse(
            Type:   type,
            Title:  title,
            Status: status,
            Errors: errors.Select(e => e.Message));
    }

    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        Func<T, IActionResult> onSuccess)
    {
        if (result.IsSuccess)
            return onSuccess(result.Value);

        return result.Errors.FirstOrDefault() switch
        {
            UsernameAlreadyTakenError => new ConflictObjectResult(
                result.Errors.ToResponse(409)),
            
            EmailAlreadyTakenError => new ConflictObjectResult(
                result.Errors.ToResponse(409)),

            NotFoundError => new NotFoundObjectResult(
                result.Errors.ToResponse(404)),

            ValidationError => new UnprocessableEntityObjectResult(
                result.Errors.ToResponse(422)),

            InvalidCredentialsError => new BadRequestObjectResult(
                result.Errors.ToResponse(400)),

            _ => new ObjectResult(result.Errors.ToResponse(500))
            {
                StatusCode = 500
            }
        };
    }

    public static IActionResult ToActionResult(
        this Result result,
        Func<IActionResult> onSuccess)
    {
        if (result.IsSuccess)
            return onSuccess();

        return result.Errors.FirstOrDefault() switch
        {
            NotFoundError => new NotFoundObjectResult(
                result.Errors.ToResponse(404)),

            ValidationError => new UnprocessableEntityObjectResult(
                result.Errors.ToResponse(422)),

            _ => new ObjectResult(result.Errors.ToResponse(500))
            {
                StatusCode = 500
            }
        };
    }
}