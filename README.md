# Palantir / Палантир

> ⚠️ Project status: **In development**  
> ⚠️ Статус проекта: **находится в разработке**

---

## 🇷🇺 Описание проекта

**Палантир** — это веб-приложение для визуализации пространственно-временных данных о исторических военных конлфиктах.

Цель проекта — создать интерактивную систему, которая позволит отображать военные конфликты на карте с привязкой к времени. Приложение ориентировано на работу с историческими, отображение событий, операций, воинских соединений, линий фронта и зон контроля.

Проект находится в активной разработке, поэтому часть функциональности может быть не реализована или изменяться в процессе работы.

---

## Основная идея

Обычные исторические карты часто показывают только статичную ситуацию на определённую дату.  
**Палантир** должен позволить рассматривать боевые действия как динамический процесс:

- отображать изменения на карте по датам;
- показывать линии фронта и зоны контроля;
- связывать события с конкретным местом и временем;
- отображать информацию о военных операциях, сторонах конфликта и соединениях;
- использовать временную шкалу для навигации по хронологии событий.

---

## Планируемые возможности

- Интерактивная карта.
- Временная шкала для выбора даты или периода.
- Отображение боевых операций и сражений.
- Отображение сторон конфликта.
- Отображение воинских соединений.
- Визуализация линий фронта и зон контроля.
- Информационные карточки событий.
- Фильтрация данных по конфликту, стороне, периоду времени и типу объекта.
- Работа с пространственными данными в формате GeoJSON.

---

## Технологический стек

### Backend

- C#
- ASP.NET Core
- REST API
- Entity Framework Core
- LINQ
- Dependency Injection

### Database

- PostgreSQL
- PostGIS

### Frontend

- HTML
- CSS
- JavaScript
- Bootstrap

### Data formats

- JSON
- GeoJSON

---

## Архитектура проекта

Проект строится на основе многоуровневой архитектуры:

- **Presentation** — уровень представления, API-контроллеры и пользовательский интерфейс.
- **Application** — прикладная логика и сценарии работы системы.
- **Domain** — доменные сущности предметной области.
- **Infrastructure** — работа с базой данных, репозитории, Entity Framework Core, PostGIS.

Такое разделение позволяет отделить бизнес-логику от инфраструктурных деталей и упростить дальнейшее развитие проекта.

---

## Основные сущности предметной области

В проекте предполагается работа со следующими сущностями:

- конфликты;
- стороны конфликта;
- театры военных действий;
- военные операции;
- сражения;
- события;
- армии и воинские соединения;
- позиции соединений;
- линии фронта;
- зоны контроля;
- географические объекты;
- материалы и источники.

---

## Текущий статус

Проект находится на этапе разработки MVP.

На данный момент ведётся работа над:

- доменной моделью;
- структурой базы данных;
- репозиториями;
- сервисным слоем;
- DTO / Request / Response моделями;
- REST API;
- базовой архитектурой приложения.

---

## Цель проекта

Проект разрабатывается в учебных целях как веб-информационная система для визуализации данных о боевых действиях.

Основной акцент делается на:

- проектирование базы данных;
- работу с пространственными данными;
- построение REST API;
- многоуровневую архитектуру;
- разделение ответственности между слоями приложения;
- подготовку основы для будущей интерактивной карты.

---

---

## License

Лицензия пока не указана.

---

# Palantir

> ⚠️ Project status: **In development**  
> ⚠️ Статус проекта: **находится в разработке**

---

## 🇬🇧 Project Description

**Palantir** is a web application for visualizing spatio-temporal data about historycal military conflicts.

The goal of the project is to create an interactive system that displays military conflicts on a map with a connection to time. The application is intended to work with both historical and modern conflicts, including events, operations, military units, front lines, and control zones.

The project is currently in active development, so some features may be incomplete or changed during development.

---

## Main Idea

Traditional historical maps usually show a static situation for a specific date.  
**Palantir** aims to represent military conflicts as a dynamic process:

- display map changes by date;
- show front lines and control zones;
- connect events with a specific place and time;
- display information about military operations, conflict sides, and military units;
- use a timeline for chronological navigation.

---

## Planned Features

- Interactive map.
- Timeline for selecting a date or period.
- Display of military operations and battles.
- Display of conflict sides.
- Display of military units.
- Visualization of front lines and control zones.
- Event information cards.
- Filtering by conflict, side, time period, and object type.
- Support for spatial data using GeoJSON.

---

## Technology Stack

### Backend

- C#
- ASP.NET Core
- REST API
- Entity Framework Core
- LINQ
- Dependency Injection

### Database

- PostgreSQL
- PostGIS

### Frontend

- HTML
- CSS
- JavaScript
- Bootstrap

### Data Formats

- JSON
- GeoJSON

---

## Project Architecture

The project is based on a layered architecture:

- **Presentation** — API controllers and user interface.
- **Application** — application logic and use cases.
- **Domain** — domain entities and core business objects.
- **Infrastructure** — database access, repositories, Entity Framework Core, and PostGIS integration.

This structure separates business logic from infrastructure details and makes the project easier to maintain and extend.

---

## Main Domain Entities

The project is expected to work with the following entities:

- conflicts;
- conflict sides;
- theaters of war;
- military operations;
- battles;
- events;
- armies and military units;
- unit positions;
- front lines;
- control zones;
- geographic objects;
- materials and sources.

---

## Current Status

The project is currently at the MVP development stage.

Current development focus:

- domain model;
- database structure;
- repositories;
- service layer;
- DTO / Request / Response models;
- REST API;
- basic application architecture.

---

## Project Goal

This project is being developed for educational purposes as a web information system for visualizing data about military conflicts.

The main focus is on:

- database design;
- working with spatial data;
- building a REST API;
- layered architecture;
- separation of responsibilities between application layers;
- preparing the foundation for a future interactive map.

---

## License

The license has not been specified yet.
