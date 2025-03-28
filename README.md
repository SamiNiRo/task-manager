# Task-менеджер
## Команда №7 "Славяночка"
#### Список участников

| Участник                         | Аккаунт GitHub                                 | Роль                 |
| -------------------------------- | ---------------------------------------------- | -------------------- |
| Маркелов Сергей Александрович    | [@SamiNiRo](https://github.com/SamiNiRo)       | Тимлид               |
| Подтёлков Владислав Владимирович | [@Feym4n](https://github.com/Feym4n)           | Backend-разработчик  |
| Фатькина Алёна Дмитриевна        | [@saenshisunn](https://github.com/saenshisunn) | Frontend-разработчик |
## Описание продукта
**Task-менеджер** – это приложение для управления задачами. Он помогает планировать, отслеживать и организовывать работу, устанавливать дедлайны, назначать исполнителей и контролировать выполнение. Приложение выполнено в стиле [канбан-доски](https://ru.wikipedia.org/wiki/%D0%9A%D0%B0%D0%BD%D0%B1%D0%B0%D0%BD-%D0%B4%D0%BE%D1%81%D0%BA%D0%B0)
Основные функции task-менеджера:
- **Создание задач** – можно записывать задачи, делить их на подзадачи
- **Приоритизация** – определение важности и срочности задач
- **Дедлайны** – установка сроков выполнения
- **Назначение исполнителей** – если это командная работа
- **Отслеживание прогресса** – статус выполнения задач
- **Уведомления и напоминания** – чтобы не забыть о задачах
## Макет интерфейса
![Макет интерфейса](/Pictures/01.%20Макет%20интерфейса.png)
## Используемые технологии
1. **WPF** – это технология для создания красивых десктопных приложений на C# с использованием XAML (язык разметки интерфейса)
2. **MVVM** – это шаблон (правило), который помогает разделить код на три части:
	- Model (Модель) – данные (список задач)
	- View (Представление) – интерфейс (кнопки, списки, окна)
	- ViewModel (Логика) – связывает данные и интерфейс
3. **PRISM** – это библиотека для WPF, которая помогает сделать приложение модульным
4. **Git** – это система контроля версий
5. **SQLite** – это встраиваемая реляционная база данных, которая позволяет приложению хранить данные в локальном файле
## Статус разработки
На данный момент разработан следующий функционал
- Интерфейс канбан-доски с 5 колонками: 
	- **Бэклог** - задачи, которые ставит команда
	- **Запросы** - запросы от пользователей
	- **Выбрано** - задачи, которые планируется выполнить (берутся из Бэклога и Запросов)
	- **В работе** - задачи, над которыми сейчас ведется работа
	- **Завершено** - выполненные задачи
	  
	![Интерфейс](/Pictures/02.%20Интерфейс.png)
- Возможность создания новых задач в любой колонке  
	![Создание задач](/Pictures/03.%20Создание%20задач.png)
- Возможность редактирования и удаления существующих задач  
	![Редактирование задач](/Pictures/04.%20Редактирование%20задач.png)
- Перемещение задач между колонками через drag-and-drop
- Автоматическое сохранение всех изменений в БД  
	![База данных](/Pictures/05.%20База%20данных.png)
## Планы на дальнейшую разработку
Будет реализовано точно:
- Подзадачи
- Комментарии к задачам
- Теги, метки
- Дедлайны
- Метка "Сделано"
- Прогресс задач
- Возможность переключения между досками
- Поиск задач
- Фильтрация задач
- Исполнители
  
Под вопросом:
- Аккаунты (как расширение функционала исполнителей)
- Уведомления
- Темная тема
- Интеграция с Google Calendar
