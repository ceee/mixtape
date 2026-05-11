using FluentValidation.Results;
using System.Runtime.Serialization;

namespace Mixtape.Models;


[DataContract(Name = "result", Namespace = "")]
public class Result
{
  [DataMember(Name = "success")]
  public bool IsSuccess { get; set; }

  [DataMember(Name = "errors")]
  public List<ResultError> Errors { get; set; } = [];

  public Result() { }

  public Result(ValidationResult validation)
  {
    IsSuccess = validation.IsValid;
    Errors = validation.Errors.Select(x => new ResultError()
    {
      Property = x.PropertyName,
      Message = x.ErrorMessage
    }).ToList();
  }

  public virtual object GetModel() => null;

  public static Result Maybe(bool isSuccess) => isSuccess ? Success() : Fail();

  public static Result Success() => new() { IsSuccess = true };

  public static Result Fail() => new() { };

  public static Result Fail(string property, string message)
  {
    Result result = new();
    result.Errors.Add(new(property, message));
    return result;
  }

  public static Result Fail(string message)
  {
    Result result = new();
    result.AddError(message);
    return result;
  }

  public static Result Fail(ValidationResult validation) => new(validation);

  public static Result Fail(ResultError error) => Fail(error.Property, error.Message);

  public static Result Fail(IEnumerable<ResultError> errors) => new() { IsSuccess = !errors.Any(), Errors = errors.ToList() };
}


public class Result<T> : Result
{
  [DataMember(Name = "model")]
  public T Model { get; set; }

  public Result() : base() { }

  public Result(ValidationResult validation) : base(validation) { }


  public new static Result<T> Success() => new() { IsSuccess = true };

  public static Result<T> Success(T model) => new() { IsSuccess = true, Model = model };

  public new static Result<T> Fail() => new() { };

  public new static Result<T> Fail(string property, string message)
  {
    Result<T> result = new();
    result.Errors.Add(new(property, message));
    return result;
  }

  public new static Result<T> Fail(string message)
  {
    Result<T> result = new();
    result.AddError(message);
    return result;
  }

  public new static Result<T> Fail(ValidationResult validation) => new(validation);

  public new static Result<T> Fail(ResultError error) => Fail(error.Property, error.Message);

  public new static Result<T> Fail(IEnumerable<ResultError> errors) => new() { IsSuccess = !errors.Any(), Errors = errors.ToList() };

  public override object GetModel() => Model;
}


[DataContract(Name = "resultError", Namespace = "")]
public class ResultError
{
  [DataMember(Name = "property")]
  public string Property { get; set; }

  [DataMember(Name = "message")]
  public string Message { get; set; }

  public ResultError() { }

  public ResultError(string property, string message)
  {
    Property = property;
    Message = message;
  }
}
