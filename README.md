# csharp-file-watcher-service

Сервис мониторинга каталогов и авто-сортировки файлов на C# (.NET 8).

## Возможности
- Фоновое отслеживание событий файловой системы через `FileSystemWatcher`:
  - Создание файлов (`Created`)
  - Изменение содержимого (`Changed`)
  - Удаление (`Deleted`)
  - Переименование (`Renamed`)
- Автоматическая сортировка входящих файлов по категориям (Documents, Images, Archives, Code, Other).
- Запись детального журнала событий с временными метками в файл `file_watcher.log`.
- Интерактивный режим работы с возможностью остановки по нажатию `Q`.

## Сборка
```bash
dotnet build -c Release -o releases
```

## Использование
```bash
./releases/csharp-file-watcher-service.exe [путь_к_папке]
```
