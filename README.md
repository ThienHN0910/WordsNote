# WordsNote

A full-stack Vocabulary Learning System featuring note-taking, flashcards with spaced repetition, and Quizlet-style CSV import.

## Architecture

```
WordsNote/
├── src/
│   ├── backend/         # .NET 8 Web API (DDD Architecture)
│   ├── frontend/        # Vue 3 + Vite + Pinia + Tailwind CSS
│   └── extension/       # Chrome Extension (Vue 3 + Vite)
├── docker-compose.yml
└── .env.example
```

## Backend (.NET 8 + DDD)

### Projects

| Project | Description |
|---------|-------------|
| `WordsNote.Domain` | Entities (Deck, Card), Value Objects, Domain Events, Repository Interfaces |
| `WordsNote.Application` | MediatR CQRS, DTOs, AutoMapper profiles |
| `WordsNote.Infrastructure` | MongoDB Atlas repositories, External Services |
| `WordsNote.API` | ASP.NET Core Controllers, JWT Authentication, Swagger |

### Key Features

- **Spaced Repetition** (SM-2 / Leitner System): Cards scheduled using the SM-2 algorithm
- **CQRS with MediatR**: Clean separation of reads and writes
- **JWT Authentication**: Secure API access
- **MongoDB Atlas**: Flexible document storage

### Running the Backend

#### With Docker Compose

1. Copy `.env.example` to `.env` and update the MongoDB Atlas connection string:
   ```bash
   cp .env.example .env
   # Edit .env with your MongoDB Atlas connection string
   ```

2. Start services:
   ```bash
   docker-compose up -d
   ```

3. Access Swagger UI at `http://localhost:5000/swagger`

#### Local Development

```bash
cd src/backend
dotnet restore
dotnet run --project WordsNote.API
```

### API Endpoints

#### Decks
- `GET /api/decks` — List all decks
- `POST /api/decks` — Create a deck
- `GET /api/decks/{id}` — Get deck details
- `PUT /api/decks/{id}` — Update deck
- `DELETE /api/decks/{id}` — Delete deck
- `POST /api/decks/{id}/import` — Import cards from CSV file
- `POST /api/decks/{id}/reset` — Reset all card progress

#### Cards
- `GET /api/cards?deckId={id}` — List cards in a deck
- `POST /api/cards` — Create a card
- `GET /api/cards/due` — Get cards due for review today
- `POST /api/cards/{id}/review` — Submit review (0=Again, 1=Hard, 2=Good, 3=Easy)
- `DELETE /api/cards/{id}` — Delete a card

#### Auth
- `POST /api/auth/login` — Get JWT token

### CSV Import Format

```
Front,Back,Notes
Hello,Xin chào,Vietnamese greeting
Goodbye,Tạm biệt,
```

## Web Frontend (Vue 3 + Vite)

### Features

- **Dashboard**: View and manage all decks with due card counts
- **Deck Detail**: Add/remove cards, import CSV, reset progress
- **Flashcard Player**: 3D flip animation with spaced repetition review buttons
- **Progress Tracking**: Visual progress bar during study sessions

### Running the Frontend

```bash
cd src/frontend
npm install
cp .env.example .env
npm run dev
```

Access at `http://localhost:5173`

## Chrome Extension (Vue 3 + Vite)

### Features

- **Popup**: Quick view of daily due cards
- **Content Script**: Highlight text on any page → save to Inbox deck
- **Background Service Worker**: Handles API calls from content script

### Installing the Extension

1. Build:
   ```bash
   cd src/extension
   npm install
   npm run build
   ```

2. Open `chrome://extensions/`, enable **Developer Mode**, click **Load unpacked**, select `dist/`

## Data Schema

### Card
| Field | Type | Description |
|-------|------|-------------|
| Front | string | Question/word |
| Back | string | Answer/definition |
| Notes | string | Additional notes |
| Status | CardStatus | New/Learning/Learned |
| Interval | int | Days until next review |
| NextReviewDate | DateTime | Scheduled review date |

### CardStatus
- `New (0)` — Not yet reviewed
- `Learning (1)` — Active learning phase
- `Learned (2)` — Mastered (interval ≥ 21 days)

## Spaced Repetition (SM-2)

- **Correct**: `NewInterval = max(1, round(Interval × EaseFactor))`
- **Incorrect**: `Interval = 1`, `EaseFactor = max(1.3, EaseFactor - 0.2)`
