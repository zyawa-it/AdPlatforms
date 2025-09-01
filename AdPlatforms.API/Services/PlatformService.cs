namespace AdPlatforms.API.Services;

public class PlatformService : IPlatformService
{
    private volatile Dictionary<string, List<string>> _locationToPlatforms = new();

    public async Task LoadPlatformsAsync(Stream fileStream)
    {
        var newLocationToPlatforms = new Dictionary<string, List<string>>();

        using (var reader = new StreamReader(fileStream))
        {
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                // Пропуск пустых или содержащих только пробелы строк
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Делим строку по первому ':' на 2 части
                var split = line.Split(':', 2);

                // Если не получилось 2 части - пропускаем строку (invalid format)
                if (split.Length != 2) continue;

                // Получение имя и блок локаций без пробелов
                var platformName = split[0].Trim();
                var locationPart = split[1].Trim();

                // Если имя или локации пустые - пропускаем
                if (string.IsNullOrEmpty(platformName) ||
                    string.IsNullOrEmpty(locationPart)) continue;

                // Разделения блока локаций по запятым
                var locations = locationPart.Split(',');

                // Заполнение словаря
                foreach (var location in locations)
                {
                    var trimmedLocation = location.Trim();
                    if (string.IsNullOrEmpty(trimmedLocation)) continue;

                    // Есть ли уже в словаре ключ с такой локацией
                    if (!newLocationToPlatforms.ContainsKey(trimmedLocation))
                    {
                        // Если нет - создаем новый пустой список
                        newLocationToPlatforms[trimmedLocation] = new List<string>();
                    }

                    // Добавляем имя в список для этой локации
                    newLocationToPlatforms[trimmedLocation].Add(platformName);
                }
            }
        }

        // Заменяем старый словарь - новым
        _locationToPlatforms = newLocationToPlatforms;
    }

    public IEnumerable<string> FindPlatforms(string location)
    {
        // Проверка корректности входных данных
        if (string.IsNullOrEmpty(location) || !location.StartsWith("/"))
        {
            // Если incorrect - возвращаем пустую коллекцию
            return Enumerable.Empty<string>();
        }

        // Создаем HashSet (хранит уникальные данные)
        var result = new HashSet<string>();
        var currentLocation = location;

        // Цикл к верхам по дереву локаций
        while (!string.IsNullOrEmpty(currentLocation))
        {
            // Ищем площадки для текущей локации в словаре
            if (_locationToPlatforms.TryGetValue(currentLocation, out var platforms))
            {
                // Если есть - добавляем в result
                foreach (var platform in platforms)
                {
                    result.Add(platform);
                }
            }

            var lastSlashIndex = currentLocation.LastIndexOf('/');

            // Если слэш не в начале строки
            if (lastSlashIndex > 0)
            {
                // Обрезаем строку до этого слэша.
                currentLocation = currentLocation.Substring(0, lastSlashIndex);
            }
            else
            {
                currentLocation = (lastSlashIndex == 0 && currentLocation.Length > 1) ? "/" : string.Empty;
            }
        }

        // Дополнительно проверяем корень "/", если он не был обработан в цикле.
        if (_locationToPlatforms.TryGetValue("/", out var rootPlatforms))
        {
            foreach (var platform in rootPlatforms)
            {
                result.Add(platform);
            }
        }

        // Шаг 5: Возвращаем собранные результаты.
        return result;
        
    }
}