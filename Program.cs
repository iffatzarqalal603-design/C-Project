using System;
using System.IO;
using System.Text.Json;
using System.Globalization;

class CalculatorSettings
{
	public string UserName { get; set; } = "User";
	public int Precision { get; set; } = 2;
	public string[] AllowedOperations { get; set; } = new[] { "+", "-", "*", "/", "^", "sqrt" };
}

class Program
{
	static string SettingsPath = "calculator_settings.json";

	static void Main()
	{
		var settings = LoadOrCreateSettings();

		Console.WriteLine($"Welcome, {settings.UserName}! Simple Calculator starting.");
		Console.WriteLine("Type expressions like: 2 + 2  or  sqrt 9  or  2 ^ 3");
		Console.WriteLine("Commands: `settings` to change, `quit` to exit");

		while (true)
		{
			Console.Write($"{settings.UserName}> ");
			var line = Console.ReadLine();
			if (line == null) break;
			line = line.Trim();
			if (line.Equals("quit", StringComparison.OrdinalIgnoreCase) || line.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
			if (line.Equals("settings", StringComparison.OrdinalIgnoreCase))
			{
				settings = PromptForSettings(settings);
				SaveSettings(settings);
				continue;
			}
			if (string.IsNullOrWhiteSpace(line)) continue;

			try
			{
				var result = Evaluate(line, settings);
				if (result.HasValue)
				{
					var fmt = "F" + settings.Precision;
					Console.WriteLine(Math.Round(result.Value, settings.Precision).ToString(fmt, CultureInfo.InvariantCulture));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex.Message);
			}
		}

		Console.WriteLine("Goodbye!");
	}

	static CalculatorSettings LoadOrCreateSettings()
	{
		try
		{
			if (File.Exists(SettingsPath))
			{
				var json = File.ReadAllText(SettingsPath);
				var s = JsonSerializer.Deserialize<CalculatorSettings>(json);
				if (s != null) return s;
			}
		}
		catch { }

		var defaultSettings = new CalculatorSettings();
		Console.WriteLine("No settings found. Let's customize your calculator.");
		defaultSettings = PromptForSettings(defaultSettings);

		Console.Write("Save these settings for next time? (y/N): ");
		var ans = Console.ReadLine();
		if (!string.IsNullOrEmpty(ans) && ans.Trim().StartsWith("y", StringComparison.OrdinalIgnoreCase))
		{
			SaveSettings(defaultSettings);
			Console.WriteLine($"Settings saved to {SettingsPath}");
		}

		return defaultSettings;
	}

	static void SaveSettings(CalculatorSettings s)
	{
		try
		{
			var options = new JsonSerializerOptions { WriteIndented = true };
			var json = JsonSerializer.Serialize(s, options);
			File.WriteAllText(SettingsPath, json);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Could not save settings: " + ex.Message);
		}
	}

	static CalculatorSettings PromptForSettings(CalculatorSettings current)
	{
		Console.Write($"Name ({current.UserName}): ");
		var name = Console.ReadLine();
		if (!string.IsNullOrWhiteSpace(name)) current.UserName = name.Trim();

		Console.Write($"Precision (decimal places) ({current.Precision}): ");
		var prec = Console.ReadLine();
		if (int.TryParse(prec, out var p) && p >= 0) current.Precision = p;

		Console.WriteLine("Allowed operations (comma separated). Available: +, -, *, /, ^, sqrt");
		Console.Write($"Current: {string.Join(",", current.AllowedOperations)} : ");
		var ops = Console.ReadLine();
		if (!string.IsNullOrWhiteSpace(ops))
		{
			var parts = ops.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			current.AllowedOperations = parts;
		}

		return current;
	}

	static double? Evaluate(string expr, CalculatorSettings settings)
	{
		// Support: binary "a op b" and unary "sqrt a"
		var parts = expr.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (parts.Length == 2) // unary like: sqrt 9
		{
			var op = parts[0].ToLowerInvariant();
			if (!Array.Exists(settings.AllowedOperations, o => o.Equals(op, StringComparison.OrdinalIgnoreCase)))
				throw new InvalidOperationException($"Operation '{op}' not allowed by settings.");

			if (!double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var a))
				throw new FormatException("Cannot parse number: " + parts[1]);

			return EvaluateUnary(op, a);
		}

		if (parts.Length == 3) // binary: a op b
		{
			if (!double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var a))
				throw new FormatException("Cannot parse number: " + parts[0]);
			var op = parts[1];
			if (!double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var b))
				throw new FormatException("Cannot parse number: " + parts[2]);

			if (!Array.Exists(settings.AllowedOperations, o => o.Equals(op, StringComparison.OrdinalIgnoreCase)))
				throw new InvalidOperationException($"Operation '{op}' not allowed by settings.");

			return EvaluateBinary(op, a, b);
		}

		// Try to evaluate as a single number
		if (parts.Length == 1 && double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
			return v;

		throw new FormatException("Expression not recognized. Use 'a op b' or 'sqrt a'");
	}

	static double EvaluateUnary(string op, double a)
	{
		return op.ToLowerInvariant() switch
		{
			"sqrt" =>
				a < 0 ? throw new InvalidOperationException("Cannot take square root of negative number") : Math.Sqrt(a),
			_ => throw new InvalidOperationException($"Unknown unary operator: {op}")
		};
	}

	static double EvaluateBinary(string op, double a, double b)
	{
		return op switch
		{
			"+" => a + b,
			"-" => a - b,
			"*" or "x" => a * b,
			"/" => b == 0 ? throw new DivideByZeroException() : a / b,
			"^" => Math.Pow(a, b),
			_ => throw new InvalidOperationException($"Unknown binary operator: {op}")
		};
	}
}
