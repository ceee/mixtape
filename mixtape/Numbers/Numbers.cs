using System.Text.RegularExpressions;

namespace Mixtape.Numbers;

public class Numbers : INumbers
{
  protected IMixtapeNumberStoreDbProvider Db { get; set; }

  protected IMixtapeOptions Options { get; set; }

  const string DEFAULT_COUNTER = "Default";
  Regex templateRegex = new("{(date|number|year|random):?([a-zA-Z0-9-_]*)}", RegexOptions.IgnoreCase);
  Regex containsYearRegex = new("{(date|year|date:([^}]*y+[^}]*))}", RegexOptions.IgnoreCase);
  Random random = new();


  public Numbers(IMixtapeNumberStoreDbProvider db, IMixtapeOptions options)
  {
    Db = db;
    Options = options;
  }


  /// <inheritdoc />
  public Task<string> Next(string alias, DateTimeOffset? date = null, bool store = true)
  {
    return Next(alias, date.HasValue ? DateOnly.FromDateTime(date.Value.Date) : null, store);
  }


  /// <inheritdoc />
  public async Task<string> Next(string alias, DateOnly? date = null, bool store = true)
  {
    Number number = await Get(alias);
    string key = GetCounterKey(number);

    // get a matching counter
    NumberCounter counter = number.Counters.FirstOrDefault(x => x.Key == key) ?? new() { Key = key };

    // calculate new value
    bool hasValue = counter.Count != 0;
    long oldValue = hasValue ? counter.Count : number.StartNumber;
    long newValue = oldValue + (hasValue ? number.Step : 0);

    // increment the value and store it in database
    if (store)
    {
      counter.Count = newValue;
      if (!hasValue)
      {
        number.Counters.Add(counter);
      }
      await Db.Update(number);
    }

    // compiles the template and returns the rendered result
    return Compile(number.Template, newValue, number.MinLength, date);
  }


  /// <inheritdoc />
  public async Task<string> Set(string alias, long value, DateTimeOffset? date = null)
  {
    Number number = await Get(alias);
    string key = GetCounterKey(number);

    // get a matching counter
    NumberCounter counter = number.Counters.FirstOrDefault(x => x.Key == key) ?? new() { Key = key };
    bool isNew = counter.Count == 0;

    // set new value
    counter.Count = value;
    if (isNew)
    {
      number.Counters.Add(counter);
    }
    await Db.Update(number);

    // compiles the template and returns the rendered result
    return Compile(number.Template, value, number.MinLength, date, number.PostProcess);
  }


  /// <inheritdoc />
  public async Task Reset(string alias)
  {
    string id = Id(alias);
    Number number = await Db.Load<Number>(id);

    if (number == null)
    {
      return;
    }

    string key = GetCounterKey(number);

    NumberCounter counter = number.Counters.FirstOrDefault(x => x.Key == key);

    if (counter != null)
    {
      number.Counters.Remove(counter);
      await Db.Update(number);
    }
  }


  /// <inheritdoc />
  public async Task ResetAll(string alias)
  {
    string id = Id(alias);
    Number number = await Db.Load<Number>(id);

    if (number == null)
    {
      return;
    }

    number.Counters = [];
    await Db.Update(number);
  }


  /// <inheritdoc />
  public string Compile(string template, long value, int minLength, DateTimeOffset? date = null, Func<string, string> postProcess = null)
  {
    return Compile(template, value, minLength, date.HasValue ? DateOnly.FromDateTime(date.Value.Date) : null, postProcess);
  }


  /// <inheritdoc />
  public string Compile(string template, long value, int minLength, DateOnly? date = null, Func<string, string> postProcess = null)
  {
    string output = template;
    MatchCollection matches = templateRegex.Matches(output);
    DateOnly usedDate = date ?? DateOnly.FromDateTime(DateTime.Now);

    foreach (Match match in matches)
    {
      if (!match.Success)
      {
        continue;
      }

      string original = match.Value;
      string type = match.Groups[1].Value;
      string options = match.Groups[2].Value;
      string result = string.Empty;

      if (type == "year")
      {
        result = usedDate.Year.ToString();
      }
      else if (type == "date")
      {
        result = usedDate.ToString(options.IsNullOrWhiteSpace() ? "dd-MM-yyyy" : options);
      }
      else if (type == "number")
      {
        result = value.ToString("D" + (minLength > 0 && minLength <= 16 ? minLength : 1));
      }
      else if (type == "random")
      {
        int length = 5;
        if (!options.IsNullOrWhiteSpace() && int.TryParse(options, out int oLength) && oLength > 0 && oLength <= 10)
        {
          length = oLength;
        }
        result = random.Next(0, int.MaxValue).ToString("D10").Substring(0, length);
      }

      output = output.ReplaceFirst(original, result);
    }

    if (postProcess != null)
    {
      output = postProcess(output);
    }

    return output;
  }


  /// <summary>
  /// Create a new number implementation
  /// </summary>
  protected async Task<Number> Get(string alias)
  {
    NumberOptionsItem options = Options.For<NumberOptions>().GetValueOrDefault(alias);

    if (options == null)
    {
      throw new KeyNotFoundException($"Could not find number options (mixtape:numbers:) for alias {alias}");
    }

    string id = Id(alias);

    Number number = await Db.Load<Number>(id);
    bool exists = number != null;

    if (!exists)
    {
      number = new()
      {
        Id = id,
        Template = options.Template,
        StartNumber = options.StartNumber,
        MinLength = options.MinLength,
        Step = options.Step,
        ResetPerYear = options.ResetPerYear
      };

      await Db.Create(number);
    }
    else
    {
      bool changed = false;

      if (!number.Template.Equals(options.Template))
      {
        number.Template = options.Template;
        changed = true;
      }
      if (!number.StartNumber.Equals(options.StartNumber))
      {
        number.StartNumber = options.StartNumber;
        changed = true;
      }
      if (!number.MinLength.Equals(options.MinLength))
      {
        number.MinLength = options.MinLength;
        changed = true;
      }
      if (!number.Step.Equals(options.Step))
      {
        number.Step = options.Step;
        changed = true;
      }
      if (!number.ResetPerYear.Equals(options.ResetPerYear))
      {
        number.ResetPerYear = options.ResetPerYear;
        changed = true;
      }

      if (changed)
      {
        await Db.Update(number);
      }
    }

    return number;
  }


  /// <summary>
  /// Generate number ID
  /// </summary>
  protected string Id(string alias)
  {
    return "numbers." + alias;
  }


  /// <summary>
  /// Get the key for the current counter
  /// </summary>
  protected virtual string GetCounterKey(Number number)
  {
    if (!number.ResetPerYear || !containsYearRegex.IsMatch(number.Template))
    {
      return DEFAULT_COUNTER;
    }

    return DateTimeOffset.Now.Year.ToString();
  }
}


public interface INumbers
{
  /// <summary>
  /// Get the next available number for the specified template
  /// </summary>
  Task<string> Next(string alias, DateTimeOffset? date = null, bool store = true);

  /// <summary>
  /// Get the next available number for the specified template
  /// </summary>
  Task<string> Next(string alias, DateOnly? date = null, bool store = true);

  /// <summary>
  /// Set a number to a value
  /// </summary>
  Task<string> Set(string alias, long value, DateTimeOffset? date = null);

  /// <summary>
  /// Resets the current counter for the specified template
  /// </summary>
  Task Reset(string alias);

  /// <summary>
  /// Resets all counters for the specified template
  /// </summary>
  Task ResetAll(string alias);

  /// <summary>
  /// Renders the final output from the template and the value
  /// </summary>
  string Compile(string template, long value, int minLength, DateTimeOffset? date = null, Func<string, string> postProcess = null);

  /// <summary>
  /// Renders the final output from the template and the value
  /// </summary>
  string Compile(string template, long value, int minLength, DateOnly? date = null, Func<string, string> postProcess = null);
}