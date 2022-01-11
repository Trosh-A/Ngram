using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Troshkin.Ngram.ConsoleApp
{
  class Program
  {
    static async Task Main(string[] args)
    {
      var timer = Stopwatch.StartNew();
      Console.WriteLine("Начало работы программы");
      if (args.Length == 0)
      {
        Console.WriteLine("Не задан путь к файлу");
        Console.ReadKey();
        return;
      }
      string pathToFile = args[0];
      try
      {
        long completedLines = 0;
        var cts = new CancellationTokenSource();
        var progressIndicator = new Progress<int>(line => Interlocked.Increment(ref completedLines));
        using var sr = new StreamReader(pathToFile, new FileStreamOptions() { Mode = FileMode.Open, Access = FileAccess.Read });
        var concreteTimer = Stopwatch.StartNew();
        Console.CancelKeyPress += (_, _) => cts.Cancel();
        var results = await Ngram.GetMostFreqTrigramsAsync(sr, 10, progressIndicator, cts.Token);
        concreteTimer.Stop();
        if (results is null)
        {
          Console.WriteLine("Ошибка выполнения операции");
          Console.ReadKey();
        }
        else
        {
          Console.Write($"Обработано строк: ");
          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine(completedLines);
          Console.ResetColor();
          if (!results.Any()) Console.WriteLine("Триплетов в тексте нет");
          else
          {
            Console.WriteLine("Топ 10 самых встречающихся триплетов");
            foreach (var ngram in results)
            {
              Console.WriteLine($"{ngram.Ngram} - {ngram.Count}");
            }
            Console.WriteLine($"Время работы задачи по поиску триплетов: {concreteTimer.ElapsedMilliseconds} мс");
          }
        }
      }
      catch (OverflowException)
      {
        Console.WriteLine("Текст слишком большой");
        Console.ReadKey();
      }
      catch (FileNotFoundException)
      {
        Console.WriteLine("Файл не найден");
        Console.ReadKey();
      }
      catch (PathTooLongException)
      {
        Console.WriteLine("Слишком длинный путь к файлу");
      }
      catch (DirectoryNotFoundException)
      {
        Console.WriteLine("Папка не найдена");
      }
      catch (UnauthorizedAccessException)
      {
        //readonly или не поддерживается средой исполнения или нет прав доступа
        Console.WriteLine("Нет доступа к файлу");
      }
      catch (NotSupportedException)
      {
        Console.WriteLine("Неверный формат");
      }
      //нет разрешение на чтение файла
      catch (SecurityException)
      {
        Console.WriteLine("Нет разрешения на чтение файла");
      }
      catch (IOException)
      {
        Console.WriteLine("Ошибка подсистемы ввода/вывода");
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        return;
      }
      finally
      {
        timer.Stop();
        Console.WriteLine($"Время работы всей программы {timer.ElapsedMilliseconds} мс");
        Console.WriteLine("Нажмите любую кнопку для выхода...");
        Console.ReadKey();
      }
    }
  }
}