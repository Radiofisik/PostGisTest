---
title: PostGis
description: Работа с геоданными
---

Для использования [PostGis](https://www.npgsql.org/efcore/mapping/nts.html) с EF Core установим из nuget `NetTopologySuite` и плагин на Postgres.  Добавим в код контекста

```c#
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres",
                o => o.UseNetTopologySuite());
        }

protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasPostgresExtension("postgis");
        }
```

создадим сущность с использованием геоданных

```c#
 public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [Column(TypeName = "geometry (point)")]
        public Point Location { get; set; }
    }
```

добавим данные в таблицу

```c#
var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
var cities = new List<City>()
    {
        new City() {Name = "Kaliningrad", Location = geometryFactory.CreatePoint(new Coordinate(20.4640505, 54.7406165))},
        new City() {Name = "Pionerskiy", Location = geometryFactory.CreatePoint(new Coordinate(20.2377613, 54.9487813))}
    };

var context = new BloggingContext();
await context.Database.MigrateAsync();

context.Cities.Add(cities);
context.SaveChanges();
```

Тут важно заметить что для задания координат используется система координат с SRID 4326 (WGS 84) [используемую в GPS](https://docs.microsoft.com/ru-ru/ef/core/modeling/spatial). Координаты представлены в виде (X, Y) где Х -  долгота, Y -  широта, что выглядит не привычно, потому что обычно они следуют в обратном порядке.

Пусть есть точка с координатами 54.718202, 20.4976303 Найдем города, расстояние до которых меньше 10км.

```C#
  var cities = context.Cities.Where(c => c.Location.Distance(work) < 10/111.0).ToList();
```

Тут используется то приближение что в одном градусе примерно 111 километров, а .Location.Distance() преобразуется в функцию ST_Distance, которая возвращает результат в градусах.

Теперь пусть есть точка с координатами 54.718202, 20.4976303 определим расстояние до городов

```sql
SELECT "Name", ST_AsEWKT("Location"),
       ST_DistanceSphere(
         "Location",
         ST_GeomFromEWKT('SRID=4326;POINT(20.4976303 54.718202)')
       )  AS dist
FROM "Cities" ORDER BY dist LIMIT 3;


Kaliningrad	SRID=4326;POINT(20.4640505 54.7406165)	3295.56496736
Pionerskiy	SRID=4326;POINT(20.2377613 54.9487813)	30567.22264395
```

тут используется функция `ST_DistanceSphere` которая вычисляет расстояние в метрах в отличие от ST_Distance. Для повышения точности можно использовать функцию ST_DistanceSpheroid которая принимает параметром параметры геоида `SPHEROID["WGS 84",6378137,298.257223563]`

```sql
SELECT "Name", ST_AsEWKT("Location"),
       ST_DistanceSpheroid(
         "Location",
         ST_GeomFromEWKT('SRID=4326;POINT(20.4976303 54.718202)'),
           'SPHEROID["WGS 84",6378137,298.257223563]'
       )  AS dist
FROM "Cities" ORDER BY dist LIMIT 3;

Kaliningrad	SRID=4326;POINT(20.4640505 54.7406165)	3302.3994771128814
Pionerskiy	SRID=4326;POINT(20.2377613 54.9487813)	30621.986248194833
```

Более оптимально использовать функцию ST_DWithin, которая умеет работать с индексами

```sql
SELECT "Name", ST_AsEWKT("Location")
FROM "Cities"
WHERE ST_DWithin(
        "Location",
        ST_GeomFromEWKT('SRID=4326;POINT(20.4976303 54.718202)'),
        0.1);
```



Нашел неплохие заметки по оптимизации запросов https://eax.me/postgis/

Для отображения геоданных можно использовать QGIS https://habr.com/ru/post/321710/