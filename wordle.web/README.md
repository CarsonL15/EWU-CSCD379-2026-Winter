# Word Wizardry

A Wordle-inspired word guessing game built with Nuxt 4, Vue 3, Vuetify 3, and TypeScript.

## Live Demo

**Azure URL:** https://icy-wave-03d88090f.4.azurestaticapps.net

## Features

- Daily word of the day mode
- Random word mode for unlimited play
- Two word lists (common words for answers, extended list for valid guesses)
- Hint generator
- Statistics tracking (wins, losses, streaks, average guesses)
- Word definitions displayed on win
- Dark/light mode with custom color themes
- Responsive design for mobile and desktop

## Setup

Make sure to install dependencies:

```bash
npm install
```

## Development Server

Start the development server on `http://localhost:3000`:

```bash
npm run dev
```

## Production

Build the application for production:

```bash
npm run build
```

Generate static site:

```bash
npm run generate
```

## Testing

Run unit tests:

```bash
npm run test
```

## Technologies

- [Nuxt 4](https://nuxt.com/)
- [Vue 3](https://vuejs.org/)
- [Vuetify 3](https://vuetifyjs.com/)
- [TypeScript](https://www.typescriptlang.org/)
- [Vitest](https://vitest.dev/)

## Deployment

This project is deployed to Azure Static Web Apps via GitHub Actions CI/CD pipeline.
