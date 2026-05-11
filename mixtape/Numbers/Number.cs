namespace Mixtape.Numbers;

/// <summary>
/// Configuration of number generation
/// Template can use the following placeholders:
/// {number} for the current number
/// {random} for a random number (default length is 5)
/// {random:7} for a random number with the specified length
/// {year} for the date in yyyy
/// {date} for the date in dd-mm-yyyy
/// {date:dmy} for the date in a custom format, where dmy is a nested placeholder for the format
/// </summary>
public class Number : MixtapeEntity
{
  /// <summary>
  /// The template to build the number.
  /// The number will reset to StartNumber once a new year is reached.
  /// This is only applicable if the template contains the placeholder {year}.
  /// Example: {year}-{number} will result in 2020-01, 2020-02, ... 2021-01, 2021-02, ...
  /// </summary>
  public string Template { get; set; } = "{number}";

  /// <summary>
  /// Where to start, lol
  /// </summary>
  public long StartNumber { get; set; } = 1;

  /// <summary>
  /// Minimum length of the number (append 0's when it is too short)
  /// </summary>
  public int MinLength { get; set; } = 1;

  /// <summary>
  /// How far to increase
  /// </summary>
  public int Step { get; set; } = 1;

  /// <summary>
  /// Whether to start a new number when a new year is reached and the template contains the yeae
  /// </summary>
  public bool ResetPerYear { get; set; } = false;

  /// <summary>
  /// Store current counters
  /// </summary>
  public List<NumberCounter> Counters { get; set; } = [];

  /// <summary>
  /// Post process the compiled number
  /// </summary>
  public virtual string PostProcess(string compiled) => compiled;
}


public class NumberCounter
{
  public string Key { get; set; }

  public long Count { get; set; }
}