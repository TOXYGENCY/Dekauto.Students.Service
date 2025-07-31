# Dekauto: 🔵 Сервис Студентов (Dekauto.Students.Service)
### Сервис управления данными студентов, групп и ОО (обр. организаций) и взаимодествия с базой данных в отношении этих объектов. Связующий сервис между сервисами [Импорт](https://github.com/TOXYGENCY/Dekauto.Import.Service) и [Экспорт](https://github.com/TOXYGENCY/Dekauto.Export.Service).

### 🔸 Функции
- Управление (CRUD) объектами Student и Group (+ Oo).
- Импорт данных из Excel-файлов:
  - Перенаправление входных Excel-файлов в сервис [Импорт](https://github.com/TOXYGENCY/Dekauto.Import.Service) для парсинга.
  - Загрузка полученных данных в базу данных.
- Экспорт данных:
  - Сбор и агрегирование данных для экспорта.
  - Направление этих данных в сервис [Экспорт](https://github.com/TOXYGENCY/Dekauto.Export.Service) для формирования готового файла.
  - Перенаправление результата экспорта в ответ на запрос.

### 🛠 Технологии
- .NET 8 (ASP.NET Core 8)
- OpenAPI Swagger
- Git
- Docker
- CI (GitHub Actions)
- PostgreSQL 17 (+ Entity Framework Core)
- Grafana Loki + Promtail + Prometheus (логирование и метрики)

## ❇ API-справка
### Контроллер: Students (требует роль Admin)
- `GET    api/students` - **GetAllStudentsAsync** - Список всех студентов
- `GET    api/students/{studentId}` - **GetStudentByIdAsync** - Студент по GUID
- `PUT    api/students/{studentId}` - **UpdateStudentAsync** - Обновление данных студента
- `POST   api/students` - **AddStudentAsync** - Добавление нового студента
- `DELETE api/students/{studentId}` - **DeleteStudentAsync** - Удаление студента

### Контроллер: Groups (требует роль Admin)
- `GET    api/groups` - **GetAllGroupsAsync** - Список всех групп
- `GET    api/groups/{groupId}` - **GetGroupByIdAsync** - Группа по GUID
- `PUT    api/groups/{groupId}` - **UpdateGroupAsync** - Обновление данных группы
- `POST   api/groups` - **AddGroupAsync** - Добавление новой группы
- `DELETE api/groups/{groupId}` - **DeleteGroupAsync** - Удаление группы

### Контроллер: Import (требует роль Admin)
- `POST   api/import` - **ImportFilesFromFrontendAsync** - Импорт файлов (принимает multipart/form-data)

### Контроллер: Export (требует роль Admin)
- `POST   api/export/student/{studentId}` - **ExportStudentCardAsync** - Экспорт карточки студента (Excel)
- `POST   api/export/group/{groupId}` - **ExportGroupCardsAsync** - Экспорт карточек группы (ZIP-архив)

_Контроллер: Metrics (DEPRECATED - будет удален)_
- `GET    api/students/metrics/healthcheck` - **HealthCheckAsync** - Проверка работоспособности БД (возвращает true/false)
- `GET    api/students/metrics/requests` - **RequestsPerPeriodAsync** - Получение метрик запросов (требует роль Admin)

---
># ℹ О Dekauto
>### Что такое Dekauto?
>Dekauto - это web-сервис, направленный на автоматизацию некоторых процессов работы деканата высшего учебного заведения. На данный момент система специализирована для работы в определенном ВУЗе и исполняет функции хранения, агрегации и вывода данных студентов. Ввод осуществляется через Excel-файлы определенного формата. Выводом является Excel-файл карточки студента с заполненными данными. 
>
>### Общая структура Dekauto
>* ⚪ [Dekauto.Auth.Service](https://github.com/TOXYGENCY/Dekauto.Auth.Service) - Сервис управления аккаунтами и входом. 
>    * DockerHub-образ: `toxygency/dekauto_auth_service:release`
>* 🔵 [Dekauto.Students.Service](https://github.com/TOXYGENCY/Dekauto.Students.Service) - Сервис управления Студентами. _(Вы здесь)_
>    * DockerHub-образ: `toxygency/dekauto_students_service:release`
>* 🟣 [Dekauto.Import.Service](https://github.com/TOXYGENCY/Dekauto.Import.Service) - Сервис парсинга файлов Excel для импорта.
>    * DockerHub-образ: `toxygency/dekauto_import_service:release`
>* 🟢 [Dekauto.Export.Service](https://github.com/TOXYGENCY/Dekauto.Export.Service) - Сервис формирования выходного Excel-файла.
>    * DockerHub-образ: `toxygency/dekauto_export_service:release`
>* 🟠 [Dekauto.Angular.Frontend](https://github.com/TOXYGENCY/Dekauto.Angular.Frontend) - Фронтенд: Web-приложение на Angular v19 + NGINX.
>    * DockerHub-образ: `toxygency/dekauto_frontend_nginx:release`
