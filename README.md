# WebArchiveViewer
**Исследователь веб-архива** – десктопное приложение на WPF'е, позволяющее просматривать и анализировать список ссылок на сохраненные в [веб-архиве](https://archive.org/web/) страницы сайтов. 



## Возможности
* Получение списка ссылок на сохраненные в архиве страницы указанного сайта;
* Просмотр списка ссылок, возможность фильтрации, сортировки и группировки;
* Категоризация ссылок по настраиваемым правилам;
* Сохранение списка ссылок в формате XML;
* Загрузка HTML-кода страницы, контроль процесса загрузки.

![Список ссылок](https://i.ibb.co/xSLZ07T/image.png)

## Запуск приложения
Необходима Windows 7 и выше с .NET Framework не ранее 4.8.

Для установки распаковать архив в папку и запустить оттуда .exe файл приложения. Версии приложения присутствуют в [релизах](https://github.com/Alleaxx/WebArchiveViewer/releases/).

## Технологии
С# 7, WPF, .NET Framework 4.8, MSTest для пары тестов, Ookii.Dialogs.WPF (окно выбора папки) и Newtonsoft Json.

Для получения ссылок используется [API веб-архива](https://github.com/internetarchive/wayback/tree/master/wayback-cdx-server).

### Проекты
- ```WebArchive.Data``` - библиотека с некоторыми общими сущностями, содержит основную логику.
- ```WebArchive.Viewer``` - WPF-приложение с интерфейсом.
- ```WebArсhive.Tests``` - содержит парочку модульных тестов для проектов. 

## Использование
Для загрузки ссылок есть два варианта:
- *Получить ссылки через веб-архив*. Для этого необходимо указать ссылку, для которой требуется найти архивные версии и, при желании, повертеть настройки.
- Использовать *сохраненные в файле* ссылки - если программкой уже попользовались и успели что-то сохранить.

Так или иначе полученный список можно сохранить в файл и использовать по прямому предназначению - посмотреть, отфильтровать, категоризовать, оценить или перейти.

По списку ссылок также можно загрузить содержимое страниц. Более подробно об этой возможности написано далее.

## Подробности
### Получение ссылок:
Производится с помощью запроса к серверу веб-архива. Для этого по правилам API веб-архива формируется строка ~ следующего вида.

```http://web.archive.org/cdx/search/cdx?url=http://ru-minecraft.ru/forum&matchType=prefix&output=json&from=20110727120000&to=20120727120000```

Запрос в приложении создается автоматически на основе введенной информации. При необходимости текст запроса можно скопировать и выполнить вручную.

![Настройки запроса](https://i.ibb.co/nBZf9Nx/image.png)

#### Коды и типы

Для фильтрации по кодам и типам следует ввести их через точку с запятой друг за другом, например:

``` text/html; !image/png```

Где ! используется для отрицания.
Используется условие И, так что единовременное задание кодов ответа 200 и 303 гарантирует 0 ссылок.

### Просмотр ссылок
Представляет постраничный вывод списка ссылок с основной информацией о каждой. Позволяет быстро перейти к архивной или актуальной версии страницы.

**Настройки отображения списка:**
* Количество отображаемых элементов на странице
* Фильтрация: по названию, дате, типу, категории
* Сортировка (для всего списка): по дате, имени, типу, адресу, порядку
* Группировка (для каждой страницы): по ссылке, имени страницы, типу, коду, категории

#### Имена страницы
Веб-архив не передает в ответе информацию о названии веб-страницы (тег title), однако это имя возможно загрузить вручную. Для этого ссылка должна иметь тип *text/html* и положительный статус-код.
#### Категоризация
Категоризация ссылок происходит по редактируемым правилам, которые могут быть вложены друг в друга. 
Правило считается подходящим, если указанная строка присутствует в имени ссылки. Категорией ссылки выбирается имя первого наиболее "глубокого" подходящего правила.

![Категоризованные ссылки](https://i.ibb.co/V3HCfsR/image.png)

### Сохранение ссылок в файл
Список полученных ссылок можно сохранить в JSON-файл для последующего использования его вместо обращения к серверу веб-архива. Сохранить можно не все ссылки, а только лишь отфильтрованные (или наоборот).

В файл также переносится информация обо всех используемых правилах.

### Загрузка содержимого страниц
Для загруженного снапшота можно указать папку для сохранения содержимого страниц, после чего начать его загрузку. С какой-то скоростью в папке будут появляться файлы, которые используются по усмотрению пользователя.

Каждый загруженный файл попутно заполняет полное имя ссылки (эту возможность можно отключить). Пути к загруженным файлам с HTML-разметкой отображаются при просмотре ссылок.

Сам прогресс загрузки сохраняется в сохраняемом файле JSON. При необходимости можно сбросить накопленный прогресс загрузки и начать заново. 

## В будущем
* Оптимизация больших списков
* Больше параметров для настройки просмотра
* Оптимизация вывода ссылок
* ???

