# OlxClone

Платформа за обяви (в стил OLX), написана на **ASP.NET Core MVC 8** с **Entity Framework Core** и **MS SQL Server**.

## Съдържание

- [Важно за Fedora и LocalDB](#важно-за-fedora-и-localdb)
- [Инсталация стъпка по стъпка](#инсталация-стъпка-по-стъпка)
- [Акаунти по подразбиране](#акаунти-по-подразбиране)
- [Функционалности](#функционалности)
- [Структура на проекта](#структура-на-проекта)
- [Модел на данните](#модел-на-данните)
- [Често срещани проблеми](#често-срещани-проблеми)

---

## Важно за Fedora и LocalDB

**LocalDB не работи на Linux.** SQL Server Express LocalDB е компонент само за Windows — няма Linux версия и не може да се инсталира на Fedora.

Затова проектът е конфигуриран за **пълноценен MS SQL Server 2022**, пуснат в контейнер. Това е официално поддържаният от Microsoft начин за SQL Server на Linux и е стандартът за разработка на Fedora. Приложението и SQL кодът остават 100% същите — сменя се само connection string-ът, така че ако някога отвориш проекта на Windows с LocalDB, работи без промени по кода.

---

## Инсталация стъпка по стъпка

### 1. Инсталирай .NET 8 SDK

```bash
sudo dnf install dotnet-sdk-8.0
dotnet --version    # трябва да покаже 8.0.xxx
```

### 2. Инсталирай Podman (ако го нямаш)

Podman е предпочитаният контейнер engine на Fedora и идва предварително инсталиран в повечето издания.

```bash
sudo dnf install podman
```

### 3. Стартирай SQL Server

От основната папка на проекта:

```bash
chmod +x start-db.sh
./start-db.sh
```

Скриптът сваля образа на SQL Server 2022, стартира го на порт `1433` и изчаква, докато базата започне да приема връзки. Данните се пазят в volume `olxclone-sqldata`, така че оцеляват след рестарт на контейнера.

> Ако предпочиташ compose: `podman-compose up -d` (или `docker compose up -d`).

**Проверка, че върви:**

```bash
podman ps
```

### 4. Стартирай приложението

```bash
cd OlxClone.Web
dotnet restore
dotnet run
```

Отвори <http://localhost:5289> в браузъра.

При първото стартиране приложението автоматично:
- прилага миграциите и създава базата `OlxCloneDb`,
- създава ролите `Admin` и `User`,
- създава администраторския акаунт.

Не е нужно да пускаш `dotnet ef database update` ръчно.

### 5. Спиране

```bash
podman stop olxclone-sql     # спира базата
podman start olxclone-sql    # пуска я отново
```

---

## Акаунти по подразбиране

| Роля | Имейл | Парола |
|---|---|---|
| Админ | `admin@olxclone.local` | `Admin123!` |
| Потребител | регистрирай се сам от формата | — |

Всеки нов регистриран акаунт автоматично получава роля `User`. Данните на админа се сменят от `appsettings.json` → секция `AdminSeed`.

---

## Функционалности

### Регистрация и роли
- Регистрация с имейл и парола (ASP.NET Core Identity).
- Две роли: **Admin** и **User**.
- Всички обяви са видими само за влезли потребители — неавтентикираните се пренасочват към формата за вход.

### Обяви
- Обявата съдържа **заглавие, описание, телефонен номер и снимки** (няколко на брой).
- Потребителят създава, **редактира** и **изтрива** собствените си обяви.
- При изтриване се трият и файловете със снимките от диска.
- При редакция обявата се връща за повторно одобрение.

### Одобрение от админ
- Новата обява влиза със статус **Pending** и не е публична.
- Админът вижда опашка на `/Admin/Pending` и може да **одобри** или **откаже** обявата, като по желание напише причина за отказа.
- Отказаната обява се вижда само от собственика ѝ, заедно с причината.
- `/Admin/All` дава преглед на всички обяви с филтър по статус.

### Търсене
- Търсене по заглавие от полето в навигацията (`LIKE '%term%'`, с индекс върху колоната `Title`).

### Любими
- Бутон-сърце върху всяка карта и в детайлната страница.
- Отделно меню **Любими** (`/Favorites`) показва само отбелязаните обяви.
- Уникален индекс `(UserId, ListingId)` пази от дублиране.

### Брояч на прегледи
- Полето `ViewCount` се увеличава при всяко отваряне на детайлната страница на обявата.
- Показва се на картата, в детайлите и в таблиците на потребителя и админа.

---

## Структура на проекта

```
OlxClone/
├── OlxClone.sln
├── docker-compose.yml          # SQL Server за локална разработка
├── start-db.sh                 # помощен скрипт (podman/docker)
└── OlxClone.Web/
    ├── Program.cs              # DI, Identity, pipeline, авто-миграции
    ├── appsettings.json        # connection string + данни на админа
    ├── Data/
    │   ├── ApplicationDbContext.cs
    │   └── SeedData.cs         # роли + админ акаунт
    ├── Models/
    │   ├── ApplicationUser.cs
    │   ├── Listing.cs
    │   ├── ListingImage.cs
    │   ├── ListingStatus.cs
    │   ├── Favorite.cs
    │   └── ViewModels/
    ├── Controllers/
    │   ├── AccountController.cs    # регистрация / вход / изход
    │   ├── ListingsController.cs   # CRUD, търсене, прегледи
    │   ├── FavoritesController.cs  # добавяне/махане от любими
    │   ├── AdminController.cs      # одобрение и отказ
    │   └── HomeController.cs
    ├── Views/
    ├── Migrations/
    └── wwwroot/
        ├── css/site.css
        ├── js/site.js
        └── uploads/listings/       # качените снимки
```

---

## Модел на данните

```
AspNetUsers (ApplicationUser)
   │  1                          1  │
   │                                │
   │  *                          *  │
Listings ──── 1 : * ────► ListingImages
   │  1
   │
   │  *
Favorites ◄──── * : 1 ──── AspNetUsers
```

**Listing:** `Id`, `Title`, `Description`, `PhoneNumber`, `Status` (Pending/Approved/Rejected), `RejectionReason`, `ViewCount`, `CreatedAt`, `UpdatedAt`, `UserId`

**ListingImage:** `Id`, `ListingId`, `FileName`, `SortOrder` — самите файлове стоят на диска в `wwwroot/uploads/listings/{ListingId}/`, в базата се пази само името.

**Favorite:** `Id`, `UserId`, `ListingId`, `CreatedAt` — с уникален индекс върху `(UserId, ListingId)`.

### Работа с миграции

Проектът вече съдържа началната миграция. Ако промениш модел:

```bash
cd OlxClone.Web
dotnet ef migrations add ИмеНаПромяната
dotnet ef database update
```

Ако нямаш `dotnet ef`:

```bash
dotnet tool install --global dotnet-ef
export PATH="$PATH:$HOME/.dotnet/tools"
```

---

## Често срещани проблеми

**`A network-related or instance-specific error occurred`**
SQL Server контейнерът не върви. Пусни `podman ps`, а ако липсва — `./start-db.sh`.

**`Login failed for user 'sa'`**
Паролата в `appsettings.json` не съвпада с тази на контейнера. Двете места трябва да са еднакви: `MSSQL_SA_PASSWORD` в `start-db.sh` / `docker-compose.yml` и connection string-ът.

**`The certificate chain was issued by an authority that is not trusted`**
Connection string-ът трябва да съдържа `TrustServerCertificate=True` — вече е зададено.

**Контейнерът стартира и веднага спира**
Най-често паролата е твърде слаба — SQL Server изисква поне 8 символа с главна буква, малка буква, цифра и специален знак. Виж логовете: `podman logs olxclone-sql`.

**Порт 1433 е зает**
`sudo ss -tulpn | grep 1433` показва кой го държи. Или смени порта на `1434:1433` в скрипта и съответно в connection string-а.

**Снимките не се показват**
Провери, че папката `wwwroot/uploads/listings` съществува и приложението има права за писане в нея.

**SELinux блокира volume-а (специфично за Fedora)**
Ако контейнерът не може да пише, добави `:Z` към volume-а или ползвай named volume — скриптът вече ползва named volume, така че този проблем не би трябвало да се появи.
