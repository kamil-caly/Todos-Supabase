# Todos-Supabase

Prosta aplikacja **Todo List** z rozdzielonym frontendem i backendem.

## Technologie

### Frontend

* HTML5
* CSS3
* JavaScript

### Backend

* ASP.NET Core Web API (.NET 10)
* Dapper
* PostgreSQL

### Hosting

* **Frontend:** GitHub Pages
* **Backend:** Render (Docker)
* **Baza danych:** Supabase (PostgreSQL)

## Architektura

```text
GitHub Pages
      │
      ▼
HTML + JavaScript
      │
      ▼
ASP.NET Core Web API (Render)
      │
      ▼
Supabase PostgreSQL
```

## Funkcjonalności

* Dodawanie zadań
* Wyświetlanie listy zadań
* Edycja zadań
* Usuwanie zadań
* Komunikacja z backendem za pomocą REST API

## Uruchomienie lokalne

### Backend

1. Przejdź do katalogu `backend`.
2. Skonfiguruj połączenie z bazą danych w `appsettings.Development.json` lub za pomocą zmiennej środowiskowej `ConnectionStrings__Supabase`.
3. Uruchom projekt:

```bash
dotnet run
```

### Frontend

Otwórz plik `index.html` przy użyciu lokalnego serwera (np. Live Server w Visual Studio Code).

Frontend automatycznie korzysta z lokalnego API podczas pracy na `localhost` oraz z produkcyjnego API po wdrożeniu na GitHub Pages.

## Deployment

* Frontend jest automatycznie publikowany na **GitHub Pages** przy każdym pushu do gałęzi `master` za pomocą **GitHub Actions**.
* Backend jest automatycznie wdrażany na **Render** przy każdym pushu do gałęzi `master`.
* Dane aplikacji przechowywane są w bazie **Supabase PostgreSQL**.
