# Jordan Queue Mobile

React Native (Expo) customer app for joining virtual queues and tracking tickets.

## Prerequisites

- Node.js 20+
- [Expo Go](https://expo.dev/go) on your phone, or Android/iOS simulator
- Jordan Queue API running locally

## Setup

```bash
cd mobile/JordanQueue.Mobile
npm install
cp .env.example .env   # optional — edit API URL for physical devices
```

## Run

```bash
npm start
```

Then press:
- `a` — Android emulator
- `i` — iOS simulator (macOS only)
- Scan QR code — Expo Go on a physical device

## API URL

| Environment | URL |
|-------------|-----|
| Android emulator | `http://10.0.2.2:5257` (default) |
| iOS simulator / web | `http://localhost:5257` (default) |
| Physical device | `http://YOUR_PC_IP:5257` — set `EXPO_PUBLIC_API_URL` in `.env` |

## Test account

| Email | Password |
|-------|----------|
| customer1@jordanqueue.dev | Customer123! |

## Features

- Browse and search businesses
- View business details and queue status
- Register / login as customer
- Join queue and receive ticket (A001, A002, …)
- Live ticket screen with auto-refresh
- Cancel ticket while waiting
- My tickets history
- In-app notifications
- Arabic / English (default Arabic)

## Screens

| Tab / Screen | Description |
|--------------|-------------|
| Explore | Search and list businesses |
| My tickets | Ticket history and active tickets |
| Notifications | In-app notifications |
| Profile | Login, register, logout |
| Business detail | Services and join queue |
| Ticket | Live position and status |
