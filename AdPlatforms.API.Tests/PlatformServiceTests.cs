using System.Text;
using AdPlatforms.API.Services;

namespace AdPlatforms.API.Tests;

public class PlatformServiceTests
{
    private readonly IPlatformService _service;

    // Конструктор теста.
    // Мы один раз создаем сервис и загружаем в него тестовые данные.
    public PlatformServiceTests()
    {
        _service = new PlatformService();
        var fileContent = @"
Яндекс.Директ:/ru
Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
Крутая реклама:/ru/svrd
";
        // Эмулируем файл в памяти, чтобы не зависеть от реальных файлов.
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
        // .Wait() нужен, чтобы дождаться завершения асинхронной операции в синхронном конструкторе.
        _service.LoadPlatformsAsync(stream).Wait();
    }

    // Запустить один тест с разными наборами данных.
    [Theory]
    // Каждый [InlineData] - это один запуск теста с конкретными параметрами.
    [InlineData("/ru/msk", new[] { "Газета уральских москвичей", "Яндекс.Директ" })]
    [InlineData("/ru/svrd", new[] { "Крутая реклама", "Яндекс.Директ" })]
    [InlineData("/ru/svrd/revda", new[] { "Ревдинский рабочий", "Крутая реклама", "Яндекс.Директ" })]
    [InlineData("/ru", new[] { "Яндекс.Директ" })]
    [InlineData("/ru/svrd/ekb", new[] { "Крутая реклама", "Яндекс.Директ" })] // Тест на вложенную, но несуществующую локацию
    [InlineData("/com", new string[0])] // Тест на локацию, для которой ничего не должно найтись
    public void FindPlatforms_ReturnsCorrectPlatforms(string location, string[] expected)
    {
        // Вызываем тестируемый метод
        var result = _service.FindPlatforms(location).ToList();

        // Сравниваем результат с ожидаемым
        Assert.Equal(expected.Length, result.Count);
        
        // Проверяем, что все ожидаемые элементы есть в результате.
        Assert.All(expected, e => Assert.Contains(e, result));
    }

}